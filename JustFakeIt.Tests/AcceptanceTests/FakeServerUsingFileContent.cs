using FluentAssertions;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace JustFakeIt.Tests.AcceptanceTests
{
    public class FakeServerUsingFileContent
    {
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
    }
}
