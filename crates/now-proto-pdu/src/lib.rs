#![doc = include_str!("../../../README.md")]
#![doc(
    html_logo_url = "https://webdevolutions.blob.core.windows.net/images/projects/devolutions/logos/devolutions-icon-shadow.svg"
)]
#![no_std]

extern crate alloc;
extern crate ironrdp_core;

/// Asserts that constant expressions evaluate to `true`.
///
/// From <https://docs.rs/static_assertions/1.1.0/src/static_assertions/const_assert.rs.html#51-57>
#[macro_export]
macro_rules! const_assert {
    ($x:expr $(,)?) => {
        #[allow(unknown_lints, clippy::eq_op)]
        const _: [(); 0 - !{
            const ASSERT: bool = $x;
            ASSERT
        } as usize] = [];
    };
}

// Ensure that we do not compile on platforms with less than 4 bytes per u32. It is pretty safe
// to assume that NOW-PROTO will not ever be used on 8/16-bit MCUs or CPUs.
//
// This is required to safely cast u32 to usize without additional checks.
const_assert!(size_of::<usize>() >= 4);

#[macro_use]
mod macros;

mod core;
mod exec;
mod message;
mod session;
mod system;

pub use core::*;

pub use exec::*;
pub use message::*;
pub use session::*;
pub use system::*;
