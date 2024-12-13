namespace Devolutions.NowProto
{
    public enum SessionMessageKind : byte
    {
        Lock = 0x01,
        Logoff = 0x02,
        MsgBoxReq = 0x03,
        MsgBoxRsp = 0x04,
    }
}