using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Owin;

namespace JustFakeIt
{
    public static class ExpetcationExtensions
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
            if (actualBody.Length == 0 && string.IsNullOrEmpty(expected.Body))
            {
                return true;
            }

            string stringBody;

            using (var sr = new StreamReader(actualBody))
            {
                stringBody = sr.ReadToEnd();
            }

            return stringBody.Equals(expected.Body);
        }
    }
}