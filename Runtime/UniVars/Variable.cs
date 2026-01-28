using System.Collections.Generic;
using UniCore.Signal;

namespace UniCore.Vars
{
    public class Variable<T>
    {
        protected readonly string key;
        protected T value;

        public Variable(string key, T value)
        {
            this.key = key;
            this.value = value;
        }

        public virtual void Set(T v)
        {
            if (EqualityComparer<T>.Default.Equals(value, v)) return;

            var old = value;
            value = v;

            SignalSystem.Dispatch(new VariableChangedSignal<T>
            {
                Key = key,
                OldValue = old,
                NewValue = v
            });
        }

        public static implicit operator T(Variable<T> variable)
        {
            return variable.value;
        }
    }
}