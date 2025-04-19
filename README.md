#Or FreeMediator

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
- Generic request handlers of same arity as IRequestHandler<,>
- Generic notification handlers of same arity as INotificationHandler<>

Feel free to send a PR if you want to add any missing features, (make sure to read [CONTRIBUTING.md](CONTRIBUTING.md)
first).

OR just open an [Issue](https://github.com/steffenskov/FreeMediator/issues) for missing features, and I'll have a look.