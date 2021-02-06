using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using ProtoAttributor.Services;
using ProtoBuf;
using System;
using Xunit;

namespace ProtoAttributor.Tests
{
    [ProtoContract]
    public class TestClass
    {
        [ProtoMember(10, Name = "test")]
        public int MyProperty { get; set; }

        [ProtoMember(11)]
        public int MyProperty11 { get; set; }

    }

}
