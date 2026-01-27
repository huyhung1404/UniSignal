using System.Collections.Generic;

namespace UniCore.Vars
{
    public static class VarHub
    {
        public static VariableStore Global { get; }

        static VarHub()
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