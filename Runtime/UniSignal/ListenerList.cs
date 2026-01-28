using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniCore.Signal
{
    internal interface IListenerList
    {
        public int Count { get; }
        public object Get(int index);
        public void Add(object listener);
        public void Remove(object listener);
    }

    internal sealed class ListenerList<T> : IListenerList where T : ISignalEvent
    {
        private readonly List<ISignalListener<T>> list = new(8);

        public int Count => list.Count;

        public object Get(int index)
        {
            return list[index];
        }

        public void Add(object o)
        {
            var listener = (ISignalListener<T>)o;
            if (list.Contains(listener)) return;

            var p = listener.Priority;
            var i = list.Count;
            list.Add(listener);

            while (i > 0 && list[i - 1].Priority < p)
            {
                list[i] = list[i - 1];
                i--;
            }

            list[i] = listener;
        }

        public void Remove(object o) => list.Remove((ISignalListener<T>)o);

        public void Dispatch(T signal, SignalScope scope)
        {
            var c = list.Count;
            for (var i = 0; i < c; i++)
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
                        $"while handling {typeof(T).Name}\n{ex}"
                    );
                }
            }
        }
    }
}