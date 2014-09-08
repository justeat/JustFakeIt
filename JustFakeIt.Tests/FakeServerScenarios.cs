using System;
using System.Net;
using FluentAssertions;
using Xunit;

namespace JustFakeIt.Tests
{
    public class FakeServerScenarios
    {
        [Fact]
        public void SimpleReturn()
        {
            const string expectedResult = "{ 'RestaurantId' : '1234' }";
            const string baseAddress = "http://localhost:12354";
            
            const string url = "/restaurant/1234";

            using (var fakeServer = new FakeServer(new Uri(baseAddress)))
            {
                fakeServer.Expect(Http.Get, url)
                    .Returns(expectedResult);

                fakeServer.Start();

                var result = new WebClient().DownloadString(new Uri(baseAddress + url));

                result.Should().Be(expectedResult);
            }
        }
    }
}
