using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace JustFakeIt
{
    public class PartialJsonMatching : IBodyMatchingOption
    {
        public bool MatchBody(string expectedBody, string actualBody)
        {
            return string.IsNullOrEmpty(expectedBody) || actualBody.Equals(expectedBody) || JsonKeyValuesMatch(expectedBody, actualBody);
        }

        public bool JsonKeyValuesMatch(string expectedBody, string actualBody)
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
