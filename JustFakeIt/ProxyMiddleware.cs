using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace JustFakeIt
{
    internal class ProxyMiddleware
    {
        private readonly RequestDelegate next;
        private readonly Expect expect;
        private readonly IList<HttpRequestExpectation> capturedRequests;

        public ProxyMiddleware(RequestDelegate next, Expect expect, IList<HttpRequestExpectation> capturedRequests = null)
        {
            this.next = next;
            this.expect = expect;
            this.capturedRequests = capturedRequests;
        }

        public async Task Invoke(HttpContext context)
        {
            await next(context);

            var body = CaptureRequest(context.Request);

            var matchingExpectation = 
                expect.Expectations.FirstOrDefault(e => RequestAndExpectedHttpMethodAndPathsMatch(context, e.Request, body));

            if (matchingExpectation == null)
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync(string.Empty);
                return;
            }

            var missingRequestHeaders = MissingRequestHeaders(context.Request.Headers, matchingExpectation.Request.Headers);

            if (!string.IsNullOrWhiteSpace(missingRequestHeaders))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync(missingRequestHeaders);
                return;
            }

            ProcessMatchingExpectation(context.Response, matchingExpectation);
        }

        private string CaptureRequest(HttpRequest request)
        {
            string body;
            
            using (var sr = new StreamReader(request.Body))
            {
                body = sr.ReadToEnd();
            }

            if (capturedRequests != null)
            {
                var method = (Http)Enum.Parse(typeof(Http), request.Method, true);
                var url = request.GetUri().GetComponents(UriComponents.PathAndQuery, UriFormat.Unescaped);
                capturedRequests.Add(new HttpRequestExpectation(method, url, body));
            }

            return body;
        }

        private static bool RequestAndExpectedHttpMethodAndPathsMatch(HttpContext context, HttpRequestExpectation requestExpectation, string actualBody)
        {
            return
                requestExpectation.MatchesActualPath(context.Request.GetUri().PathAndQuery) &&
                requestExpectation.MatchesActualHttpMethod(context.Request.Method) &&
                requestExpectation.MatchesActualBody(actualBody);
        }

        private string MissingRequestHeaders(IHeaderDictionary headers, WebHeaderCollection expectedRequestHeaders)
        {
            if (expectedRequestHeaders == null)
            {
                return "";
            }

            var errors = new StringBuilder();
            foreach (var key in expectedRequestHeaders.AllKeys)
            {
                if (headers.ContainsKey(key))
                {
                    if (headers[key] != expectedRequestHeaders[key])
                    {
                        errors.AppendFormat("{0} header value not as expected.\r\n\tExpected: {1}\r\n\tProvided: {2}\r\n", key, expectedRequestHeaders[key], headers[key]);
                    }
                }
                else
                {
                    errors.AppendFormat("{0} header was not provided.\r\n", key);
                }
            }

            return errors.ToString();
        }

        private void ProcessMatchingExpectation(HttpResponse response, HttpExpectation httpExpectation)
        {
            var httpResponseExpectation = httpExpectation.Response;
            if (httpExpectation.ResponseExpectationCallback != null)
            {
                httpResponseExpectation = httpExpectation.ResponseExpectationCallback.Invoke();
            }

            var expectedResults = string.Empty;
            if (httpResponseExpectation != null)
            {
                response.StatusCode = (int)httpResponseExpectation.StatusCode;
                expectedResults = httpResponseExpectation.ExpectedResult;

                if (httpResponseExpectation.Headers != null)
                {
                    foreach (var key in httpResponseExpectation.Headers.AllKeys)
                        response.Headers.Add(key, new[] {httpResponseExpectation.Headers[key]});
                }
            }

            response.Headers?.Add("Content-Type", new[] {"application/json"});

            Task.Delay(expect.ResponseTime).Wait();

            response.WriteAsync(expectedResults);
        }
    }
}
