using System;
using System.Collections.Generic;
using UniCore.Signal;

namespace UniCore.Editor
{
    internal static class SignalDispatchHistory
    {
        private const int MaxRecords = 50;
        private static readonly Queue<string> _records = new(MaxRecords);

        public static void Record(Type signalType, SignalScope scope)
        {
            if (_records.Count >= MaxRecords)
                _records.Dequeue();

            _records.Enqueue(
                $"[{UnityEngine.Time.frameCount}] {signalType.Name} | Scope: {scope}"
            );
        }

        public static IEnumerable<string> Records => _records;
        public static void Clear() => _records.Clear();
    }
}