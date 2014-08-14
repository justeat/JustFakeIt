using System;
using System.Collections.Generic;
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
                    app.Map(expectation.Request.Url, builder => builder.Run(context =>
                    {
                        context.Response.Headers.Add("Content-Type", new[] { "application/json" });
                        context.Response.StatusCode = expectation.Response.StatusCode;
                        return context.Response.WriteAsync(expectation.Response.ExpectedResult);
                    }));
                }
            });
        }

        public HttpExpectation Expect(Http method, string url)
        {
            var httpExpectation = new HttpExpectation
            {
                Request = new HttpRequestExpectation(method, url),
            };

            _expectations.Add(httpExpectation);

            return httpExpectation;
        }
    }
}
