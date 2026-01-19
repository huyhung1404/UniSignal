using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniSignal
{
    public static class SignalBus
    {
        internal static readonly Dictionary<Type, IList> listeners = new();

        public static void Register<T>(ISignalListener<T> listener) where T : ISignalEvent
        {
            var type = typeof(T);

            if (!listeners.TryGetValue(type, out var rawList))
            {
                rawList = new List<ISignalListener<T>>();
                listeners[type] = rawList;
            }

            var list = (List<ISignalListener<T>>)rawList;
            if (list.Contains(listener)) return;
            list.Add(listener);
            list.Sort(static (a, b) => b.Priority.CompareTo(a.Priority));
        }

        public static void Unregister<T>(ISignalListener<T> listener) where T : ISignalEvent
        {
            var type = typeof(T);
            if (!listeners.TryGetValue(type, out var rawList)) return;
            var list = (List<ISignalListener<T>>)rawList;
            list.Remove(listener);
        }

        public static void Dispatch<T>(T signal) where T : ISignalEvent => Dispatch(signal, signal.Scope);

        public static void Dispatch<T>(T signal, SignalScope scope) where T : ISignalEvent
        {
            var type = typeof(T);

            if (!listeners.TryGetValue(type, out var rawList)) return;

            var list = (List<ISignalListener<T>>)rawList;
            for (var i = 0; i < list.Count; i++)
            {
                var listener = list[i];
                if (!listener.ListenScope.Intersects(scope)) continue;

                try
                {
                    listener.OnSignal(signal);
                }
                catch (Exception ex)
                {
                    Debug.LogError(
                        $"[UniSignal] Exception in {listener.GetType().Name} " +
                        $"while handling {type.Name}\n{ex}"
                    );
                }
            }
        }

        public static void ReleaseEmptyLists()
        {
            if (listeners.Count == 0) return;
            var temp = new List<Type>();
            foreach (var kvp in listeners)
            {
                if (kvp.Value.Count == 0) temp.Add(kvp.Key);
            }

            for (var i = 0; i < temp.Count; i++) listeners.Remove(temp[i]);
        }

        public static void Clear()
        {
            listeners.Clear();
        }
    }
}