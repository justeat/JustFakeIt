using FluentAssertions;
using System.Threading.Tasks;
using Xunit;

namespace JustFakeIt.Tests.AcceptanceTests
{
    public class FakeServerDeleteScenarios
    {
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
    }
}
