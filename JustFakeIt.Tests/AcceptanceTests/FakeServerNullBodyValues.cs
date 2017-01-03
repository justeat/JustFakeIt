using FluentAssertions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;

namespace JustFakeIt.Tests.AcceptanceTests
{
    public class FakeServerNullBodyValues
    {
        [Test]
        public async Task FakeServer_ExpectPostWithNullReturnsString_ResponseMatchesExpectation()
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

        [Test]
        public async Task FakeServer_ExpectPostWithNullBodyAndPostWithNullBody_ResponseMatchesExpectation()
        {
            const string expectedResult = "Some String Data";

            const string path = "/some-path";

            using (var fakeServer = new FakeServer())
            {
                fakeServer.Expect.Post(path).Returns(expectedResult);

                fakeServer.Start();

                var resp = await fakeServer.Client.PostAsync(path, null);
                var result = await resp.Content.ReadAsStringAsync();

                result.Should().Be(expectedResult);
            }
        }

        [Test]
        public async Task FakeServer_ExpectPostWithBodyAndPostWithNullBody_ResponseIsNotFound()
        {
            const string expectedResult = "Some String Data";

            const string path = "/some-path";

            using (var fakeServer = new FakeServer())
            {
                fakeServer.Expect.Post(path, "jiberish").Returns(expectedResult);

                fakeServer.Start();

                var resp = await fakeServer.Client.PostAsync(path, null);

                resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }
    }
}
