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
        public async Task FakeServer_ExpectGetReturnsString_ResponseMatchesExpectation()
        {
            const string expectedResult = "Some String Data";

            const string path = "/some-path";

            using (var fakeServer = new FakeServer())
            {
                fakeServer.Expect.Get(path).Returns(expectedResult);
                fakeServer.Start();

                var result = await fakeServer.Client.GetStringAsync(path);
                result.Should().Be(expectedResult);
            }
        }

        [Fact]
        public async Task FakeServer_ExpectGetWithResponseHeadersSpecified_ResponseMatchesExpectionAndHasHeaders()
        {
            const string expectedResult = "Some String Data";

            const string path = "/some-path";

            using (var fakeServer = new FakeServer())
            {
                fakeServer.Expect.Get(path).Returns(expectedResult, new WebHeaderCollection { { "foo", "bar" } });
                fakeServer.Start();

                var result = await fakeServer.Client.GetAsync(path);

                result.Content.ReadAsStringAsync().Result.Should().Be(expectedResult);
                result.Headers.Should().ContainSingle(x => x.Key == "foo");
            }
        }

        [Fact]
        public async Task FakeServer_ExpectGetWithResponseHeadersSpecifiedFluently_ResponseMatchesExpectionAndHasHeaders()
        {
            const string expectedResult = "Some String Data";

            const string path = "/some-path";

            using (var fakeServer = new FakeServer())
            {
                fakeServer.Expect.Get(path).Returns(expectedResult).WithHeader("Content-Language", "es").WithHeader("Some-Header", "Header-McHeaderFace");
                fakeServer.Start();

                var result = await fakeServer.Client.GetAsync(path);

                result.Content.ReadAsStringAsync().Result.Should().Be(expectedResult);
                result.Headers.Should().ContainSingle(x => x.Key == "Some-Header");
            }
        }

        [Fact]
        public async Task FakeServer_ExpectGetWithRequestHeadersSpecified_WhenRequestHeadersProvided_ResponseMatchesExpection()
        {
            const string expectedResult = "Some String Data";


            const string path = "/some-path";

            using (var fakeServer = new FakeServer())
            {
                var expectedRequestHeaders = new WebHeaderCollection
                {
                    {"X-Dummy1", "dummy1val"},
                    {"X-Dummy2", "dummy2val"},
                    {"X-Dummy3", "dummy3val"}
                };

                fakeServer.Expect.Get(path, expectedRequestHeaders).Returns(expectedResult);
                fakeServer.Start();

                var client = fakeServer.Client;

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.TryAddWithoutValidation("X-Dummy1", "dummy1val");
                client.DefaultRequestHeaders.TryAddWithoutValidation("X-Dummy2", "dummy2val");
                client.DefaultRequestHeaders.TryAddWithoutValidation("X-Dummy3", "dummy3val");

                var result = await client.GetAsync(path);

                result.StatusCode.Should().Be(HttpStatusCode.OK);
                result.Content.ReadAsStringAsync().Result.Should().Be(expectedResult);
            }
        }

        [Fact]
        public async Task FakeServer_ExpectGetWithRequestHeadersSpecified_WhenRequestHeadersNotProvided_ResponseIsBadRequest()
        {
            const string expectedResult = "X-Dummy1 header value not as expected.\r\n\tExpected: dummy1val\r\n\tProvided: other1val\r\nX-Dummy2 header was not provided.\r\n";


            const string path = "/some-path";

            using (var fakeServer = new FakeServer())
            {
                var expectedRequestHeaders = new WebHeaderCollection
                {
                    {"X-Dummy1", "dummy1val"},  // will have different value
                    {"X-Dummy2", "dummy2val"},  // will be missing
                    {"X-Dummy3", "dummy3val"}   // will match
                };

                fakeServer.Expect.Get(path, expectedRequestHeaders).Returns(expectedResult);
                fakeServer.Start();

                var client = fakeServer.Client;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.TryAddWithoutValidation("X-Dummy1", "other1val");
                client.DefaultRequestHeaders.TryAddWithoutValidation("X-Dummy3", "dummy3val");

                var result = await client.GetAsync(path);

                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                result.Content.ReadAsStringAsync().Result.Should().Be(expectedResult);
            }
        }

        [Fact]
        public async Task FakeServer_ExpectGetWithQueryParametersReturnsString_ResponseMatchesExpectation()
        {
            const string expectedResult = "Some String Data";
            
            const string path = "/some-path?id=1234";

            using (var fakeServer = new FakeServer())
            {
                fakeServer.Expect.Get(path).Returns(expectedResult);

                fakeServer.Start();

                var result = await fakeServer.Client.GetStringAsync(path);

                result.Should().Be(expectedResult);
            }
        }

        [Fact]
        public async Task FakeServer_ExpectGetReturnsObject_ResponseMatchesExpectation()
        {
            var expectedResult = new { RestaurantId = 1234 };

            const string path = "/restaurant/1234";

            using (var fakeServer = new FakeServer())
            {
                fakeServer.Expect.Get(path).Returns(expectedResult);

                fakeServer.Start();

                var resp = await fakeServer.Client.GetStringAsync(path);
                var result = JsonConvert.DeserializeObject<dynamic>(resp);

                Assert.Equal(expectedResult.RestaurantId, (int)result.RestaurantId);
            }
        }

        [Fact]
        public async Task FakeServer_ExpectPostWithNoBodyReturnsString_ResponseMatchesExpectation()
        {
            const string expectedResult = "Some String Data";

            const string path = "/some-path";

            var content = new StringContent(string.Empty);

            using (var fakeServer = new FakeServer())
            {
                fakeServer.Expect.Post(path, string.Empty).Returns(expectedResult);

                fakeServer.Start();

                var resp = await fakeServer.Client.PostAsync(path, content);
                var result = await resp.Content.ReadAsStringAsync();

                result.Should().Be(expectedResult);
            }
        }

        [Fact]
        public async Task FakeServer_ExpectPostWithMismatchingBody_Returns404()
        {
            const string expectedResult = "Some String Data";

            const string path = "/some-path";

            using (var fakeServer = new FakeServer())
            {
                fakeServer.Expect.Post(path, "jibberish").Returns(expectedResult);

                fakeServer.Start();

                var resp = await fakeServer.Client.PostAsync(path, new StringContent(string.Empty));

                resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task FakeServer_ExpectGetWithMismatchingPath_Returns404()
        {
            const string expectedResult = "Some String Data";

            using (var fakeServer = new FakeServer())
            {
                fakeServer.Expect.Get("/some-jibberish-path").Returns(expectedResult);

                fakeServer.Start();

                var resp = await fakeServer.Client.GetAsync("/home");

                resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task FakeServer_ExpectGetWithMismatchingMethod_Returns404()
        {
            const string expectedResult = "Some String Data";
            const string path = "/some-path";

            using (var fakeServer = new FakeServer())
            {
                fakeServer.Expect.Get(path).Returns(expectedResult);

                fakeServer.Start();

                var resp = await fakeServer.Client.PostAsync(path, new StringContent(string.Empty));

                resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task FakeServer_ExpectPutWithNoBodyReturnsString_ResponseMatchesExpectation()
        {
            const string expectedResult = "Some String Data";

            const string path = "/some-path";

            using (var fakeServer = new FakeServer())
            {
                fakeServer.Expect.Put(path, string.Empty).Returns(expectedResult);

                fakeServer.Start();

                var resp = await fakeServer.Client.PutAsync(path, new StringContent(string.Empty));
                var result = await resp.Content.ReadAsStringAsync();

                result.Should().Be(expectedResult);
            }
        }

        [Fact]
        public async Task FakeServer_ExpectDeleteReturnsString_ResponseMatchesExpectation()
        {
            const string expectedResult = "Some String Data";

            const string path = "/some-path";

            using (var fakeServer = new FakeServer())
            {
                fakeServer.Expect.Delete(path).Returns(expectedResult);
                fakeServer.Start();

                var resp = await fakeServer.Client.DeleteAsync(path);
                var result = await resp.Content.ReadAsStringAsync();

                result.Should().Be(expectedResult);
            }
        }

        [Fact]
        public async Task FakeServer_ExpectGetReturnsGeneratedTemplateFromPath_ResponseMatchesTemplate()
        {
            const string path = "/some-path";

            using (var fakeServer = new FakeServer())
            {
                fakeServer.Expect.Get(path)
                    .ReturnsFromTemplate(@"TestData\TestTemplate.json", new { Id = 2343, UserId = 2343, UserEmail = "mick.hucknall@just-eat.com" })
                    .RespondsIn(TimeSpan.FromSeconds(1))
                    .WithHttpStatus(HttpStatusCode.Accepted);

                fakeServer.Start();

                var result = await fakeServer.Client.GetStringAsync(path);

                dynamic deserialised = JsonConvert.DeserializeObject(result);

                ((int)deserialised.id).Should().Be(2343);
                ((int)deserialised.userId).Should().Be(2343);
                ((string)deserialised.userEmail).Should().Be("mick.hucknall@just-eat.com");
            }
        }

        [Fact]
        public async Task FakeServer_ExpectGetReturnsFileContent_ResponseMatchesExpectation()
        {
            const string path = "/some-path";

            using (var fakeServer = new FakeServer())
            {
                fakeServer.Expect.Get(path)
                    .ReturnsFromFile(@"TestData\TestResponse.json");
                
                fakeServer.Start();

                var result = await fakeServer.Client.GetStringAsync(path);

                dynamic deserialised = JsonConvert.DeserializeObject(result);

                ((string)deserialised.name).Should().Be("Mick Hucknall");
            }
        }

        [Fact]
        public async Task FakeServer_ExpectPutWithNoBodyReturns201_Returns201()
        {
            const string expectedResult = "Some String Data";

            const string path = "/some-path";

            using (var fakeServer = new FakeServer())
            {
                fakeServer.Expect.Put(path, string.Empty).Returns(HttpStatusCode.Created, expectedResult);
                fakeServer.Start();

                var result = await fakeServer.Client.PutAsync(path, new StringContent(string.Empty));
                
                result.StatusCode.Should().Be(HttpStatusCode.Created);
            }
        }

        [Fact]
        public async Task FakeServer_ExpectPutWithNoBodyReturns2011_Returns201()
        {
            const string expectedResult = "Some String Data";

            const string path = "/some-path";

            using (var fakeServer = new FakeServer())
            {
                fakeServer.Expect.Put(path, string.Empty).Returns(expectedResult).WithHttpStatus(HttpStatusCode.Created);
                fakeServer.Start();

                var result = await fakeServer.Client.PutAsync(path, new StringContent(string.Empty));

                result.StatusCode.Should().Be(HttpStatusCode.Created);
            }
        }

        [Fact]
        public async Task FakeServer_ExpectPutWithComplexBodyReturnsComplexObject()
        {
            const string expectedResult = "{\"Complex\":{\"Property1\":1,\"Property2\":true}}";
            const string body = "{\"Complex\":1}";

            using (var fakeServer = new FakeServer())
            {
                fakeServer.Expect.Put("/some-path", body).Returns(HttpStatusCode.Created, expectedResult);
                fakeServer.Start();

                var result = await fakeServer.Client.PutAsync("/some-path", new StringContent(body));

                var content = await result.Content.ReadAsStringAsync();

                result.StatusCode.Should().Be(HttpStatusCode.Created);
                content.Should().Be(expectedResult);
            }
        }

        [Fact]
        public async Task FakeServer_ExpectPutWithObjectBodyReturns201_Returns201()
        {
            var expectedResult = new {RestaurantId = 1234};

            const string path = "/some-path";

            using (var fakeServer = new FakeServer())
            {
                fakeServer.Expect.Put(path, string.Empty).Returns(HttpStatusCode.Created, expectedResult);
                fakeServer.Start();

                var result = await fakeServer.Client.PutAsync(path, new StringContent(string.Empty));

                result.StatusCode.Should().Be(HttpStatusCode.Created);
            }
        }

        [Fact]
        public async Task FakeServer_IgnoredParameter_Returns200()
        {
            var expectedResult = new { ResourceId = 1234 };

            const string fakepath = "/some-resource/{ignore}/some-resource?date={ignore}&type={ignore}";
            const string actualpath = "/some-resource/1234/some-resource?date=2015-02-06T09:52:10&type=1";

            using (var fakeServer = new FakeServer(12354))
            {
                fakeServer.Expect.Get(fakepath).Returns(HttpStatusCode.Accepted, expectedResult);
                fakeServer.Start();

                var result = await fakeServer.Client.GetAsync(actualpath);

                result.StatusCode.Should().Be(HttpStatusCode.Accepted);
            }
        }

        [Fact]
        public async Task FakeServer_IgnoredParameterInRestfulpath_Returns200()
        {
            var expectedResult = new { ResourceId = 1234 };

            const string fakepath = "/some-resource/{ignore}/some-method";
            const string actualpath = "/some-resource/1234/some-method";

            using (var fakeServer = new FakeServer())
            {
                fakeServer.Expect.Get(fakepath).Returns(HttpStatusCode.Accepted, expectedResult);
                fakeServer.Start();

                var result = await fakeServer.Client.GetAsync(actualpath);

                result.StatusCode.Should().Be(HttpStatusCode.Accepted);
            }            
        }

        [Fact]
        public async Task FakeServer_ShouldHandleMultipleRegistrationOnSameEndPoint_WithDifferentBodies_ReturnExpectedData()
        {
            var expectedResultA = "1234";
            var expectedResultB = "5678";

            const string fakepath = "/some-path";

            using (var fakeServer = new FakeServer())
            {
                fakeServer.Expect.Post(fakepath, "messageA").Returns(HttpStatusCode.OK, expectedResultA);
                fakeServer.Expect.Post(fakepath, "messageB").Returns(HttpStatusCode.OK, expectedResultB);

                fakeServer.Start();

                var resultA = await fakeServer.Client.PostAsync(fakepath, new StringContent("messageA"));
                var resultB = await fakeServer.Client.PostAsync(fakepath, new StringContent("messageB"));

                (await resultA.Content.ReadAsStringAsync()).Should().Be(expectedResultA);
                (await resultB.Content.ReadAsStringAsync()).Should().Be(expectedResultB);
            }
        }

        [Fact]
        public async Task FakeServer_ExpectAllEndpointsToHaveAFiveSecondResponseTime_ResponseTimeIsGreaterThanFiveSeconds()
        {
            var expectedResult = new { ResourceId = 1234 };
            var expectedResponseTime = TimeSpan.FromSeconds(6);
            
            using (var fakeServer = new FakeServer())
            {
                fakeServer.Expect.ResponseTime = expectedResponseTime;
                fakeServer.Expect.Get("/some-path").Returns(HttpStatusCode.OK, expectedResult);
                fakeServer.Start();

                var stopwatch = Stopwatch.StartNew();

                await fakeServer.Client.GetAsync("/some-path");

                stopwatch.Stop();

                stopwatch.Elapsed.Should().BeGreaterOrEqualTo(expectedResponseTime);
            }
        }

        [Fact]
        public async Task FakeServer_ExpectGetToAnEndpointWithAFiveSecondResponseTime_ResponseTimeIsGreaterThanFiveSeconds()
        {
            var expectedResult = new { ResourceId = 1234 };
            var expectedResponseTime = TimeSpan.FromSeconds(1);

            using (var fakeServer = new FakeServer())
            {
                fakeServer.Expect.ResponseTime = expectedResponseTime;
                fakeServer.Expect.Get("/some-path").Returns(expectedResult).RespondsIn(TimeSpan.FromSeconds(5)).WithHttpStatus(HttpStatusCode.OK);
                fakeServer.Start();

                var stopwatch = Stopwatch.StartNew();

                await fakeServer.Client.GetAsync("/some-path");

                stopwatch.Stop();

                stopwatch.Elapsed.Should().BeGreaterOrEqualTo(expectedResponseTime);
            }
        }

        [Fact]
        public async Task FakeServer_ShouldExecuteResponseExpectationCallback_ReturnExpectedData()
        {
            const string expectedResult = "Some String Data";

            const string path = "/some-path?id=1234";

            using (var fakeServer = new FakeServer())
            {
                fakeServer.Expect.Get(path).Callback(() => new HttpResponseExpectation(HttpStatusCode.OK, expectedResult));

                fakeServer.Start();

                var result = await fakeServer.Client.GetStringAsync(path);

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
                
                var path1 = "/request1";
                var path2 = "/request2";
                var path3 = "/request3";
                var path4 = "/request4";

                var httpClient = new HttpClient();
                await httpClient.DeleteAsync(baseAddress + path1);
                await Repeat(() => httpClient.GetAsync(baseAddress + path2), 2);
                await Repeat(() => httpClient.PostAsync(baseAddress + path3, new StringContent(path3)), 3);
                await Repeat(() => httpClient.PutAsync(baseAddress + path4, new StringContent(path4)), 4);

                fakeServer.CapturedRequests.Count(x => x.Method == Http.Delete && x.Url == path1).Should().Be(1);
                fakeServer.CapturedRequests.Count(x => x.Method == Http.Get && x.Url == path2).Should().Be(2);
                fakeServer.CapturedRequests.Count(x => x.Method == Http.Post && x.Url == path3 && x.Body == path3).Should().Be(3);
                fakeServer.CapturedRequests.Count(x => x.Method == Http.Put && x.Url == path4 && x.Body == path4).Should().Be(4);
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
