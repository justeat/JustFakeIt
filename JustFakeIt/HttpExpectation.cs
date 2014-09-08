using Newtonsoft.Json;

namespace JustFakeIt
{
    public class HttpExpectation
    {
        public string Path { get; set; }
        public int StatusCode { get; set; }
        public object Data { get; set; }
        public HttpRequestExpectation Request { get; set; }
        public HttpResponseExpectation Response { get; set; }

        public void Returns(string expectedResult)
        {
            Response = new HttpResponseExpectation(HttpStatusCode.Ok, expectedResult);
        }
        
        public void Returns(object expectedResult)
        {
            Response = new HttpResponseExpectation(HttpStatusCode.Ok, JsonConvert.SerializeObject( expectedResult));
        }
    }
}