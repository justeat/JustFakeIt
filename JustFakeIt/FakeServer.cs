using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Hosting;

namespace JustFakeIt
{
    public class FakeServer : IDisposable
    {
        public Uri BaseUri { get; }
        public Expect Expect { get; protected set; }

        public IReadOnlyList<HttpRequestExpectation> CapturedRequests => capturedRequests.ToArray();

        private IDisposable webApp;
        private readonly IList<HttpRequestExpectation> capturedRequests;

        public FakeServer() : this(Ports.GetFreeTcpPort())
        {
        }

        public FakeServer(int basePort)
        {
            BaseUri = new UriBuilder("http", "127.0.0.1", basePort, string.Empty).Uri;
            Expect = new Expect();
            capturedRequests = new List<HttpRequestExpectation>();
        }

        public void Dispose()
        {
            if (webApp != null)
            {
                webApp.Dispose();
                webApp = null;
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
            var host = new WebHostBuilder()
                .UseUrls(BaseUri.AbsoluteUri)
                .Configure(app => app.UseProxyMiddleware(Expect, capturedRequests))
                .UseKestrel()
                .Build();

            webApp = host;

            host.Start();
        }
    }
}
