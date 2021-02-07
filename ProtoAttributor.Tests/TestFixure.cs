using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using ProtoAttributor.Services;
using ProtoBuf;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace ProtoAttributor.Tests
{
    public class TestFixure
    {
        public string LoadTestFile(string relativePath)
        {
            return File.ReadAllText(relativePath);
        }

        public void AssertOutputContainsCount(string[] source, string searchTerm, int numOfTimes)
        {
            var matchQuery = from word in source
                             where word.IndexOf(searchTerm, StringComparison.InvariantCultureIgnoreCase) > -1
                             select word;


            matchQuery.Count().Should().Be(numOfTimes);
        }
    }

}
