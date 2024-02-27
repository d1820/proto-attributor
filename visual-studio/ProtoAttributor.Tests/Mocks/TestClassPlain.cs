using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using ProtoBuf;

namespace ProtoAttributor.Tests.Mocks
{
    [DataContract]
    public class TestClassPlain
    {
        [Required]
        public int MyProperty { get; set; }

        public int MyProperty2 { get; set; }
    }
}
