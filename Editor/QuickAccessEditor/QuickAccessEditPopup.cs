using UnityEditor;
using UnityEngine;

namespace UniCore.Editor.QuickAccess
{
    public class QuickAccessEditPopup : EditorWindow
    {
        private AssetAddress asset;
        private Color colorValue;
        private bool isFavorite;

        public static void Open(AssetAddress a, bool isFav)
        {
            var w = CreateInstance<QuickAccessEditPopup>();
            w.asset = a;
            w.isFavorite = isFav;
            var mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            var size = new Vector2(260, 60);
            w.position = new Rect(mousePos.x, mousePos.y, size.x, size.y);
            w.ShowPopup();
        }

        public void OnLostFocus() => Close();

        private void OnGUI()
        {
            PopupGUI.BeginPopup();

            var db = QuickAccessStorage.Database();
            
            EditorGUI.BeginChangeCheck();
            asset.name = EditorGUILayout.TextField("Name", asset.name);
            if (EditorGUI.EndChangeCheck())
            {
                QuickAccessStorage.Save(db);
                RepaintAll();
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Delete"))
            {
                if (isFavorite)
                {
                    db.stats.RemoveAll(s => s.guid == asset.guidAsset);
                }
                else
                {
                    foreach (var g in db.groups)
                        g.assets.Remove(asset);

                    db.stats.RemoveAll(s => s.guid == asset.guidAsset);
                }

                QuickAccessStorage.Save(db);
                Close();
            }

            PopupGUI.EndPopup();
        }

        private static void RepaintAll()
        {
            foreach (var w in Resources.FindObjectsOfTypeAll<QuickAccessWindow>()) w.Repaint();
        }
    }
}