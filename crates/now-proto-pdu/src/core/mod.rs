//! This module contains `NOW-PROTO` core types definitions.

mod buffer;
mod header;
mod number;
mod status;
mod string;

pub use buffer::NowVarBuf;
pub use header::{NowHeader, NowMessageClass};
pub use number::VarU32;
pub use status::{NowSeverity, NowStatus, NowStatusCode};
pub use string::NowVarStr;
