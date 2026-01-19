using System;

namespace UniSignal
{
    /// <summary>
    /// Represents a scope of signals identified by a unique mask for filtering
    /// and handling specific signals in the UniSignal system.
    /// </summary>
    /// <remarks>
    /// This struct is used to define the scope in which a signal listener operates.
    /// The mask system allows bitwise operations to evaluate and combine signal scopes.
    /// </remarks>
    /// <seealso cref="ISignalListener{T}"/>
    /// <seealso cref="ISignalEvent"/>
    public readonly struct SignalScope : IEquatable<SignalScope>
    {
        public readonly ulong Mask;

        public SignalScope(ulong mask)
        {
            Mask = mask;
        }

        public static readonly SignalScope All = new SignalScope(ulong.MaxValue);

        public static SignalScope operator |(SignalScope a, SignalScope b) => new(a.Mask | b.Mask);

        public bool Intersects(SignalScope other) => (Mask & other.Mask) != 0;

        public bool Equals(SignalScope other)
        {
            return Mask == other.Mask;
        }

        public override bool Equals(object obj)
        {
            return obj is SignalScope other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Mask.GetHashCode();
        }

        public override string ToString() => $"0x{Mask:X}";
    }
}