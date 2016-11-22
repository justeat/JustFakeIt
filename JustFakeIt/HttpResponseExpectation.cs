using System;
using System.Collections.Specialized;
using System.Net;

namespace JustFakeIt
{
    public class HttpResponseExpectation
    {
        public HttpStatusCode StatusCode { get; set; }
        public string ExpectedResult { get; set; }
        public WebHeaderCollection Headers { get; set; }
        public TimeSpan ResponseTime { get; set; }

        public HttpResponseExpectation(HttpStatusCode expectedStatusCode, string expectedResult, NameValueCollection headers = null)
        {
            StatusCode = expectedStatusCode;
            ExpectedResult = expectedResult;
            Headers = new WebHeaderCollection();
            if (headers != null)
            {
                Headers.Add(headers);
            }
        }
    }
}