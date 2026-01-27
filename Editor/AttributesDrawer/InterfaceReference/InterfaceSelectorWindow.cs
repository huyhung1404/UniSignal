using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace UniCore.Editor.AttributesDrawer
{
    public class InterfaceSelectorWindow : EditorWindow
    {
        public class Tab : Toggle
        {
            public Tab(string text)
            {
                this.text = text;
                RemoveFromClassList(ussClassName);
                AddToClassList(ussClassName);
            }
        }

        public class ToggleGroup
        {
            private readonly List<Toggle> toggles = new List<Toggle>();

            public event EventHandler<Toggle> OnToggleChanged;

            public void RegisterToggle(Toggle toggle)
            {
                if (toggle == null || toggles.Contains(toggle)) return;

                toggles.Add(toggle);
                toggle.RegisterValueChangedCallback(ToggleValueChanged);
            }

            public void UnregisterToggle(Toggle toggle)
            {
                if (!toggles.Remove(toggle)) return;

                toggle.UnregisterValueChangedCallback(ToggleValueChanged);
            }

            public void Validate()
            {
                if (toggles.Count == 0) return;

                var activeToggle = GetFirstActiveToggle();
                if (activeToggle == null)
                {
                    activeToggle = toggles[0];
                    activeToggle.value = true;
                }

                foreach (var toggle in toggles.Where(toggle => toggle.value))
                {
                    toggle.SetValueWithoutNotify(false);
                }
            }

            public Toggle GetFirstActiveToggle()
            {
                return toggles.Find(x => x.value);
            }

            public bool IsAnyOn()
            {
                return GetFirstActiveToggle() != null;
            }

            private void ToggleValueChanged(ChangeEvent<bool> evt)
            {
                HandleToggleChanged(evt.target as Toggle);
            }

            private void HandleToggleChanged(Toggle targetToggle)
            {
                ValidateToggleIsInGroup(targetToggle);

                foreach (var toggle in toggles.Where(toggle => toggle != targetToggle))
                {
                    toggle.SetValueWithoutNotify(false);
                }

                if (targetToggle.value)
                    OnToggleChanged?.Invoke(this, targetToggle);
                else
                    targetToggle.value = true;
            }

            private void ValidateToggleIsInGroup(Toggle toggle)
            {
                if (toggle == null || !toggles.Contains(toggle))
                    throw new ArgumentException(string.Format("Toggle {0} is not part of ToggleGroup {1}", new object[] { toggle, this }));
            }
        }

        public class ItemInfo
        {
            public Texture Icon;
            public int? InstanceID;
            public string Label;
        }

        internal static InterfaceSelectorWindow Instance { get; private set; }
        private static readonly ItemInfo nullItem = new ItemInfo() { InstanceID = null, Label = "None" };
        private Action<Object> selectionChangedCallback;
        private Action<Object, bool> selectorClosedCallback;
        private ObjectSelectorFilter filter;
        private SerializedProperty editingProperty;
        private List<ItemInfo> allItems;
        private List<ItemInfo> filteredItems;
        private ItemInfo currentItem;
        private string searchText;
        private bool userCanceled;
        private bool showSceneObjects = true;
        private int undoGroup;
        private ToolbarSearchField searchBox;
        private ListView listView;
        private Label detailsLabel;
        private Label detailsIndexLabel;
        private Label detailsTypeLabel;
        private Tab sceneTab;
        private Tab assetsTab;

        public string SearchText
        {
            get => searchText;
            set
            {
                searchText = value;
                FilterItems();
            }
        }

        public static void Show(SerializedProperty property,
            Action<Object> onSelectionChanged,
            Action<Object, bool> onSelectorClosed,
            ObjectSelectorFilter filter,
            Type objectType)
        {
            if (Instance == null) Instance = CreateInstance<InterfaceSelectorWindow>();
            var isScriptableObject = objectType.IsSubclassOf(typeof(ScriptableObject)) || objectType == typeof(ScriptableObject);
            Instance.showSceneObjects = !isScriptableObject;
            Instance.editingProperty = property;
            Instance.selectionChangedCallback = onSelectionChanged;
            Instance.selectorClosedCallback = onSelectorClosed;
            Instance.filter = filter;
            Instance.Init();
            Instance.ShowAuxWindow();
        }

        private void Init()
        {
            InitData();
            InitVisualElements();
            BindVisualElements();
            FinishInit();
        }

        private void InitData()
        {
            undoGroup = Undo.GetCurrentGroup();
            searchText = "";
            allItems = new List<ItemInfo>();
            filteredItems = new List<ItemInfo>();

            var target = editingProperty.objectReferenceValue;
            if (target != null) showSceneObjects = !AssetDatabase.Contains(target);

            PopulateItems();
            FilterItems();
        }

        private void InitVisualElements()
        {
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.huyhung1404.unicore/Editor/AttributesDrawer/InterfaceReference/Style.uss");

            rootVisualElement.styleSheets.Add(styleSheet);

            searchBox = new ToolbarSearchField();
            searchBox.RegisterValueChangedCallback(SearchFilterChanged);
            rootVisualElement.Add(searchBox);

            var tabContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row
                }
            };
            assetsTab = new Tab("Assets");
            sceneTab = new Tab("Scene");
            tabContainer.Add(assetsTab);
            tabContainer.Add(sceneTab);
            rootVisualElement.Add(tabContainer);

            listView = new ListView(filteredItems, 16, MakeItem, BindItem);
            listView.selectionChanged += ItemSelectionChanged;
            listView.itemsChosen += ItemsChosen;
            rootVisualElement.Add(listView);

            detailsLabel = new Label();
            detailsTypeLabel = new Label();
            detailsIndexLabel = new Label();

            var details = new VisualElement();
            details.AddToClassList("details");
            details.Add(detailsLabel);
            details.Add(detailsIndexLabel);
            details.Add(detailsTypeLabel);
            rootVisualElement.Add(details);
        }

        private void BindVisualElements()
        {
            var activeTab = showSceneObjects ? sceneTab : assetsTab;
            activeTab.SetValueWithoutNotify(true);

            var toggleGroup = new ToggleGroup();
            toggleGroup.RegisterToggle(assetsTab);
            toggleGroup.RegisterToggle(sceneTab);
            toggleGroup.OnToggleChanged += HandleGroupChanged;

            if (GetIndexOfEditingPropertyValue(out var index))
                listView.selectedIndex = index;
        }

        private void FinishInit()
        {
            EditorApplication.delayCall += () => { listView.Focus(); };
        }

        private bool GetIndexOfEditingPropertyValue(out int index)
        {
            index = -1;
            var targetObject = editingProperty.objectReferenceValue;
            if (targetObject)
            {
                var instanceID = targetObject.GetInstanceID();
                index = filteredItems.FindIndex(x => x.InstanceID == instanceID);
            }

            return index >= 0;
        }

        private bool GetIndexOfCurrentItem(out int index)
        {
            index = -1;
            if (currentItem != null)
                index = filteredItems.FindIndex(0, x => x.InstanceID == currentItem.InstanceID);
            return index >= 0;
        }

        private void HandleGroupChanged(object sender, Toggle toggle)
        {
            if (showSceneObjects && toggle == this.sceneTab) return;
            showSceneObjects = !showSceneObjects;
            PopulateItems();
            FilterItems();
            var list = new List<int>();
            if (GetIndexOfCurrentItem(out var index)) list.Add(index);
            listView.SetSelectionWithoutNotify(list);
            listView.Focus();
        }

        private void OnDisable()
        {
            selectorClosedCallback?.Invoke(GetCurrentObject(), userCanceled);
            if (userCanceled)
                Undo.RevertAllDownToGroup(undoGroup);
            else
                Undo.CollapseUndoOperations(undoGroup);
            Instance = null;
        }

        private void PopulateItems()
        {
            allItems.Clear();
            filteredItems.Clear();
            allItems.AddRange(showSceneObjects ? FetchAllComponents() : FetchAllAssets());
            allItems.Sort((item, other) => string.Compare(item.Label, other.Label, StringComparison.Ordinal));
        }

        private void SearchFilterChanged(ChangeEvent<string> evt)
        {
            SearchText = evt.newValue;
        }

        private void FilterItems()
        {
            filteredItems.Clear();
            filteredItems.Add(nullItem);
            filteredItems.AddRange(allItems.Where(item =>
                string.IsNullOrEmpty(SearchText) || item.Label.IndexOf(SearchText, StringComparison.InvariantCultureIgnoreCase) >= 0));

            listView?.Rebuild();
        }

        private void BindItem(VisualElement listItem, int index)
        {
            if (index < 0 || index >= filteredItems.Count)
                return;

            var label = listItem.Q<Label>();
            if (label != null)
                label.text = filteredItems[index].Label;
            var image = listItem.Q<Image>();
            image.image = filteredItems[index].Icon;
        }

        private static VisualElement MakeItem()
        {
            var ve = new VisualElement();
            var image = new Image();
            var label = new Label();
            ve.Add(image);
            ve.Add(label);

            ve.AddToClassList("list-item");
            label.AddToClassList("list-item__text");
            image.AddToClassList("list-item__icon");

            return ve;
        }

        private void ItemSelectionChanged(IEnumerable<object> selectedItems)
        {
            currentItem = selectedItems.FirstOrDefault() as ItemInfo;
            UpdateDetails();
            selectionChangedCallback?.Invoke(GetCurrentObject());
        }

        private void ItemsChosen(IEnumerable<object> selectedItems)
        {
            currentItem = selectedItems.FirstOrDefault() as ItemInfo;
            userCanceled = false;
            Close();
        }

        private void UpdateDetails()
        {
            GetText(currentItem, out var infoText, out var indexText, out var typeText);

            void SetText(Label label, string text)
            {
                label.text = String.IsNullOrEmpty(text) ? "" : text;
            }

            SetText(detailsLabel, infoText);
            SetText(detailsIndexLabel, indexText);
            SetText(detailsTypeLabel, typeText);
        }

        private static void GetText(ItemInfo itemInfo, out string text, out string indexText, out string typeText)
        {
            text = null;
            indexText = null;
            typeText = null;

            if (itemInfo == null) return;
            if (itemInfo.InstanceID == null)
            {
                text = itemInfo.Label;
                return;
            }

            var obj = EditorUtility.InstanceIDToObject((int)itemInfo.InstanceID);
            if (AssetDatabase.Contains(obj))
            {
                text = AssetDatabase.GetAssetPath(obj);
            }
            else
            {
                var transform = obj is GameObject go ? go.transform : (obj as Component)?.transform;
                // ReSharper disable once CoVariantArrayConversion
                // ReSharper disable once PossibleNullReferenceException
                var compIndex = Array.IndexOf(transform.gameObject.GetComponents(typeof(Component)), obj);
                text = $"{GetTransformPath(transform)}";
                indexText = $"[{compIndex}]";
            }

            typeText = $"({obj.GetType().Name})";
        }

        private static string GetTransformPath(Transform transform)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(transform.name);
            while (transform.parent != null)
            {
                var parent = transform.parent;
                sb.Insert(0, parent.name + "/");
                transform = parent;
            }

            return sb.ToString();
        }

        private IEnumerable<ItemInfo> FetchAllAssets()
        {
            var property = new HierarchyProperty(HierarchyType.Assets, false);
            property.SetSearchFilter(filter.AssetSearchFilter, 0);

            while (property.Next(null))
            {
                yield return new ItemInfo { Icon = property.icon, InstanceID = property.instanceID, Label = property.name };
            }
        }

        private IEnumerable<ItemInfo> FetchAllComponents()
        {
            var property = new HierarchyProperty(HierarchyType.GameObjects, false);

            while (property.Next(null))
            {
                var go = property.pptrValue as GameObject;
                if (go == null) continue;

                if (CheckFilter(go))
                    yield return new ItemInfo { Icon = property.icon, InstanceID = property.instanceID, Label = property.name };

                foreach (var comp in go.GetComponents(typeof(Component)))
                {
                    if (CheckFilter(comp))
                        yield return new ItemInfo
                            { Icon = EditorGUIUtility.ObjectContent(comp, comp.GetType()).image, InstanceID = comp.GetInstanceID(), Label = property.name };
                }
            }
        }

        private bool CheckFilter(Object obj)
        {
            var matchFilterConstraint = filter.SceneFilterCallback?.Invoke(obj);
            return (!matchFilterConstraint.HasValue || matchFilterConstraint.Value);
        }

        private Object GetCurrentObject()
        {
            if (currentItem == null || currentItem.InstanceID == null) return null;
            return EditorUtility.InstanceIDToObject((int)currentItem.InstanceID);
        }
    }

    public class ObjectSelectorFilter
    {
        public readonly string AssetSearchFilter;
        public readonly Func<Object, bool> SceneFilterCallback;

        public ObjectSelectorFilter() : this("", _ => true)
        {
        }

        public ObjectSelectorFilter(string assetSearchFilter, Func<Object, bool> sceneFilterCallback)
        {
            AssetSearchFilter = assetSearchFilter;
            SceneFilterCallback = sceneFilterCallback;
        }
    }
}