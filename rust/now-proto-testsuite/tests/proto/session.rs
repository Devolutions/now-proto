use expect_test::expect;
use now_proto_pdu::*;
use now_proto_testsuite::proto::now_msg_roundtrip;

#[test]
fn roundtrip_session_lock() {
    now_msg_roundtrip(
        NowSessionLockMsg::default(),
        expect!["[00, 00, 00, 00, 12, 01, 00, 00]"],
    );
}

#[test]
fn roundtrip_session_logoff() {
    now_msg_roundtrip(
        NowSessionLogoffMsg::default(),
        expect!["[00, 00, 00, 00, 12, 02, 00, 00]"],
    );
}

#[test]
fn roundtrip_session_msgbox_req() {
    let msg = NowSessionMsgBoxReqMsg::new(0x76543210, "hello")
        .unwrap()
        .with_response()
        .with_style(NowMessageBoxStyle::ABORT_RETRY_IGNORE)
        .with_title("world")
        .unwrap()
        .with_timeout(core::time::Duration::from_secs(3))
        .unwrap();

    let decoded = now_msg_roundtrip(
        msg,
        expect!["[1A, 00, 00, 00, 12, 03, 0F, 00, 10, 32, 54, 76, 02, 00, 00, 00, 03, 00, 00, 00, 05, 77, 6F, 72, 6C, 64, 00, 05, 68, 65, 6C, 6C, 6F, 00]"]
    );

    let actual = match decoded {
        NowMessage::Session(NowSessionMessage::MsgBoxReq(msg)) => msg,
        _ => panic!("Expected NowSessionMsgBoxReqMsg"),
    };

    assert_eq!(actual.request_id(), 0x76543210);
    assert_eq!(actual.message(), "hello");
    assert!(actual.is_response_expected());
    assert_eq!(actual.style(), NowMessageBoxStyle::ABORT_RETRY_IGNORE);
    assert_eq!(actual.title().unwrap(), "world");
    assert_eq!(actual.timeout().unwrap(), core::time::Duration::from_secs(3));
}

#[test]
fn roundtrip_session_msgbox_req_simple() {
    let msg = NowSessionMsgBoxReqMsg::new(0x76543210, "hello").unwrap();

    let decoded = now_msg_roundtrip(
        msg,
        expect!["[15, 00, 00, 00, 12, 03, 00, 00, 10, 32, 54, 76, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 05, 68, 65, 6C, 6C, 6F, 00]"]
    );

    let actual = match decoded {
        NowMessage::Session(NowSessionMessage::MsgBoxReq(msg)) => msg,
        _ => panic!("Expected NowSessionMsgBoxReqMsg"),
    };

    assert_eq!(actual.request_id(), 0x76543210);
    assert_eq!(actual.message(), "hello");
    assert!(!actual.is_response_expected());
    assert_eq!(actual.style(), NowMessageBoxStyle::OK);
    assert!(actual.title().is_none());
    assert!(actual.timeout().is_none());
}

#[test]
fn roundtrip_session_msgbox_rsp() {
    let msg = NowSessionMsgBoxRspMsg::new_success(0x01234567, NowMsgBoxResponse::RETRY);

    let decoded = now_msg_roundtrip(
        msg,
        expect![
            "[12, 00, 00, 00, 12, 04, 00, 00, 67, 45, 23, 01, 04, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00]"
        ],
    );

    let actual = match decoded {
        NowMessage::Session(NowSessionMessage::MsgBoxRsp(msg)) => msg,
        _ => panic!("Expected NowSessionMsgBoxRspMsg"),
    };

    assert_eq!(actual.request_id(), 0x01234567);
    assert_eq!(actual.to_result().unwrap(), NowMsgBoxResponse::RETRY);
}

#[test]
fn roundtrip_session_msgbox_rsp_error() {
    let msg = NowSessionMsgBoxRspMsg::new_error(
        0x01234567,
        NowStatusError::from(NowStatusErrorKind::Now(NowProtoError::NotImplemented)),
    )
    .unwrap();

    let decoded = now_msg_roundtrip(
        msg,
        expect![
            "[12, 00, 00, 00, 12, 04, 00, 00, 67, 45, 23, 01, 00, 00, 00, 00, 01, 00, 01, 00, 07, 00, 00, 00, 00, 00]"
        ],
    );

    let actual = match decoded {
        NowMessage::Session(NowSessionMessage::MsgBoxRsp(msg)) => msg,
        _ => panic!("Expected NowSessionMsgBoxRspMsg"),
    };

    assert_eq!(actual.request_id(), 0x01234567);
    assert_eq!(
        actual.to_result().unwrap_err(),
        NowStatusError::from(NowStatusErrorKind::Now(NowProtoError::NotImplemented))
    );
}

#[test]
fn roundtrip_session_set_kbd_layout_specific() {
    let msg = NowSessionSetKbdLayoutMsg::new_specific("00000409").unwrap();

    let decoded = now_msg_roundtrip(
        msg,
        expect!["[0A, 00, 00, 00, 12, 05, 00, 00, 08, 30, 30, 30, 30, 30, 34, 30, 39, 00]"],
    );

    let actual = match decoded {
        NowMessage::Session(NowSessionMessage::SetKbdLayout(msg)) => msg,
        _ => panic!("Expected NowSessionSetKbdLayoutMsg"),
    };

    assert_eq!(actual.layout(), SetKbdLayoutOption::Specific("00000409"));
}

#[test]
fn roundtrip_session_set_kbd_layout_next() {
    let msg = NowSessionSetKbdLayoutMsg::new_next();

    let decoded = now_msg_roundtrip(msg, expect!["[02, 00, 00, 00, 12, 05, 01, 00, 00, 00]"]);

    let actual = match decoded {
        NowMessage::Session(NowSessionMessage::SetKbdLayout(msg)) => msg,
        _ => panic!("Expected NowSessionSetKbdLayoutMsg"),
    };

    assert_eq!(actual.layout(), SetKbdLayoutOption::Next);
}

#[test]
fn roundtrip_session_set_kbd_layout_prev() {
    let msg = NowSessionSetKbdLayoutMsg::new_prev();

    let decoded = now_msg_roundtrip(msg, expect!["[02, 00, 00, 00, 12, 05, 02, 00, 00, 00]"]);

    let actual = match decoded {
        NowMessage::Session(NowSessionMessage::SetKbdLayout(msg)) => msg,
        _ => panic!("Expected NowSessionSetKbdLayoutMsg"),
    };

    assert_eq!(actual.layout(), SetKbdLayoutOption::Prev);
}
