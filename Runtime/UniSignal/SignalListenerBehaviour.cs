using UnityEngine;

namespace UniCore.Signal
{
    public abstract class SignalListenerBehaviour<T> : MonoBehaviour, ISignalListener<T> where T : ISignalEvent
    {
        protected virtual void OnEnable()
        {
            SignalSystem.Register(this);
        }

        protected virtual void OnDisable()
        {
            SignalSystem.Unregister(this);
        }

        public abstract void OnSignal(T signal);
    }
}