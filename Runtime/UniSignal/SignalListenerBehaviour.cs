using UnityEngine;

namespace UniCore.Signal
{
    public abstract class SignalListenerBehaviour<T> : MonoBehaviour, ISignalListener<T> where T : ISignalEvent
    {
        protected virtual void OnEnable()
        {
            SignalBus.Register(this);
        }

        protected virtual void OnDisable()
        {
            SignalBus.Unregister(this);
        }

        public abstract void OnSignal(T signal);
    }
}