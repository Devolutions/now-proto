use ironrdp_core::{invalid_field_err, Decode, DecodeResult, Encode, EncodeResult, IntoOwned, ReadCursor, WriteCursor};

use crate::{NowChannelMessage, NowChannelMsgKind, NowHeader, NowMessage, NowMessageClass, NowStatus};

/// Channel termination notice, could be sent by either parties at any moment of communication to
/// gracefully close DVC channel.
///
/// NOW-PROTO: NOW_CHANNEL_TERMINATE_MSG
#[derive(Debug, Clone, PartialEq, Eq)]
pub struct NowChannelTerminateMsg<'a> {
    status: NowStatus<'a>,
}

impl_pdu_borrowing!(NowChannelTerminateMsg<'_>, OwnedNowChannelTerminateMsg);

impl IntoOwned for NowChannelTerminateMsg<'_> {
    type Owned = OwnedNowChannelTerminateMsg;

    fn into_owned(self) -> Self::Owned {
        OwnedNowChannelTerminateMsg {
            status: self.status.into_owned(),
        }
    }
}

impl<'a> NowChannelTerminateMsg<'a> {
    const NAME: &'static str = "NOW_CHANNEL_TERMINATE_MSG";

    pub fn from_status(status: NowStatus<'a>) -> EncodeResult<Self> {
        let msg = Self { status };

        msg.ensure_message_size().expect("validated in constructor");

        Ok(msg)
    }

    pub(super) fn decode_from_body(_header: NowHeader, src: &mut ReadCursor<'a>) -> DecodeResult<Self> {
        let status = NowStatus::decode(src)?;

        Ok(Self { status })
    }

    fn ensure_message_size(&self) -> EncodeResult<()> {
        let _message_size =
            u32::try_from(self.status.size()).map_err(|_| invalid_field_err!("size", "message size overflow"))?;

        Ok(())
    }
}

impl Encode for NowChannelTerminateMsg<'_> {
    fn encode(&self, dst: &mut WriteCursor<'_>) -> EncodeResult<()> {
        let header = NowHeader {
            size: self.status.size().try_into().expect("validated in constructor"),
            class: NowMessageClass::CHANNEL,
            kind: NowChannelMsgKind::TERMINATE.0,
            flags: 0,
        };

        header.encode(dst)?;

        self.status.encode(dst)?;

        Ok(())
    }

    fn name(&self) -> &'static str {
        Self::NAME
    }

    fn size(&self) -> usize {
        NowHeader::FIXED_PART_SIZE + self.status.size()
    }
}

impl<'de> Decode<'de> for NowChannelTerminateMsg<'de> {
    fn decode(src: &mut ReadCursor<'de>) -> DecodeResult<Self> {
        let header = NowHeader::decode(src)?;

        match (header.class, NowChannelMsgKind(header.kind)) {
            (NowMessageClass::CHANNEL, NowChannelMsgKind::TERMINATE) => Self::decode_from_body(header, src),
            _ => Err(invalid_field_err!("type", "invalid message type")),
        }
    }
}

impl<'a> From<NowChannelTerminateMsg<'a>> for NowMessage<'a> {
    fn from(msg: NowChannelTerminateMsg<'a>) -> Self {
        NowMessage::Channel(NowChannelMessage::Terminate(msg))
    }
}
