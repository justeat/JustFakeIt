JustFakeIt
==========
_An InProcess HTTP server which can be mocked and asserted against to allow for full stack HTTP testing_

---

* Introduction
* Installation
* Getting Started
* Contributing
* Copyright

## Introduction

JustFaxkeIt is a mockable HTTP server which is hosted within your unit test process and provides you with the ability to test your applications entire HTTP stack without having to provide mocks for anything in the stack. Because it's hosted In Process, it's really fast and takes very little effort to use.

## Installation

Pre-requisites: The project is built in .net v4.0.

* From source: https://github.com/justeat/JustFakeIt
* By hand: https://www.nuget.org/packages/JustFakeIt

Via NuGet:

		PM> Install-Package JustFakeIt


## Getting Started

Once you have the package installed into your test project, a standard wire-up will look like this.

```
[Fact]
public void FakeServer_ExpectGetReturnsString_ResponseMatchesExpectation()
{
    const string expectedResult = "Some String Data";
    const string baseAddress = "http://localhost:12354";
    
    const string url = "/some-url";

    using (var fakeServer = new FakeServer(new Uri(baseAddress)))
    {
        fakeServer.Expect.Get(url).Returns(expectedResult);

        fakeServer.Start();

        var result = new WebClient().DownloadString(new Uri(baseAddress + url));

        result.Should().Be(expectedResult);
    }
}
```

## Contributing

If you find a bug, have a feature request or even want to contribute an enhancement or fix, please follow the contributing guidelines included in the repository.


## Copyright

Copyright © JUST EAT PLC 2014
