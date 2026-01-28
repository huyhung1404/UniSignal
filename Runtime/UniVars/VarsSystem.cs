using System.Collections.Generic;

namespace UniCore.Vars
{
    public static class VarsSystem
    {
        public static VariableStore Global { get; }

        static VarsSystem()
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