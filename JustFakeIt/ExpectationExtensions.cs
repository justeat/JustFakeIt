using System;
using System.IO;
using System.Text.RegularExpressions;

namespace JustFakeIt
{
    public static class ExpectationExtensions
    {
        public const string IgnoreParameter = "{ignore}";

        public static bool MatchesActualPath(this HttpRequestExpectation expected, string actualPath)
        {
            var newPath = MatchIgnoredParameters(expected, actualPath);
            return newPath.Equals(expected.Url);
        }

        private static string MatchIgnoredParameters(HttpRequestExpectation expected, string actualPath)
        {
            if (expected.Url.IndexOf(IgnoreParameter, StringComparison.Ordinal) == -1) return actualPath;

            var segments = Regex.Split(expected.Url, IgnoreParameter);
            for (var i = 0; i < segments.Length; i++)
            {
                if (segments[i] == "") continue;
                var segmentEnd = actualPath.IndexOf(segments[i], StringComparison.Ordinal) + segments[i].Length;
                var nextSegmentStart = actualPath.Length;
                if (segments.Length != i + 1)
                {
                    if (segments[i + 1] != "")
                    {
                        nextSegmentStart = actualPath.IndexOf(segments[i + 1], StringComparison.Ordinal);
                    }
                }

                actualPath = actualPath.Replace(actualPath.Substring(segmentEnd, nextSegmentStart - segmentEnd),
                    IgnoreParameter);
            }
            return actualPath;
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