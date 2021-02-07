using System;
using Xunit;
using ProtoBuf;
using System.ComponentModel.DataAnnotations;

namespace ProtoAttributor.Tests
{
    [ProtoContract]
    public class TestCodeWithAttributes
    {
        [Required]
        [ProtoMember(1)]
        public int MyProperty { get; set; }
        [ProtoMember(2)]
        public int MyProperty2 { get; set; }
        public int MyProperty3 { get; set; }
        public int MyProperty4 { get; set; }
    }
}
