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
            Returns(HttpStatusCode.OK, expectedResult);
        }
        
        public void Returns(object expectedResult)
        {
            Returns(HttpStatusCode.OK, JsonConvert.SerializeObject(expectedResult));
        }

        public void Returns(HttpStatusCode expectedStatusCode, string someStringData)
        {
            Response = new HttpResponseExpectation(expectedStatusCode, someStringData);
        }

        public void Returns(HttpStatusCode expectedStatusCode, object expectedResult)
        {
            Returns(expectedStatusCode, JsonConvert.SerializeObject(expectedResult));
        }
    }
}