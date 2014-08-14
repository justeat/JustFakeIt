namespace JustFakeIt
{
    public class HttpResponseExpectation
    {
        public int StatusCode { get; set; }
        public string ExpectedResult { get; set; }

        public HttpResponseExpectation(int statusCode, string expectedResult)
        {
            StatusCode = statusCode;
            ExpectedResult = expectedResult;
        }
    }
}