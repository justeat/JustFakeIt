using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Owin.Hosting;
using Owin;

namespace JustFakeIt
{
    public class FakeServer : IDisposable
    {
        private const int Retries = 5;

        private static readonly object ListeningCreationLock = new object();

        private Uri GetBaseUri()
        {
            var port = _basePort ?? Ports.GetFreeTcpPort();
            return new UriBuilder(Uri.UriSchemeHttp, "127.0.0.1", port).Uri;
        }

        private IDisposable _webApp;
        private readonly IList<HttpRequestExpectation> _capturedRequests;
        private int? _basePort;

        public Expect Expect { get; protected set; }

        public IReadOnlyList<HttpRequestExpectation> CapturedRequests => _capturedRequests.ToArray();

        public Uri BaseUri
        {
            get
            {
                if (_basePort.HasValue) return GetBaseUri();
                throw new InvalidOperationException("Cannot give you the base URL before server has started");
            }
        }

        public FakeServer()
        {
            Expect = new Expect();
            _capturedRequests = new List<HttpRequestExpectation>();
        }

        public FakeServer(int basePort) : this()
        {
            _basePort = basePort;
        }

        public void Start()
        {
            _basePort = Retry(Retries, () => DoWithLock(StartAndReturnPort));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
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

        private T DoWithLock<T>(Func<T> action)
        {
            lock (ListeningCreationLock)
            {
                return action();
            }
        }

        private T Retry<T>(int times, Func<T> action)
        {
            Exception exception = null;
            for (var i = 0; i < times; i++)
            {
                try
                {
                    return action();
                }
                catch (Exception e)
                {
                    exception = e;
                }
            }
            throw exception ?? new InvalidOperationException("Something went horribly wrong");
        }

        private int StartAndReturnPort()
        {
            var uri = GetBaseUri();
            _webApp = WebApp.Start(uri.ToString(), app => app.Use<ProxyMiddleware>(Expect, _capturedRequests));
            return uri.Port;
        }
    }
}
