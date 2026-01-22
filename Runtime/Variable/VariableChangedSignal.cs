namespace UniSignal.Variable
{
    public class VariableChangedSignal<T> : ISignalEvent
    {
        public string Key;
        public T OldValue;
        public T NewValue;
    }
}