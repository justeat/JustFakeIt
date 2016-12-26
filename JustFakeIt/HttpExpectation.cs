using System;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using RazorEngine;
using RazorEngine.Templating;

namespace JustFakeIt
{
    public class HttpExpectation
    {
        public HttpRequestExpectation Request { get; set; }
        public HttpResponseExpectation Response { get; set; }
        public Func<HttpResponseExpectation> ResponseExpectationCallback { get; private set; }

        public HttpExpectation Returns(string expectedResult, WebHeaderCollection expectedHeaders = null)
        {
            Returns(HttpStatusCode.OK, expectedResult, expectedHeaders);
            return this;
        }

        public HttpExpectation Returns(object expectedResult, WebHeaderCollection expectedHeaders = null)
        {
            Returns(HttpStatusCode.OK, JsonConvert.SerializeObject(expectedResult), expectedHeaders);
            return this;
        }

        private string BuildAbsolutePath(string relativePath)
        {
            var absolutePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
            return absolutePath;
        }

        public HttpExpectation ReturnsFromFile(string relativePath, WebHeaderCollection expectedHeaders = null)
        {
            var response = File.ReadAllText(BuildAbsolutePath(relativePath));
            Returns(HttpStatusCode.OK, response, expectedHeaders);
            return this;
        }

        public HttpExpectation ReturnsFromTemplate(string relativePath, object values, WebHeaderCollection expectedHeaders = null)
        {
            var absolutePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
            string compiled = Engine.Razor.RunCompile(File.ReadAllText(absolutePath), Guid.NewGuid().ToString(), null, values);
            Returns(HttpStatusCode.OK, compiled, expectedHeaders);
            return this;
        }

        public HttpExpectation Returns(HttpStatusCode expectedStatusCode, string someStringData, WebHeaderCollection expectedHeaders = null)
        {
            Response = new HttpResponseExpectation(expectedStatusCode, someStringData, expectedHeaders);
            return this;
        }


        public HttpExpectation Returns(HttpStatusCode expectedStatusCode, object expectedResult, WebHeaderCollection expectedHeaders = null)
        {
            Returns(expectedStatusCode, JsonConvert.SerializeObject(expectedResult), expectedHeaders);
            return this;
        }

        public HttpExpectation Callback(Func<HttpResponseExpectation> responseExpectationFunc)
        {
            ResponseExpectationCallback = responseExpectationFunc;
            return this;
        }

        public HttpExpectation WithHttpStatus(HttpStatusCode expectedStatusCode)
        {
            Response.StatusCode = expectedStatusCode;
            return this;
        }

        public HttpExpectation WithHeader(string name, string value)
        {
            Response.Headers.Add(name, value);
            return this;
        }

        public HttpExpectation RespondsIn(TimeSpan time)
        {
            Response.ResponseTime = time;
            return this;
        }

        public HttpExpectation WithPartialJsonMatching()
        {
            Request.BodyMatching = new PartialJsonMatching();
            return this;
        }

        public HttpExpectation UseActualBodyMatching()
        {
            Request.BodyMatching = new AbsoluteBodyMatching();
            return this;
        }
    }
}