namespace JustFakeIt
{
    public class HttpRequestExpectation
    {
        public Http Method { get; private set; }
        public string Url { get; private set; }

        public HttpRequestExpectation(Http method, string url)
        {
            Method = method;
            Url = url;
        }
    }
}