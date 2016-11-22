[![Build status](https://ci.appveyor.com/api/projects/status/3c20wb6doej6wqv6?svg=true)](https://ci.appveyor.com/project/justeattech/justfakeit)
JustFakeIt
==========

[![Gitter](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/justeat/JustFakeIt?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
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

```csharp
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

Ignore parameters by replacing the value with "{ignore}"

```csharp
 var fakeurl = "/some-resource/{ignore}/some-resource?date={ignore}&type={ignore}";
 fakeServer.Expect.Get(fakeurl).Returns(HttpStatusCode.Accepted, expectedResult);
```

Set a response time accross all endpoints by setting the `ResponseTime` property

```csharp
 fakeServer.Expect.ResponseTime = TimeSpan.FromSeconds(5);
```

Set the response time for a specific endpoint with `RespondsIn()`

```csharp
 fakeServer.Expect.Get(fakeurl).Returns(HttpStatusCode.Accepted, expectedResult).RespondsIn(TimeSpan.FromSeconds(5));
```
Assert against captured requests

```csharp
 fakeServer.CapturedRequests.Count(x => x.Method == Http.Delete && x.Url == "/some-url").Should().Be(1);
```

Return content from razor template file

```csharp
var path = "template.razor"
fakeServer.Expect.Get(url).ReturnsFromTemplate(path, new { Id = 2343, UserId = 2343, UserEmail = "mick.hucknall@just-eat.com" })
```

Return content from file

```csharp
var path = "response.txt"
fakeServer.Expect.Get(url).ReturnsFromFile(path)
```


## Contributing

If you find a bug, have a feature request or even want to contribute an enhancement or fix, please follow the [contributing guidelines](CONTRIBUTING.md) included in the repository.


## Copyright

Copyright © JUST EAT PLC 2014
