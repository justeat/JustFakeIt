﻿using NUnit.Framework;

namespace JustFakeIt.Tests.JsonMatching
{
    public class CompareJsonStrings
    {
        private PartialJsonMatching _jsonMatcher;

        [SetUp]
        public void Setup()
        {
            _jsonMatcher = new PartialJsonMatching();
        }

        [Test]
        public void PartialExpectedJsonComparison_ReturnsTrue()
        {
            var expected = @"{ Key: ""Value"" }";
            var actual = @"{ Key: ""Value"", Key2: ""Value2"" }";

            var result = _jsonMatcher.MatchBody(expected, actual);
            Assert.True(result);
        }

        [TestCase(@"{ Key: ""Value"" }")]
        [TestCase(@"{ Key: ""Value"", Key2: { NestedKey1: ""NestedValue1""} }", TestName = "Partial Nested Json Match")]
        [TestCase(@"{ Key: ""Value"", Key2: { NestedKey1: ""NestedValue1"", NestedKey2: ""NestedValue2""} }", TestName = "Full Nested Json Match")]
        public void NestedJson_ReturnsTrue(string expected)
        {
            var actual = 
                @"{ 
                    Key: ""Value"", 
                    Key2: { 
                        NestedKey1: ""NestedValue1"", 
                        NestedKey2: ""NestedValue2""
                    } 
                }";

            var result = _jsonMatcher.MatchBody(expected, actual);
            Assert.True(result);
        }

        [Test]
        public void RecursiveNestedJson_ReturnsTrue()
        {
            var expected = @"{ Key: ""Value"", Key2: { NestedKey1: ""NestedValue1""}}";
            var actual = @"{ Key: ""Value"", Key2: { NestedKey1: ""NestedValue1"", NestedKey2: {NestedNestedKey: ""NestedNestedValue""}} }";

            var result = _jsonMatcher.MatchBody(expected, actual);
            Assert.True(result);
        }

        [TestCase(@"{key:""Value""}", @"{  key : ""Value""}", TestName = "Handles Spaces correctly")]
        [TestCase(@"{key:""Value""}", @"{ ""key"" : ""Value""}", TestName = "Handles Quotes correctly")]
        [TestCase(@"{key:Value}", @"{key:Value}", TestName = "Handles lack of quotes correctly")]
        [TestCase(@"{key:null}", @"{key:null}", TestName = "Handles null correctly")]
        [TestCase(@"{key:1}", @"{key:1}", TestName = "Handles integers correctly")]
        public void FormattingIssues_StillReturnTrue(string expected, string actual)
        {
            var result = _jsonMatcher.MatchBody(expected, actual);
            Assert.True(result);
        }

        [Test]
        public void NonMatchingKeys_ReturnsFalse()
        {
            var expected = @"{ Key: ""Value"" }";
            var actual = @"{ NotSameKey: ""Value"" }";

            var result = _jsonMatcher.MatchBody(expected, actual);
            Assert.False(result);
        }

        [Test]
        public void NonMatchingValues_ReturnFalse()
        {
            var expected = @"{ Key: ""Value"" }";
            var actual = @"{ Key: ""Not Your Value"" }";

            var result = _jsonMatcher.MatchBody(expected, actual);
            Assert.False(result);
        }

        [TestCase(@"{ Key: ""Value"", Key2: { NestedKey1: ""Not Your Nested Value""} }", TestName = "Nested Json Value doesn't match")]
        [TestCase(@"{ Key: ""Value"", Key2: { NotYourNestedKey: ""Value2""} }", TestName = "Nested Json Key doesn't match")]
        public void NestedJsonThatDontMatch_ReturnsFalse(string expected)
        {
            var actual =
                @"{ 
                    Key: ""Value"", 
                    Key2: { 
                        NestedKey1: ""NestedValue1"", 
                        NestedKey2: ""NestedValue2""
                    } 
                }";

            var result = _jsonMatcher.MatchBody(expected, actual);
            Assert.True(result);
        }

        [Test]
        public void MatchingNullValues_ReturnsTrue()
        {
            var expected = @"{ Key: null }";
            var actual = @"{ Key: null }";

            var result = _jsonMatcher.MatchBody(expected, actual);
            Assert.True(result);
        }
    }
}
