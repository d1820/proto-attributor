using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using System.ComponentModel.DataAnnotations;
using System;
using Xunit;

namespace ProtoAttributor.Tests.Mocks
{
    public class TestClassPlain
    {
        [Required]
        public int MyProperty { get; set; }
        public int MyProperty2 { get; set; }
    }
}
