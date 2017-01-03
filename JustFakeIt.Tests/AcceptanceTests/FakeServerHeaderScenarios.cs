using FluentAssertions;
using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;

namespace JustFakeIt.Tests.AcceptanceTests
{
    public class FakeServerHeaderScenarios
    {
        [Test]
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

        [Test]
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

        [Test]
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
    }
}
