using NUnit.Framework;

namespace JustFakeIt.Tests.JsonMatching
{
    [TestFixture]
    public class CompareStrings
    {
        [Test]
        public void TwoStrings_ReturnTrue()
        {
            var expected = "string";
            var actual = "string";

            var result = ExpectationExtensions.MatchBody(expected, actual);
            Assert.True(result);
        }

        [Test]
        public void SimpleJson_ReturnsTrue()
        {
            var expected = @"{ Key: ""Value"" }";
            var actual = @"{ Key: ""Value"" }";

            var result = ExpectationExtensions.MatchBody(expected, actual);
            Assert.True(result);
        }

        [Test]
        public void EmptyString_ReturnsTrue()
        {
            var expected = "";
            var actual = "";

            var result = ExpectationExtensions.MatchBody(expected, actual);
            Assert.True(result);
        }

        [Test]
        public void NullString_ReturnsTrue()
        {
            string expected = null;
            string actual = null;

            var result = ExpectationExtensions.MatchBody(expected, actual);
            Assert.True(result);
        }

        [Test]
        public void TwoStringsWithDifferentLetterCasing_ReturnFalse()
        {
            var expected = "string";
            var actual = "STRING";

            var result = ExpectationExtensions.MatchBody(expected, actual);
            Assert.False(result);
        }

        [Test]
        public void TwoDifferentStrings_ReturnFalse()
        {
            var expected = "string";
            var actual = "not same string";

            var result = ExpectationExtensions.MatchBody(expected, actual);
            Assert.False(result);
        }

        [Test]
        public void NotAValidJson_ReturnsFalse()
        {
            var expected = @"{{ Key: ""Value"" }}";
            var actual = @"{ Key: ""Value"" }";

            var result = ExpectationExtensions.MatchBody(expected, actual);
            Assert.False(result);
        }
    }
}
