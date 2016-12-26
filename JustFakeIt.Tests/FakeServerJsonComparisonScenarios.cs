using FluentAssertions;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace JustFakeIt.Tests
{
    public class FakeServerJsonComparisonScenarios
    {
        [Fact]
        public async Task FakeServer_ExpectPostWithPartialExpectedJsonBody_ResponseMatchesExpected()
        {
            const string expectedResult = "Some String Data";

            const string path = "/some-path";

            var content = new StringContent(@"{ Key: ""Value"", Key2: ""Value"" }");
            var expectedContent = @"{ Key: ""Value"" }";

            using (var fakeServer = new FakeServer())
            {
                fakeServer.Expect.Post(path, expectedContent).WithPartialJsonMatching().Returns(expectedResult);

                fakeServer.Start();

                var resp = await fakeServer.Client.PostAsync(path, content);
                var result = await resp.Content.ReadAsStringAsync();

                result.Should().Be(expectedResult);
            }
        }

        [Fact]
        public async Task FakeServer_ExpectPostWithPartialExpectedJsonBodyWithNestedElements_ResponseMatchesExpected()
        {
            const string expectedResult = "Some String Data";

            const string path = "/some-path";

            var content = new StringContent(@"{ Key: ""Value"", key2: { NestedKey1: ""NestedValue1"", NestedKey2: ""NestedValue2"" }, Key3: ""Value"" }");
            var expectedContent = @"{ Key: ""Value"", key2: { NestedKey1: ""NestedValue1"" } }";

            using (var fakeServer = new FakeServer())
            {
                fakeServer.Expect.Post(path, expectedContent).WithPartialJsonMatching().Returns(expectedResult);

                fakeServer.Start();

                var resp = await fakeServer.Client.PostAsync(path, content);
                var result = await resp.Content.ReadAsStringAsync();

                result.Should().Be(expectedResult);
            }
        }
    }
}
