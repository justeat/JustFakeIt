using System;
using System.IO;

namespace JustFakeIt
{
    public static class ExpectationExtensions
    {
        public static bool MatchesActualPath(this HttpRequestExpectation expected, string actualPath)
        {
            return actualPath.Equals(expected.Url);
        }

        public static bool MatchesActualHttpMethod(this HttpRequestExpectation expected, string actualHttpMethod)
        {
            return actualHttpMethod.Equals(expected.Method.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public static bool MatchesActualBody(this HttpRequestExpectation expected, Stream actualBody)
        {
            string stringBody;

            using (var sr = new StreamReader(actualBody))
            {
                stringBody = sr.ReadToEnd();
            }

            return string.IsNullOrEmpty(expected.Body) || stringBody.Equals(expected.Body);
        }
    }
}