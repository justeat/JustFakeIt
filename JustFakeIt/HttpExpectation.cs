using System;
using System.Net;
using Newtonsoft.Json;

namespace JustFakeIt
{
    public class HttpExpectation
    {
        public HttpRequestExpectation Request { get; set; }
        public HttpResponseExpectation Response { get; set; }
        public Func<HttpResponseExpectation> ResponseExpectationCallback { get; private set; }

        public void Returns(string expectedResult, WebHeaderCollection expectedHeaders = null)
        {
            Returns(HttpStatusCode.OK, expectedResult, expectedHeaders);
        }

        public void Returns(object expectedResult, WebHeaderCollection expectedHeaders = null)
        {
            Returns(HttpStatusCode.OK, JsonConvert.SerializeObject(expectedResult), expectedHeaders);
        }

        public void Returns(HttpStatusCode expectedStatusCode, string someStringData, WebHeaderCollection expectedHeaders = null)
        {
            Response = new HttpResponseExpectation(expectedStatusCode, someStringData, expectedHeaders);
        }

        public void Returns(HttpStatusCode expectedStatusCode, object expectedResult, WebHeaderCollection expectedHeaders = null)
        {
            Returns(expectedStatusCode, JsonConvert.SerializeObject(expectedResult), expectedHeaders);
        }

        public void Callback(Func<HttpResponseExpectation> responseExpectationFunc)
        {
            ResponseExpectationCallback = responseExpectationFunc;
        }
    }
}