# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [Unreleased]

### Added

- Guide for migrating from MediatR to FreeMediator


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
