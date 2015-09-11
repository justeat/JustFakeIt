using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace JustFakeIt
{
    public class ProxyMiddleware :  OwinMiddleware
    {
        private readonly Expect _expect;
        private readonly IList<HttpRequestExpectation> _capturedRequests;

        public ProxyMiddleware(OwinMiddleware next, Expect expect, IList<HttpRequestExpectation> capturedRequests = null) : base(next)
        {
            _expect = expect;
            _capturedRequests = capturedRequests;
        }

        public override Task Invoke(IOwinContext context)
        {
            var body = CaptureRequest(context.Request);

            Debug.WriteLine("Looking for registration that matches: ");
            Debug.WriteLine("\t\t\tPath:\t\t\t" + context.Request.Uri.PathAndQuery);
            Debug.WriteLine("\t\t\tMethod:\t\t\t" + context.Request.Method);
            Debug.WriteLine("\t\t\tBody:\t\t\t" + body);
            
            var matchingExpectation = 
                _expect.Expectations.FirstOrDefault(e => RequestAndExpectedHttpMethodAndPathsMatch(context, e.Request));

            if (matchingExpectation == null)
            {
                context.Response.StatusCode = 404;
                return context.Response.WriteAsync(new byte[0]);
            }

            return ProcessMatchingExpectation(context.Response, matchingExpectation);
        }

        private string CaptureRequest(IOwinRequest request)
        {
            string body;
            using (var sr = new StreamReader(request.Body))
                body = sr.ReadToEnd();
            request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));

            if (_capturedRequests != null)
            {
                var method = (Http)Enum.Parse(typeof(Http), request.Method, true);
                var url = request.Uri.GetComponents(UriComponents.PathAndQuery, UriFormat.Unescaped);
                _capturedRequests.Add(new HttpRequestExpectation(method, url, body));
            }

            return body;
        }

        private static bool RequestAndExpectedHttpMethodAndPathsMatch(IOwinContext context, HttpRequestExpectation requestExpectation)
        {
            return
                requestExpectation.MatchesActualPath(context.Request.Uri.GetComponents(UriComponents.PathAndQuery, UriFormat.Unescaped)) &&
                requestExpectation.MatchesActualHttpMethod(context.Request.Method) &&
                requestExpectation.MatchesActualBody(context.Request.Body);
        }

        private Task ProcessMatchingExpectation(IOwinResponse response, HttpExpectation httpExpectation)
        {
            foreach (var key in httpExpectation.Response.Headers.AllKeys)
                response.Headers.Add(key, new[] { httpExpectation.Response.Headers[key] });

            response.Headers.Add("Content-Type", new[] { "application/json" });
            response.StatusCode = (int)httpExpectation.Response.StatusCode;

            Task.Delay(_expect.ResponseTime).Wait();

            return response.WriteAsync(httpExpectation.Response.ExpectedResult);
        }
    }
}