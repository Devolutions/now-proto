use now_proto_pdu::*;
use rstest::rstest;

#[rstest]
#[case("hello", &[0x05, b'h', b'e', b'l', b'l', b'o', 0x00])]
#[case("", &[0x00, 0x00])]
fn now_varstr_roundtrip(#[case] value: &str, #[case] expected_encoded: &'static [u8]) {
    let mut encoded_value = [0u8; 32];
    let encoded_size = ironrdp_core::encode(&NowVarStr::new(value).unwrap(), &mut encoded_value).unwrap();

    assert_eq!(encoded_size, expected_encoded.len());
    assert_eq!(&encoded_value[..encoded_size], expected_encoded);

    let decoded_value = ironrdp_core::decode::<NowVarStr>(&encoded_value).unwrap();
    assert_eq!(decoded_value.value(), value);
}

#[test]
fn decoded_now_varstr_invalid_utf8() {
    let encoded = [0x01, 0xFF, 0x00];
    ironrdp_core::decode::<NowVarStr>(&encoded).unwrap_err();
}