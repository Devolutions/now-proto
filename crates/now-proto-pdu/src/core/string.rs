//! String types

use alloc::string::String;

use ironrdp_core::{
    cast_length, ensure_size, invalid_field_err, Decode, DecodeResult, Encode, EncodeResult,
    ReadCursor, WriteCursor,
};

use crate::VarU32;

/// String value up to 2^31 bytes long (Length has compact variable length encoding).
///
/// NOW-PROTO: NOW_VARSTR
#[derive(Debug, Clone, PartialEq, Eq)]
pub struct NowVarStr(String);

impl NowVarStr {
    pub const MAX_SIZE: usize = VarU32::MAX as usize;

    const NAME: &'static str = "NOW_VARSTR";

    /// Returns empty string.
    pub fn empty() -> Self {
        Self(String::new())
    }

    /// Creates `NowVarStr` from std string. Returns error if string is too big for the protocol.
    pub fn new(value: impl Into<String>) -> EncodeResult<Self> {
        let value = value.into();
        // IMPORTANT: we need to check for encoded UTF-8 size, not the string length.

        let _: u32 = value
            .as_bytes()
            .len()
            .try_into()
            .ok()
            .and_then(|val| if val <= VarU32::MAX { Some(val) } else { None })
            .ok_or_else(|| invalid_field_err!("string value", "too large string"))?;

        Ok(NowVarStr(value))
    }

    pub fn value(&self) -> &str {
        &self.0
    }
}

impl Encode for NowVarStr {
    fn encode(&self, dst: &mut WriteCursor<'_>) -> EncodeResult<()> {
        let encoded_size = self.size();
        ensure_size!(in: dst, size: encoded_size);

        let len: u32 = self.0.len().try_into().expect("BUG: validated in constructor");

        VarU32::new(len)?.encode(dst)?;
        dst.write_slice(self.0.as_bytes());
        dst.write_u8(b'\0');

        Ok(())
    }

    fn name(&self) -> &'static str {
        Self::NAME
    }

    // LINTS: Use of VarU32 ensures that the overall size value is within the bounds of usize.
    #[allow(clippy::arithmetic_side_effects)]
    fn size(&self) -> usize {
        VarU32::new(self.0.len().try_into().expect("buffer size always fits into u32"))
            .expect("buffer size is validated in constructor and should not overflow")
            .size() /* variable-length size */
            + self.0.len() /* utf-8 bytes */
            + 1 /* null terminator */
    }
}

impl Decode<'_> for NowVarStr {
    fn decode(src: &mut ReadCursor<'_>) -> DecodeResult<Self> {
        let len_u32 = VarU32::decode(src)?.value();
        let len: usize = cast_length!("len", len_u32)?;

        ensure_size!(in: src, size: len);
        let bytes = src.read_slice(len);
        ensure_size!(in: src, size: 1);
        let _null = src.read_u8();

        let string =
            String::from_utf8(bytes.to_vec()).map_err(|_| invalid_field_err!("string value", "invalid utf-8"))?;

        Ok(NowVarStr(string))
    }
}

impl From<NowVarStr> for String {
    fn from(value: NowVarStr) -> Self {
        value.0
    }
}
