using System;
using UnityEditor;
using UnityEngine;

namespace UniCore.Editor
{
    public static class EditorExtensions
    {
        public static (bool, object) DrawDefaultValue(Type type, GUIContent label, object value)
        {
            if (type == typeof(int))
                return (true, EditorGUILayout.IntField(label, (int)(value ?? 0)));

            if (type == typeof(float))
                return (true, EditorGUILayout.FloatField(label, (float)(value ?? 0f)));

            if (type == typeof(bool))
                return (true, EditorGUILayout.Toggle(label, (bool)(value ?? false)));

            if (type == typeof(string))
                return (true, EditorGUILayout.TextField(label, (string)value ?? string.Empty));

            if (type == typeof(Vector3))
                return (true, EditorGUILayout.Vector3Field(label, value != null ? (Vector3)value : Vector3.zero));

            return (false, null);
        }
    }
}