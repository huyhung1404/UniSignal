using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UniCore.Editor
{
    public class ObjectEditPopup : EditorWindow
    {
        private object target;
        private Type targetType;
        private Action<object> onApply;
        private int fieldCount;

        public static void Open(string title, object value, Action<object> onApply)
        {
            var window = CreateInstance<ObjectEditPopup>();
            if (value != null)
            {
                window.target = Clone(value);
                window.targetType = value.GetType();
            }

            window.onApply = onApply;
            window.titleContent = new GUIContent(title);
            window.CalculateFieldCount();
            window.FitSizeToContent();
            window.ShowUtility();
        }

        private void CalculateFieldCount()
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;

            fieldCount = 0;

            foreach (var field in targetType.GetFields(flags))
            {
                if (!field.IsInitOnly) fieldCount++;
            }

            foreach (var prop in targetType.GetProperties(flags))
            {
                if (prop.CanWrite && prop.GetIndexParameters().Length == 0) fieldCount++;
            }
        }

        private void FitSizeToContent()
        {
            var line = EditorGUIUtility.singleLineHeight;
            const float spacing = 4f;
            var height = line * 1.5f + spacing * 4f;
            height += fieldCount * (line + spacing);
            height += line * 1.8f;

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
            EditorGUILayout.LabelField(targetType.Name, EditorStyles.boldLabel);
            EditorGUILayout.Space();

            DrawFields();
            
            GUILayout.FlexibleSpace();
            if (!GUILayout.Button("Apply")) return;
            onApply?.Invoke(target);
            Close();
        }

        private void DrawFields()
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;

            foreach (var field in targetType.GetFields(flags))
            {
                if (field.IsInitOnly) continue;

                var value = field.GetValue(target);
                var newValue = DrawValue(field.FieldType, field.Name, value);

                if (!Equals(value, newValue))
                    field.SetValue(target, newValue);
            }

            foreach (var prop in targetType.GetProperties(flags))
            {
                if (!prop.CanWrite || prop.GetIndexParameters().Length > 0)
                    continue;

                var value = prop.GetValue(target);
                var newValue = DrawValue(prop.PropertyType, prop.Name, value);

                if (!Equals(value, newValue))
                    prop.SetValue(target, newValue);
            }
        }

        private static object DrawValue(Type type, string label, object value)
        {
            var v = EditorExtensions.DrawDefaultValue(type, new GUIContent(label), value);
            if (v.Item1) return v.Item2;
            EditorGUILayout.LabelField(label, $"(Unsupported: {type.Name})");
            return value;
        }

        private static object Clone(object source)
        {
            if (source is ICloneable cloneable) return cloneable.Clone();
            return source;
        }
    }
}