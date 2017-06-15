using FluentAssertions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace JustFakeIt.Tests.AcceptanceTests
{
    public class FakeServerPutScenarios
    {
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
        public async Task FakeServer_ExpectPutWithoutSlashAtTheStartOfUrl_ResponseMatchesExpectation()
        {
            const string expectedResult = "Some String Data";

            const string path = "some-path";

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
            var expectedResult = new { RestaurantId = 1234 };

            const string path = "/some-path";

            using (var fakeServer = new FakeServer())
            {
                fakeServer.Expect.Put(path, string.Empty).Returns(HttpStatusCode.Created, expectedResult);
                fakeServer.Start();

                var result = await fakeServer.Client.PutAsync(path, new StringContent(string.Empty));

                result.StatusCode.Should().Be(HttpStatusCode.Created);
            }
        }
    }
}
