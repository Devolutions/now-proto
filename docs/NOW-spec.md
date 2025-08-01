<!--
TOC is generated in [Obsidian](obsidian.md) via
[TOC plugin](https://github.com/hipstersmoothie/obsidian-plugin-toc)
-->

# NOW-PROTO 1.0

- [Transport](#transport)
- [Message Syntax](#message-syntax)
	- [Common Structures](#common-structures)
		- [NOW_INTEGER](#now_integer)
			- [NOW_VARU32](#now_varu32)
		- [NOW_STRING](#now_string)
			- [NOW_VARSTR](#now_varstr)
		- [NOW_HEADER](#now_header)
		- [NOW_STATUS](#now_status)
	- [Channel Messages](#channel-messages)
		- [NOW_CHANNEL_MSG](#now_channel_msg)
		- [NOW_CHANNEL_CAPSET_MSG](#now_channel_capset_msg)
		- [NOW_CHANNEL_HEARTBEAT_MSG](#now_channel_heartbeat_msg)
		- [NOW_CHANNEL_CLOSE_MSG](#now_channel_close_msg)
	- [System Messages](#system-messages)
		- [NOW_SYSTEM_MSG](#now_system_msg)
		- [NOW_SYSTEM_SHUTDOWN_MSG](#now_system_shutdown_msg)
	- [Session Messages](#session-messages)
		- [NOW_SESSION_MSG](#now_session_msg)
		- [NOW_SESSION_LOCK_MSG](#now_session_lock_msg)
		- [NOW_SESSION_LOGOFF_MSG](#now_session_logoff_msg)
		- [NOW_SESSION_MSGBOX_REQ_MSG](#now_session_msgbox_req_msg)
		- [NOW_SESSION_MSGBOX_RSP_MSG](#now_session_msgbox_rsp_msg)
		- [NOW_SESSION_SET_KBD_LAYOUT_MSG](#now_session_set_kbd_layout_msg)
	- [Execution Messages](#execution-messages)
		- [NOW_EXEC_MSG](#now_exec_msg)
		- [NOW_EXEC_ABORT_MSG](#now_exec_abort_msg)
		- [NOW_EXEC_CANCEL_REQ_MSG](#now_exec_cancel_req_msg)
		- [NOW_EXEC_CANCEL_RSP_MSG](#now_exec_cancel_rsp_msg)
		- [NOW_EXEC_RESULT_MSG](#now_exec_result_msg)
		- [NOW_EXEC_DATA_MSG](#now_exec_data_msg)
		- [NOW_EXEC_STARTED_MSG](#now_exec_started_msg)
		- [NOW_EXEC_RUN_MSG](#now_exec_run_msg)
		- [NOW_EXEC_PROCESS_MSG](#now_exec_process_msg)
		- [NOW_EXEC_SHELL_MSG](#now_exec_shell_msg)
		- [NOW_EXEC_BATCH_MSG](#now_exec_batch_msg)
		- [NOW_EXEC_WINPS_MSG](#now_exec_winps_msg)
		- [NOW_EXEC_PWSH_MSG](#now_exec_pwsh_msg)
	- [Version History](#version-history)

# Messages

## Transport

The NOW virtual channel protocol use an RDP dynamic virtual channel ("Devolutions::Now::Agent") as a transport type.

## Message Syntax

The following sections specify the NOW protocol message syntax.
Unless otherwise specified, all fields defined in this document use the little-endian format.

### Common Structures

#### NOW_INTEGER

Signed and unsigned integer encoding structures of various sizes.

##### NOW_VARU32

The NOW_VARU32 structure is used to encode signed integer values in the range [0, 0x3FFFFFFF].

```mermaid
packet-beta
  0-1: "c"
  2-7: "val1"
  8-15: "val2 (optional)"
  16-23: "val3 (optional)"
  24-31: "val4 (optional)"
```

**c (2 bits)**: A 2-bit integer containing an encoded representation of the number of bytes in this structure.

| Value | Meaning |
|-------|---------|
| 0 | The val1 field is present (1 byte). |
| 1 | The val1, val2 fields are present (2 bytes). |
| 2 | The val1, val2, val3 fields are present (3 bytes). |
| 3 | The val1, val2, val3, val4 fields are present (4 bytes). |

**val1 (6 bits)**: A 6-bit integer containing the 6 most significant bits of the integer value represented by this structure.

**val2 (1 byte)**: An 8-bit integer containing the second most significant bits of the integer value represented by this structure.

**val3 (1 byte)**: An 8-bit integer containing the third most significant bits of the integer value represented by this structure.

**val4 (1 byte)**: An 8-bit integer containing the least significant bits of the integer value represented by this structure.

#### NOW_STRING

##### NOW_VARSTR

The NOW_VARSTR structure is used to represent variable-length strings that could be large, while remaining compact in size for small strings.

```mermaid
packet-beta
  0-31: "len (variable)"
  32-63: "str (variable)"
```

**len (variable)**: A NOW_VARU32 structure containing the string length, excluding the null terminator.

**str (variable)**: The UTF-8 encoded string excluding the null terminator.

#### NOW_HEADER

The NOW_HEADER structure is the header common to all NOW protocol messages.

```mermaid
packet-beta
  0-31: "msgSize"
  32-39: "msgClass"
  40-47: "msgType"
  48-63: "msgFlags"
```

**msgSize (4 bytes)**: The message size, excluding the header size (8 bytes).

**msgClass (1 byte)**: The message class.

| Flag                            | Meaning              |
|---------------------------------|----------------------|
| NOW_CHANNEL_MSG_CLASS_ID<br>0x10 | Channel message class. |
| NOW_SYSTEM_MSG_CLASS_ID<br>0x11 | System message class. |
| NOW_SESSION_MSG_CLASS_ID<br>0x12 | Session message class. |
| NOW_EXEC_MSG_CLASS_ID<br>0x13 | Exec message class. |

**msgType (1 byte)**: The message type, specific to the message class.

**msgFlags (2 bytes)**: The message flags, specific to the message type and class.

#### NOW_STATUS
Operation status code.

```mermaid
packet-beta
  0-15: "flags"
  16-23: "kind"
  24-31: "reserved"
  32-63: "code"
  64-95: "errorMessage(variable)"
```

**flags (2 bytes)**: Status flags.

| Value | Meaning |
|-------|---------|
| NOW_STATUS_ERROR<br>0x0001 | This flag set for all error statuses. If flag is not set, operation was successful. |
| NOW_STATUS_ERROR_MESSAGE<br>0x0002 | `errorMessage` contains optional error message. |

**kind (1 byte)**: The status kind.
When `NOW_STATUS_ERROR` is set, this field represents error kind.

| Value | Meaning |
|-------|---------|
| NOW_STATUS_ERROR_KIND_GENERIC<br>0x0000 | `code` value is undefined and could be ignored. |
| NOW_STATUS_ERROR_KIND_NOW<br>0x0001 | `code` contains NowProto-defined error code. |
| NOW_STATUS_ERROR_KIND_WINAPI<br>0x0002 | `code` field contains Windows error code. |
| NOW_STATUS_ERROR_KIND_UNIX<br>0x0003 | `code` field contains Unix error code. |

For successful operation this field value is operation specific.

**reserved (1 byte)**: Reserved value. Should be set to 0 and ignored during parsing.

**code (4 bytes)**: The status code.

- If `NOW_STATUS_ERROR` flag is NOT set, this value should contain `0` value
- If `NOW_STATUS_ERROR` is set, this value represents error code according to
  `NOW_STATUS_ERROR_KIND_*` value. If no error kind flags set, value of this
  field is undefined and should be ignored.

    - `NOW_STATUS_ERROR_KIND_NOW` codes:

        | Value | Meaning |
        |-------|---------|
        | NOW_CODE_IN_USE<br>0x0001 | Resource (e.g. exec session id is already in use). |
        | NOW_CODE_INVALID_REQUEST<br>0x0002 | Sent request is invalid (e.g. invalid exec request params). |
        | NOW_CODE_ABORTED<br>0x0003 | Operation has been aborted on the server side. |
        | NOW_CODE_NOT_FOUND<br>0x0004 | Resource not found. |
        | NOW_CODE_ACCESS_DENIED<br>0x0005 | Resource can't be accessed. |
        | NOW_CODE_INTERNAL<br>0x0006 | Internal error. |
        | NOW_CODE_NOT_IMPLEMENTED<br>0x0007 | Operation is not implemented on current platform. |
        | NOW_CODE_PROTOCOL_VERSION<br>0x0008 | Incompatible protocol versions. |

    - `NOW_STATUS_ERROR_KIND_WINAPI`: code contains standard WinAPI error.
    - `NOW_STATUS_ERROR_KIND_UNIX`: code contains standard UNIX error code.

**errorMessage(variable)**: this value contains either an error message if
`NOW_STATUS_ERROR_MESSAGE` flag is set, or empty string if the flag is not set.

### Channel Messages
Channel negotiation and life cycle messages.

#### NOW_CHANNEL_MSG

```mermaid
packet-beta
  0-31: "msgSize"
  32-39: "msgClass"
  40-47: "msgType"
  48-63: "msgFlags"
```

**msgSize (4 bytes)**: The message size, excluding the header size (8 bytes).

**msgClass (1 byte)**: The message class (NOW_NEGOTIATION_MSG_CLASS_ID).

**msgType (1 byte)**: The message type.

| Value                           | Meaning              |
|---------------------------------|----------------------|
| NOW_CHANNEL_CAPSET_MSG_ID<br>0x01 | NOW_CHANNEL_CAPSET_MSG |
| NOW_CHANNEL_HEARTBEAT_MSG_ID<br>0x02 | NOW_CHANNEL_HEARTBEAT_MSG |
| NOW_CHANNEL_CLOSE_MSG_ID<br>0x03 | NOW_CHANNEL_CLOSE_MSG |

#### NOW_CHANNEL_CAPSET_MSG

This message is first set by the client side, to advertise capabilities.

Received client message should be downgraded by the server (remove non-intersecting capabilities)
and sent back to the client at the start of DVC channel communications. DVC channel should be
closed if protocol versions are not compatible.

```mermaid
packet-beta
  0-31: "msgSize"
  32-39: "msgClass"
  40-47: "msgType"
  48-63: "msgFlags"
  64-79: "versionMajor"
  80-95: "versionMinor"
  96-111: "systemCapset"
  112-127: "sessionCapset"
  128-143: "execCapset"
  144-175: "heartbeatInterval"
```

**msgSize (4 bytes)**: The message size, excluding the header size (8 bytes).

**msgClass (1 byte)**: The message class (NOW_CHANNEL_MSG_CLASS_ID).

**msgType (1 byte)**: The message type (NOW_CHANNEL_CAPSET_MSG_ID).

**msgFlags (2 bytes)**: Message flags.

| Flag | Meaning |
|-------|---------|
| NOW_CHANNEL_SET_HEARTBEAT<br>0x0001 | Set if `heartbeat` specify channel heartbeat interval. |

**versionMajor (1 byte)**: Major protocol version. Breaking changes in protocol should
increment major version; Protocol implementations with different major version are not compatible.

**versionMinor (1 byte)**: Minor protocol version. Incremented when new non-breaking feature is added.

**systemCapset (2 bytes)**: System commands capabilities set.

| Flag | Meaning |
|-------|---------|
| NOW_CAP_SYSTEM_SHUTDOWN<br>0x0001 | System shutdown command support. |

**sessionCapset (2 bytes)**: Session commands capabilities set.

| Flag | Meaning |
|-------|---------|
| NOW_CAP_SESSION_LOCK<br>0x0001 | Session lock command support. |
| NOW_CAP_SESSION_LOGOFF<br>0x0002 | Session logoff command support. |
| NOW_CAP_SESSION_MSGBOX<br>0x0004 | Message box command support. |
| NOW_CAP_SESSION_SET_KBD_LAYOUT<br>0x0008 | Set keyboard layout command support. |

**execCapset (2 bytes)**: Remote execution capabilities set.

| Flag | Meaning |
|-------|---------|
| NOW_CAP_EXEC_STYLE_RUN<br>0x0001 | Generic "Run" execution style. |
| NOW_CAP_EXEC_STYLE_PROCESS<br>000002 | CreateProcess() execution style. |
| NOW_CAP_EXEC_STYLE_SHELL<br>0x0004 | System shell (.sh) execution style. |
| NOW_CAP_EXEC_STYLE_BATCH<br>0x0008 | Windows batch file (.bat) execution style. |
| NOW_CAP_EXEC_STYLE_WINPS<br>0x0010 | Windows PowerShell (.ps1) execution style. |
| NOW_CAP_EXEC_STYLE_PWSH<br>0x0020 | PowerShell 7 (.ps1) execution style. |
| NOW_CAP_EXEC_IO_REDIRECTION<br>0x1000 | Set if host implements exec session IO redirection. |

<!-- TODO: add AppleScript command -->

**heartbeatInterval (4 bytes, optional)**: A 32-bit unsigned integer, which represents
periodic heartbeat interval *hint* for a server (60 seconds by default).
Disables periodic heartbeat if set to `0`. Ignored if `NOW_CHANNEL_SET_HEARTBEAT` is not set.


#### NOW_CHANNEL_HEARTBEAT_MSG

Periodic heartbeat message sent by the server. If the client does not receive this message within
the specified interval, it should consider the connection as lost.

```mermaid
packet-beta
  0-31: "msgSize"
  32-39: "msgClass"
  40-47: "msgType"
  48-63: "msgFlags"
```

**msgSize (4 bytes)**: The message size, excluding the header size (8 bytes).

**msgClass (1 byte)**: The message class (NOW_CHANNEL_MSG_CLASS_ID).

**msgType (1 byte)**: The message type (NOW_CHANNEL_HEARTBEAT_MSG_ID).

**msgFlags (2 bytes)**: The message flags.

#### NOW_CHANNEL_CLOSE_MSG

Channel close notice, could be sent by either parties at any moment of communication to gracefully
close DVC channel.

```mermaid
packet-beta
  0-31: "msgSize"
  32-39: "msgClass"
  40-47: "msgType"
  48-63: "msgFlags"
  64-95: "status (variable)"
```

**msgSize (4 bytes)**: The message size, excluding the header size (8 bytes).

**msgClass (1 byte)**: The message class (NOW_CHANNEL_MSG_CLASS_ID).

**msgType (1 byte)**: The message type (NOW_CHANNEL_CLOSE_MSG_ID).

**msgFlags (2 bytes)**: The message flags.

**status (variable)**: Channel close status represented as NOW_STATUS structure.

### System Messages

#### NOW_SYSTEM_MSG

```mermaid
packet-beta
  0-31: "msgSize"
  32-39: "msgClass"
  40-47: "msgType"
  48-63: "msgFlags"
```

**msgSize (4 bytes)**: The message size, excluding the header size (8 bytes).

**msgClass (1 byte)**: The message class (NOW_SYSTEM_MSG_CLASS_ID).

**msgType (1 byte)**: The message type.

| Value                           | Meaning              |
|---------------------------------|----------------------|
| NOW_SYSTEM_INFO_REQ_ID<br>0x01 | NOW_SYSTEM_INFO_REQ_MSG |
| NOW_SYSTEM_INFO_RSP_ID<br>0x02 | NOW_SYSTEM_INFO_RSP_MSG |
| NOW_SYSTEM_SHUTDOWN_ID<br>0x03 | NOW_SYSTEM_SHUTDOWN_MSG |

<!-- TODO: Define NOW_SYSTEM_INFO_REQ_MSG, NOW_SYSTEM_INFO_RSP_MSG   -->

#### NOW_SYSTEM_SHUTDOWN_MSG

The NOW_SYSTEM_SHUTDOWN_MSG structure is used to request a system shutdown.

```mermaid
packet-beta
  0-31: "msgSize"
  32-39: "msgClass"
  40-47: "msgType"
  48-63: "msgFlags"
  64-95: "timeout"
  96-127: "message (variable)"
```

**msgSize (4 bytes)**: The message size, excluding the header size (8 bytes).

**msgClass (1 byte)**: The message class (NOW_SYSTEM_MSG_CLASS_ID).

**msgType (1 byte)**: The message type (NOW_SYSTEM_SHUTDOWN_MSG_ID)

**msgFlags (2 bytes)**: The message flags.

| Flag | Meaning |
|------|---------|
| NOW_SHUTDOWN_FLAG_FORCE<br>0x0001 | Force shutdown |
| NOW_SHUTDOWN_FLAG_REBOOT<br>0x0002 | Reboot after shutdown |

**timeout (4 bytes)**: This system shutdown timeout, in seconds.

**message (variable)**: A NOW_STRING structure containing an optional shutdown message.

### Session Messages

#### NOW_SESSION_MSG

```mermaid
packet-beta
  0-31: "msgSize"
  32-39: "msgClass"
  40-47: "msgType"
  48-63: "msgFlags"
```

**msgSize (4 bytes)**: The message size, excluding the header size (8 bytes).

**msgClass (1 byte)**: The message class (NOW_SESSION_MSG_CLASS_ID).

**msgType (1 byte)**: The message type.

| Value                           | Meaning              |
|---------------------------------|----------------------|
| NOW_SESSION_LOCK_MSG_ID<br>0x01 | NOW_SESSION_LOCK_MSG |
| NOW_SESSION_LOGOFF_MSG_ID<br>0x02 | NOW_SESSION_LOGOFF_MSG |
| NOW_SESSION_MESSAGE_BOX_MSG_REQ_ID<br>0x03 | NOW_SESSION_MESSAGE_BOX_MSG |
| NOW_SESSION_MESSAGE_BOX_RSP_MSG_ID<br>0x04 | NOW_SESSION_MESSAGE_RSP_MSG |
| NOW_SESSION_SWITCH_KBD_LAYOUT_MSG_ID<br>0x05 | NOW_SESSION_SWITCH_KBD_LAYOUT_MSG |

**msgFlags (2 bytes)**: The message flags.

#### NOW_SESSION_LOCK_MSG

The NOW_SESSION_LOCK_MSG is used to request locking the user session.

```mermaid
packet-beta
  0-31: "msgSize"
  32-39: "msgClass"
  40-47: "msgType"
  48-63: "msgFlags"
```

**msgSize (4 bytes)**: The message size, excluding the header size (8 bytes).

**msgClass (1 byte)**: The message class (NOW_SESSION_MSG_CLASS_ID).

**msgType (1 byte)**: The message type (NOW_SESSION_LOCK_MSG_ID).

**msgFlags (2 bytes)**: The message flags.

#### NOW_SESSION_LOGOFF_MSG

The NOW_SESSION_LOGOFF_MSG is used to request a user session logoff.

```mermaid
packet-beta
  0-31: "msgSize"
  32-39: "msgClass"
  40-47: "msgType"
  48-63: "msgFlags"
```

**msgSize (4 bytes)**: The message size, excluding the header size (8 bytes).

**msgClass (1 byte)**: The message class (NOW_SESSION_MSG_CLASS_ID).

**msgType (1 byte)**: The message type (NOW_SESSION_LOGOFF_MSG_ID).

**msgFlags (2 bytes)**: The message flags.

#### NOW_SESSION_MSGBOX_REQ_MSG

The NOW_SESSION_MSGBOX_REQ_MSG is used to show a message box in the user session, similar to what the [WTSSendMessage function](https://learn.microsoft.com/en-us/windows/win32/api/wtsapi32/nf-wtsapi32-wtssendmessagew) does.

```mermaid
packet-beta
  0-31: "msgSize"
  32-39: "msgClass"
  40-47: "msgType"
  48-63: "msgFlags"
  64-95: "requestId"
  96-127: "style"
  128-159: "timeout"
  160-191: "text (variable)"
  192-223: "title (variable)"
```

**msgSize (4 bytes)**: The message size, excluding the header size (8 bytes).

**msgClass (1 byte)**: The message class (NOW_SESSION_MSG_CLASS_ID).

**msgType (1 byte)**: The message type (NOW_SESSION_MESSAGE_BOX_MSG_ID).

**msgFlags (2 bytes)**: The message flags.

| Flag                                | Meaning                                 |
|-------------------------------------|-----------------------------------------|
| NOW_MSGBOX_FLAG_TITLE<br>0x0001 | The `title` field is contains a non-default value. |
| NOW_MSGBOX_FLAG_STYLE<br>0x0002 | The `style` field contains a non-default value. |
| NOW_MSGBOX_FLAG_TIMEOUT<br>0x0004 | The `timeout` field contains a non-default value. |
| NOW_MSGBOX_FLAG_RESPONSE<br>0x0008 | A response message is expected (don't fire and forget). |

**requestId (4 bytes)**: the message request id, sent back in the response.

**style (4 bytes)**: The message box style, ignored if NOW_MSGBOX_FLAG_STYLE is not set. MBOK is the default, refer to the
[MessageBox function](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-messagebox)
for all possible styles. This field may be ignored on platforms other than Windows.

**timeout (4 bytes)**: The timeout, in seconds, that the message box dialog should wait for the user response. This value is ignored if NOW_MSGBOX_FLAG_TIMEOUT is not set.

**text (variable)**: The message box text.

**title (variable)**: The message box title. Ignored if NOW_MSGBOX_FLAG_TITLE is not set.

#### NOW_SESSION_MSGBOX_RSP_MSG

The NOW_SESSION_MSGBOX_RSP_MSG is a message sent in response to NOW_SESSION_MSGBOX_REQ_MSG if the NOW_MSGBOX_FLAG_RESPONSE has been set, and contains the result from the message box dialog.

```mermaid
packet-beta
  0-31: "msgSize"
  32-39: "msgClass"
  40-47: "msgType"
  48-63: "msgFlags"
  64-95: "requestId"
  96-127: "response"
  128-159: "status (variable)"
```

**msgSize (4 bytes)**: The message size, excluding the header size (8 bytes).

**msgClass (1 byte)**: The message class (NOW_SESSION_MSG_CLASS_ID).

**msgType (1 byte)**: The message type (NOW_SESSION_MESSAGE_RSP_MSG_ID).

**msgFlags (2 bytes)**: The message flags.

**requestId (4 bytes)**: Message box request id.

**response (4 bytes)**: Message response code. If **status** is successful, response value is defined as following:

| Value        | Meaning |
|--------------|---------|
| NOW_MSGBOX_RSP_ABORT<br>3 | Abort   |
| NOW_MSGBOX_RSP_CANCEL<br>2 | Cancel   |
| NOW_MSGBOX_RSP_CONTINUE<br>11 | Continue   |
| NOW_MSGBOX_RSP_IGNORE<br>5 | Ignore   |
| NOW_MSGBOX_RSP_NO<br>7 | No   |
| NOW_MSGBOX_RSP_OK<br>1 | OK   |
| NOW_MSGBOX_RSP_RETRY<br>4 | Retry   |
| NOW_MSGBOX_RSP_TRYAGAIN<br>10 | Try Again   |
| NOW_MSGBOX_RSP_YES<br>6 | Yes   |
| NOW_MSGBOX_RSP_TIMEOUT<br>32000 | Timeout   |

If `status` specifies error, this field should be set to `0`.

**status (variable)**: `NOW_STATUS` structure containing message box response status.

#### NOW_SESSION_SET_KBD_LAYOUT_MSG

The NOW_SESSION_SET_KBD_LAYOUT_MSG message is used to set the keyboard layout for the active
foreground window. The request is fire-and-forget, invalid layout identifiers are ignored.

```mermaid
packet-beta
  0-31: "msgSize"
  32-39: "msgClass"
  40-47: "msgType"
  48-63: "msgFlags"
  64-95: "kbdLayoutId(variable)"
```

**msgSize (4 bytes)**: The message size, excluding the header size (8 bytes).

**msgClass (1 byte)**: The message class (NOW_SESSION_MSG_CLASS_ID).

**msgType (1 byte)**: The message type (NOW_SESSION_SWITCH_KBD_LAYOUT_MSG_ID).

**msgFlags (2 bytes)**: The message flags.

| Flag                                | Meaning                                 |
|-------------------------------------|-----------------------------------------|
| NOW_SET_KBD_LAYOUT_FLAG_NEXT<br>0x0001 | Switches to next keyboard layout. kbdLayoutId field should contain empty string. Conflicts with NOW_SET_KBD_LAYOUT_FLAG_PREV. |
| NOW_SET_KBD_LAYOUT_FLAG_PREV<br>0x0002 | Switches to previous keyboard layout. kbdLayoutId field should contain empty string. Conflicts with NOW_SET_KBD_LAYOUT_FLAG_NEXT. |

**kbdLayoutId (variable)**: NOW_STRING structure containing the keyboard layout identifier usually represented as [Windows Keyboard Layout Identifier](https://learn.microsoft.com/en-us/windows-hardware/manufacture/desktop/windows-language-pack-default-values) (HKL).

### Execution Messages

#### NOW_EXEC_MSG

The NOW_EXEC_MSG message is used to execute remote commands or scripts.

```mermaid
packet-beta
  0-31: "msgSize"
  32-39: "msgClass"
  40-47: "msgType"
  48-63: "msgFlags"
```

**msgSize (4 bytes)**: The message size, excluding the header size (8 bytes).

**msgClass (1 byte)**: The message class (NOW_EXEC_MSG_CLASS_ID).

**msgType (1 byte)**: The message type.

| Value | Meaning |
|-------|---------|
| NOW_EXEC_ABORT_MSG_ID<br>0x01 | NOW_EXEC_ABORT_MSG |
| NOW_EXEC_CANCEL_REQ_MSG_ID<br>0x02 | NOW_EXEC_CANCEL_REQ_MSG |
| NOW_EXEC_CANCEL_RSP_MSG_ID<br>0x03 | NOW_EXEC_CANCEL_RSP_MSG |
| NOW_EXEC_RESULT_MSG_ID<br>0x04 | NOW_EXEC_RESULT_MSG |
| NOW_EXEC_DATA_MSG_ID<br>0x05 | NOW_EXEC_DATA_MSG |
| NOW_EXEC_STARTED_MSG_ID<br>0x06 | NOW_EXEC_STARTED_MSG |
| NOW_EXEC_RUN_MSG_ID<br>0x10 | NOW_EXEC_RUN_MSG |
| NOW_EXEC_PROCESS_MSG_ID<br>0x11 | NOW_EXEC_PROCESS_MSG |
| NOW_EXEC_SHELL_MSG_ID<br>0x12 | NOW_EXEC_SHELL_MSG |
| NOW_EXEC_BATCH_MSG_ID<br>0x13 | NOW_EXEC_BATCH_MSG |
| NOW_EXEC_WINPS_MSG_ID<br>0x14 | NOW_EXEC_WINPS_MSG |
| NOW_EXEC_PWSH_MSG_ID<br>0x15 | NOW_EXEC_PWSH_MSG |

**msgFlags (2 bytes)**: The message flags.

#### NOW_EXEC_ABORT_MSG

The NOW_EXEC_ABORT_MSG message is used to abort a remote execution immediately.
See NOW_EXEC_CANCEL_REQ if the graceful session cancellation is needed instead.
This message can be sent by the client at any point of session lifetime.
The session is considered aborted as soon as this message is sent.

```mermaid
packet-beta
  0-31: "msgSize"
  32-39: "msgClass"
  40-47: "msgType"
  48-63: "msgFlags"
  64-95: "sessionId"
  96-127: "exitCode"
```

**msgSize (4 bytes)**: The message size, excluding the header size (8 bytes).

**msgClass (1 byte)**: The message class (NOW_EXEC_MSG_CLASS_ID).

**msgType (1 byte)**: The message type (NOW_EXEC_ABORT_MSG_ID).

**msgFlags (2 bytes)**: The message flags.

**sessionId (4 bytes)**: A 32-bit unsigned integer containing a unique remote execution session id.

**exitCode (4 bytes)**: Exit code for application abort (Ignored if not supported by OS).

#### NOW_EXEC_CANCEL_REQ_MSG

The NOW_EXEC_CANCEL_REQ_MSG message is used to cancel a remote execution session.

```mermaid
packet-beta
  0-31: "msgSize"
  32-39: "msgClass"
  40-47: "msgType"
  48-63: "msgFlags"
  64-95: "sessionId"
```

**msgSize (4 bytes)**: The message size, excluding the header size (8 bytes).

**msgClass (1 byte)**: The message class (NOW_EXEC_MSG_CLASS_ID).

**msgType (1 byte)**: The message type (NOW_EXEC_CANCEL_REQ_MSG_ID).

**msgFlags (2 bytes)**: The message flags.

**sessionId (4 bytes)**: A 32-bit unsigned integer containing a unique remote execution session id.

#### NOW_EXEC_CANCEL_RSP_MSG

The NOW_EXEC_CANCEL_RSP_MSG message is used to respond to a remote execution cancel request.

```mermaid
packet-beta
  0-31: "msgSize"
  32-39: "msgClass"
  40-47: "msgType"
  48-63: "msgFlags"
  64-95: "sessionId"
  96-127: "status (variable)"
```

**msgSize (4 bytes)**: The message size, excluding the header size (8 bytes).

**msgClass (1 byte)**: The message class (NOW_EXEC_MSG_CLASS_ID).

**msgType (1 byte)**: The message type (NOW_EXEC_CANCEL_RSP_MSG_ID).

**msgFlags (2 bytes)**: The message flags.

**sessionId (4 bytes)**: A 32-bit unsigned integer containing a unique remote execution session id.

**status (4 bytes)**: `NOW_STATUS` structure containing execution session cancellation request status.

#### NOW_EXEC_RESULT_MSG

The NOW_EXEC_RESULT_MSG message is used to return the result of an execution request.
The session is considered terminated as soon as this message is sent.

```mermaid
packet-beta
  0-31: "msgSize"
  32-39: "msgClass"
  40-47: "msgType"
  48-63: "msgFlags"
  64-95: "sessionId"
  96-127: "exitCode"
  128-159: "status (variable)"
```

**msgSize (4 bytes)**: The message size, excluding the header size (8 bytes).

**msgClass (1 byte)**: The message class (NOW_EXEC_MSG_CLASS_ID).

**msgType (1 byte)**: The message type (NOW_EXEC_RESULT_MSG_ID).

**msgFlags (2 bytes)**: The message flags.

**sessionId (4 bytes)**: A 32-bit unsigned integer containing a unique remote execution session id.

**exitCode (4 bytes)**: Value containing either process exit code or `0` value if
`status` field specifies error.

**status (variable)**: `NOW_STATUS` structure containing session execution result.

#### NOW_EXEC_DATA_MSG

The NOW_EXEC_DATA_MSG message is used to send input/output data as part of a remote execution.

```mermaid
packet-beta
  0-31: "msgSize"
  32-39: "msgClass"
  40-47: "msgType"
  48-63: "msgFlags"
  64-95: "sessionId"
  96-127: "data (variable)"
```

**msgSize (4 bytes)**: The message size, excluding the header size (8 bytes).

**msgClass (1 byte)**: The message class (NOW_EXEC_MSG_CLASS_ID).

**msgType (1 byte)**: The message type (NOW_EXEC_DATA_MSG_ID).

**msgFlags (2 bytes)**: The message flags.

| Flag                                   | Meaning                         |
|----------------------------------------|---------------------------------|
| NOW_EXEC_FLAG_DATA_LAST<br>0x0001 | This is the last data message, the command completed execution. |
| NOW_EXEC_FLAG_DATA_STDIN<br>0x0002 | The data is from the standard input. |
| NOW_EXEC_FLAG_DATA_STDOUT<br>0x0004 | The data is from the standard output. |
| NOW_EXEC_FLAG_DATA_STDERR<br>0x0008 | The data is from the standard error. |

Message should contain exactly one of `NOW_EXEC_FLAG_DATA_STDIN`, `NOW_EXEC_FLAG_DATA_STDOUT` or `NOW_EXEC_FLAG_DATA_STDERR` flags set.

`NOW_EXEC_FLAG_DATA_LAST` should indicate EOF for a stream, all consecutive messages for the given stream will be ignored by either party (client or sever).


**sessionId (4 bytes)**: A 32-bit unsigned integer containing a unique remote execution session id.

**data (variable)**: The input/output data represented as `NOW_VARBUF`

#### NOW_EXEC_STARTED_MSG

The NOW_EXEC_STARTED_MSG message is sent by the server after the execution session has been successfully
started.

```mermaid
packet-beta
  0-31: "msgSize"
  32-39: "msgClass"
  40-47: "msgType"
  48-63: "msgFlags"
  64-95: "sessionId"
```

**msgSize (4 bytes)**: The message size, excluding the header size (8 bytes).

**msgClass (1 byte)**: The message class (NOW_EXEC_MSG_CLASS_ID).

**msgType (1 byte)**: The message type (NOW_EXEC_RESULT_MSG_ID).

**msgFlags (2 bytes)**: The message flags.

**sessionId (4 bytes)**: A 32-bit unsigned integer containing a unique remote execution session id.

#### NOW_EXEC_RUN_MSG

The NOW_EXEC_RUN_MSG message is used to send a run request. This request type maps to starting a program by using the “Run” menu on operating systems (the Start Menu on Windows, the Dock on macOS etc.). The execution of programs started with NOW_EXEC_RUN_MSG is not followed and does not send back the output.

```mermaid
packet-beta
  0-31: "msgSize"
  32-39: "msgClass"
  40-47: "msgType"
  48-63: "msgFlags"
  64-95: "sessionId"
  96-127: "command (variable)"
  128-159: "directory (variable)"
```

**msgSize (4 bytes)**: The message size, excluding the header size (8 bytes).

**msgClass (1 byte)**: The message class (NOW_EXEC_MSG_CLASS_ID).

**msgType (1 byte)**: The message type (NOW_EXEC_RUN_MSG_ID).

**msgFlags (2 bytes)**: The message flags.

| Flag                                   | Meaning                   |
|----------------------------------------|---------------------------|
| NOW_EXEC_FLAG_RUN_DIRECTORY_SET<br>0x0001 | `directory` field contains non-default value. |

**sessionId (4 bytes)**: A 32-bit unsigned integer containing a unique remote execution session id.

**command (variable)**: A NOW_VARSTR structure containing the command to execute.

**directory (variable)**: A NOW_VARSTR structure containing the command working directory. Ignored if
NOW_EXEC_FLAG_RUN_DIRECTORY_SET is not set.

#### NOW_EXEC_PROCESS_MSG

The NOW_EXEC_PROCESS_MSG message is used to send a Windows [CreateProcess()](https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-createprocessw) request.

```mermaid
packet-beta
  0-31: "msgSize"
  32-39: "msgClass"
  40-47: "msgType"
  48-63: "msgFlags"
  64-95: "sessionId"
  96-127: "filename (variable)"
  128-159: "parameters (variable)"
  160-191: "directory (variable)"
```

**msgSize (4 bytes)**: The message size, excluding the header size (8 bytes).

**msgClass (1 byte)**: The message class (NOW_EXEC_MSG_CLASS_ID).

**msgType (1 byte)**: The message type (NOW_EXEC_PROCESS_MSG_ID).

**msgFlags (2 bytes)**: The message flags.

| Flag                                   | Meaning                   |
|----------------------------------------|---------------------------|
| NOW_EXEC_FLAG_PROCESS_PARAMETERS_SET<br>0x0001 | `parameters` field contains non-default value. |
| NOW_EXEC_FLAG_PROCESS_DIRECTORY_SET<br>0x0002 | `directory` field contains non-default value.|
| NOW_EXEC_FLAG_PROCESS_IO_REDIRECTION<br>0x1000 | Enable stdio (stdout, stderr, stdin) redirection. |


**sessionId (4 bytes)**: A 32-bit unsigned integer containing a unique remote execution session id.

**filename (variable)**: A NOW_VARSTR structure containing the file name. Corresponds to the lpApplicationName parameter.

**parameters (variable)**: A NOW_VARSTR structure containing the command parameters. Corresponds to the lpCommandLine parameter. Ignored if NOW_EXEC_FLAG_PROCESS_PARAMETERS_SET is not set.

**directory (variable)**: A NOW_VARSTR structure containing the command working directory. Corresponds to the lpCurrentDirectory parameter. Ignored if NOW_EXEC_FLAG_PROCESS_DIRECTORY_SET is not set.

#### NOW_EXEC_SHELL_MSG

The NOW_EXEC_SHELL_MSG message is used to execute a remote shell script.

```mermaid
packet-beta
  0-31: "msgSize"
  32-39: "msgClass"
  40-47: "msgType"
  48-63: "msgFlags"
  64-95: "sessionId"
  96-127: "command (variable)"
  128-159: "shell (variable)"
  160-191: "directory (variable)"
```

**msgSize (4 bytes)**: The message size, excluding the header size (8 bytes).

**msgClass (1 byte)**: The message class (NOW_EXEC_MSG_CLASS_ID).

**msgType (1 byte)**: The message type (NOW_EXEC_SHELL_MSG_ID).

**msgFlags (2 bytes)**: The message flags.

| Flag                                   | Meaning                   |
|----------------------------------------|---------------------------|
| NOW_EXEC_FLAG_SHELL_SHELL_SET<br>0x0001 | `shell` field contains non-default value. |
| NOW_EXEC_FLAG_SHELL_DIRECTORY_SET<br>0x0002 | `directory` field contains non-default value. |
| NOW_EXEC_FLAG_SHELL_IO_REDIRECTION<br>0x1000 | Enable stdio (stdout, stderr, stdin) redirection. |

**sessionId (4 bytes)**: A 32-bit unsigned integer containing a unique remote execution session id.

**command (variable)**: A NOW_VARSTR structure containing the script file contents to execute.

**shell (variable)**: A NOW_VARSTR structure containing the shell to use for execution.
If no shell is specified, the default system shell (/bin/sh) will be used.
Ignored if NOW_EXEC_FLAG_SHELL_SHELL_SET is not set.

**directory (variable)**: A NOW_VARSTR structure containing the command working directory. Ignored if
NOW_EXEC_FLAG_SHELL_DIRECTORY_SET is not set.

#### NOW_EXEC_BATCH_MSG

The NOW_EXEC_BATCH_MSG message is used to execute a remote batch script.

```mermaid
packet-beta
  0-31: "msgSize"
  32-39: "msgClass"
  40-47: "msgType"
  48-63: "msgFlags"
  64-95: "sessionId"
  96-127: "command (variable)"
  128-159: "directory (variable)"
```

**msgSize (4 bytes)**: The message size, excluding the header size (8 bytes).

**msgClass (1 byte)**: The message class (NOW_EXEC_MSG_CLASS_ID).

**msgType (1 byte)**: The message type (NOW_EXEC_BATCH_MSG_ID).

**msgFlags (2 bytes)**: The message flags.

| Flag                                   | Meaning                   |
|----------------------------------------|---------------------------|
| NOW_EXEC_FLAG_BATCH_DIRECTORY_SET<br>0x00001 | `directory` field contains non-default value. |
| NOW_EXEC_FLAG_BATCH_IO_REDIRECTION<br>0x1000 | Enable stdio (stdout, stderr, stdin) redirection. |

**sessionId (4 bytes)**: A 32-bit unsigned integer containing a unique remote execution session id.

**command (variable)**: A NOW_VARSTR structure containing the script file contents to execute.

**directory (variable)**: A NOW_VARSTR structure containing the command working directory. Ignored
if NOW_EXEC_FLAG_BATCH_DIRECTORY_SET is not set.

#### NOW_EXEC_WINPS_MSG

The NOW_EXEC_WINPS_MSG message is used to execute a remote Windows PowerShell (powershell.exe) command.

```mermaid
packet-beta
  0-31: "msgSize"
  32-39: "msgClass"
  40-47: "msgType"
  48-63: "msgFlags"
  64-95: "sessionId"
  96-127: "command (variable)"
  128-159: "directory (variable)"
  160-191: "executionPolicy (variable)"
  192-223: "configurationName (variable)"
```

**msgSize (4 bytes)**: The message size, excluding the header size (8 bytes).

**msgClass (1 byte)**: The message class (NOW_EXEC_MSG_CLASS_ID).

**msgType (1 byte)**: The message type (NOW_EXEC_WINPS_MSG_ID).

**msgFlags (2 bytes)**: The message flags, specifying the PowerShell command-line arguments.

| Flag                                          | Meaning                                                                                                        |
| --------------------------------------------- | -------------------------------------------------------------------------------------------------------------- |
| NOW_EXEC_FLAG_PS_NO_LOGO<br>0x0001            | PowerShell -NoLogo option                                                                                      |
| NOW_EXEC_FLAG_PS_NO_EXIT<br>0x0002            | PowerShell -NoExit option                                                                                      |
| NOW_EXEC_FLAG_PS_STA<br>0x0004                | PowerShell -Sta option                                                                                         |
| NOW_EXEC_FLAG_PS_MTA<br>0x0008                | PowerShell -Mta option                                                                                         |
| NOW_EXEC_FLAG_PS_NO_PROFILE<br>0x0010         | PowerShell -NoProfile option                                                                                   |
| NOW_EXEC_FLAG_PS_NON_INTERACTIVE<br>0x0020    | PowerShell -NonInteractive option                                                                              |
| NOW_EXEC_FLAG_PS_EXECUTION_POLICY<br>0x0040   | `executionPolicy` field contains non-default value and specifies the PowerShell -ExecutionPolicy parameter     |
| NOW_EXEC_FLAG_PS_CONFIGURATION_NAME<br>0x0080 | `configurationName` field contains non-default value and specifies the PowerShell -ConfigurationName parameter |
| NOW_EXEC_FLAG_PS_DIRECTORY_SET<br>0x0100      | `directory` field contains non-default value and specifies command working directory                           |
| NOW_EXEC_FLAG_PS_IO_REDIRECTION<br>0x1000     | Enable stdio (stdout, stderr, stdin) redirection.                                                               |

**sessionId (4 bytes)**: A 32-bit unsigned integer containing a unique remote execution session id.

**command (variable)**: A NOW_VARSTR structure containing the command to execute.

**directory (variable)**: A NOW_VARSTR structure containing the command working directory.
Corresponds to the lpCurrentDirectory parameter.
Ignored if NOW_EXEC_FLAG_PROCESS_DIRECTORY_SET is not set.

**executionPolicy (variable)**: A NOW_VARSTR structure containing the execution policy (-ExecutionPolicy) parameter value.
Ignored if NOW_EXEC_FLAG_PS_EXECUTION_POLICY is not set.

**configurationName (variable)**: A NOW_VARSTR structure containing the configuration name (-ConfigurationName) parameter value.
Ignored if NOW_EXEC_FLAG_PS_CONFIGURATION_NAME is not set.

#### NOW_EXEC_PWSH_MSG

The NOW_EXEC_PWSH_MSG message is used to execute a remote PowerShell 7 (pwsh) command.

```mermaid
packet-beta
  0-31: "msgSize"
  32-39: "msgClass"
  40-47: "msgType"
  48-63: "msgFlags"
  64-95: "sessionId"
  96-127: "command (variable)"
  128-159: "directory (variable)"
  160-191: "executionPolicy (variable)"
  192-223: "configurationName (variable)"
```

**msgSize (4 bytes)**: The message size, excluding the header size (8 bytes).

**msgClass (1 byte)**: The message class (NOW_EXEC_MSG_CLASS_ID).

**msgType (1 byte)**: The message type (NOW_EXEC_PWSH_MSG_ID).

**msgFlags (2 bytes)**: The message flags, specifying the PowerShell command-line arguments, same as with NOW_EXEC_WINPS_MSG.

**sessionId (4 bytes)**: A 32-bit unsigned integer containing a unique remote execution session id.

**command (variable)**: A NOW_VARSTR structure containing the command to execute.

**directory (variable)**: A NOW_VARSTR structure, same as with NOW_EXEC_WINPS_MSG.

**executionPolicy (variable)**: A NOW_VARSTR structure, same as with NOW_EXEC_WINPS_MSG.

**configurationName (variable)**: A NOW_VARSTR structure, same as with NOW_EXEC_WINPS_MSG.

### Version History
- 1.0
    - Initial protocol version
- 1.1
    - Add IO redirection capability flag and explicit IO redirection flags for exec messages.
    - Add working directory specification for Run (ShellExecute) messages.
