NOW-proto
=========

Devolutions::Now::Agent RDP virtual channel protocol libraries and clients.

### Specification

Current protocol specification: [read](./docs/NOW-spec.md)

### now-proto-pdu (rust)

This repository contains the [Rust implementation](./crates/now-proto-pdu/README.md) of the
NOW-proto protocol encoding/decoding library.

### Updating protocol

In order to update the protocol, the following steps should be followed:

1. Update the protocol specification in `./docs/NOW-spec.md`.
  - Bump the protocol version number.
1. Update the Rust implementation of the protocol in `./rust/now-proto-pdu`.
  - Bump current protocol version defined in `now_proto_pdu::capset::NowProtoVersion::CURRENT`
1. Update the C# implementation of the protocol in `./dotnet/Devolutions.NowProto`
  - Bump current protocol version in `Devolutions.NowProto.NowProtoVersion.Current`
