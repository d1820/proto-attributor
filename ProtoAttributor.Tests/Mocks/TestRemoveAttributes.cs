using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using System.ComponentModel.DataAnnotations;
using System;
using Xunit;
using ProtoBuf;

namespace ProtoAttributor.Tests.Mocks
{
    [Serializable]
    [ProtoContract]
    [ProtoInclude(100, typeof(string))]
    [ProtoInclude(101, typeof(string))]
    public class TestRemoveAttributes
    {

        [Required]
        [ProtoMember(10, Name = "test")]
        public int MyProperty { get; set; }
        [ProtoMember(20)]
        public int MyProperty2 { get; set; }

        [Required]
        public int MyProperty3 { get; set; }

        [ProtoIgnore]
        public int MyProperty4 { get; set; }

        public int MyProperty5 { get; set; }
    }
}
