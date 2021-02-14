using System.ComponentModel.DataAnnotations;

namespace ProtoAttributor.Tests.Mocks
{
    public class TestClassPlain
    {
        [Required]
        public int MyProperty { get; set; }

        public int MyProperty2 { get; set; }
    }
}
