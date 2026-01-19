# Changelog

All notable changes to **UniSignal** will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/).

---

## [1.0.0] - Initial Public Release

### Added

* Core **SignalBus** for type-safe, interface-based event dispatching
* **ISignalEvent** abstraction for pure data-driven signals
* **ISignalListener<T>** interface with explicit priority support
* **SignalListenerBehaviour<T>** base class for automatic registration and unregistration
* **Flag-based SignalScope** (`Scene`, `Gameplay`, `UI`, `All`)
* Support for dispatching a single signal to multiple scopes
* Listener execution order based on priority (higher first)
* Support for a single class listening to multiple signal types
* Built-in runtime safety with clear error logging on listener failures
* Editor-only **Signal Debug Window** for inspecting signal flow and execution order

### Design

* Zero-allocation runtime design (no delegates, no lambdas, no reflection)
* Explicit, compile-time safe APIs
* IL2CPP and AOT-friendly architecture
* Debug-first philosophy for large and long-lived projects

---

## [0.0.1] - Prototype

### Added

* Initial internal implementation of the UniSignal architecture
* Early experiments with interface-based observers
* Basic dispatcher and listener lifecycle handling

### Notes

* This version was not intended for public use
* APIs were unstable and subject to change

---

## Planned

### [0.2.0]

* Scene-instance scoped signals
* Automatic cleanup on scene unload
* Optional signal replay for debugging race conditions
* Lightweight performance profiler for signal dispatching

### [0.3.0]

* Optional signal channels / tags
* Better tooling for large projects
* Extended documentation and samples

---

> For breaking changes, migration guides will be provided in future releases.
