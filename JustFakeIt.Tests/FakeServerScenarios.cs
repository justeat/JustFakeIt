using System;
using System.Diagnostics;
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
        public void FakeServer_ExpectGetReturnsString_ResponseMatchesExpectation()
        {
            const string expectedResult = "Some String Data";

            const string url = "/some-url";

            using (var fakeServer = new FakeServer())
            {
                fakeServer.Expect.Get(url).Returns(expectedResult);
                fakeServer.Start();
                
                var baseAddress = fakeServer.BaseUri;
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


        [Fact]
        public void FakeServer_IgnoredParameter_Returns200()
        {
            var expectedResult = new { ResourceId = 1234 };
            const string baseAddress = "http://localhost:12354";

            const string fakeurl = "/some-resource/{ignore}/some-resource?date={ignore}&type={ignore}";
            const string actualurl = "/some-resource/1234/some-resource?date=2015-02-06T09:52:10&type=1";

            using (var fakeServer = new FakeServer(12354))
            {
                fakeServer.Expect.Get(fakeurl).Returns(HttpStatusCode.Accepted, expectedResult);
                fakeServer.Start();

                var t = new HttpClient().GetAsync(new Uri(baseAddress + actualurl));
                t.Wait();
                var result = t.Result;

                result.StatusCode.Should().Be(HttpStatusCode.Accepted);
            }
        }

        [Fact]
        public void FakeServer_ExpectGetToAnEndpointWithAFiveSecondResponseTime_ResponseTimeIsGreaterThanFiveSeconds()
        {
            var expectedResult = new { ResourceId = 1234 };
            const string baseAddress = "http://localhost:12354";
            var expectedResponseTime = TimeSpan.FromSeconds(5);

            const string fakeurl = "/some-url";
            
            using (var fakeServer = new FakeServer(12354))
            {
                fakeServer.Expect.ResponseTime = expectedResponseTime;
                fakeServer.Expect.Get(fakeurl).Returns(HttpStatusCode.OK, expectedResult);
                fakeServer.Start();

                var stopwatch = Stopwatch.StartNew();

                var t = new HttpClient().GetAsync(new Uri(baseAddress) + fakeurl);
                t.Wait();

                stopwatch.Stop();

                stopwatch.Elapsed.Should().BeGreaterOrEqualTo(expectedResponseTime);
            }
        }

    }
}
