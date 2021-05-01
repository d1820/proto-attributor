using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ProtoAttributor.Tests
{
    [DataContract]
    public class TestCodeWithDataMemberAttributesMIssingOrderProperties
    {
        [Required]
        [DataMember]
        public int MyProperty { get; set; }

        [DataMember(Order = 2)]
        public int MyProperty2 { get; set; }

        [DataMember(Name = "Test")]
        public int MyProperty3 { get; set; }

        public int MyProperty4 { get; set; }
    }
}
