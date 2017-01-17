using Xunit;

namespace JustFakeIt.Tests.JsonMatching
{
    public class CompareStrings
    {
        private readonly PartialJsonMatching _jsonMatcher;
        
        public CompareStrings()
        {
            _jsonMatcher = new PartialJsonMatching();
        }

        [Fact]
        public void TwoStrings_ReturnTrue()
        {
            var expected = "string";
            var actual = "string";

            var result = _jsonMatcher.MatchBody(expected, actual);
            Assert.True(result);
        }

        [Fact]
        public void SimpleJson_ReturnsTrue()
        {
            var expected = @"{ Key: ""Value"" }";
            var actual = @"{ Key: ""Value"" }";

            var result = _jsonMatcher.MatchBody(expected, actual);
            Assert.True(result);
        }

        [Fact]
        public void EmptyString_ReturnsTrue()
        {
            var expected = "";
            var actual = "";

            var result = _jsonMatcher.MatchBody(expected, actual);
            Assert.True(result);
        }

        [Fact]
        public void NullString_ReturnsTrue()
        {
            string expected = null;
            string actual = null;

            var result = _jsonMatcher.MatchBody(expected, actual);
            Assert.True(result);
        }

        [Fact]
        public void TwoStringsWithDifferentLetterCasing_ReturnFalse()
        {
            var expected = "string";
            var actual = "STRING";

            var result = _jsonMatcher.MatchBody(expected, actual);
            Assert.False(result);
        }

        [Fact]
        public void TwoDifferentStrings_ReturnFalse()
        {
            var expected = "string";
            var actual = "not same string";

            var result = _jsonMatcher.MatchBody(expected, actual);
            Assert.False(result);
        }

        [Fact]
        public void NotAValidJson_ReturnsFalse()
        {
            var expected = @"{{ Key: ""Value"" }}";
            var actual = @"{ Key: ""Value"" }";

            var result = _jsonMatcher.MatchBody(expected, actual);
            Assert.False(result);
        }

        [Fact]
        public void NullActual_ReturnsFalse()
        {
            string expected = "some string";
            string actual = null;

            var result = _jsonMatcher.MatchBody(expected, actual);
            Assert.False(result);
        }

        [Fact]
        public void NullExpected_ReturnsTrue()
        {
            string actual = "some string";
            string expected = null;

            var result = _jsonMatcher.MatchBody(expected, actual);
            Assert.True(result);
        }
    }
}
