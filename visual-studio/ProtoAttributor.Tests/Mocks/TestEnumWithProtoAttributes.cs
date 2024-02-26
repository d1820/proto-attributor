
using ProtoBuf;

namespace ProtoAttributor.Tests.Mocks
{
    [ProtoContract]
    public enum TestEnumWithProtoAttributes
    {
        One,
        [ProtoEnum]
        Two,
        [ProtoEnum]
        Three,
        Four,
        Five
    }
}
