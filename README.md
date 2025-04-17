# FreeMediator

FreeMediator is a free and open-source alternative to the popular [MediatR](https://github.com/jbogard/MediatR) package
by [Jimmy Bogard](https://github.com/jbogard).

- It strives to be a drop-in replacement for MediatR, with the same API and functionality, and is created purely on the
  background of [MediatR going commercial](https://www.jimmybogard.com/automapper-and-mediatr-going-commercial/).
- It is not a fork of MediatR, but rather a new implementation that aims to be as compatible as possible with the
  original.

As such the first version only deals with the basic stuff (sending requests, returning responses), but here's what's to
come:

- Pipeline behavior support
- Streaming support