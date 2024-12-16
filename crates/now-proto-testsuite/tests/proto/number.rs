use now_proto_pdu::*;
use rstest::rstest;

#[rstest]
#[case(0x00, &[0x00])]
#[case(0x3F, &[0x3F])]
#[case(0x40, &[0x40, 0x40])]
#[case(0x14000, &[0x81, 0x40, 0x00])]
#[case(0x3FFFFFFF, &[0xFF, 0xFF, 0xFF, 0xFF])]
fn var_u32_roundtrip(#[case] value: u32, #[case] expected_encoded: &'static [u8]) {
    let mut encoded_value = [0u8; 4];
    let encoded_size = ironrdp_core::encode(&VarU32::new(value).unwrap(), &mut encoded_value).unwrap();

    assert_eq!(encoded_size, expected_encoded.len());
    assert_eq!(&encoded_value[..encoded_size], expected_encoded);

    let decoded_value = ironrdp_core::decode::<VarU32>(&encoded_value).unwrap();
    assert_eq!(decoded_value.value(), value);
}

#[test]
fn constructed_var_int_too_large() {
    VarU32::new(0x40000000).unwrap_err();
}
