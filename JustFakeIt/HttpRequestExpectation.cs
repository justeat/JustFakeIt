namespace JustFakeIt
{
    public class HttpRequestExpectation
    {
        public Http Method { get; private set; }
        public string Url { get; private set; }
        public string Body { get; private set; }

        public HttpRequestExpectation(Http method, string url, string body = null)
        {
            Method = method;
            Url = url;
            Body = body;
        }
    }
}