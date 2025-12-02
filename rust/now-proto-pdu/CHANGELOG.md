# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).


## [[0.4.2](https://github.com/Devolutions/now-proto/compare/now-proto-pdu-v0.4.1...now-proto-pdu-v0.4.2)] - 2025-12-02

### <!-- 1 -->Features

- Add window recording support to protocol and libraries ([#52](https://github.com/Devolutions/now-proto/issues/52)) ([e455c4c6e3](https://github.com/Devolutions/now-proto/commit/e455c4c6e3c06e54fc585c8d6f14c315177dd7cf)) 

  This PR adds new messages for the current active windows tracking.

## [[0.4.1](https://github.com/Devolutions/now-proto/compare/now-proto-pdu-v0.4.0...now-proto-pdu-v0.4.1)] - 2025-11-18

### <!-- 1 -->Features

- Add detached exec mode (ARC-411) ([#48](https://github.com/Devolutions/now-proto/issues/48)) ([a4ce1b2d16](https://github.com/Devolutions/now-proto/commit/a4ce1b2d163b023e4268b6bb6a0afeaf851e23f5)) 

## [[0.4.0](https://github.com/Devolutions/now-proto/compare/now-proto-pdu-v0.3.2...now-proto-pdu-v0.4.0)] - 2025-09-24

### <!-- 1 -->Features

- Implemented NOW-Proto 1.3 features in rust crate ([b99bbeae0c](https://github.com/Devolutions/now-proto/commit/b99bbeae0cda6f6ee20e0f29b6b36ee9abdd34e9)) 

### <!-- 4 -->Bug Fixes

- Update version numbers in libraries ([7296b6d325](https://github.com/Devolutions/now-proto/commit/7296b6d325df4fc08ca18faa1a4e24a322ba2bb7)) 

## [[0.3.2](https://github.com/Devolutions/now-proto/compare/now-proto-pdu-v0.3.1...now-proto-pdu-v0.3.2)] - 2025-09-11

### <!-- 4 -->Bug Fixes

- Add missing NowExecPwshMsg::is_server_mode method ([27fe1341f8](https://github.com/Devolutions/now-proto/commit/27fe1341f8145316f911cd89f83c223a539bc048)) 



## [[0.3.1](https://github.com/Devolutions/now-proto/compare/now-proto-pdu-v0.3.0...now-proto-pdu-v0.3.1)] - 2025-09-11

### <!-- 1 -->Features

- Add PowerShell server mode flag support ([2177c8ece1](https://github.com/Devolutions/now-proto/commit/2177c8ece131a9e82c545caa9a38769cb6b9267b)) 



## [[0.3.0](https://github.com/Devolutions/now-proto/compare/now-proto-pdu-v0.2.0...now-proto-pdu-v0.3.0)] - 2025-08-20

### <!-- 1 -->Features

- Add IO redirection flags to all exec sessions; add missing working directory option to ShellExecute (#29) ([ce0afe06c4](https://github.com/Devolutions/now-proto/commit/ce0afe06c4d1a9f1750eb0055034fd0b896db407)) 

### <!-- 4 -->Bug Fixes

- Add missing forward-compatibility logic to message decoding (#32) ([0adfc78cfa](https://github.com/Devolutions/now-proto/commit/0adfc78cfa350b3086f6444758d7a5da220c23e8)) 

- [**breaking**] Change incorrect `NowExecRunMsg::directory` method (#33) ([edba71a91e](https://github.com/Devolutions/now-proto/commit/edba71a91ec63735c0aeb3ae839fda3b570d0bc6)) 

## [[0.2.0](https://github.com/Devolutions/now-proto/compare/now-proto-pdu-v0.1.0...now-proto-pdu-v0.2.0)] - 2025-03-14

### <!-- 1 -->Features

- Set keyboard layout functionality (#22) ([31e0c79318](https://github.com/Devolutions/now-proto/commit/31e0c793186d558c0369fe188a2525b99911af30)) 

### <!-- 6 -->Documentation

- Update README.md and uniformize wording (#19) ([17719140e7](https://github.com/Devolutions/now-proto/commit/17719140e7b52b209cda9c17d0ef892cf006f723)) 

