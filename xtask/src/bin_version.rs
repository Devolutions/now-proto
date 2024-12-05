// We pin the binaries to specific versions so we use the same artifact everywhere.
// Hash of this file is used in CI for caching.

use crate::bin_install::CargoPackage;

pub const TYPOS_CLI: CargoPackage = CargoPackage::new("typos-cli", "1.28.2").with_binary_name("typos");
