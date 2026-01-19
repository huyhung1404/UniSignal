using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UniSignal.Editor
{
    public class SignalDebugWindow : EditorWindow
    {
        private Vector2 scroll;
        private Vector2 historyScroll;
        private double lastRepaint;
        private string sceneFilter = string.Empty;
        private string scopeFilter = string.Empty;
        private string signalFilter = string.Empty;
        private string listenerFilter = string.Empty;
        private bool autoRefresh = true;
        private readonly Dictionary<Type, bool> foldouts = new();

        [MenuItem("Window/UniSignal")]
        public static void Open()
        {
            GetWindow<SignalDebugWindow>("Signal");
        }

        private void OnEnable()
        {
            autoRefresh = EditorPrefs.GetBool("UniSignal.AutoRefresh", true);
            EditorApplication.update += UpdateLoop;
        }

        private void OnDisable()
        {
            EditorPrefs.SetBool("UniSignal.AutoRefresh", autoRefresh);
            EditorApplication.update -= UpdateLoop;
        }

        private void UpdateLoop()
        {
            if (!autoRefresh) return;
            if (!(EditorApplication.timeSinceStartup - lastRepaint > 0.2f)) return;
            lastRepaint = EditorApplication.timeSinceStartup;
            Repaint();
        }

        private void OnGUI()
        {
            DrawToolbar();

            scroll = EditorGUILayout.BeginScrollView(scroll);
            foreach (var kvp in SignalBus.listeners)
            {
                if (!PassSignalFilter(kvp.Key)) continue;
                DrawSignalType(kvp.Key, kvp.Value);
            }

            EditorGUILayout.EndScrollView();

            DrawDispatchHistory();
        }

        private bool PassSignalFilter(Type signalType)
        {
            return string.IsNullOrEmpty(signalFilter) || signalType.Name.Contains(signalFilter, StringComparison.OrdinalIgnoreCase);
        }

        #region UI Sections

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                autoRefresh = GUILayout.Toggle(autoRefresh, "Auto Refresh", EditorStyles.toolbarButton);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Release Empty Lists", EditorStyles.toolbarButton)) SignalBus.ReleaseEmptyLists();
                if (GUILayout.Button("Clear All", EditorStyles.toolbarButton)) SignalBus.Clear();
            }

            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUILayout.LabelField("Signal", EditorStyles.miniBoldLabel, GUILayout.MaxWidth(36));
                signalFilter = GUILayout.TextField(signalFilter, EditorStyles.toolbarSearchField);

                EditorGUILayout.LabelField("Listener", EditorStyles.miniBoldLabel, GUILayout.MaxWidth(45));
                listenerFilter = GUILayout.TextField(listenerFilter, EditorStyles.toolbarSearchField);

                EditorGUILayout.LabelField("Scene", EditorStyles.miniBoldLabel, GUILayout.MaxWidth(36));
                sceneFilter = GUILayout.TextField(sceneFilter, EditorStyles.toolbarSearchField);

                EditorGUILayout.LabelField("Scope", EditorStyles.miniBoldLabel, GUILayout.MaxWidth(36));
                scopeFilter = GUILayout.TextField(scopeFilter, EditorStyles.toolbarSearchField);
            }
        }

        private void DrawSignalType(Type signalType, IList list)
        {
            foldouts.TryAdd(signalType, true);

            EditorGUILayout.BeginVertical("box");

            using (new EditorGUILayout.HorizontalScope())
            {
                foldouts[signalType] = EditorGUILayout.Foldout(foldouts[signalType], $"{signalType.Name} ({list.Count})", true);

                if (GUILayout.Button("Send", GUILayout.Width(60)))
                {
                    SignalSendPopup.Open(signalType);
                }
            }

            if (foldouts[signalType])
            {
                EditorGUI.indentLevel++;
                for (var i = 0; i < list.Count; i++)
                {
                    if (!PassFilter(list[i])) continue;
                    DrawListener(signalType, list[i]);
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }

        private bool PassFilter(object listener)
        {
            if (!string.IsNullOrEmpty(listenerFilter))
            {
                if (!listener.GetType().Name
                        .Contains(listenerFilter, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            if (!string.IsNullOrEmpty(sceneFilter))
            {
                if (listener is MonoBehaviour mb)
                {
                    var sceneName = mb.gameObject.scene.name;
                    if (!sceneName.Contains(sceneFilter, StringComparison.OrdinalIgnoreCase)) return false;
                }
                else
                {
                    return false;
                }
            }

            if (string.IsNullOrEmpty(scopeFilter)) return true;
            foreach (var itf in listener.GetType().GetInterfaces())
            {
                if (!itf.IsGenericType || itf.GetGenericTypeDefinition() != typeof(ISignalListener<>)) continue;
                var scope = itf.GetProperty("ListenScope")?.GetValue(listener);
                if (scope == null) return false;
                var value = SignalScopeRegistry.GetReadableScope((SignalScope)scope);
                if (!value.Contains(scopeFilter, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            return true;
        }

        private static void DrawListener(Type signalType, object listener)
        {
            var priority = GetListenerPriority(listener);
            var oldColor = GUI.color;
            GUI.color = GetPriorityColor(priority);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(listener.GetType().Name, GUILayout.Width(150)))
                {
                    if (SignalDebugUtil.TryGetUnityObject(listener, out var obj))
                    {
                        EditorGUIUtility.PingObject(obj);
                        Selection.activeObject = obj;
                    }
                }

                EditorGUILayout.LabelField(SignalDebugUtil.GetSource(listener), GUILayout.Width(80));

                DrawListenerMeta(listener);

                GUILayout.FlexibleSpace();
                if (GUILayout.Button("X", GUILayout.Width(EditorGUIUtility.singleLineHeight)))
                {
                    UnregisterListener(signalType, listener);
                }
            }

            GUI.color = oldColor;
        }

        private static int GetListenerPriority(object listener)
        {
            foreach (var itf in listener.GetType().GetInterfaces())
            {
                if (!itf.IsGenericType ||
                    itf.GetGenericTypeDefinition() != typeof(ISignalListener<>))
                    continue;

                var prop = itf.GetProperty("Priority");
                if (prop != null) return (int)prop.GetValue(listener);
            }

            return 0;
        }

        private static Color GetPriorityColor(int priority)
        {
            priority = Mathf.Clamp(priority, 0, 100);

            var white = Color.white;
            var green = new Color(0.7f, 1f, 0.7f);
            var yellow = new Color(1f, 1f, 0.6f);
            var red = new Color(1f, 0.6f, 0.6f);

            switch (priority)
            {
                case <= 10:
                {
                    var t = priority / 10f;
                    return Color.Lerp(white, green, t);
                }
                case <= 50:
                {
                    var t = (priority - 10f) / 40f;
                    return Color.Lerp(green, yellow, t);
                }
                default:
                {
                    var tFinal = (priority - 50f) / 50f;
                    return Color.Lerp(yellow, red, tFinal);
                }
            }
        }

        private static void DrawListenerMeta(object listener)
        {
            foreach (var itf in listener.GetType().GetInterfaces())
            {
                if (!itf.IsGenericType || itf.GetGenericTypeDefinition() != typeof(ISignalListener<>)) continue;
                var priority = itf.GetProperty("Priority")?.GetValue(listener);
                var scope = itf.GetProperty("ListenScope")?.GetValue(listener);
                var scopeText = scope != null ? SignalScopeRegistry.GetReadableScope((SignalScope)scope) : string.Empty;
                EditorGUILayout.LabelField($"P: {priority} | S: {scopeText}");
            }
        }

        private void DrawDispatchHistory()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Dispatch History", EditorStyles.boldLabel);

            historyScroll = EditorGUILayout.BeginScrollView(historyScroll, GUILayout.Height(120));
            foreach (var record in SignalDispatchHistory.Records) EditorGUILayout.LabelField(record);
            EditorGUILayout.EndScrollView();
            if (GUILayout.Button("Clear History")) SignalDispatchHistory.Clear();
        }

        #endregion

        #region Unregister

        private static void UnregisterListener(Type signalType, object listener)
        {
            var method = typeof(SignalBus).GetMethod("Unregister")?.MakeGenericMethod(signalType);
            method?.Invoke(null, new[] { listener });
        }

        #endregion
    }
}