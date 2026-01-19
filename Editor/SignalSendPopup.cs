using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UniSignal.Editor
{
    public class SignalSendPopup : EditorWindow
    {
        private Type signalType;
        private object signalInstance;
        private SignalScope scope = SignalScope.All;

        public static void Open(Type signalType)
        {
            var window = CreateInstance<SignalSendPopup>();
            window.signalType = signalType;
            window.signalInstance = Activator.CreateInstance(signalType);
            window.titleContent = new GUIContent($"Send {signalType.Name}");
            window.ShowUtility();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Properties", EditorStyles.boldLabel);
            DrawSignalFields();

            EditorGUILayout.Space();
            DrawScopeField();

            EditorGUILayout.Space();
            if (!GUILayout.Button("Send Signal")) return;
            Send();
            Close();
        }

        private void DrawSignalFields()
        {
            EditorGUILayout.BeginVertical("box");

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;

            foreach (var field in signalType.GetFields(flags))
            {
                if (field.IsInitOnly) continue;

                var value = field.GetValue(signalInstance);
                var newValue = DrawValue(field.FieldType, field.Name, value);

                if (!Equals(value, newValue))
                    field.SetValue(signalInstance, newValue);
            }

            foreach (var prop in signalType.GetProperties(flags))
            {
                if (!prop.CanWrite || prop.GetIndexParameters().Length > 0)
                    continue;

                var value = prop.GetValue(signalInstance);
                var newValue = DrawValue(prop.PropertyType, prop.Name, value);

                if (!Equals(value, newValue))
                    prop.SetValue(signalInstance, newValue);
            }

            EditorGUILayout.EndVertical();
        }

        private static object DrawValue(Type type, string label, object value)
        {
            if (type == typeof(int))
                return EditorGUILayout.IntField(label, (int)(value ?? 0));

            if (type == typeof(float))
                return EditorGUILayout.FloatField(label, (float)(value ?? 0f));

            if (type == typeof(bool))
                return EditorGUILayout.Toggle(label, (bool)(value ?? false));

            if (type == typeof(string))
                return EditorGUILayout.TextField(label, (string)value ?? string.Empty);

            if (type == typeof(Vector3))
                return EditorGUILayout.Vector3Field(label, value != null ? (Vector3)value : Vector3.zero);

            EditorGUILayout.LabelField(label, $"(Unsupported: {type.Name})");
            return value;
        }

        private void DrawScopeField()
        {
            EditorGUILayout.LabelField("Scope", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");

            var entries = SignalScopeRegistry.scopeNames;
            var currentMask = scope.Mask;

            foreach (var kvp in entries)
            {
                var mask = kvp.Key;
                var vName = kvp.Value;

                if (mask == ulong.MaxValue)
                {
                    var isAll = currentMask == ulong.MaxValue;
                    var newAll = EditorGUILayout.ToggleLeft("All", isAll);

                    if (newAll)
                    {
                        currentMask = ulong.MaxValue;
                    }
                    else if (isAll)
                    {
                        currentMask = 0;
                    }

                    EditorGUILayout.Space(4);
                    continue;
                }

                var hasFlag = (currentMask & mask) != 0;
                var newHasFlag = EditorGUILayout.ToggleLeft(vName, hasFlag);

                if (newHasFlag != hasFlag)
                {
                    if (currentMask == ulong.MaxValue)
                        currentMask = 0;

                    if (newHasFlag)
                        currentMask |= mask;
                    else
                        currentMask &= ~mask;
                }
            }

            scope = currentMask == 0 ? new SignalScope(0) : new SignalScope(currentMask);
            EditorGUILayout.HelpBox($"Selected: {SignalScopeRegistry.GetReadableScope(scope)}", MessageType.Info);
            EditorGUILayout.EndVertical();
        }

        private void Send()
        {
            var method = typeof(SignalBus)
                .GetMethod("Dispatch", new[] { signalType, typeof(SignalScope) })
                ?.MakeGenericMethod(signalType);

            method?.Invoke(null, new[] { signalInstance, scope });
        }
    }
}