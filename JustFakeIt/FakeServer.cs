using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Owin.Hosting;
using Owin;
using System.Net.Http;

namespace JustFakeIt
{
    public class FakeServer : IDisposable
    {
        public Uri BaseUri { get; private set; }
        public Expect Expect { get; protected set; }
        public HttpClient Client { get; private set; }

        public IReadOnlyList<HttpRequestExpectation> CapturedRequests
        {
            get { return _capturedRequests.ToArray(); }
        }

        private IDisposable _webApp;
        private readonly IList<HttpRequestExpectation> _capturedRequests;

        public FakeServer() : this(Ports.GetFreeTcpPort())
        {
        }

        public FakeServer(int basePort)
        {
            BaseUri = new UriBuilder(Uri.UriSchemeHttp, "127.0.0.1", basePort).Uri;
            Expect = new Expect();
            _capturedRequests = new List<HttpRequestExpectation>();
        }
        public void Dispose()
        {
            if (_webApp != null)
            {
                _webApp.Dispose();
                _webApp = null;
            }
            
            // Owin Hosting adds trace listeners to the Trace
            // just removing them here to keep the environment clean
            var hostingTraceListeners = Trace.Listeners.Cast<TraceListener>()
                .Where(listener => listener.Name == "HostingTraceListener")
                .ToList();

            hostingTraceListeners.ForEach(x => Trace.Listeners.Remove(x));
        }

        public void Start()
        {
            _webApp = WebApp.Start(BaseUri.ToString(), app => app.Use<ProxyMiddleware>(Expect, _capturedRequests));
            Client = new HttpClient();
            Client.BaseAddress = BaseUri;
        }
    }
}
