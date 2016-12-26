using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace JustFakeIt.Tests
{
    public class FakeServerScenarios
    {
        [Fact]
        public async Task FakeServer_IgnoredParameter_Returns200()
        {
            var expectedResult = new { ResourceId = 1234 };

            const string fakepath = "/some-resource/{ignore}/some-resource?date={ignore}&type={ignore}";
            const string actualpath = "/some-resource/1234/some-resource?date=2015-02-06T09:52:10&type=1";

            using (var fakeServer = new FakeServer(12354))
            {
                fakeServer.Expect.Get(fakepath).Returns(HttpStatusCode.Accepted, expectedResult);
                fakeServer.Start();

                var result = await fakeServer.Client.GetAsync(actualpath);

                result.StatusCode.Should().Be(HttpStatusCode.Accepted);
            }
        }

        [Fact]
        public async Task FakeServer_IgnoredParameterInRestfulpath_Returns200()
        {
            var expectedResult = new { ResourceId = 1234 };

            const string fakepath = "/some-resource/{ignore}/some-method";
            const string actualpath = "/some-resource/1234/some-method";

            using (var fakeServer = new FakeServer())
            {
                fakeServer.Expect.Get(fakepath).Returns(HttpStatusCode.Accepted, expectedResult);
                fakeServer.Start();

                var result = await fakeServer.Client.GetAsync(actualpath);

                result.StatusCode.Should().Be(HttpStatusCode.Accepted);
            }            
        }

        [Fact]
        public async Task FakeServer_ShouldHandleMultipleRegistrationOnSameEndPoint_WithDifferentBodies_ReturnExpectedData()
        {
            var expectedResultA = "1234";
            var expectedResultB = "5678";

            const string fakepath = "/some-path";

            using (var fakeServer = new FakeServer())
            {
                fakeServer.Expect.Post(fakepath, "messageA").Returns(HttpStatusCode.OK, expectedResultA);
                fakeServer.Expect.Post(fakepath, "messageB").Returns(HttpStatusCode.OK, expectedResultB);

                fakeServer.Start();

                var resultA = await fakeServer.Client.PostAsync(fakepath, new StringContent("messageA"));
                var resultB = await fakeServer.Client.PostAsync(fakepath, new StringContent("messageB"));

                (await resultA.Content.ReadAsStringAsync()).Should().Be(expectedResultA);
                (await resultB.Content.ReadAsStringAsync()).Should().Be(expectedResultB);
            }
        }

        [Fact]
        public async Task FakeServer_ShouldExecuteResponseExpectationCallback_ReturnExpectedData()
        {
            const string expectedResult = "Some String Data";

            const string path = "/some-path?id=1234";

            using (var fakeServer = new FakeServer())
            {
                fakeServer.Expect.Get(path).Callback(() => new HttpResponseExpectation(HttpStatusCode.OK, expectedResult));

                fakeServer.Start();

                var result = await fakeServer.Client.GetStringAsync(path);

                result.Should().Be(expectedResult);
            }
        }

        [Fact]
        public async Task FakeServer_CapturesAllRequests()
        {
            using (var fakeServer = new FakeServer())
            {
                fakeServer.Start();
                var baseAddress = fakeServer.BaseUri;
                
                var path1 = "/request1";
                var path2 = "/request2";
                var path3 = "/request3";
                var path4 = "/request4";

                var httpClient = new HttpClient();
                await httpClient.DeleteAsync(baseAddress + path1);
                await Repeat(() => httpClient.GetAsync(baseAddress + path2), 2);
                await Repeat(() => httpClient.PostAsync(baseAddress + path3, new StringContent(path3)), 3);
                await Repeat(() => httpClient.PutAsync(baseAddress + path4, new StringContent(path4)), 4);

                fakeServer.CapturedRequests.Count(x => x.Method == Http.Delete && x.Url == path1).Should().Be(1);
                fakeServer.CapturedRequests.Count(x => x.Method == Http.Get && x.Url == path2).Should().Be(2);
                fakeServer.CapturedRequests.Count(x => x.Method == Http.Post && x.Url == path3 && x.Body == path3).Should().Be(3);
                fakeServer.CapturedRequests.Count(x => x.Method == Http.Put && x.Url == path4 && x.Body == path4).Should().Be(4);
            }
        }

        private async Task Repeat(Func<Task> a, int times)
        {
            for (var i = 0; i < times; i++)
            {
                await a();
            }
        }
    }
}
