using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace JustFakeIt
{
    public class ProxyMiddleware :  OwinMiddleware
    {
        private readonly Expect _expect;

        public ProxyMiddleware(OwinMiddleware next, Expect expect) : base(next)
        {
            _expect = expect;
        }

        public override Task Invoke(IOwinContext context)
        {
            var delayTask = Task.Delay(_expect.ResponseTime);

            Debug.WriteLine("Looking for registration that matches: ");
            Debug.WriteLine("\t\t\tPath:\t\t\t" + context.Request.Uri.PathAndQuery);
            Debug.WriteLine("\t\t\tMethod:\t\t\t" + context.Request.Method);
            Debug.WriteLine("\t\t\tBody:\t\t\t" + context.Request.Body);
            
            var matchingExpectation = 
                _expect.Expectations.FirstOrDefault(e => RequestAndExpectedHttpMethodAndPathsMatch(context, e.Request));

            if (matchingExpectation == null)
            {
                context.Response.StatusCode = 404;
                return context.Response.WriteAsync(new byte[0]);
            }

            foreach (var key in matchingExpectation.Response.Headers.AllKeys)
            {
                // BUG: http allows duplicate headers; this doesn't.
                context.Response.Headers.Add(key, new [] {matchingExpectation.Response.Headers[key]});
            }

            context.Response.Headers.Add("Content-Type", new[] {"application/json"});
            context.Response.StatusCode = (int)matchingExpectation.Response.StatusCode;

            delayTask.Wait();

            return context.Response.WriteAsync(matchingExpectation.Response.ExpectedResult);

        }

        private static bool RequestAndExpectedHttpMethodAndPathsMatch(IOwinContext context, HttpRequestExpectation requestExpectation)
        {
            return
                requestExpectation.MatchesActualPath(context.Request.Uri.GetComponents(UriComponents.PathAndQuery, UriFormat.Unescaped)) &&
                requestExpectation.MatchesActualHttpMethod(context.Request.Method) &&
                requestExpectation.MatchesActualBody(context.Request.Body);
        }
    }
}