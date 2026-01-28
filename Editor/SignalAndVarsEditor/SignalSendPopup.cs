using System;
using System.Reflection;
using UniCore.Signal;
using UnityEditor;
using UnityEngine;

namespace UniCore.Editor
{
    public class SignalSendPopup : EditorWindow
    {
        private Type signalType;
        private object signalInstance;
        private SignalScope scope = SignalScope.All;
        private int fieldCount;

        public static void Open(Type signalType)
        {
            var window = CreateInstance<SignalSendPopup>();
            window.signalType = signalType;
            window.signalInstance = Activator.CreateInstance(signalType);
            window.titleContent = new GUIContent($"Send {signalType.Name}");
            window.CalculateFieldCount();
            window.FitSizeToContent();
            window.ShowUtility();
        }

        private void CalculateFieldCount()
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;

            fieldCount = 0;

            foreach (var field in signalType.GetFields(flags))
            {
                if (!field.IsInitOnly)
                    fieldCount++;
            }

            foreach (var prop in signalType.GetProperties(flags))
            {
                if (prop.CanWrite && prop.GetIndexParameters().Length == 0)
                    fieldCount++;
            }

            fieldCount += 1;
        }

        private void FitSizeToContent()
        {
            var line = EditorGUIUtility.singleLineHeight;
            const float spacing = 4f;
            
            var height = line * 1.5f + spacing * 4f;
            
            height += fieldCount * (line + spacing);
            height += line * 2f;
            
            height += 12f;

            const float width = 360f;
            
            if (fieldCount > 12)
            {
                minSize = new Vector2(width, 400);
                maxSize = new Vector2(width, 600);
            }
            else
            {
                minSize = maxSize = new Vector2(width, height);
            }
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
            var v = EditorExtensions.DrawDefaultValue(type, new GUIContent(label), value);
            if (v.Item1) return v.Item2;
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
            var method = typeof(SignalSystem)
                .GetMethod("Dispatch", new[] { signalType, typeof(SignalScope) })
                ?.MakeGenericMethod(signalType);

            method?.Invoke(null, new[] { signalInstance, scope });
        }
    }
}