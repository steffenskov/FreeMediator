# FreeMediator

FreeMediator is a free and open-source alternative to the popular [MediatR](https://github.com/jbogard/MediatR) package
by [Jimmy Bogard](https://github.com/jbogard).

- It strives to be a drop-in replacement for MediatR, with the same API and functionality, and is created purely on the
  background of [MediatR going commercial](https://www.jimmybogard.com/automapper-and-mediatr-going-commercial/).
- It is not a fork of MediatR, but rather a new implementation that aims to be as compatible as possible with the
  original. Let me know in the [Issues](https://github.com/steffenskov/FreeMediator/issues) section if you find any
  incompatibilities.

As such the first version only deals with the basic stuff:

- Sending requests and receiving responses
- Pipeline behavior support for requests (both open and closed behaviors)
- Publishing notifications
- Generic request handlers of same arity as `IRequestHandler<>`, `IRequestHandler<,>`, `IRequestHandler<,FIXED_TYPE>` or `IRequestHandler<FIXED_TYPE,>` (the last two being partial arity match, check [changelog v.1.2.0](CHANGELOG.md#120---2025-05-02) for details)
- Generic notification handlers of same arity as `INotificationHandler<>`

Feel free to send a PR if you want to add any missing features, (make sure to read [CONTRIBUTING.md](CONTRIBUTING.md)
first).

OR just open an [Issue](https://github.com/steffenskov/FreeMediator/issues) for missing features, and I'll have a look.

## Features outside of the original MediatR

- Pipeline behavior support for notifications (both open and closed behaviors)

## Current limitations

- No support for streaming
- No support for implementing multiple handlers in a single class if they have the same return type

# Usage

FreeMediator is intended to be used with .Net Dependency Injection, so start off by adding it to your DI:

```csharp
services.AddMediator(config => 
{
    config.RegisterServicesFromAssemblyContaining<SomeRequest>(); // There are other ways to register services as well
});
```

This will scan the assembly containing `SomeRequest` for all classes implementing `IRequestHandler<TRequest, TResponse>`
and `INotificationHandler<TNotification>` and register them with the DI container.

It also registers the `IMediator` interface itself (as well as a `ISender` and `IPublisher` interface, which are both
subsets of `IMediator`).

From here on simply inject `IMediator` (or `ISender`/`IPublisher`) into your classes and use it to send requests or
publish notifications:

```csharp
public class SomeService
{
    private readonly IMediator _mediator;

    public SomeService(IMediator mediator
    {
        _mediator = mediator;
    }

    public async Task<SomeResponse> DoSomethingAsync(CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new SomeRequest(), cancellationToken);
        return response;
    }
}
```

# Changelog

See the [CHANGELOG](CHANGELOG.md) for a list of changes and new features.

# Documentation

Auto generated documentation via [DocFx](https://github.com/dotnet/docfx) is available
here: https://steffenskov.github.io/FreeMediator/
