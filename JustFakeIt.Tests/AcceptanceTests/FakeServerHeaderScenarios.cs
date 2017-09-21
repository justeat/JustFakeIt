using System.Collections.Generic;
using System.Collections.Specialized;
using FluentAssertions;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace JustFakeIt.Tests.AcceptanceTests
{
    public class FakeServerHeaderScenarios
    {
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
        public async Task FakeServer_GivenHeadersSetInClientRequest_WhenActualRequestsReturned_ContainsHeadersSent()
        {
            const string expectedResult = "SomeResult";
            const string path = "/some-path";

            using (var fakeServer = new FakeServer())
            {
                var expectedRequestHeaders = new Dictionary<string,string>
                {
                    {"X-Dummy1", "other1val"},  
                    {"X-Dummy3", "dummy3val"} 
                };

                fakeServer.Expect.Get(path).Returns(expectedResult);
                fakeServer.Start();

                var client = fakeServer.Client;
                client.DefaultRequestHeaders.Accept.Clear();
                foreach (var header in expectedRequestHeaders)
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                }
                
                var result = await client.GetAsync(path);
                var capturedRequests = fakeServer.CapturedRequests;

                result.StatusCode.Should().Be(HttpStatusCode.OK);
                result.Content.ReadAsStringAsync().Result.Should().Be(expectedResult);
                capturedRequests.Count.Should().Be(1);
                foreach (var header in expectedRequestHeaders)
                {
                    capturedRequests[0].Headers[header.Key].Should().Be(header.Value);
                }
                


            }
        }

        [Fact]
        public async Task FakeServer_GivenHeadersSetInClientRequestWithMultipleValuesForSingleHeader_WhenActualRequestsReturned_ContainsHeadersSent()
        {
            const string expectedResult = "SomeResult";
            const string path = "/some-path";

            using (var fakeServer = new FakeServer())
            {
                var expectedRequestHeaders = new NameValueCollection
                {
                    {"X-Dummy1", "other1val"},
                    {"X-Dummy1", "other2val"},
                    {"X-Dummy3", "dummy3val"}
                };

                fakeServer.Expect.Get(path).Returns(expectedResult);
                fakeServer.Start();

                var client = fakeServer.Client;
                client.DefaultRequestHeaders.Accept.Clear();
                foreach (var header in expectedRequestHeaders.AllKeys)
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation(header, expectedRequestHeaders[header]);
                }

                var result = await client.GetAsync(path);
                var capturedRequests = fakeServer.CapturedRequests;

                result.StatusCode.Should().Be(HttpStatusCode.OK);
                result.Content.ReadAsStringAsync().Result.Should().Be(expectedResult);
                capturedRequests.Count.Should().Be(1);
                foreach (var header in expectedRequestHeaders.AllKeys)
                {
                    capturedRequests[0].Headers[header].Should().Be(expectedRequestHeaders[header]);
                }
            }
        }
    }
}
