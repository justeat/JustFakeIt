using System.Net;

namespace JustFakeIt
{
    public class HttpResponseExpectation
    {
        public HttpStatusCode StatusCode { get; set; }
        public string ExpectedResult { get; set; }

        public HttpResponseExpectation(HttpStatusCode expectedStatusCode, string expectedResult)
        {
            StatusCode = expectedStatusCode;
            ExpectedResult = expectedResult;
        }
    }
}