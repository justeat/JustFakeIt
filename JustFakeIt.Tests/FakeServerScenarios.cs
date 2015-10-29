using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
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
                var uri = new Uri(baseAddress + url);
                var result = new WebClient().DownloadString(uri);

                result.Should().Be(expectedResult);
            }
        }

        [Fact]
        public async Task FakeServer_ExpectGetWithHeadersSpecified_ResponseMatchesExpectionAndHasHeaders()
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
                var result = await client.GetAsync(url);

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

                var uri = new Uri(baseAddress + url);
                var result = new WebClient().DownloadString(uri);

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

                var uri = new Uri(baseAddress + url);
                var stringResult = new WebClient().DownloadString(uri);
                var result = JsonConvert.DeserializeObject<dynamic>(stringResult);

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

                var uri = new Uri(baseAddress + url);
                var result = new WebClient().UploadString(uri, string.Empty);

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

                var uri = new Uri(baseAddress + url);
                var ex = Assert.Throws<WebException>(() => new WebClient().UploadString(uri, string.Empty));

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

                var uri = new Uri(baseAddress + "/home");
                var ex = Assert.Throws<WebException>(() => new WebClient().DownloadString(uri));

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

                var uri = new Uri(baseAddress + path);
                var ex = Assert.Throws<WebException>(() => new WebClient().UploadString(uri, string.Empty));

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

                var uri = new Uri(baseAddress + url);
                var result = new WebClient().UploadString(uri, "PUT", string.Empty);

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

                var uri = new Uri(baseAddress + url);
                var result = new WebClient().UploadString(uri, "DELETE", string.Empty);

                result.Should().Be(expectedResult);
            }
        }

        [Fact]
        public async Task FakeServer_ExpectPutWithNoBodyReturns201_Returns201()
        {
            const string expectedResult = "Some String Data";
            const string baseAddress = "http://localhost:12354";

            const string url = "/some-url";

            using (var fakeServer = new FakeServer(12354))
            {
                fakeServer.Expect.Put(url, string.Empty).Returns(HttpStatusCode.Created, expectedResult);
                fakeServer.Start();

                var uri = new Uri(baseAddress + url);
                var result = await new HttpClient().PutAsync(uri, new StringContent(String.Empty));
                
                result.StatusCode.Should().Be(HttpStatusCode.Created);
            }
        }

        [Fact]
        public async Task FakeServer_ExpectPutWithComplexBodyReturnsComplexObject()
        {
            const string expectedResult = "{\"Complex\":{\"Property1\":1,\"Property2\":true}}";
            const string body = "{\"Complex\":1}";

            using (var fakeServer = new FakeServer(12354))
            {
                fakeServer.Expect.Put("/some-url", body).Returns(HttpStatusCode.Created, expectedResult);
                fakeServer.Start();

                var uri = new Uri("http://localhost:12354" + "/some-url");
                var result = await new HttpClient().PutAsync(uri, new StringContent(body));

                var content = await result.Content.ReadAsStringAsync();

                result.StatusCode.Should().Be(HttpStatusCode.Created);
                content.Should().Be(expectedResult);
            }
        }

        [Fact]
        public async Task FakeServer_ExpectPutWithObjectBodyReturns201_Returns201()
        {
            var expectedResult = new {RestaurantId = 1234};
            const string baseAddress = "http://localhost:12354";

            const string url = "/some-url";

            using (var fakeServer = new FakeServer(12354))
            {
                fakeServer.Expect.Put(url, string.Empty).Returns(HttpStatusCode.Created, expectedResult);
                fakeServer.Start();

                var uri = new Uri(baseAddress + url);
                var result = await new HttpClient().PutAsync(uri, new StringContent(String.Empty));

                result.StatusCode.Should().Be(HttpStatusCode.Created);
            }
        }

        [Fact]
        public async Task FakeServer_IgnoredParameter_Returns200()
        {
            var expectedResult = new { ResourceId = 1234 };
            const string baseAddress = "http://localhost:12354";

            const string fakeurl = "/some-resource/{ignore}/some-resource?date={ignore}&type={ignore}";
            const string actualurl = "/some-resource/1234/some-resource?date=2015-02-06T09:52:10&type=1";

            using (var fakeServer = new FakeServer(12354))
            {
                fakeServer.Expect.Get(fakeurl).Returns(HttpStatusCode.Accepted, expectedResult);
                fakeServer.Start();

                var uri = new Uri(baseAddress + actualurl);
                var result = await new HttpClient().GetAsync(uri);

                result.StatusCode.Should().Be(HttpStatusCode.Accepted);
            }
        }

        [Fact]
        public async Task FakeServer_IgnoredParameterInRestfulUrl_Returns200()
        {
            var expectedResult = new { ResourceId = 1234 };
            const string baseAddress = "http://localhost:12354";

            const string fakeUrl = "/some-resource/{ignore}/some-method";
            const string actualUrl = "/some-resource/1234/some-method";

            using (var fakeServer = new FakeServer(12354))
            {
                fakeServer.Expect.Get(fakeUrl).Returns(HttpStatusCode.Accepted, expectedResult);
                fakeServer.Start();

                var uri = new Uri(baseAddress + actualUrl);
                var result = await new HttpClient().GetAsync(uri);

                result.StatusCode.Should().Be(HttpStatusCode.Accepted);
            }            
        }

        [Fact]
        public void FakeServer_ShouldHandleMultipleRegistrationOnSameEndPoint_WithDifferentBodies_ReturnExpectedData()
        {
            var expectedResultA = "1234";
            var expectedResultB = "5678";

            const string baseAddress = "http://localhost:12354";
            const string fakeurl = "/some-url";

            using (var fakeServer = new FakeServer(12354))
            {
                fakeServer.Expect.Post(fakeurl, "messageA").Returns(HttpStatusCode.OK, expectedResultA);
                fakeServer.Expect.Post(fakeurl, "messageB").Returns(HttpStatusCode.OK, expectedResultB);

                fakeServer.Start();

                var resultA = new WebClient().UploadString(new Uri(baseAddress + fakeurl), "POST", "messageA");
                var resultB = new WebClient().UploadString(new Uri(baseAddress + fakeurl), "POST", "messageB");

                resultA.Should().Be(expectedResultA);
                resultB.Should().Be(expectedResultB);
            }
        }

        [Fact]
        public async Task FakeServer_ExpectGetToAnEndpointWithAFiveSecondResponseTime_ResponseTimeIsGreaterThanFiveSeconds()
        {
            var expectedResult = new { ResourceId = 1234 };
            const string baseAddress = "http://localhost:12354";
            var expectedResponseTime = TimeSpan.FromSeconds(5);

            const string fakeUrl = "/some-url";
            
            using (var fakeServer = new FakeServer(12354))
            {
                fakeServer.Expect.ResponseTime = expectedResponseTime;
                fakeServer.Expect.Get(fakeUrl).Returns(HttpStatusCode.OK, expectedResult);
                fakeServer.Start();

                var stopwatch = Stopwatch.StartNew();

                var uri = new Uri(baseAddress + fakeUrl);
                await new HttpClient().GetAsync(uri);

                stopwatch.Stop();

                stopwatch.Elapsed.Should().BeGreaterOrEqualTo(expectedResponseTime);
            }
        }

        [Fact]
        public async Task FakeServer_CapturesAllRequests()
        {
            using (var fakeServer = new FakeServer())
            {
                fakeServer.Start();
                var baseAddress = fakeServer.BaseUri;
                
                var url1 = "/request1";
                var url2 = "/request2";
                var url3 = "/request3";
                var url4 = "/request4";

                var httpClient = new HttpClient();
                await httpClient.DeleteAsync(baseAddress + url1);
                await Repeat(() => httpClient.GetAsync(baseAddress + url2), 2);
                await Repeat(() => httpClient.PostAsync(baseAddress + url3, new StringContent(url3)), 3);
                await Repeat(() => httpClient.PutAsync(baseAddress + url4, new StringContent(url4)), 4);

                fakeServer.CapturedRequests.Count(x => x.Method == Http.Delete && x.Url == url1).Should().Be(1);
                fakeServer.CapturedRequests.Count(x => x.Method == Http.Get && x.Url == url2).Should().Be(2);
                fakeServer.CapturedRequests.Count(x => x.Method == Http.Post && x.Url == url3 && x.Body == url3).Should().Be(3);
                fakeServer.CapturedRequests.Count(x => x.Method == Http.Put && x.Url == url4 && x.Body == url4).Should().Be(4);
            }
        }

        private async Task Repeat(Func<Task> a, int times)
        {
            for (var i = 0; i < times; i++)
            {
                await a();
            }
        }
    }
}
