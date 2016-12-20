using System;
using System.Collections.Generic;
using System.Net;

namespace JustFakeIt
{
    public class Expect
    {
        internal List<HttpExpectation> Expectations = new List<HttpExpectation>();
        
        public TimeSpan ResponseTime { get; set; }
        
        public HttpExpectation Post(string url, string body = null, WebHeaderCollection expectedHeaders = null)
        {
            return Method(Http.Post, url, body, expectedHeaders);
        }

        public HttpExpectation Get(string url, WebHeaderCollection expectedHeaders = null)
        {
            return Method(Http.Get, url, null, expectedHeaders);
        }

        public HttpExpectation Put(string url, string body, WebHeaderCollection expectedHeaders = null)
        {
            return Method(Http.Put, url, body, expectedHeaders);
        }

        public HttpExpectation Delete(string url, WebHeaderCollection expectedHeaders = null)
        {
            return Method(Http.Delete, url, null, expectedHeaders);
        }

        private HttpExpectation Method(Http method, string url, string body = null, WebHeaderCollection expectedHeaders = null)
        {
            var httpExpectation = new HttpExpectation
            {
                Request = new HttpRequestExpectation(method, url, body, expectedHeaders)
            };

            Expectations.Add(httpExpectation);

            return httpExpectation;
        }
    }
}