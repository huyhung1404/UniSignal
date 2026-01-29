using System;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace UniCore.Editor.QuickAccess
{
    public class QuickAccessApplyPopup : EditorWindow
    {
        private string json;

        public static void Open(Rect parentWindowRect)
        {
            var w = CreateInstance<QuickAccessApplyPopup>();
            var size = new Vector2(260, 120);
            var x = parentWindowRect.x + parentWindowRect.width - size.x;
            var y = parentWindowRect.y;
            w.position = new Rect(x, y, size.x, size.y);
            w.ShowPopup();
            w.Focus();
        }

        private void OnLostFocus() => Close();

        private void OnGUI()
        {
            PopupGUI.BeginPopup();

            json = EditorGUILayout.TextArea(json, GUILayout.Height(80));
            
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Apply"))
            {
                try
                {
                    if (string.IsNullOrEmpty(json)) throw new Exception("Json is empty");
                    var db = JsonConvert.DeserializeObject<QuickAccessDB>(json);
                    QuickAccessStorage.Save(db);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                finally
                {
                    Close();
                }
            }

            PopupGUI.EndPopup();
        }
    }
}