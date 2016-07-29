using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;

namespace JustFakeIt
{
    internal static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseProxyMiddleware(
            this IApplicationBuilder app,
            Expect expect,
            IList<HttpRequestExpectation> expectationsList)
        {
            return app.UseMiddleware<ProxyMiddleware>(expect, expectationsList);
        }
    }
}
