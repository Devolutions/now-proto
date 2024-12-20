mod capset;
mod heartbeat;
mod terminate;

use ironrdp_core::{DecodeResult, Encode, EncodeResult, IntoOwned, ReadCursor, WriteCursor};

use crate::NowHeader;

pub use capset::NowChannelCapsetMsg;
pub use heartbeat::NowChannelHeartbeatMsg;
pub use terminate::{NowChannelTerminateMsg, OwnedNowChannelTerminateMsg};

// Wrapper for the `NOW_CHANNEL_MSG_CLASS_ID` message class.
#[derive(Debug, Clone, PartialEq, Eq)]
pub enum NowChannelMessage<'a> {
    Capset(NowChannelCapsetMsg),
    Heartbeat(NowChannelHeartbeatMsg),
    Terminate(NowChannelTerminateMsg<'a>),
}

pub type OwnedNowChannelMessage = NowChannelMessage<'static>;

impl IntoOwned for NowChannelMessage<'_> {
    type Owned = OwnedNowChannelMessage;

    fn into_owned(self) -> Self::Owned {
        match self {
            Self::Capset(msg) => OwnedNowChannelMessage::Capset(msg),
            Self::Heartbeat(msg) => OwnedNowChannelMessage::Heartbeat(msg),
            Self::Terminate(msg) => OwnedNowChannelMessage::Terminate(msg.into_owned()),
        }
    }
}

impl<'a> NowChannelMessage<'a> {
    const NAME: &'static str = "NOW_CHANNEL_MSG";

    pub fn decode_from_body(header: NowHeader, src: &mut ReadCursor<'a>) -> DecodeResult<Self> {
        match NowChannelMsgKind(header.kind) {
            NowChannelMsgKind::CAPSET => Ok(Self::Capset(NowChannelCapsetMsg::decode_from_body(header, src)?)),
            NowChannelMsgKind::HEARTBEAT => Ok(Self::Heartbeat(NowChannelHeartbeatMsg::default())),
            NowChannelMsgKind::TERMINATE => Ok(Self::Terminate(NowChannelTerminateMsg::decode_from_body(header, src)?)),
            _ => Err(unsupported_message_err!(class: header.class.0, kind: header.kind)),
        }
    }
}

impl Encode for NowChannelMessage<'_> {
    fn encode(&self, dst: &mut WriteCursor<'_>) -> EncodeResult<()> {
        match self {
            Self::Capset(msg) => msg.encode(dst),
            Self::Heartbeat(msg) => msg.encode(dst),
            Self::Terminate(msg) => msg.encode(dst),
        }
    }

    fn name(&self) -> &'static str {
        Self::NAME
    }

    fn size(&self) -> usize {
        match self {
            Self::Capset(msg) => msg.size(),
            Self::Heartbeat(msg) => msg.size(),
            Self::Terminate(msg) => msg.size(),
        }
    }
}

/// NOW-PROTO: NOW_CHANNEL_*_ID
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct NowChannelMsgKind(pub u8);

impl NowChannelMsgKind {
    /// NOW-PROTO: NOW_CHANNEL_CAPSET_MSG_ID
    pub const CAPSET: Self = Self(0x01);
    /// NOW-PROTO: NOW_CHANNEL_HEARTBEAT_MSG_ID
    pub const HEARTBEAT: Self = Self(0x02);
    /// NOW-PROTO: NOW_CHANNEL_TERMINATE_MSG_ID
    pub const TERMINATE: Self = Self(0x03);
}
