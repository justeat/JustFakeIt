using FluentAssertions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace JustFakeIt.Tests.AcceptanceTests
{
    public class FakeServerPostScenarios
    {
        [Fact]
        public async Task FakeServer_ExpectPostWithNoBodyReturnsString_ResponseMatchesExpectation()
        {
            const string expectedResult = "Some String Data";

            const string path = "/some-path";

            var content = new StringContent(string.Empty);

            using (var fakeServer = new FakeServer())
            {
                fakeServer.Expect.Post(path).Returns(expectedResult);

                fakeServer.Start();

                var resp = await fakeServer.Client.PostAsync(path, content);
                var result = await resp.Content.ReadAsStringAsync();

                result.Should().Be(expectedResult);
            }
        }
        [Fact]
        public async Task FakeServer_ExpectPostWithoutSlashAtTheStartOfUrl_ResponseMatchesExpectation()
        {
            const string expectedResult = "Some String Data";

            const string path = "some-path";

            var content = new StringContent(string.Empty);

            using (var fakeServer = new FakeServer())
            {
                fakeServer.Expect.Post(path).Returns(expectedResult);

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
    }
}
