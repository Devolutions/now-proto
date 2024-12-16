//! Buffer types for NOW protocol.

use alloc::vec::Vec;

use ironrdp_core::{
    cast_length, ensure_size, invalid_field_err, Decode, DecodeResult, Encode, EncodeResult,
    ReadCursor, WriteCursor,
};

use crate::VarU32;

/// Buffer up to 2^31 bytes long (Length has compact variable length encoding).
///
/// NOW-PROTO: NOW_VARBUF
#[derive(Debug, Clone, PartialEq, Eq)]
pub struct NowVarBuf(Vec<u8>);

impl NowVarBuf {
    const NAME: &'static str = "NOW_VARBUF";

    /// Create a new `NowVarBuf` instance. Returns an error if the provided value is too large.
    pub fn new(value: impl Into<Vec<u8>>) -> DecodeResult<Self> {
        let value = value.into();

        let _: u32 = value
            .len()
            .try_into()
            .ok()
            .and_then(|val| if val <= VarU32::MAX { Some(val) } else { None })
            .ok_or_else(|| invalid_field_err!("data", "too large buffer"))?;

        Ok(NowVarBuf(value))
    }

    /// Get the buffer value.
    pub fn value(&self) -> &[u8] {
        self.0.as_slice()
    }
}

impl Encode for NowVarBuf {
    fn encode(&self, dst: &mut WriteCursor<'_>) -> EncodeResult<()> {
        let encoded_size = self.size();
        ensure_size!(in: dst, size: encoded_size);

        let len: u32 = self.0.len().try_into().expect("BUG: validated in constructor");

        VarU32::new(len)?.encode(dst)?;
        dst.write_slice(self.0.as_slice());

        Ok(())
    }

    fn name(&self) -> &'static str {
        Self::NAME
    }

    fn size(&self) -> usize {
        // <variable-length size> + <data bytes>
        // NOTE: Wrapping add will not overflow because the size is limited by VarU32::MAX
        VarU32::new(self.0.len().try_into().expect("buffer size always fits into u32"))
            .expect("buffer size is validated in constructor and should not overflow")
            .size()
            .wrapping_add(self.0.len())
    }
}

impl Decode<'_> for NowVarBuf {
    fn decode(src: &mut ReadCursor<'_>) -> DecodeResult<Self> {
        let len_u32 = VarU32::decode(src)?.value();
        let len: usize = cast_length!("len", len_u32)?;

        ensure_size!(in: src, size: len);
        let bytes = src.read_slice(len);

        Ok(NowVarBuf(bytes.to_vec()))
    }
}

impl From<NowVarBuf> for Vec<u8> {
    fn from(buf: NowVarBuf) -> Self {
        buf.0
    }
}
