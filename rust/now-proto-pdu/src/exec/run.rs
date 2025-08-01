use alloc::borrow::Cow;

use bitflags::bitflags;
use ironrdp_core::{
    cast_length, ensure_fixed_part_size, invalid_field_err, Decode, DecodeResult, Encode, EncodeResult, IntoOwned,
    ReadCursor, WriteCursor,
};

use crate::{NowExecMessage, NowExecMsgKind, NowHeader, NowMessage, NowMessageClass, NowVarStr};

bitflags! {
    /// NOW-PROTO: NOW_EXEC_RUN_MSG msgFlags field.
    #[derive(Debug, Clone, Copy, PartialEq, Eq)]
    struct NowExecRunFlags: u16 {
        /// Set if directory field contains non-default value.
        ///
        /// NOW-PROTO: NOW_EXEC_FLAG_RUN_DIRECTORY_SET
        const DIRECTORY_SET = 0x0001;
    }
}

/// The NOW_EXEC_RUN_MSG message is used to send a run request. This request type maps to starting
/// a program by using the “Run” menu on operating systems (the Start Menu on Windows, the Dock on
/// macOS etc.). The execution of programs started with NOW_EXEC_RUN_MSG is not followed and does
/// not send back the output.
///
/// NOW_PROTO: NOW_EXEC_RUN_MSG
#[derive(Debug, Clone, PartialEq, Eq)]
pub struct NowExecRunMsg<'a> {
    flags: NowExecRunFlags,
    session_id: u32,
    command: NowVarStr<'a>,
    directory: NowVarStr<'a>,
}

impl_pdu_borrowing!(NowExecRunMsg<'_>, OwnedNowExecRunMsg);

impl IntoOwned for NowExecRunMsg<'_> {
    type Owned = OwnedNowExecRunMsg;

    fn into_owned(self) -> Self::Owned {
        OwnedNowExecRunMsg {
            flags: self.flags,
            session_id: self.session_id,
            command: self.command.into_owned(),
            directory: self.directory.into_owned(),
        }
    }
}

impl<'a> NowExecRunMsg<'a> {
    const NAME: &'static str = "NOW_EXEC_RUN_MSG";
    const FIXED_PART_SIZE: usize = 4;

    pub fn new(session_id: u32, command: impl Into<Cow<'a, str>>) -> EncodeResult<Self> {
        let msg = Self {
            flags: NowExecRunFlags::empty(),
            session_id,
            command: NowVarStr::new(command)?,
            directory: NowVarStr::default(),
        };

        ensure_now_message_size!(Self::FIXED_PART_SIZE, msg.command.size());

        Ok(msg)
    }

    pub fn with_directory(mut self, directory: impl Into<Cow<'a, str>>) -> EncodeResult<Self> {
        self.flags |= NowExecRunFlags::DIRECTORY_SET;
        self.directory = NowVarStr::new(directory)?;

        self.ensure_message_size()?;

        Ok(self)
    }

    pub fn session_id(&self) -> u32 {
        self.session_id
    }

    pub fn command(&self) -> &str {
        &self.command
    }

    pub fn directory(&self) -> &str {
        &self.directory
    }

    fn ensure_message_size(&self) -> EncodeResult<()> {
        ensure_now_message_size!(Self::FIXED_PART_SIZE, self.command.size(), self.directory.size());

        Ok(())
    }

    // LINTS: Overall message size is validated in the constructor/decode method
    #[allow(clippy::arithmetic_side_effects)]
    fn body_size(&self) -> usize {
        Self::FIXED_PART_SIZE + self.command.size() + self.directory.size()
    }

    pub(super) fn decode_from_body(_header: NowHeader, src: &mut ReadCursor<'a>) -> DecodeResult<Self> {
        ensure_fixed_part_size!(in: src);

        let mut flags = NowExecRunFlags::empty();
        let session_id = src.read_u32();
        let command: NowVarStr<'_> = NowVarStr::decode(src)?;

        // Directory field has been added in v1.1.
        let directory = if !src.is_empty() {
            let directory = NowVarStr::decode(src)?;

            if !directory.is_empty() {
                flags |= NowExecRunFlags::DIRECTORY_SET;
            }

            directory
        } else {
            NowVarStr::default()
        };

        let msg = Self {
            flags,
            session_id,
            command,
            directory,
        };

        Ok(msg)
    }
}

impl Encode for NowExecRunMsg<'_> {
    fn encode(&self, dst: &mut WriteCursor<'_>) -> EncodeResult<()> {
        let header = NowHeader {
            size: cast_length!("size", self.body_size())?,
            class: NowMessageClass::EXEC,
            kind: NowExecMsgKind::RUN.0,
            flags: self.flags.bits(),
        };

        header.encode(dst)?;

        ensure_fixed_part_size!(in: dst);
        dst.write_u32(self.session_id);
        self.command.encode(dst)?;
        self.directory.encode(dst)?;

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

impl<'de> Decode<'de> for NowExecRunMsg<'de> {
    fn decode(src: &mut ReadCursor<'de>) -> DecodeResult<Self> {
        let header = NowHeader::decode(src)?;

        match (header.class, NowExecMsgKind(header.kind)) {
            (NowMessageClass::EXEC, NowExecMsgKind::RUN) => Self::decode_from_body(header, src),
            _ => Err(invalid_field_err!("type", "invalid message type")),
        }
    }
}

impl<'a> From<NowExecRunMsg<'a>> for NowMessage<'a> {
    fn from(msg: NowExecRunMsg<'a>) -> Self {
        NowMessage::Exec(NowExecMessage::Run(msg))
    }
}
