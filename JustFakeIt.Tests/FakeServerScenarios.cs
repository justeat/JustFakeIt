using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace JustFakeIt.Tests
{
    public class FakeServerScenarios
    {
        [Fact]
        public void FakeServer_NewWebClientCreated_ProxyShouldBeConfigured()
        {
            using (var fakeServer = new FakeServer(12354))
            {
                fakeServer.Start();

                var wc = new WebClient();
                var defaultWebProxyForUri = WebRequest.DefaultWebProxy.GetProxy(new Uri("http://www.google.com/some-url2"));

                defaultWebProxyForUri.Should().Be(fakeServer.BaseUri);
                ((WebProxy) wc.Proxy).Address.Should().Be(fakeServer.BaseUri);
            }
        }

        [Fact]
        public void FakeServer_RequestingAUriThatHasNotBeenModified_ResponseMatchesExpectation()
        {
            const string expectedResult = "Some String Data";

            using (var fakeServer = new FakeServer(12354))
            {
                fakeServer.Expect.Get("/123").Returns(expectedResult);
                fakeServer.Start();
                
                var result = new WebClient().DownloadString(new Uri("http://www.bing.com/123"));

                result.Should().Be(expectedResult);
            }
        }

        [Fact]
        public void FakeServer_ExpectGetReturnsString_ResponseMatchesExpectation()
        {
            const string expectedResult = "Some String Data";

            var port = Ports.GetFreeTcpPort();

            var baseAddress = "http://localhost:" + port;
            
            const string url = "/some-url";

            using (var fakeServer = new FakeServer(port))
            {
                fakeServer.Expect.Get(url).Returns(expectedResult);
                fakeServer.Start();

                var result = new WebClient().DownloadString(new Uri(baseAddress + url));

                result.Should().Be(expectedResult);
            }
        }

        [Fact]
        public void FakeServer_ExpectGetWithHeadersSpecified_ResponseMatchesExpectionAndHasHeaders()
        {
            const string expectedResult = "Some String Data";

            var port = Ports.GetFreeTcpPort();

            var baseAddress = "http://localhost:" + port;

            const string url = "/some-url";

            using (var fakeServer = new FakeServer(port))
            {
                fakeServer.Expect.Get(url).Returns(expectedResult, new WebHeaderCollection { { "foo", "bar" } });
                fakeServer.Start();

                var client = new HttpClient {BaseAddress = new Uri(baseAddress)};
                var result = client.GetAsync(url).Result;

                result.Content.ReadAsStringAsync().Result.Should().Be(expectedResult);
                result.Headers.Should().ContainSingle(x => x.Key == "foo");
            }
        }

        [Fact]
        public void FakeServer_ExpectGetWithQueryParametersReturnsString_ResponseMatchesExpectation()
        {
            const string expectedResult = "Some String Data";
            const string baseAddress = "http://localhost:12354";
            
            const string url = "/some-url?id=1234";

            using (var fakeServer = new FakeServer(12354))
            {
                fakeServer.Expect.Get(url).Returns(expectedResult);

                fakeServer.Start();

                var result = new WebClient().DownloadString(new Uri(baseAddress + url));

                result.Should().Be(expectedResult);
            }
        }

        [Fact]
        public void FakeServer_ExpectGetReturnsObject_ResponseMatchesExpectation()
        {
            var expectedResult = new { RestaurantId = 1234 };
            const string baseAddress = "http://localhost:12354";

            const string url = "/restaurant/1234";

            using (var fakeServer = new FakeServer(12354))
            {
                fakeServer.Expect.Get(url).Returns(expectedResult);

                fakeServer.Start();

                var result = JsonConvert.DeserializeObject<dynamic>(new WebClient().DownloadString(new Uri(baseAddress + url)));

                Assert.Equal(expectedResult.RestaurantId, (int)result.RestaurantId);
            }
        }

        [Fact]
        public void FakeServer_ExpectPostWithNoBodyReturnsString_ResponseMatchesExpectation()
        {
            const string expectedResult = "Some String Data";
            const string baseAddress = "http://localhost:12354";

            const string url = "/some-url";

            using (var fakeServer = new FakeServer(12354))
            {
                fakeServer.Expect.Post(url, string.Empty).Returns(expectedResult);

                fakeServer.Start();

                var result = new WebClient().UploadString(new Uri(baseAddress + url), string.Empty);

                result.Should().Be(expectedResult);
            }
        }

        [Fact]
        public void FakeServer_ExpectPostWithMismatchingBody_Returns404()
        {
            const string expectedResult = "Some String Data";
            const string baseAddress = "http://localhost:12354";

            const string url = "/some-url";

            using (var fakeServer = new FakeServer(12354))
            {
                fakeServer.Expect.Post(url, "jibberish").Returns(expectedResult);

                fakeServer.Start();

                var ex = Assert.Throws<WebException>(() => new WebClient().UploadString(new Uri(baseAddress + url), string.Empty));

                ((HttpWebResponse)ex.Response).StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public void FakeServer_ExpectGetWithMismatchingPath_Returns404()
        {
            const string expectedResult = "Some String Data";
            const string baseAddress = "http://localhost:12354";

            using (var fakeServer = new FakeServer(12354))
            {
                fakeServer.Expect.Get("/some-jibberish-url").Returns(expectedResult);

                fakeServer.Start();

                var ex = Assert.Throws<WebException>(() => new WebClient().DownloadString(new Uri(baseAddress + "/home")));

                ((HttpWebResponse)ex.Response).StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public void FakeServer_ExpectGetWithMismatchingMethod_Returns404()
        {
            const string expectedResult = "Some String Data";
            const string baseAddress = "http://localhost:12354";
            const string path = "/some-url";

            using (var fakeServer = new FakeServer(12354))
            {
                fakeServer.Expect.Get(path).Returns(expectedResult);

                fakeServer.Start();

                var ex = Assert.Throws<WebException>(() => new WebClient().UploadString(new Uri(baseAddress + path), string.Empty));

                ((HttpWebResponse)ex.Response).StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public void FakeServer_ExpectPutWithNoBodyReturnsString_ResponseMatchesExpectation()
        {
            const string expectedResult = "Some String Data";
            const string baseAddress = "http://localhost:12354";

            const string url = "/some-url";

            using (var fakeServer = new FakeServer(12354))
            {
                fakeServer.Expect.Put(url, string.Empty).Returns(expectedResult);

                fakeServer.Start();

                var result = new WebClient().UploadString(new Uri(baseAddress + url), "PUT", string.Empty);

                result.Should().Be(expectedResult);
            }
        }

        [Fact]
        public void FakeServer_ExpectDeleteReturnsString_ResponseMatchesExpectation()
        {
            const string expectedResult = "Some String Data";
            const string baseAddress = "http://localhost:12354";

            const string url = "/some-url";

            using (var fakeServer = new FakeServer(12354))
            {
                fakeServer.Expect.Delete(url).Returns(expectedResult);
                fakeServer.Start();

                var result = new WebClient().UploadString(new Uri(baseAddress + url), "DELETE", string.Empty);

                result.Should().Be(expectedResult);
            }
        }

        [Fact]
        public void FakeServer_ExpectPutWithNoBodyReturns201_Returns201()
        {
            const string expectedResult = "Some String Data";
            const string baseAddress = "http://localhost:12354";

            const string url = "/some-url";

            using (var fakeServer = new FakeServer(12354))
            {
                fakeServer.Expect.Put(url, string.Empty).Returns(HttpStatusCode.Created, expectedResult);
                fakeServer.Start();

                var t = new HttpClient().PutAsync(new Uri(baseAddress + url), new StringContent(String.Empty));
                t.Wait();
                var result = t.Result;
                
                result.StatusCode.Should().Be(HttpStatusCode.Created);
            }
        }

        [Fact]
        public void FakeServer_ExpectPutWithComplexBodyReturnsComplexObject()
        {
            const string expectedResult = "{\"Complex\":{\"Property1\":1,\"Property2\":true}}";
            const string body = "{\"Complex\":1}";

            using (var fakeServer = new FakeServer(12354))
            {
                fakeServer.Expect.Put("/some-url", body).Returns(HttpStatusCode.Created, expectedResult);
                fakeServer.Start();

                var t = new HttpClient().PutAsync(new Uri("http://localhost:12354" + "/some-url"), new StringContent(body));
                t.Wait();
                var result = t.Result;

                var resultTask = result.Content.ReadAsStringAsync();
                resultTask.Wait();

                result.StatusCode.Should().Be(HttpStatusCode.Created);
                resultTask.Result.Should().Be(expectedResult);
            }
        }

        [Fact]
        public void FakeServer_ExpectPutWithObjectBodyReturns201_Returns201()
        {
            var expectedResult = new {RestaurantId = 1234};
            const string baseAddress = "http://localhost:12354";

            const string url = "/some-url";

            using (var fakeServer = new FakeServer(12354))
            {
                fakeServer.Expect.Put(url, string.Empty).Returns(HttpStatusCode.Created, expectedResult);
                fakeServer.Start();

                var t = new HttpClient().PutAsync(new Uri(baseAddress + url), new StringContent(String.Empty));
                t.Wait();
                var result = t.Result;

                result.StatusCode.Should().Be(HttpStatusCode.Created);
            }
        }

    }
}
