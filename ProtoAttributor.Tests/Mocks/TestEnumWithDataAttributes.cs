
using System.Runtime.Serialization;

namespace ProtoAttributor.Tests.Mocks
{

    [DataContract]
    public enum TestEnumWithDataAttributes
    {
        [EnumMember]
        One,
        [EnumMember]
        Two,
        Three,
        Four,
        Five
    }
}
