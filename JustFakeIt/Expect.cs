using System.Collections.Generic;

namespace JustFakeIt
{
    public class Expect
    {
        internal List<HttpExpectation> Expectations = new List<HttpExpectation>();

        public HttpExpectation Post(string url, string body)
        {
            var httpExpectation = new HttpExpectation
            {
                Request = new HttpRequestExpectation(Http.Post, url, body),
            };

            Expectations.Add(httpExpectation);

            return httpExpectation;
        }

        public HttpExpectation Get(string url)
        {
            var httpExpectation = new HttpExpectation
            {
                Request = new HttpRequestExpectation(Http.Get, url),
            };

            Expectations.Add(httpExpectation);

            return httpExpectation;
        }

        public HttpExpectation Put(string url, string body)
        {
            var httpExpectation = new HttpExpectation
            {
                Request = new HttpRequestExpectation(Http.Put, url, body),
            };

            Expectations.Add(httpExpectation);

            return httpExpectation;
        }
    }
}