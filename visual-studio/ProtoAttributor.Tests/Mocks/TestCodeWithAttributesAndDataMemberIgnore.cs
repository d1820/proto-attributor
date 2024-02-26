using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ProtoAttributor.Tests
{
    [DataContract]
    public class TestCodeWithAttributesAndDataMemberIgnore
    {
        [Required]
        [DataMember(Order = 1)]
        public int MyProperty { get; set; }

        [DataMember(Order = 2)]
        public int MyProperty2 { get; set; }

        [IgnoreDataMember]
        public int MyProperty3 { get; set; }

        public int MyProperty4 { get; set; }
    }
}
