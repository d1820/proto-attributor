
using ProtoBuf;

namespace ProtoAttributor.Tests.Mocks
{
    [ProtoContract]
    public enum TestEnumWithProtoIgnore
    {
        One,
        [ProtoIgnore]
        Two,
        Three,
        Four,
        [ProtoIgnore]
        Five
    }
}
