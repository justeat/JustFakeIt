using FluentAssertions;
using System;
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

                // Do not use stopwatch:
                var elapsedTime = await CallFakeServerWithAccurateUtcTimer(fakeServer);

                elapsedTime.Should().BeGreaterOrEqualTo(expectedResponseTime);
            }
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

                // Do not use stopwatch:
                var elapsedTime = await CallFakeServerWithAccurateUtcTimer(fakeServer);

                elapsedTime.Should().BeGreaterOrEqualTo(expectedResponseTime);
            }
        }

        private static async Task<TimeSpan> CallFakeServerWithAccurateUtcTimer(FakeServer fakeServer)
        {
            // The Stopwatch class uses the Windows "performance counter". 
            // I have often read that on some systems it returns inaccurate data. 
            // This appears to happen with older hardware and/or older operating system versions.
            // Stack overflow: http://stackoverflow.com/questions/36725825/c-sharp-await-task-delay1000-only-takes-640ms-to-return
            var startTime = DateTime.Now.ToUniversalTime();

            await fakeServer.Client.GetAsync("/some-path");

            var endTime = DateTime.Now.ToUniversalTime();

            return endTime - startTime;
        }
    }
}
