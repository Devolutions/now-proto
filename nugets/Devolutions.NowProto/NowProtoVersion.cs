namespace Devolutions.NowProto
{
    public readonly record struct NowProtoVersion(ushort Major, ushort Minor)
    {
        public static NowProtoVersion Current => new(1, 0);

        // -- IComparable -- 
        public int CompareTo(NowProtoVersion other)
        {
            return Major != other.Major
                ? Major.CompareTo(other.Major)
                : Minor.CompareTo(other.Minor);
        }
    }
}