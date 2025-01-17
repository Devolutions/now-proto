use alloc::borrow::Cow;
use core::time;

use bitflags::bitflags;
use ironrdp_core::{
    cast_length, ensure_fixed_part_size, invalid_field_err, Decode, DecodeResult, Encode, EncodeResult, IntoOwned,
    ReadCursor, WriteCursor,
};

use crate::{NowHeader, NowMessage, NowMessageClass, NowSessionMessage, NowSessionMessageKind, NowVarStr};

/// Message box style; Directly maps to the WinAPI MessageBox function message box style field.
///
/// NOW_PROTO: `style` field from NOW_SESSION_MESSAGE_BOX_REQ_MSG
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct NowMessageBoxStyle(u32);

impl NowMessageBoxStyle {
    pub const OK: Self = Self(0x00000000);
    pub const OK_CANCEL: Self = Self(0x00000001);
    pub const ABORT_RETRY_IGNORE: Self = Self(0x00000002);
    pub const YES_NO_CANCEL: Self = Self(0x00000003);
    pub const YES_NO: Self = Self(0x00000004);
    pub const RETRY_CANCEL: Self = Self(0x00000005);
    pub const CANCEL_TRY_CONTINUE: Self = Self(0x00000006);
    pub const HELP: Self = Self(0x00004000);

    pub fn new(style: u32) -> Self {
        Self(style)
    }

    pub fn value(&self) -> u32 {
        self.0
    }
}

bitflags! {
    #[derive(Debug, Clone, Copy, PartialEq, Eq)]
    pub struct NowSessionMessageBoxFlags: u16 {
        /// The title field contains non-default value.
        ///
        /// NOW_PROTO: NOW_SESSION_MSGBOX_FLAG_TITLE
        const TITLE = 0x0001;

        /// The style field contains non-default value.
        ///
        /// NOW_PROTO: NOW_SESSION_MSGBOX_FLAG_STYLE
        const STYLE = 0x0002;

        /// The timeout field contains non-default value.
        ///
        /// NOW_PROTO: NOW_SESSION_MSGBOX_FLAG_TIMEOUT
        const TIMEOUT = 0x0004;

        /// A response message is expected (don't fire and forget)
        ///
        /// NOW_PROTO: NOW_SESSION_MSGBOX_FLAG_RESPONSE
        const RESPONSE = 0x0008;
    }
}

/// The NOW_SESSION_MSGBOX_REQ_MSG is used to show a message box in the user session, similar to
/// what the [WTSSendMessage function](https://learn.microsoft.com/en-us/windows/win32/api/wtsapi32/nf-wtsapi32-wtssendmessagew)
/// does.
///
/// NOW_PROTO: NOW_SESSION_MSGBOX_REQ_MSG
#[derive(Debug, Clone, PartialEq, Eq)]
pub struct NowSessionMsgBoxReqMsg<'a> {
    flags: NowSessionMessageBoxFlags,
    request_id: u32,
    style: NowMessageBoxStyle,
    timeout: u32,
    title: NowVarStr<'a>,
    message: NowVarStr<'a>,
}

impl_pdu_borrowing!(NowSessionMsgBoxReqMsg<'_>, OwnedNowSessionMsgBoxReqMsg);

impl IntoOwned for NowSessionMsgBoxReqMsg<'_> {
    type Owned = OwnedNowSessionMsgBoxReqMsg;

    fn into_owned(self) -> Self::Owned {
        OwnedNowSessionMsgBoxReqMsg {
            flags: self.flags,
            request_id: self.request_id,
            style: self.style,
            timeout: self.timeout,
            title: self.title.into_owned(),
            message: self.message.into_owned(),
        }
    }
}

impl<'a> NowSessionMsgBoxReqMsg<'a> {
    const NAME: &'static str = "NOW_SESSION_MSGBOX_REQ_MSG";
    const FIXED_PART_SIZE: usize = 12;

    pub fn new(request_id: u32, message: impl Into<Cow<'a, str>>) -> EncodeResult<Self> {
        let msg = Self {
            flags: NowSessionMessageBoxFlags::empty(),
            request_id,
            style: NowMessageBoxStyle::OK,
            timeout: 0,
            title: NowVarStr::default(),
            message: NowVarStr::new(message)?,
        };

        Ok(msg)
    }

    fn ensure_message_size(&self) -> EncodeResult<()> {
        ensure_now_message_size!(Self::FIXED_PART_SIZE, self.title.size(), self.message.size());
        Ok(())
    }

    pub fn with_title(mut self, title: impl Into<Cow<'a, str>>) -> EncodeResult<Self> {
        self.flags |= NowSessionMessageBoxFlags::TITLE;
        self.title = NowVarStr::new(title)?;

        self.ensure_message_size()?;

        Ok(self)
    }

    #[must_use]
    pub fn with_style(mut self, style: NowMessageBoxStyle) -> Self {
        self.flags |= NowSessionMessageBoxFlags::STYLE;
        self.style = style;
        self
    }

    pub fn with_timeout(mut self, timeout: time::Duration) -> EncodeResult<Self> {
        // Sanity check: Limit message box timeout to ~1 week.
        const MAX_MSGBOX_TINEOUT: time::Duration = time::Duration::from_secs(60 * 60 * 24 * 7);

        if timeout > MAX_MSGBOX_TINEOUT {
            return Err(invalid_field_err!("timeout", "too big message box timeout"));
        }

        let timeout = u32::try_from(timeout.as_secs()).expect("timeout is within u32 range");

        self.flags |= NowSessionMessageBoxFlags::TIMEOUT;
        self.timeout = timeout;
        Ok(self)
    }

    #[must_use]
    pub fn with_response(mut self) -> Self {
        self.flags |= NowSessionMessageBoxFlags::RESPONSE;
        self
    }

    pub fn request_id(&self) -> u32 {
        self.request_id
    }

    pub fn style(&self) -> NowMessageBoxStyle {
        if self.flags.contains(NowSessionMessageBoxFlags::STYLE) {
            self.style
        } else {
            NowMessageBoxStyle::OK
        }
    }

    pub fn timeout(&self) -> Option<time::Duration> {
        if self.flags.contains(NowSessionMessageBoxFlags::TIMEOUT) && self.timeout > 0 {
            Some(time::Duration::from_secs(self.timeout.into()))
        } else {
            None
        }
    }

    pub fn title(&self) -> Option<&str> {
        if self.flags.contains(NowSessionMessageBoxFlags::TITLE) {
            Some(&self.title)
        } else {
            None
        }
    }

    pub fn message(&self) -> &str {
        &self.message
    }

    pub fn is_response_expected(&self) -> bool {
        self.flags.contains(NowSessionMessageBoxFlags::RESPONSE)
    }

    // LINTS: Overall message size is validated in the constructor/decode method
    #[allow(clippy::arithmetic_side_effects)]
    fn body_size(&self) -> usize {
        Self::FIXED_PART_SIZE + self.title.size() + self.message.size()
    }

    pub(super) fn decode_from_body(header: NowHeader, src: &mut ReadCursor<'a>) -> DecodeResult<Self> {
        ensure_fixed_part_size!(in: src);

        let flags = NowSessionMessageBoxFlags::from_bits_retain(header.flags);
        let request_id = src.read_u32();
        let style = NowMessageBoxStyle(src.read_u32());
        let timeout = src.read_u32();
        let title = NowVarStr::decode(src)?;
        let message = NowVarStr::decode(src)?;

        let msg = Self {
            flags,
            request_id,
            style,
            timeout,
            title,
            message,
        };

        Ok(msg)
    }
}

impl Encode for NowSessionMsgBoxReqMsg<'_> {
    fn encode(&self, dst: &mut WriteCursor<'_>) -> EncodeResult<()> {
        let header = NowHeader {
            size: cast_length!("size", self.body_size())?,
            class: NowMessageClass::SESSION,
            kind: NowSessionMessageKind::MSGBOX_REQ.0,
            flags: self.flags.bits(),
        };

        header.encode(dst)?;

        ensure_fixed_part_size!(in: dst);
        dst.write_u32(self.request_id);
        dst.write_u32(self.style.value());
        dst.write_u32(self.timeout);
        self.title.encode(dst)?;
        self.message.encode(dst)?;

        Ok(())
    }

    fn name(&self) -> &'static str {
        Self::NAME
    }

    // LINTS: See body_size()
    #[allow(clippy::arithmetic_side_effects)]
    fn size(&self) -> usize {
        NowHeader::FIXED_PART_SIZE + self.body_size()
    }
}

impl<'de> Decode<'de> for NowSessionMsgBoxReqMsg<'de> {
    fn decode(src: &mut ReadCursor<'de>) -> DecodeResult<Self> {
        let header = NowHeader::decode(src)?;

        match (header.class, NowSessionMessageKind(header.kind)) {
            (NowMessageClass::SESSION, NowSessionMessageKind::MSGBOX_REQ) => Self::decode_from_body(header, src),
            _ => Err(unsupported_message_err!(class: header.class.0, kind: header.kind)),
        }
    }
}

impl<'a> From<NowSessionMsgBoxReqMsg<'a>> for NowMessage<'a> {
    fn from(val: NowSessionMsgBoxReqMsg<'a>) -> Self {
        NowMessage::Session(NowSessionMessage::MsgBoxReq(val))
    }
}
