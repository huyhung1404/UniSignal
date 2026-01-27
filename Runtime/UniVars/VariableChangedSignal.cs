using UniCore.Signal;

namespace UniCore.Vars
{
    public class VariableChangedSignal<T> : ISignalEvent
    {
        public string Key;
        public T OldValue;
        public T NewValue;
    }
}