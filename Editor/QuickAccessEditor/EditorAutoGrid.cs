using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UniCore.Editor.QuickAccess
{
    internal static class EditorAutoGrid
    {
        private const float k_MenuWidth = 18f;
        private const float k_Width = 100f;
        private const float k_Height = 21f;
        private static readonly GUIContent moreIcon = EditorGUIUtility.IconContent("_Menu");
        private static GUIStyle gridBgStyle;
        private static GUIStyle buttonStyle;
        private static GUIStyle labelStyle;

        private static void InitGridStyle()
        {
            if (gridBgStyle != null) return;

            gridBgStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(6, 6, 6, 6),
                margin = new RectOffset(4, 4, 0, 4)
            };

            buttonStyle = new GUIStyle("Button");

            labelStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                clipping = TextClipping.Clip,
                wordWrap = false
            };
        }

        public static void DrawGrid(List<AssetAddress> items, string search,
            System.Action<AssetAddress> onClick,
            System.Action<AssetAddress> onEdit)
        {
            InitGridStyle();

            if (!string.IsNullOrEmpty(search)) items = items.Where(i => GetLabel(i).ToLower().Contains(search.ToLower())).ToList();

            EditorGUILayout.BeginVertical(gridBgStyle);

            var width = EditorGUIUtility.currentViewWidth - 32f;
            var columns = Mathf.Max(1, Mathf.FloorToInt(width / k_Width));
            var rows = Mathf.CeilToInt(items.Count / (float)columns);

            for (var r = 0; r < rows; r++)
            {
                var rowRect = GUILayoutUtility.GetRect(0, k_Height);

                const float spacing = 4f;
                var itemWidth = (rowRect.width - (columns - 1) * spacing) / columns;

                for (var c = 0; c < columns; c++)
                {
                    var index = r * columns + c;
                    if (index >= items.Count) break;

                    var rect = new Rect(rowRect.x + c * (itemWidth + spacing), rowRect.y, itemWidth, k_Height);
                    DrawSplitItem(rect, items[index], onClick, onEdit);
                }

                if (r < rows - 1) GUILayout.Space(spacing);
            }

            EditorGUILayout.EndVertical();
        }

        private static void DrawSplitItem(Rect rect, AssetAddress a,
            System.Action<AssetAddress> onMainClick,
            System.Action<AssetAddress> onMenuClick)
        {
            var evt = Event.current;

            GUI.Box(rect, GUIContent.none, buttonStyle);

            var mainRect = new Rect(rect.x, rect.y, rect.width - k_MenuWidth, rect.height);
            var menuRect = new Rect(rect.x + rect.width - k_MenuWidth, rect.y, k_MenuWidth, rect.height);

            var fullLabel = GetAssetLabel(a);

            var truncated = Truncate(string.IsNullOrEmpty(a.name) ? fullLabel : a.name, labelStyle, mainRect.width - 6);
            GUI.Label(mainRect, new GUIContent(truncated, fullLabel), labelStyle);

            GUI.Label(menuRect, moreIcon, new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter
            });

            if (rect.Contains(evt.mousePosition))
                EditorGUI.DrawRect(rect, new Color(1, 1, 1, 0.04f));

            if (evt.type == EventType.MouseDown && rect.Contains(evt.mousePosition))
            {
                if (menuRect.Contains(evt.mousePosition))
                    onMenuClick?.Invoke(a);
                else
                    onMainClick?.Invoke(a);

                evt.Use();
            }
        }

        private static string GetLabel(AssetAddress a)
        {
            var path = AssetDatabase.GUIDToAssetPath(a.guidAsset);
            var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
            return string.IsNullOrEmpty(a.name) ? obj?.name ?? "Missing" : a.name;
        }

        private static string GetAssetLabel(AssetAddress a)
        {
            var path = AssetDatabase.GUIDToAssetPath(a.guidAsset);
            var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
            return obj?.name ?? "Missing";
        }

        private static string Truncate(string text, GUIStyle style, float width)
        {
            if (style.CalcSize(new GUIContent(text)).x <= width) return text;

            while (text.Length > 0 && style.CalcSize(new GUIContent(text + "...")).x > width)
                text = text.Substring(0, text.Length - 1);

            return text + "...";
        }
    }
}