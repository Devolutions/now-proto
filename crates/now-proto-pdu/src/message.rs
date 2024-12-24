use ironrdp_core::{Decode, DecodeResult, Encode, EncodeResult, IntoOwned, ReadCursor, WriteCursor};

use crate::{NowChannelMessage, NowExecMessage, NowHeader, NowMessageClass, NowSessionMessage, NowSystemMessage};

/// Wrapper type for messages transferred over the NOW-PROTO communication channel.
///
/// NOW-PROTO: NOW_*_MSG messages
#[derive(Debug, Clone, PartialEq, Eq)]
pub enum NowMessage<'a> {
    Channel(NowChannelMessage<'a>),
    System(NowSystemMessage<'a>),
    Session(NowSessionMessage<'a>),
    Exec(NowExecMessage<'a>),
}

impl_pdu_borrowing!(NowMessage<'_>, OwnedNowMessage);

impl IntoOwned for NowMessage<'_> {
    type Owned = OwnedNowMessage;

    fn into_owned(self) -> Self::Owned {
        match self {
            Self::Channel(msg) => OwnedNowMessage::Channel(msg.into_owned()),
            Self::System(msg) => OwnedNowMessage::System(msg.into_owned()),
            Self::Session(msg) => OwnedNowMessage::Session(msg.into_owned()),
            Self::Exec(msg) => OwnedNowMessage::Exec(msg.into_owned()),
        }
    }
}

impl NowMessage<'_> {
    const NAME: &'static str = "NOW_MSG";
}

impl Encode for NowMessage<'_> {
    fn encode(&self, dst: &mut WriteCursor<'_>) -> EncodeResult<()> {
        match self {
            Self::Channel(msg) => msg.encode(dst),
            Self::System(msg) => msg.encode(dst),
            Self::Session(msg) => msg.encode(dst),
            Self::Exec(msg) => msg.encode(dst),
        }
    }

    fn name(&self) -> &'static str {
        Self::NAME
    }

    fn size(&self) -> usize {
        match self {
            Self::Channel(msg) => msg.size(),
            Self::System(msg) => msg.size(),
            Self::Session(msg) => msg.size(),
            Self::Exec(msg) => msg.size(),
        }
    }
}

impl<'de> Decode<'de> for NowMessage<'de> {
    fn decode(src: &mut ReadCursor<'de>) -> DecodeResult<Self> {
        let header = NowHeader::decode(src)?;
        Self::decode_from_body(header, src)
    }
}

impl<'a> NowMessage<'a> {
    pub fn decode_from_body(header: NowHeader, src: &mut ReadCursor<'a>) -> DecodeResult<Self> {
        match NowMessageClass(header.class.0) {
            NowMessageClass::CHANNEL => Ok(Self::Channel(NowChannelMessage::decode_from_body(header, src)?)),
            NowMessageClass::SYSTEM => Ok(Self::System(NowSystemMessage::decode_from_body(header, src)?)),
            NowMessageClass::SESSION => Ok(Self::Session(NowSessionMessage::decode_from_body(header, src)?)),
            NowMessageClass::EXEC => Ok(Self::Exec(NowExecMessage::decode_from_body(header, src)?)),
            // Handle unknown class; Unknown kind is handled by underlying message type.
            _ => Err(unsupported_message_err!(class: header.class.0, kind: header.kind)),
        }
    }
}
