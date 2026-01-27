using UniCore.Storage;
using UnityEditor;
using UnityEngine;

namespace UniCore.Editor
{
    [CustomEditor(typeof(StorageSettings))]
    public class StorageSettingsEditor : UnityEditor.Editor
    {
        private SerializedProperty versionProperty;

        private SerializedProperty serializationTypeProperty;
        private SerializedProperty serializerCustomProperty;

        private SerializedProperty keyTypeProperty;
        private SerializedProperty keyCustomProperty;

        private SerializedProperty encryptionTypeProperty;
        private SerializedProperty encryptorCustomProperty;

        private SerializedProperty protectorTypeProperty;
        private SerializedProperty protectorCustomProperty;

        private SerializedProperty storageTypeProperty;
        private SerializedProperty storageCustomProperty;

        [MenuItem("UniCore/Storage/Storage Settings", priority = 1)]
        public static void CreateStorageSettings()
        {
            var assetPath = $"Assets/Resources/{nameof(StorageSettings)}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<StorageSettings>(assetPath);
            if (existing != null)
            {
                Selection.activeObject = existing;
                EditorGUIUtility.PingObject(existing);
                return;
            }

            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            var settings = CreateInstance<StorageSettings>();
            AssetDatabase.CreateAsset(settings, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = settings;
            EditorGUIUtility.PingObject(settings);
        }

        private void OnEnable()
        {
            versionProperty = serializedObject.FindProperty("version");

            serializationTypeProperty = serializedObject.FindProperty("serializationType");
            serializerCustomProperty = serializedObject.FindProperty("serializerCustom");

            keyTypeProperty = serializedObject.FindProperty("keyType");
            keyCustomProperty = serializedObject.FindProperty("keyCustom");

            encryptionTypeProperty = serializedObject.FindProperty("encryptionType");
            encryptorCustomProperty = serializedObject.FindProperty("encryptorCustom");

            protectorTypeProperty = serializedObject.FindProperty("protectorType");
            protectorCustomProperty = serializedObject.FindProperty("protectorCustom");

            storageTypeProperty = serializedObject.FindProperty("storageType");
            storageCustomProperty = serializedObject.FindProperty("storageCustom");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(versionProperty);
            EditorGUILayout.Space(5);

            DrawField("Serialize", serializationTypeProperty, (int)SerializationType.Custom, serializerCustomProperty);

            if (encryptionTypeProperty.enumValueIndex == (int)EncryptionType.AES ||
                protectorTypeProperty.enumValueIndex == (int)ProtectorType.SHA256)
            {
                DrawField("Key", keyTypeProperty, (int)KeyType.Custom, keyCustomProperty);
            }

            DrawField("Encryptor", encryptionTypeProperty, (int)EncryptionType.Custom, encryptorCustomProperty);
            DrawField("Protector", protectorTypeProperty, (int)ProtectorType.Custom, protectorCustomProperty);
            DrawField("Storage", storageTypeProperty, (int)StorageType.Custom, storageCustomProperty);
        }

        private static void DrawField(string title, SerializedProperty enumProperty, int targetCustomId, SerializedProperty customProperty)
        {
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(enumProperty, GUIContent.none);
            if (customProperty != null && enumProperty.enumValueIndex == targetCustomId)
            {
                EditorGUILayout.PropertyField(customProperty, GUIContent.none);
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
    }
}