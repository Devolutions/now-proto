NOW-proto
=========
Devolutions::Now::Agent RDP virtual channel protocol libraries and clients.

### Specification
Current protocol specification: [read](./doc/NOW-spec.md)

### now-proto-pdu (rust)
This repository contains the [Rust implementation](./crates/now-proto-pdu/README.md) of the
NOW-proto protocol encoding/decoding library.

### Updating protocol
In order to update the protocol, the following steps should be followed:
1. Update the protocol specification in `doc/NOW-spec.md`.
    1. Bump the protocol version number.
1. Update the Rust implementation of the protocol in `crates/now-proto-pdu`.
    1. Bump current protocol version in `NowProtoVersion::CURRENT`
1. Update the C# protocol implementation (WIP)
1. Update C# clients (WIP)