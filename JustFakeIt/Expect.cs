using System;
using System.Collections.Generic;

namespace JustFakeIt
{
    public class Expect
    {
        internal List<HttpExpectation> Expectations = new List<HttpExpectation>();
        
        public TimeSpan ResponseTime { get; set; }
        
        public HttpExpectation Post(string url, string body)
        {
            return Method(Http.Post, url, body);
        }

        public HttpExpectation Get(string url)
        {
            return Method(Http.Get, url);
        }

        public HttpExpectation Put(string url, string body)
        {
            return Method(Http.Put, url, body);
        }

        public HttpExpectation Delete(string url)
        {
            return Method(Http.Delete, url);
        }

        private HttpExpectation Method(Http method, string url, string body = null)
        {
            var httpExpectation = new HttpExpectation
            {
                Request = new HttpRequestExpectation(method, url, body)
            };

            Expectations.Add(httpExpectation);

            return httpExpectation;
        }
    }
}