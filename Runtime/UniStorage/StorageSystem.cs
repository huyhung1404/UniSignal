namespace UniCore.Storage
{
    public delegate void VersionChanged(object data, int oldVersion, int newVersion);

    public static class StorageSystem
    {
        internal static VersionChanged onVersionChanged;
        private static StoragePipeline pipeline;

        public static event VersionChanged OnVersionChanged { add => onVersionChanged += value; remove => onVersionChanged -= value; }

        public static void SetSettings(ISettings settings)
        {
            pipeline = new StoragePipeline(settings);
        }

        public static byte[] GetKey()
        {
            InitializationIfNeed();
            return pipeline.Key;
        }

        public static void Save<T>(string fileName, T data)
        {
            InitializationIfNeed();
            pipeline.Save(fileName, data);
        }

        public static T Load<T>(string fileName)
        {
            InitializationIfNeed();
            return pipeline.Load<T>(fileName);
        }

        private static void InitializationIfNeed()
        {
            if (pipeline != null) return;
            ISettings setting = UnityEngine.Resources.Load<StorageSettings>($"{nameof(StorageSettings)}.asset");
            pipeline = new StoragePipeline(setting);
        }
    }
}