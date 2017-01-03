using FluentAssertions;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace JustFakeIt.Tests.AcceptanceTests
{
    public class FakeServerTimingScenarios
    {
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

                var elapsedTime = await CallFakeServerWithAccurateUtcTimer(fakeServer);

                elapsedTime.Should().BeGreaterOrEqualTo(expectedResponseTime);
            }
        }

        private static async Task<TimeSpan> CallFakeServerWithAccurateUtcTimer(FakeServer fakeServer)
        {
            var startTime = DateTime.Now.ToUniversalTime();

            await fakeServer.Client.GetAsync("/some-path");

            var endTime = DateTime.Now.ToUniversalTime();

            return endTime - startTime;
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

                var elapsedTime = await CallFakeServerWithAccurateUtcTimer(fakeServer);

                elapsedTime.Should().BeGreaterOrEqualTo(expectedResponseTime);
            }
        }
    }
}
