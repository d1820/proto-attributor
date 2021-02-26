
using System.Runtime.Serialization;

namespace ProtoAttributor.Tests.Mocks
{
    [DataContract]
    public enum TestEnumWithDataMemberIgnore
    {
        One,
        Two,
        [IgnoreDataMember]
        Three,
        Four,
        [IgnoreDataMember]
        Five
    }
}
