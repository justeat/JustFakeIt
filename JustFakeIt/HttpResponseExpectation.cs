using System.Net;

namespace JustFakeIt
{
    public class HttpResponseExpectation
    {
        public HttpStatusCode StatusCode { get; set; }
        public string ExpectedResult { get; set; }
        public WebHeaderCollection Headers { get; set; }

        public HttpResponseExpectation(HttpStatusCode expectedStatusCode, string expectedResult, WebHeaderCollection headers = null)
        {
            StatusCode = expectedStatusCode;
            ExpectedResult = expectedResult;
            Headers = headers ?? new WebHeaderCollection();
        }
    }
}