using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using System;
using Xunit;

namespace ProtoAttributor.Tests.Mocks
{
    /// <summary>
    /// TestClass
    /// </summary>
    public class TestClassWithComments
    {
        /// <summary>
        /// Gets or sets my property.
        /// </summary>
        /// <value>
        /// My property.
        /// </value>
        public int MyProperty { get; set; }

        /// <summary>
        /// Gets or sets my property11.
        /// </summary>
        /// <value>
        /// My property11.
        /// </value>
        public int MyProperty11 { get; set; }

    }
}
