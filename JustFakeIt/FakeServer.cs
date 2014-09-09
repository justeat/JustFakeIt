using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Owin;
using Microsoft.Owin.Hosting;
using Owin;

namespace JustFakeIt
{
    public class FakeServer : IDisposable
    {
        private readonly Uri _baseUri;

        public FakeServer(Uri baseUri)
        {
            _baseUri = baseUri;
            _expectations = new List<HttpExpectation>();
        }

        private IDisposable _webApp;
        private readonly List<HttpExpectation> _expectations;

        public void Dispose()
        {
            _webApp.Dispose();
        }

        public void Start()
        {
            _webApp = WebApp.Start(_baseUri.ToString(), app =>
            {
                foreach (var expectation in _expectations)
                {
                    app.MapWhen(
                        context => RequestAndExpectedHttpMethodAndPathsMatch(context, expectation),
                        builder => builder.Run(context =>
                        {
                            context.Response.Headers.Add("Content-Type", new[] {"application/json"});
                            context.Response.StatusCode = expectation.Response.StatusCode;
                            return context.Response.WriteAsync(expectation.Response.ExpectedResult);
                        }));
                }
            });
        }

        private static bool RequestAndExpectedHttpMethodAndPathsMatch(IOwinContext context, HttpExpectation expectation)
        {
            return RequestAndExpectedHttpMethodsMatch(context, expectation) &&
                   RequestAndExpectedPathsMatch(context, expectation) &&
                   RequestAndExpectedBodiesMatch(context, expectation);
        }

        private static bool RequestAndExpectedBodiesMatch(IOwinContext context, HttpExpectation expectation)
        {
            if(context.Request.Body.Length == 0 && string.IsNullOrEmpty(expectation.Request.Body))
            {
                return true;
            }

            string requestBody;

            using (var sr = new StreamReader(context.Request.Body))
            {
                requestBody = sr.ReadToEnd();
            }

            return requestBody.Equals(expectation.Request.Body);
        }

        private static bool RequestAndExpectedPathsMatch(IOwinContext context, HttpExpectation expectation)
        {
            return context.Request.Path.Equals(new PathString(expectation.Request.Url));
        }

        private static bool RequestAndExpectedHttpMethodsMatch(IOwinContext context, HttpExpectation expectation)
        {
            return context.Request.Method.Equals(expectation.Request.Method.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public HttpExpectation ExpectGet(string url)
        {
            var httpExpectation = new HttpExpectation
            {
                Request = new HttpRequestExpectation(Http.Get, url),
            };

            _expectations.Add(httpExpectation);

            return httpExpectation;
        }

        public HttpExpectation ExpectPost(string url, string body)
        {
            var httpExpectation = new HttpExpectation
            {
                Request = new HttpRequestExpectation(Http.Post, url, body),
            };

            _expectations.Add(httpExpectation);

            return httpExpectation;
        }
    }
}
