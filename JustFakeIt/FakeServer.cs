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
    }
}
