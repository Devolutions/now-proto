mod lock;
mod logoff;
mod msg_box_req;
mod msg_box_rsp;
mod set_kbd_layout;

use ironrdp_core::{DecodeResult, Encode, EncodeResult, IntoOwned, ReadCursor, WriteCursor};
pub use lock::NowSessionLockMsg;
pub use logoff::NowSessionLogoffMsg;
pub use msg_box_req::{NowMessageBoxStyle, NowSessionMsgBoxReqMsg, OwnedNowSessionMsgBoxReqMsg};
pub use msg_box_rsp::{NowMsgBoxResponse, NowSessionMsgBoxRspMsg, OwnedNowSessionMsgBoxRspMsg};
pub use set_kbd_layout::{NowSessionSetKbdLayoutMsg, OwnedNowSessionSetKbdLayoutMsg, SetKbdLayoutOption};

use crate::NowHeader;

/// Wrapper for the `NOW_SESSION_MSG_CLASS_ID` message class.
#[derive(Debug, Clone, PartialEq, Eq)]
pub struct NowSessionMessageKind(pub u8);

impl NowSessionMessageKind {
    /// NOW-PROTO: NOW_SESSION_LOCK_MSG_ID
    pub const LOCK: Self = Self(0x01);
    /// NOW-PROTO: NOW_SESSION_LOGOFF_MSG_ID
    pub const LOGOFF: Self = Self(0x02);
    /// NOW-PROTO: NOW_SESSION_MSGBOX_REQ_MSG_ID
    pub const MSGBOX_REQ: Self = Self(0x03);
    /// NOW-PROTO: NOW_SESSION_MSGBOX_RSP_MSG_ID
    pub const MSGBOX_RSP: Self = Self(0x04);
    /// NOW-PROTO: NOW_SESSION_SET_KBD_LAYOUT_MSG_ID
    pub const SET_KBD_LAYOUT: Self = Self(0x05);
}

// Wrapper for the `NOW_SESSION_MSG_CLASS_ID` message class.
#[derive(Debug, Clone, PartialEq, Eq)]
pub enum NowSessionMessage<'a> {
    Lock(NowSessionLockMsg),
    Logoff(NowSessionLogoffMsg),
    MsgBoxReq(NowSessionMsgBoxReqMsg<'a>),
    MsgBoxRsp(NowSessionMsgBoxRspMsg<'a>),
    SetKbdLayout(NowSessionSetKbdLayoutMsg<'a>),
}

pub type OwnedNowSessionMessage = NowSessionMessage<'static>;

impl IntoOwned for NowSessionMessage<'_> {
    type Owned = OwnedNowSessionMessage;

    fn into_owned(self) -> Self::Owned {
        match self {
            Self::Lock(msg) => OwnedNowSessionMessage::Lock(msg),
            Self::Logoff(msg) => OwnedNowSessionMessage::Logoff(msg),
            Self::MsgBoxReq(msg) => OwnedNowSessionMessage::MsgBoxReq(msg.into_owned()),
            Self::MsgBoxRsp(msg) => OwnedNowSessionMessage::MsgBoxRsp(msg.into_owned()),
            Self::SetKbdLayout(msg) => OwnedNowSessionMessage::SetKbdLayout(msg.into_owned()),
        }
    }
}

impl<'a> NowSessionMessage<'a> {
    const NAME: &'static str = "NOW_SESSION_MSG";

    pub fn decode_from_body(header: NowHeader, src: &mut ReadCursor<'a>) -> DecodeResult<Self> {
        match NowSessionMessageKind(header.kind) {
            NowSessionMessageKind::LOCK => Ok(Self::Lock(NowSessionLockMsg::default())),
            NowSessionMessageKind::LOGOFF => Ok(Self::Logoff(NowSessionLogoffMsg::default())),
            NowSessionMessageKind::MSGBOX_REQ => {
                Ok(Self::MsgBoxReq(NowSessionMsgBoxReqMsg::decode_from_body(header, src)?))
            }
            NowSessionMessageKind::MSGBOX_RSP => {
                Ok(Self::MsgBoxRsp(NowSessionMsgBoxRspMsg::decode_from_body(header, src)?))
            }
            NowSessionMessageKind::SET_KBD_LAYOUT => Ok(Self::SetKbdLayout(
                NowSessionSetKbdLayoutMsg::decode_from_body(header, src)?,
            )),
            _ => Err(unsupported_message_err!(class: header.class.0, kind: header.kind)),
        }
    }
}

impl Encode for NowSessionMessage<'_> {
    fn encode(&self, dst: &mut WriteCursor<'_>) -> EncodeResult<()> {
        match self {
            Self::Lock(msg) => msg.encode(dst),
            Self::Logoff(msg) => msg.encode(dst),
            Self::MsgBoxReq(msg) => msg.encode(dst),
            Self::MsgBoxRsp(msg) => msg.encode(dst),
            Self::SetKbdLayout(msg) => msg.encode(dst),
        }
    }

    fn name(&self) -> &'static str {
        Self::NAME
    }

    fn size(&self) -> usize {
        match self {
            Self::Lock(msg) => msg.size(),
            Self::Logoff(msg) => msg.size(),
            Self::MsgBoxReq(msg) => msg.size(),
            Self::MsgBoxRsp(msg) => msg.size(),
            Self::SetKbdLayout(msg) => msg.size(),
        }
    }
}
