using FluentAssertions;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;

namespace JustFakeIt.Tests.AcceptanceTests
{
    public class FakeServerTimingScenarios
    {
        [Test]
        public async Task FakeServer_ExpectAllEndpointsToHaveAFiveSecondResponseTime_ResponseTimeIsGreaterThanFiveSeconds()
        {
            var expectedResult = new { ResourceId = 1234 };
            var expectedResponseTime = TimeSpan.FromSeconds(6);

            using (var fakeServer = new FakeServer())
            {
                fakeServer.Expect.ResponseTime = expectedResponseTime;
                fakeServer.Expect.Get("/some-path").Returns(HttpStatusCode.OK, expectedResult);
                fakeServer.Start();

                var stopwatch = Stopwatch.StartNew();

                await fakeServer.Client.GetAsync("/some-path");

                stopwatch.Stop();

                stopwatch.Elapsed.Should().BeGreaterOrEqualTo(expectedResponseTime);
            }
        }

        [Test]
        public async Task FakeServer_ExpectGetToAnEndpointWithAFiveSecondResponseTime_ResponseTimeIsGreaterThanFiveSeconds()
        {
            var expectedResult = new { ResourceId = 1234 };
            var expectedResponseTime = TimeSpan.FromSeconds(1);

            using (var fakeServer = new FakeServer())
            {
                fakeServer.Expect.ResponseTime = expectedResponseTime;
                fakeServer.Expect.Get("/some-path").Returns(expectedResult).RespondsIn(TimeSpan.FromSeconds(5)).WithHttpStatus(HttpStatusCode.OK);
                fakeServer.Start();

                var stopwatch = Stopwatch.StartNew();

                await fakeServer.Client.GetAsync("/some-path");

                stopwatch.Stop();

                stopwatch.Elapsed.Should().BeGreaterOrEqualTo(expectedResponseTime);
            }
        }
    }
}
