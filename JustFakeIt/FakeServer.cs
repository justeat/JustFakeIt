using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Cache;
using Microsoft.Owin.Hosting;
using Owin;

namespace JustFakeIt
{
    public class FakeServer : IDisposable
    {
        public Uri BaseUri { get; private set; }
        public Expect Expect { get; protected set; }

        private IDisposable _webApp;
        private IWebProxy _initialProxy;
        private RequestCachePolicy _initialCachePolicy;

        public FakeServer(int basePort)
        {
            BaseUri = new Uri("http://127.0.0.1:" + basePort);
            Expect = new Expect();
        }

        public void Dispose()
        {
            if(_webApp != null)
                _webApp.Dispose();
            
            // Owin Hosting adds trace listeners to the Trace
            // just removing them here to keep the environment clean
            Trace.Listeners.Cast<TraceListener>()
                .Where(listener => listener.Name == "HostingTraceListener")
                .ToList()
                .ForEach(x => Trace.Listeners.Remove(x));

            WebRequest.DefaultWebProxy = _initialProxy;
            WebRequest.DefaultCachePolicy = _initialCachePolicy;
        }

        public void Start()
        {
            _webApp = WebApp.Start(BaseUri.ToString(), app => app.Use<ProxyMiddleware>(Expect));

            _initialProxy = WebRequest.DefaultWebProxy;
            _initialCachePolicy = WebRequest.DefaultCachePolicy;

            WebRequest.DefaultCachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
            WebRequest.DefaultWebProxy = new WebProxy(BaseUri, false)
            {
                UseDefaultCredentials = true,
                BypassList = new string[] {},
                Credentials = CredentialCache.DefaultCredentials,
            };
        }
    }
}
