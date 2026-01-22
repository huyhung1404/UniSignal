namespace UniSignal.Variable
{
    public static class UniVars
    {
        public static VariableStore Global { get; }

        static UniVars()
        {
            Global = new VariableStore();
        }
    }
}