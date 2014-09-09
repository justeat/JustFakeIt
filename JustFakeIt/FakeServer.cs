using System;
using System.IO;
using Microsoft.Owin;
using Microsoft.Owin.Hosting;
using Owin;

namespace JustFakeIt
{
    public class FakeServer : IDisposable
    {
        private readonly Uri _baseUri;
        public Expect Expect { get; protected set; }

        public FakeServer(Uri baseUri)
        {
            _baseUri = baseUri;
            Expect = new Expect();
        }

        private IDisposable _webApp;

        public void Dispose()
        {
            _webApp.Dispose();
        }

        public void Start()
        {
            _webApp = WebApp.Start(_baseUri.ToString(), app =>
            {
                foreach (var expectation in Expect.Expectations)
                {
                    app.MapWhen(
                        context => RequestAndExpectedHttpMethodAndPathsMatch(context, expectation.Request),
                        builder => builder.Run(context =>
                        {
                            context.Response.Headers.Add("Content-Type", new[] {"application/json"});
                            context.Response.StatusCode = expectation.Response.StatusCode;
                            return context.Response.WriteAsync(expectation.Response.ExpectedResult);
                        }));
                }
            });
        }

        private static bool RequestAndExpectedHttpMethodAndPathsMatch(IOwinContext context, HttpRequestExpectation requestExpectation)
        {
            return 
                requestExpectation.MatchesActualPath(context.Request.Path) &&
                requestExpectation.MatchesActualHttpMethod(context.Request.Method) &&
                requestExpectation.MatchesActualBody(context.Request.Body);
        }
    }
}
