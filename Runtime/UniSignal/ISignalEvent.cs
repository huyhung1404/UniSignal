namespace UniCore.Signal
{
    /// <summary>
    /// Represents a signal event that can be dispatched and processed within the UniSignal system.
    /// </summary>
    /// <remarks>
    /// A signal event serves as a communication construct for triggering specific behaviors in registered listeners.
    /// Implementations of this interface can define custom signals which are dispatched and handled
    /// according to their specified scope and context.
    /// </remarks>
    /// <seealso cref="SignalScope"/>
    /// <seealso cref="ISignalListener{T}"/>
    public interface ISignalEvent
    {
        public SignalScope Scope => SignalScope.All;
    }
}