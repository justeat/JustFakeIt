using System;
using System.Diagnostics;
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
        public async Task FakeServer_ExpectGetReturnsString_ResponseMatchesExpectation()
        {
            const string expectedResult = "Some String Data";

            const string url = "/some-url";

            using (var fakeServer = new FakeServer())
            {
                fakeServer.Expect.Get(url).Returns(expectedResult);
                fakeServer.Start();
                
                var baseAddress = fakeServer.BaseUri;
                var uri = new Uri(baseAddress, url);
                var result = await new HttpClient().GetStringAsync(uri);

                result.Should().Be(expectedResult);
            }
        }

        [Fact]
        public async Task FakeServer_ExpectGetWithResponseHeadersSpecified_ResponseMatchesExpectionAndHasHeaders()
        {
            const string expectedResult = "Some String Data";

            var port = Ports.GetFreeTcpPort();

            var baseAddress = "http://localhost:" + port;

            const string url = "/some-url";

            using (var fakeServer = new FakeServer(port))
            {
                var expectedRequestHeaders = new WebHeaderCollection
                {
                    ["foo"] = "bar"
                };

                fakeServer.Expect.Get(url).Returns(expectedResult, expectedRequestHeaders);
                fakeServer.Start();

                var client = new HttpClient {BaseAddress = new Uri(baseAddress)};
                var result = await client.GetAsync(url);

                result.Content.ReadAsStringAsync().Result.Should().Be(expectedResult);
                result.Headers.Should().ContainSingle(x => x.Key == "foo");
            }
        }

        [Fact]
        public async Task FakeServer_ExpectGetWithRequestHeadersSpecified_WhenRequestHeadersProvided_ResponseMatchesExpection()
        {
            const string expectedResult = "Some String Data";

            var port = Ports.GetFreeTcpPort();

            var baseAddress = "http://localhost:" + port;

            const string url = "/some-url";

            using (var fakeServer = new FakeServer(port))
            {
                var expectedRequestHeaders = new WebHeaderCollection
                {
                    ["X-Dummy1"] = "dummy1val",
                    ["X-Dummy2"] = "dummy2val",
                    ["X-Dummy3"] = "dummy3val"
                };

                fakeServer.Expect.Get(url, expectedRequestHeaders).Returns(expectedResult);
                fakeServer.Start();

                var client = new HttpClient { BaseAddress = new Uri(baseAddress) };

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.TryAddWithoutValidation("X-Dummy1", "dummy1val");
                client.DefaultRequestHeaders.TryAddWithoutValidation("X-Dummy2", "dummy2val");
                client.DefaultRequestHeaders.TryAddWithoutValidation("X-Dummy3", "dummy3val");

                var result = await client.GetAsync(url);

                result.StatusCode.Should().Be(HttpStatusCode.OK);
                result.Content.ReadAsStringAsync().Result.Should().Be(expectedResult);
            }
        }

        [Fact]
        public async Task FakeServer_ExpectGetWithRequestHeadersSpecified_WhenRequestHeadersNotProvided_ResponseIsBadRequest()
        {
            const string expectedResult = "X-Dummy1 header value not as expected.\r\n\tExpected: dummy1val\r\n\tProvided: other1val\r\nX-Dummy2 header was not provided.\r\n";

            var port = Ports.GetFreeTcpPort();

            var baseAddress = "http://localhost:" + port;

            const string url = "/some-url";

            using (var fakeServer = new FakeServer(port))
            {
                var expectedRequestHeaders = new WebHeaderCollection
                {
                    ["X-Dummy1"] = "dummy1val",  // will have different value
                    ["X-Dummy2"] = "dummy2val",  // will be missing
                    ["X-Dummy3"] = "dummy3val"   // will match
                };

                fakeServer.Expect.Get(url, expectedRequestHeaders).Returns(expectedResult);
                fakeServer.Start();

                var client = new HttpClient { BaseAddress = new Uri(baseAddress) };

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.TryAddWithoutValidation("X-Dummy1", "other1val");
                client.DefaultRequestHeaders.TryAddWithoutValidation("X-Dummy3", "dummy3val");

                var result = await client.GetAsync(url);

                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                result.Content.ReadAsStringAsync().Result.Should().Be(expectedResult);
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
                var result = new HttpClient().GetStringAsync(uri).Result;

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
                var stringResult = new HttpClient().GetStringAsync(uri).Result;
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
                var result = new HttpClient()
                    .SendAsync(new HttpRequestMessage(HttpMethod.Post, uri) { Content = new StringContent(string.Empty) })
                    .Result
                    .Content
                    .ReadAsStringAsync()
                    .Result;

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
                var response = new HttpClient()
                    .SendAsync(new HttpRequestMessage(HttpMethod.Post, uri) {Content = new StringContent(string.Empty)})
                    .Result;

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
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
                var response = new HttpClient().GetAsync(uri).Result;

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
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
                var response = new HttpClient()
                    .SendAsync(new HttpRequestMessage(HttpMethod.Post, uri) { Content = new StringContent(string.Empty) })
                    .Result;

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
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
                var result = new HttpClient()
                    .SendAsync(new HttpRequestMessage(HttpMethod.Put, uri) { Content = new StringContent(string.Empty) })
                    .Result
                    .Content
                    .ReadAsStringAsync()
                    .Result;

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
                var result = new HttpClient()
                    .SendAsync(new HttpRequestMessage(HttpMethod.Delete, uri)
                    {
                        Content = new StringContent(string.Empty)
                    })
                    .Result
                    .Content
                    .ReadAsStringAsync()
                    .Result;

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

                var resultA = new HttpClient()
                    .SendAsync(new HttpRequestMessage(HttpMethod.Post, new Uri(baseAddress + fakeurl))
                    {
                        Content = new StringContent("messageA")
                    })
                    .Result
                    .Content
                    .ReadAsStringAsync()
                    .Result;

                var resultB = new HttpClient()
                    .SendAsync(new HttpRequestMessage(HttpMethod.Post, new Uri(baseAddress + fakeurl))
                    {
                        Content = new StringContent("messageB")
                    })
                    .Result
                    .Content
                    .ReadAsStringAsync()
                    .Result;

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
        public void FakeServer_ShouldExecuteResponseExpectationCallback_ReturnExpectedData()
        {
            const string expectedResult = "Some String Data";
            const string baseAddress = "http://localhost:12354";

            const string url = "/some-url?id=1234";

            using (var fakeServer = new FakeServer(12354))
            {
                fakeServer.Expect.Get(url).Callback(() => new HttpResponseExpectation(HttpStatusCode.OK, expectedResult));

                fakeServer.Start();

                var result = new HttpClient().GetStringAsync(new Uri(baseAddress + url)).Result;

                result.Should().Be(expectedResult);
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
                await httpClient.DeleteAsync(new Uri(baseAddress, url1));
                await Repeat(() => httpClient.GetAsync(new Uri(baseAddress, url2)), 2);
                await Repeat(() => httpClient.PostAsync(new Uri(baseAddress, url3), new StringContent(url3)), 3);
                await Repeat(() => httpClient.PutAsync(new Uri(baseAddress, url4), new StringContent(url4)), 4);

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
