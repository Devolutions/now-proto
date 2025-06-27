use alloc::borrow::Cow;

use bitflags::bitflags;
use ironrdp_core::{
    cast_length, ensure_fixed_part_size, invalid_field_err, Decode, DecodeResult, Encode, EncodeResult, IntoOwned,
    ReadCursor, WriteCursor,
};

use crate::{NowExecMessage, NowExecMsgKind, NowHeader, NowMessage, NowMessageClass, NowVarStr};

bitflags! {
    /// NOW-PROTO: NOW_EXEC_BATCH_MSG msgFlags field.
    #[derive(Debug, Clone, Copy, PartialEq, Eq)]
    struct NowExecBatchFlags: u16 {
        /// Set if directory field contains non-default value.
        ///
        /// NOW-PROTO: NOW_EXEC_FLAG_BATCH_DIRECTORY_SET
        const DIRECTORY_SET = 0x0001;
        /// Enable stdio (stdout, stderr, stdin) redirection.
        ///
        /// NOW-PROTO: NOW_EXEC_FLAG_BATCH_IO_REDIRECTION
        const IO_REDIRECTION = 0x1000;
    }
}

/// The NOW_EXEC_BATCH_MSG message is used to execute a remote batch command.
///
/// NOW-PROTO: NOW_EXEC_BATCH_MSG
#[derive(Debug, Clone, PartialEq, Eq)]
pub struct NowExecBatchMsg<'a> {
    flags: NowExecBatchFlags,
    session_id: u32,
    command: NowVarStr<'a>,
    directory: NowVarStr<'a>,
}

impl_pdu_borrowing!(NowExecBatchMsg<'_>, OwnedNowExecBatchMsg);

impl IntoOwned for NowExecBatchMsg<'_> {
    type Owned = OwnedNowExecBatchMsg;

    fn into_owned(self) -> Self::Owned {
        OwnedNowExecBatchMsg {
            flags: self.flags,
            session_id: self.session_id,
            command: self.command.into_owned(),
            directory: self.directory.into_owned(),
        }
    }
}

impl<'a> NowExecBatchMsg<'a> {
    const NAME: &'static str = "NOW_EXEC_BATCH_MSG";
    const FIXED_PART_SIZE: usize = 4;

    pub fn new(session_id: u32, command: impl Into<Cow<'a, str>>) -> EncodeResult<Self> {
        let msg = Self {
            flags: NowExecBatchFlags::empty(),
            session_id,
            command: NowVarStr::new(command)?,
            directory: NowVarStr::default(),
        };

        msg.ensure_message_size()?;

        Ok(msg)
    }

    pub fn with_directory(mut self, directory: impl Into<Cow<'a, str>>) -> EncodeResult<Self> {
        self.flags |= NowExecBatchFlags::DIRECTORY_SET;
        self.directory = NowVarStr::new(directory)?;

        self.ensure_message_size()?;

        Ok(self)
    }

    #[must_use]
    pub fn with_io_redirection(mut self) -> Self {
        self.flags |= NowExecBatchFlags::IO_REDIRECTION;
        self
    }

    pub fn is_with_io_redirection(&self) -> bool {
        self.flags.contains(NowExecBatchFlags::IO_REDIRECTION)
    }

    pub fn session_id(&self) -> u32 {
        self.session_id
    }

    pub fn command(&self) -> &str {
        &self.command
    }

    pub fn directory(&self) -> Option<&str> {
        if self.flags.contains(NowExecBatchFlags::DIRECTORY_SET) {
            Some(&self.directory)
        } else {
            None
        }
    }

    // LINTS: Overall message size always fits into usize; VarStr size always a few powers of 2 less
    // than u32::MAX, therefore it fits into usize
    #[allow(clippy::arithmetic_side_effects)]
    fn body_size(&self) -> usize {
        Self::FIXED_PART_SIZE + self.command.size() + self.directory.size()
    }

    fn ensure_message_size(&self) -> EncodeResult<()> {
        ensure_now_message_size!(Self::FIXED_PART_SIZE, self.command.size(), self.directory.size());

        Ok(())
    }

    pub(super) fn decode_from_body(header: NowHeader, src: &mut ReadCursor<'a>) -> DecodeResult<Self> {
        ensure_fixed_part_size!(in: src);

        let flags = NowExecBatchFlags::from_bits_retain(header.flags);
        let session_id = src.read_u32();
        let command = NowVarStr::decode(src)?;
        let directory = NowVarStr::decode(src)?;

        Ok(Self {
            flags,
            session_id,
            command,
            directory,
        })
    }
}

impl Encode for NowExecBatchMsg<'_> {
    fn encode(&self, dst: &mut WriteCursor<'_>) -> EncodeResult<()> {
        let header = NowHeader {
            size: cast_length!("size", self.body_size())?,
            class: NowMessageClass::EXEC,
            kind: NowExecMsgKind::BATCH.0,
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

impl<'de> Decode<'de> for NowExecBatchMsg<'de> {
    fn decode(src: &mut ReadCursor<'de>) -> DecodeResult<Self> {
        let header = NowHeader::decode(src)?;

        match (header.class, NowExecMsgKind(header.kind)) {
            (NowMessageClass::EXEC, NowExecMsgKind::BATCH) => Self::decode_from_body(header, src),
            _ => Err(invalid_field_err!("type", "invalid message type")),
        }
    }
}

impl<'a> From<NowExecBatchMsg<'a>> for NowMessage<'a> {
    fn from(msg: NowExecBatchMsg<'a>) -> Self {
        NowMessage::Exec(NowExecMessage::Batch(msg))
    }
}
