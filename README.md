[![Build status](https://ci.appveyor.com/api/projects/status/3c20wb6doej6wqv6?svg=true)](https://ci.appveyor.com/project/justeattech/justfakeit)
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

JustFakeIt is a mockable HTTP server which is hosted within your unit test process and provides you with the ability to test your applications entire HTTP stack without having to provide mocks for anything in the stack. Because it's hosted In Process, it's really fast and takes very little effort to use.

It will configure itself as a default proxy and let you hook any WebRequest based client by default, intercepting all outbound Http calls in scope.

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
    using (var fakeServer = new FakeServer(12354))
    {
        fakeServer.Expect.Get("/123").Returns("Some String Data");
        fakeServer.Start();

        var result = new WebClient().DownloadString(new Uri("http://www.anything-at-all.com/123"));

        result.Should().Be("Some String Data");
    }
}
```

## Contributing

If you find a bug, have a feature request or even want to contribute an enhancement or fix, please follow the [contributing guidelines](CONTRIBUTING.md) included in the repository.


## Copyright

Copyright © JUST EAT PLC 2014
