using FluentAssertions;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace JustFakeIt.Tests.AcceptanceTests
{
    public class FakeServerGetScenarios
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
    }
}

