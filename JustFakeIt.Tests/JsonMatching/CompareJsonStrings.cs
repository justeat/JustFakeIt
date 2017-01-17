using Xunit;

namespace JustFakeIt.Tests.JsonMatching
{
    public class CompareJsonStrings
    {
        private readonly PartialJsonMatching _jsonMatcher;
        
        public CompareJsonStrings()
        {
            _jsonMatcher = new PartialJsonMatching();
        }

        [Fact]
        public void PartialExpectedJsonComparison_ReturnsTrue()
        {
            var expected = @"{ Key: ""Value"" }";
            var actual = @"{ Key: ""Value"", Key2: ""Value2"" }";

            var result = _jsonMatcher.MatchBody(expected, actual);
            Assert.True(result);
        }

        [InlineData(@"{ Key: ""Value"" }")]
        [InlineData(@"{ Key: ""Value"", Key2: { NestedKey1: ""NestedValue1""} }")] // Partial Nested Json Match
        [InlineData(@"{ Key: ""Value"", Key2: { NestedKey1: ""NestedValue1"", NestedKey2: ""NestedValue2""} }")] // Full Nested Json Match
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

        [Fact]
        public void RecursiveNestedJson_ReturnsTrue()
        {
            var expected = @"{ Key: ""Value"", Key2: { NestedKey1: ""NestedValue1""}}";
            var actual = @"{ Key: ""Value"", Key2: { NestedKey1: ""NestedValue1"", NestedKey2: {NestedNestedKey: ""NestedNestedValue""}} }";

            var result = _jsonMatcher.MatchBody(expected, actual);
            Assert.True(result);
        }

        [Theory]
        [InlineData(@"{key:""Value""}", @"{  key : ""Value""}")] // Handles Spaces correctly
        [InlineData(@"{key:""Value""}", @"{ ""key"" : ""Value""}")] // Handles Quotes correctly
        [InlineData(@"{key:Value}", @"{key:Value}")] // Handles lack of quotes correctly
        [InlineData(@"{key:null}", @"{key:null}")] // "Handles null correctly"
        [InlineData(@"{key:1}", @"{key:1}")] // "Handles integers correctly"s
        public void FormattingIssues_StillReturnTrue(string expected, string actual)
        {
            var result = _jsonMatcher.MatchBody(expected, actual);
            Assert.True(result);
        }

        [Fact]
        public void NonMatchingKeys_ReturnsFalse()
        {
            var expected = @"{ Key: ""Value"" }";
            var actual = @"{ NotSameKey: ""Value"" }";

            var result = _jsonMatcher.MatchBody(expected, actual);
            Assert.False(result);
        }

        [Fact]
        public void NonMatchingValues_ReturnFalse()
        {
            var expected = @"{ Key: ""Value"" }";
            var actual = @"{ Key: ""Not Your Value"" }";

            var result = _jsonMatcher.MatchBody(expected, actual);
            Assert.False(result);
        }

        [Theory]
        [InlineData(@"{ Key: ""Value"", Key2: { NestedKey1: ""Not Your Nested Value""} }")] // Nested Json Value doesn't match
        [InlineData(@"{ Key: ""Value"", Key2: { NotYourNestedKey: ""Value2""} }")] // Nested Json Key doesn't match
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

        [Fact]
        public void MatchingNullValues_ReturnsTrue()
        {
            var expected = @"{ Key: null }";
            var actual = @"{ Key: null }";

            var result = _jsonMatcher.MatchBody(expected, actual);
            Assert.True(result);
        }
    }
}
