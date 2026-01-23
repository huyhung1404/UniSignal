using System.Collections.Generic;

namespace UniSignal.Variable
{
    public static class UniVars
    {
        public static VariableStore Global { get; }

        static UniVars()
        {
            Global = new VariableStore();
        }
        
        internal static IEnumerable<(string Name, VariableStore Store)> AllStores
        {
            get
            {
                yield return ("Global", Global);
            }
        }
    }
}