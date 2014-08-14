namespace JustFakeIt
{
    public class HttpExpectation
    {
        public string Path { get; set; }
        public int StatusCode { get; set; }
        public object Data { get; set; }
        public HttpRequestExpectation Request { get; set; }

        public void Returns(string expectedResult)
        {
            Response = new HttpResponseExpectation(HttpStatusCode.Ok, expectedResult);
        }

        public HttpResponseExpectation Response { get; set; }
    }
}