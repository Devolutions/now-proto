use crate::prelude::*;

pub fn typos(sh: &Shell) -> anyhow::Result<()> {
    let _s = Section::new("TYPOS-CLI");

    if !is_installed(sh, "typos") {
        anyhow::bail!("`typos-cli` binary is missing. Please run `cargo xtask check install`.");
    }

    cmd!(sh, "typos").run()?;

    println!("All good!");
    Ok(())
}

pub fn install(sh: &Shell) -> anyhow::Result<()> {
    let _s = Section::new("TYPOS-CLI-INSTALL");

    cargo_install(sh, &TYPOS_CLI)?;

    Ok(())
}

pub fn lock_files(sh: &Shell) -> anyhow::Result<()> {
    let _s = Section::new("CHECK-LOCKS");

    // Note that we can’t really use the --locked option of cargo, because to
    // run xtask, we need to compile it using cargo first, and thus the lock
    // files are already "refreshed" as far as cargo is concerned. Instead,
    // this task will check for modifications to the lock files using git-status
    // porcelain. The side benefit is that we can check for npm lock files too.

    const LOCK_FILES: &[&str] = &["Cargo.lock"];

    let output = cmd!(sh, "git status --porcelain --untracked-files=no")
        .args(LOCK_FILES)
        .read()?;

    if !output.is_empty() {
        cmd!(sh, "git status").run()?;
        anyhow::bail!("one or more lock files are changed, you should commit those");
    }

    println!("All good!");

    Ok(())
}
