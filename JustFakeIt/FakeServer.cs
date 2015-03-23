using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Owin.Hosting;
using Owin;

namespace JustFakeIt
{
    public class FakeServer : IDisposable
    {
        public Uri BaseUri { get; private set; }
        public Expect Expect { get; protected set; }

        private IDisposable _webApp;

        public FakeServer() : this(Ports.GetFreeTcpPort())
        {
        }

        public FakeServer(int basePort)
        {
            BaseUri = new UriBuilder(Uri.UriSchemeHttp, "127.0.0.1", basePort).Uri;
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
        }

        public void Start()
        {
            _webApp = WebApp.Start(BaseUri.ToString(), app => app.Use<ProxyMiddleware>(Expect));
        }
    }
}
