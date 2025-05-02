# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [Unreleased]

### Added

- Guide for migrating from MediatR to FreeMediator

## [1.2.0] - 2025-05-02

### Added

- Support for generic `IRequestHandlers` of partial arity match, meaning these are now all valid:

`class MyHandler<TRequest> : IRequestHandler<TRequest>`
`class MyHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>`
`public class MyHandler<TResponse> : IRequestHandler<FIXED_REQUEST, TResponse>`
`public class MyHandler<TRequest> : IRequestHandler<TRequest, FIXED_RESPONSE>`

The latter two deserves a few extra words. It allows you to specify a generic constraint for `TRequest` or `TResponse` and keep the other fixed.
This is useful in quite a few scenarios in my experience, and not outright supported in MediatR.

The solution here DOES come with a caveat: You handler MUST BE PUBLIC! This is because I'm using Reflection to wrap the handler in something with the same generic arity as `IRequestHandler<TRequest, TResponse>` and this requires the handler to be public.


## [1.1.1] - 2025-04-30

### Changed

- Behaviors for `INotification` can now be used for post-handling processing as well (by placing logic AFTER the call to `next()`)
- `IRequestHandler's` `Handle` method now returns `Task` instead of `Task<Unit>` to be compatible with MediatR


## [1.1.0] - 2025-04-29

### Added

- Support for open behaviors for Notifications
- Support for closed behaviors for Notifications


## [1.0.1] - 2025-04-20

### Added

- Support for sending Requests (including generic handlers of same arity as IRequestHandler<,>)
- Support for open behaviors for Requests
- Support for closed behaviors for Requests
- Support for publishing Notifications (including generic handlers of same arity as INotificationHandler<>)
