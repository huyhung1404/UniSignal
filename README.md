# UniSignal

**UniSignal** is a zero-allocation, interface-driven event system for Unity, designed for projects that demand **high performance**, **clear debugging**, and **long-term
maintainability**.

It avoids delegates, reflection, and hidden GC allocations, while providing powerful features such as scoped events, listener priorities, automatic lifecycle handling, and
built-in debugging tools.

---

## ✨ Features

* 🚀 **Zero Allocation**
  No delegates, no lambdas, no reflection at runtime.

* 🧩 **Interface-based Observers**
  Strongly typed, compile-time safe event handling.

* 🏷️ **Flag-based Event Scopes**
  Dispatch events across `Scene`, `Gameplay`, `UI`, or any combination.

* 🔢 **Priority-aware Listeners**
  Control execution order explicitly and predictably.

* ♻️ **Automatic Unregistering**
  Listeners are safely unregistered with Unity lifecycle hooks.

* 🐞 **Debug-first Design**
  Built-in tools to inspect event flow, listener execution, and errors.

* 🛡️ **IL2CPP & Mobile Friendly**
  Designed to be safe for AOT compilation and performance-critical platforms.

---

## 📦 Installation

### Via Git URL (Unity Package Manager)

```
https://github.com/huyhung1404/unisignal.git
```

Or add manually to `Packages/manifest.json`:

```json
"com.huyhung1404.unisignal": {
  "git": "https://github.com/huyhung1404/unisignal.git"
}
```

---

## 🧠 Core Concepts

### Signal Event

Events are **pure data** structures implementing `ISignalEvent`.

```csharp
public struct PlayerDeadEvent : ISignalEvent
{
    public int playerId;
}
```

---

### Signal Scope (User-defined, Bitmask-based)

UniSignal does **not** hardcode event scopes.  
Instead, it provides a lightweight bitmask-based `SignalScope` type, allowing **users to define their own scopes** without modifying the package source.

This design ensures long-term extensibility while remaining type-safe, performant, and IL2CPP-friendly.

```csharp
public readonly struct SignalScope
{
    public readonly ulong Mask;

    public SignalScope(ulong mask)
    {
        Mask = mask;
    }

    public static SignalScope operator |(SignalScope a, SignalScope b) => new(a.Mask | b.Mask);

    public bool Intersects(SignalScope other) => (Mask & other.Mask) != 0;
}
```

Scopes are defined default as `All`

```csharp
  public static readonly SignalScope All = new SignalScope(ulong.MaxValue);
```

Scopes allow a single event to be dispatched to multiple systems without duplication.
---

### Defining Scopes (User Code)

Scopes are defined outside of UniSignal, typically in a project-level static class:

```csharp
public static class GameSignalScopes
{
    public static readonly SignalScope Gameplay = new(1UL << 0);
    public static readonly SignalScope UI       = new(1UL << 1);
    public static readonly SignalScope Network  = new(1UL << 2);
    
    static GameSignalScopes()
    {
        SignalScopeRegistry.Register("Gameplay", Gameplay);
        SignalScopeRegistry.Register("UI", UI);
        SignalScopeRegistry.Register("Network", Network);
    }
}
```

This approach is similar to Unity’s LayerMask, but designed specifically for signal dispatching.

---

### Signal Listener

Listeners implement `ISignalListener<T>` and define their own priority and listening scope.

```csharp
public interface ISignalListener<in T> where T : ISignalEvent
{
    public int Priority => 0; // Lower values are executed later.
    public SignalScope ListenScope => SignalScope.All; // Default scope is All.
    public void OnSignal(T signal);
}
```

---

## 🎮 Usage Example

### Listener Implementation

```csharp
public class GameOverUI : SignalListenerBehaviour<PlayerDeadEvent>
{
    public override int Priority => 100;
    public override SignalScope ListenScope => SignalScope.UI;

    public override void OnSignal(PlayerDeadEvent signal)
    {
        Debug.Log($"Player {signal.playerId} died");
    }
}
```

`SignalListenerBehaviour<T>` ensures:

* Automatic registration
* Automatic unregistration
* Compile-time safety against lifecycle bugs

---

### Dispatching a Signal

```csharp
SignalBus.Dispatch(new PlayerDeadEvent
{
    playerId = 1
});
```

---

## 🧩 Multiple Signals per Class

A single class can listen to multiple signals with **independent priorities and scopes** using explicit interface implementations.

```csharp
public class HUDController : MonoBehaviour,
    ISignalListener<PlayerDeadEvent>,
    ISignalListener<ScoreChangedEvent>
{
    int ISignalListener<PlayerDeadEvent>.Priority => 100;
    SignalScope ISignalListener<PlayerDeadEvent>.ListenScope => SignalScope.UI;

    int ISignalListener<ScoreChangedEvent>.Priority => 10;
    SignalScope ISignalListener<ScoreChangedEvent>.ListenScope => SignalScope.UI | SignalScope.Gameplay;

    void ISignalListener<PlayerDeadEvent>.OnSignal(PlayerDeadEvent signal) { }
    void ISignalListener<ScoreChangedEvent>.OnSignal(ScoreChangedEvent signal) { }
}
```

---

## 🐞 Debugging

UniSignal provides a built-in **Event Debug Window** (Editor only):

* Inspect dispatched signals
* View execution order
* Track frame timing
* Identify missing listeners or errors

Menu:

```
Tools ▸ UniSignal ▸ Debug Window
```

---

## 🎯 Design Philosophy

UniSignal is built on the following principles:

* Explicit over implicit
* Compile-time safety over runtime magic
* Debuggability over convenience
* Performance without sacrificing clarity

It is designed to scale from small projects to complex, long-lived productions.

---

## 🧪 Recommended Use Cases

* Gameplay → UI → Audio decoupling
* Modular feature systems
* Large projects with multiple teams
* Mobile and performance-critical games

---

## 📄 License

MIT License

---

## ❤️ Author

Created by **huyhung1404**
GitHub: [https://github.com/huyhung1404](https://github.com/huyhung1404)

---

If you find UniSignal useful, consider ⭐ starring the repository!
