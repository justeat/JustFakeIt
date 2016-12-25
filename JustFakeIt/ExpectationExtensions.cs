using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

                var parameterLength = nextSegmentStart - segmentEnd;

                if (parameterLength <= 0)
                {
                    continue;
                }

                var parameter = actualPath.Substring(segmentEnd, parameterLength);

                actualPath = actualPath.Replace(parameter, IgnoreParameter);
            }
            return actualPath;
        }

        public static bool MatchesActualHttpMethod(this HttpRequestExpectation expected, string actualHttpMethod)
        {
            return actualHttpMethod.Equals(expected.Method.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public static bool MatchesActualBody(this HttpRequestExpectation expected, string actualBody)
        {
            return MatchBody(expected.Body, actualBody);
        }

        public static bool MatchBody(string expectedBody, string actualBody)
        {
            return string.IsNullOrEmpty(expectedBody) || actualBody.Equals(expectedBody) || expectedBody.JsonKeyValuesMatch(actualBody);
        }

        public static bool JsonKeyValuesMatch(this string expectedBody, string actualBody)
        {
            JObject expectedJObject;
            JObject actualJObject;

            try
            {
                expectedJObject = JsonConvert.DeserializeObject<JObject>(expectedBody);
                actualJObject = JsonConvert.DeserializeObject<JObject>(actualBody);
            }
            catch (Exception)
            {
                return false;
            }

            if (JToken.DeepEquals(expectedJObject, actualJObject)) return true;
            
            foreach (KeyValuePair<string, JToken> expectedProperty in expectedJObject)
            {
                JProperty actualProperty = actualJObject.Property(expectedProperty.Key);

                if (actualProperty == null) return false;

                if (JToken.DeepEquals(expectedProperty.Value, actualProperty.Value)) return true;
                
                if (MatchBody(expectedProperty.Value.ToString(), actualProperty.Value.ToString())) return true;

                Debug.WriteLine($"Value don't match for key {expectedProperty.Key}.");
                Debug.WriteLine($"Expected { expectedProperty.Value} but got {actualProperty.Value}");

                return false;
            }

            return true;
        }
    }
}