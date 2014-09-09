using System.Net;
using Newtonsoft.Json;

namespace JustFakeIt
{
    public class HttpExpectation
    {
        public HttpRequestExpectation Request { get; set; }
        public HttpResponseExpectation Response { get; set; }

        public void Returns(string expectedResult)
        {
            Response = new HttpResponseExpectation(HttpStatusCode.OK, expectedResult);
        }
        
        public void Returns(object expectedResult)
        {
            Response = new HttpResponseExpectation(HttpStatusCode.OK, JsonConvert.SerializeObject(expectedResult));
        }

        public void Returns(HttpStatusCode expectedStatusCode, string someStringData)
        {
            Response = new HttpResponseExpectation(expectedStatusCode, someStringData);
        }

        public void Returns(HttpStatusCode expectedStatusCode, object expectedResult)
        {
            Response = new HttpResponseExpectation(expectedStatusCode, JsonConvert.SerializeObject(expectedResult));
        }
    }
}