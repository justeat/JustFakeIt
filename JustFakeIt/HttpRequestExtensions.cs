using System;
using Microsoft.AspNetCore.Http;

namespace JustFakeIt
{
    internal static class HttpRequestExtensions
    {
        public static Uri GetUri(this HttpRequest request)
        {
            var hostComponents = request.Host.ToUriComponent().Split(':');

            var url = new UriBuilder
            {
                Path = request.Path,
                Query = request.QueryString.ToUriComponent(),
                Scheme = request.Scheme,
                Host = hostComponents[0]
            };

            return url.Uri;
        }
    }
}
