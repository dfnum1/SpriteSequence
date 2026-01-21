#if UNITY_EDITOR
/********************************************************************
生成日期:		11:07:2020
类    名: 	TreeAssetView
作    者:	HappLI
描    述:	树形视图列表
*********************************************************************/
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using System.Linq;

namespace Framework.ED
{
    //-------------------------------------------
    public class TreeAssetView : TreeView
    {
        public struct RowArgvData
        {
            public TreeItemData itemData;
            public string label;
            public Rect rowRect;
            public int row;
            public int column;
            public bool selected;
            public bool focused;
            public bool isRenaming;
        }
        //-------------------------------------------
        public class ItemData
        {
            public int id = 0;
            public string name = "";
            public string path = "";

            public int depth = 0;

            public ItemData parent = null;

            public virtual Color itemColor()
            {
                if (AssetDatabase.Contains(id))
                    return Color.white;
                return Color.red;
            }

            public virtual Texture2D itemIcon()
            {
                return null;
            }
        }
        //-------------------------------------------
        public sealed class TreeItemData : TreeViewItem
        {
            private ItemData m_data;
            public ItemData data
            {
                get { return m_data; }
            }
            internal TreeItemData() : base(-1, -1) { }
            internal TreeItemData(int id, int depth) : base(id, depth) { }
            internal TreeItemData(int id, int depth, string displayName) : base(id, depth, displayName) { }
            internal TreeItemData(ItemData a) : base(a != null ? a.id : UnityEngine.Random.Range(int.MinValue, int.MaxValue), 0, a != null ? a.id.ToString() : "failed")
            {
                m_data = a;
                displayName = a.name;
                depth = a.depth;
                if (a != null)
                {
                    icon = a.itemIcon();
                    if(icon == null)
                        icon = AssetDatabase.GetCachedIcon(a.name) as Texture2D;
                }
            }

            private Color m_color = Color.white;
            internal Color itemColor
            {
                get
                {
                    if (data == null) return Color.red;
                    return data.itemColor();
                }
                set { m_color = value; }
            }
            internal bool ContainsChild(ItemData asset)
            {
                bool contains = false;
                if (children == null)
                    return contains;

                if (asset == null)
                    return false;
                foreach (var child in children)
                {
                    var c = child as TreeItemData;
                    if (c != null && c.data != null && c.data.id == asset.id)
                    {
                        contains = true;
                        break;
                    }
                }

                return contains;
            }
        }

        internal static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState(float width)
        {
            return new MultiColumnHeaderState(GetColumns(width));
        }
        private static MultiColumnHeaderState.Column[] GetColumns(float width)
        {
            var retVal = new MultiColumnHeaderState.Column[] {
            new MultiColumnHeaderState.Column(),
          };
            retVal[0].headerContent = new GUIContent("ID", "Short name of asset. For full name select asset and see message below");
            retVal[0].minWidth = 50;
            retVal[0].width = 80;
            retVal[0].maxWidth = 80;
            retVal[0].headerTextAlignment = TextAlignment.Left;
            retVal[0].canSort = true;
            retVal[0].autoResize = true;

            retVal[1].headerContent = new GUIContent("Asset", "Short name of asset. For full name select asset and see message below");
            retVal[1].minWidth = 50;
            retVal[1].width = width - 80;
            retVal[1].maxWidth = width - 80;
            retVal[1].headerTextAlignment = TextAlignment.Left;
            retVal[1].canSort = true;
            retVal[1].autoResize = true;
            return retVal;
        }
        public delegate bool OnCellDrawEvent(RowArgvData argvData);
        public delegate bool OnItemDrag(TreeItemData data);
        public delegate List<TreeItemData> OnSortColumEvent(List<TreeItemData> vDatas, MultiColumnHeader header, int c);

        public OnCellDrawEvent OnCellDraw = null;

        public bool bDragItemGesture = true;
        public bool buildMutiColumnDepth = false;
        public float DepthIndentWidth
        {
            get { return depthIndentWidth; }
            set { depthIndentWidth = value; }
        }
        public bool scrollWhellIgnore = false;


        public Rect             discardRect = new Rect(0, 0, 0, 0);
        public Action<ItemData> OnRemoved = null;
        public Action<ItemData> OnSelectChange = null;
        public Action<ItemData> OnItemDoubleClick = null;
        public Action<ItemData> OnItemRightClick = null;
        public Action           OnViewRightClick = null;
        public OnItemDrag OnDragItem = null;
        public OnSortColumEvent OnSortColum = null;
        List<ItemData> m_SourceAssets = new List<ItemData>();
        //--------------------------------------------------------
        public TreeAssetView(TreeViewState state, MultiColumnHeaderState mchs) : base(state, new MultiColumnHeader(mchs))
        {
            this.ShowAlternatingRowBackgrounds(true);
            showBorder = true;
            showAlternatingRowBackgrounds = true;
            multiColumnHeader.sortingChanged += OnSortingChanged;
        }
        //--------------------------------------------------------
        public TreeAssetView(List<string> colums) : base(new UnityEditor.IMGUI.Controls.TreeViewState())
        {
            this.ShowAlternatingRowBackgrounds(true);
            if (colums!=null && colums.Count>0)
            {
                MultiColumnHeaderState.Column[] headerColums = new MultiColumnHeaderState.Column[colums.Count];
                for(int i =0; i < colums.Count; ++i)
                {
                    MultiColumnHeaderState.Column headItem = new MultiColumnHeaderState.Column();
                    headItem.headerContent = new GUIContent(colums[i], "");
                    headItem.headerTextAlignment = TextAlignment.Left;
                    headItem.canSort = false;
                    headItem.autoResize = true;
                    headerColums[i] = headItem;
                }
                var headerState = new MultiColumnHeaderState(headerColums);
                this.multiColumnHeader = new MultiColumnHeader(headerState);
            }
        }
        //--------------------------------------------------------
        public TreeAssetView(string[] colums) : base(new UnityEditor.IMGUI.Controls.TreeViewState(), new MultiColumnHeader(null))
        {
            this.ShowAlternatingRowBackgrounds(true);
            if (colums != null && colums.Length > 0)
            {
                MultiColumnHeaderState.Column[] headerColums = new MultiColumnHeaderState.Column[colums.Length];
                for (int i = 0; i < colums.Length; ++i)
                {
                    MultiColumnHeaderState.Column headItem = new MultiColumnHeaderState.Column();
                    headItem.headerContent = new GUIContent(colums[i], "");
                    headItem.headerTextAlignment = TextAlignment.Left;
                    headItem.canSort = false;
                    headItem.autoResize = true;
                    headerColums[i] = headItem;
                }
                var headerState = new MultiColumnHeaderState(headerColums);
                this.multiColumnHeader.state = headerState;
            }
        }
        //--------------------------------------------------------
        public TreeAssetView(TreeViewState state) : base(state)
        {
            showBorder = true;
            showAlternatingRowBackgrounds = true;
        }
        //--------------------------------------------------------
        public MultiColumnHeaderState.Column GetColumn(int index)
        {
            return this.multiColumnHeader.GetColumn(index);
        }
        //--------------------------------------------------------
        public bool ShowScrollVerticalBar
        {
            get { return showingVerticalScrollBar; }
        }
        //--------------------------------------------------------
        public bool ShowingHorizontalScrollBar
        {
            get { return showingHorizontalScrollBar; }
        }
        //--------------------------------------------------------
        public float GetContentHeight()
        {
            return this.totalHeight;
        }
        //--------------------------------------------------------
        RowArgvData BuildArgv(RowGUIArgs args, int column =0, TreeItemData itemData = null)
        {
            RowArgvData data = new RowArgvData();
            data.column = column;
            data.row = args.row;
            data.itemData = args.item as TreeItemData;
            if(itemData!=null) data.itemData = itemData;
            data.label = args.label;
            data.selected = args.selected;
            data.focused = args.focused;
            data.isRenaming = args.isRenaming;
            data.rowRect = args.rowRect;
            return data;
        }
        //--------------------------------------------------------
        public void SetCellMargin(float margin)
        {
            cellMargin = margin;
        }
        //--------------------------------------------------------
        public void SetRowHeight(float height)
        {
            rowHeight = height;
        }
        //-------------------------------------------
        public float GetRowHeight()
        {
            return rowHeight;
        }
        //-------------------------------------------
        public Rect GetRowRectEx(int row)
        {
            return base.GetRowRect(row);
        }
        //-------------------------------------------
        public Rect GetRect()
        {
            return base.treeViewRect;
        }
        //-------------------------------------------
        public void ShowBorder(bool bShowBorder)
        {
            showBorder = bShowBorder;
        }
        //-------------------------------------------
        public void ShowAlternatingRowBackgrounds(bool bShowAlternatingRowBackgrounds)
        {
            showAlternatingRowBackgrounds = bShowAlternatingRowBackgrounds;
        }
        //--------------------------------------------------------
        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            if (multiColumnHeader == null)
            {
                return base.BuildRows(root);
            }
            else
            {
                var rows = base.BuildRows(root);
                SortIfNeeded(root, rows);
                return rows;
            }
        }
        //--------------------------------------------------------
        protected override TreeViewItem BuildRoot()
        {
            if (multiColumnHeader == null)
                return CreateTree(m_SourceAssets);
            return CreateListTreeView(m_SourceAssets);
        }
        //--------------------------------------------------------
        internal TreeItemData CreateTree(IEnumerable<ItemData> selectedBundles)
        {
            var root = new TreeItemData(-1,-1, "root");
            List<TreeViewItem> vList = new List<TreeViewItem>();
            foreach(var db in selectedBundles)
            {
                TreeItemData child = new TreeItemData(db);
                vList.Add(child);
            }
            SetupParentsAndChildrenFromDepths(root, vList);
            return root;
        }
        //--------------------------------------------------------
        internal TreeItemData CreateListTreeView(IEnumerable<ItemData> selectedBundles)
        {
            var root = new TreeItemData(-1, -1, "root");
            if (selectedBundles != null)
            {
                if (buildMutiColumnDepth)
                {
                    List<TreeViewItem> vList = new List<TreeViewItem>();
                    foreach (var bundle in selectedBundles)
                    {
                        TreeItemData child = new TreeItemData(bundle);
                        vList.Add(child);
                    }
                    SetupParentsAndChildrenFromDepths(root, vList);
                }
                else
                {
                    foreach (var bundle in selectedBundles)
                    {
                        TreeItemData item = AddToNode(bundle, root);
                    }
                }
            }
            return root;
        }
        //--------------------------------------------------------
        public List<ItemData> GetDatas()
        {
            return m_SourceAssets;
        }
        //--------------------------------------------------
        public ItemData GetItem(int id)
        {
            for (int i = 0; i < m_SourceAssets.Count; ++i)
            {
                if (m_SourceAssets[i].id == id)
                    return m_SourceAssets[i];
            }
            return null;
        }
        //--------------------------------------------------------
        public void BeginTreeData()
        {
            m_SourceAssets.Clear();
            SetSelection(new List<int>());
        }
        //--------------------------------------------------------
        public void AddData(ItemData data)
        {
            m_SourceAssets.Add(data);
        }
        //--------------------------------------------------------
        public void EndTreeData()
        {
            Reload();
        }
        //--------------------------------------------------------
        internal TreeItemData AddToNode(ItemData asset, TreeItemData node)
        {
            if (!node.ContainsChild(asset))
            {
                TreeItemData child = new TreeItemData(asset);
                node.AddChild(child);

                return child;
            }
            return null;
        }
        //--------------------------------------------------------
        protected override void DoubleClickedItem(int id)
        {
            var assetItem = FindItem(id, rootItem) as TreeItemData;
            if (assetItem != null)
            {
                if(assetItem.data.path.Length>0)
                {
                    if (!assetItem.data.path.ToLower().Contains(Application.dataPath.ToLower()))
                    {
                        UnityEngine.Object o = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetItem.data.path);
                        EditorGUIUtility.PingObject(o);
                        Selection.activeObject = o;
                    }
                }

                if (OnItemDoubleClick != null) OnItemDoubleClick(assetItem.data);
            }
        }
        //--------------------------------------------------------
        protected override void ContextClickedItem(int id)
        {
            var assetItem = FindItem(id, rootItem) as TreeItemData;
            if (OnItemRightClick != null) OnItemRightClick(assetItem!=null?assetItem.data:null);
        }
        //--------------------------------------------------------
        protected override void ContextClicked()
        {
            if (OnViewRightClick != null) OnViewRightClick();
        }
        //--------------------------------------------------------
        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if (selectedIds == null)
                return;

            List<UnityEngine.Object> selectedObjects = new List<UnityEngine.Object>();
            List<ItemData> selectedAssets = new List<ItemData>();
            foreach (var id in selectedIds)
            {
                var assetItem = FindItem(id, rootItem) as TreeItemData;
                if (assetItem != null && assetItem.data != null)
                {
                    ItemData asset = assetItem.data;
                    if (assetItem.data.path!=null && assetItem.data.path.Length > 0)
                    {
                        if(!asset.path.ToLower().Contains(Application.dataPath.ToLower()))
                        {
                            UnityEngine.Object o = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(asset.path);
                            selectedObjects.Add(o);
                            Selection.activeObject = o;
                            selectedAssets.Add(assetItem.data);
                        }
                    }

                    if (OnSelectChange != null) OnSelectChange(assetItem.data);
                }
            }
            if(selectedObjects.Count>0)
                Selection.objects = selectedObjects.ToArray();
        }
        //--------------------------------------------------------
        void OnSortingChanged(MultiColumnHeader multiColumnHeader)
        {
            SortIfNeeded(rootItem, GetRows());
        }
        //--------------------------------------------------------
        void SortIfNeeded(TreeViewItem root, IList<TreeViewItem> rows)
        {
            if (rows.Count <= 1)
                return;

            if (multiColumnHeader!=null && multiColumnHeader.sortedColumnIndex == -1)
                return;

            SortByColumn();

            rows.Clear();
            for (int i = 0; i < root.children.Count; i++)
                rows.Add(root.children[i]);

            Repaint();
        }
        //--------------------------------------------------------
        void SortByColumn()
        {
            if (multiColumnHeader == null) return;
            var sortedColumns = multiColumnHeader.state.sortedColumns;

            if (sortedColumns.Length == 0)
                return;

            if (OnSortColum!=null)
            {
                List<TreeItemData> assetList = new List<TreeItemData>();
                foreach (var item in rootItem.children)
                {
                    assetList.Add(item as TreeItemData);
                }
                for (int i = 0; i < sortedColumns.Length; ++i)
                {
                    assetList = OnSortColum(assetList, multiColumnHeader, sortedColumns[i]);
                }
                rootItem.children = new List<TreeViewItem>();
                foreach (var db in assetList)
                {
                    rootItem.AddChild(db);
                }
            }
            else
            {
                List<TreeItemData> assetList = new List<TreeItemData>();
                foreach (var item in rootItem.children)
                {
                    assetList.Add(item as TreeItemData);
                }
                var orderedItems = InitialOrder(assetList, sortedColumns);
                rootItem.children = orderedItems.Cast<TreeViewItem>().ToList();

            }
        }
        //--------------------------------------------------------
        IOrderedEnumerable<TreeItemData> InitialOrder(IEnumerable<TreeItemData> myTypes, int[] columnList)
        {
            if (multiColumnHeader == null) return myTypes.OrderBy(l => l.id);
            bool ascending = multiColumnHeader.IsSortedAscending(columnList[0]);
            return myTypes.OrderBy(l => l.id);
        }
        //--------------------------------------------------------
        private void ReloadAndSelect(IList<int> hashCodes)
        {
            Reload();
            SetSelection(hashCodes);
            SelectionChanged(hashCodes);
        }
        //--------------------------------------------------------
        protected override void RowGUI(RowGUIArgs args)
        {
            if (multiColumnHeader == null)
            {
                CellRowGUI(ref args);
                return;
            }
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
                CellGUI(args.GetCellRect(i), args.item as TreeItemData, args.GetColumn(i), ref args);
        }
        //--------------------------------------------------------
        private void CellRowGUI(ref RowGUIArgs args)
        {
            TreeItemData data = args.item as TreeItemData;
            Color oldColor = GUI.color;
            GUI.color = data.itemColor;
            if (OnCellDraw != null)
            {
                if (OnCellDraw(BuildArgv(args)))
                {
                    GUI.color = oldColor;
                    return;
                }
            }
            //             Rect toggleRect = args.rowRect;
            //             toggleRect.x += GetContentIndent(args.item);
            //             toggleRect.width = 16f;
            //             bool isStatic = EditorGUI.Toggle(toggleRect, true);
            base.RowGUI(args);
            GUI.color = oldColor;
        }
        //--------------------------------------------------------
        private void CellGUI(Rect cellRect, TreeItemData item, int column, ref RowGUIArgs args)
        {
            Color oldColor = GUI.color;
            CenterRectUsingSingleLineHeight(ref cellRect);

            if(column == 0 && buildMutiColumnDepth && item.hasChildren)
            {
                cellRect.x += DepthIndentWidth;
                cellRect.width -= DepthIndentWidth;
            }

            item.displayName = item.data.name;

            GUI.color = item.itemColor;
            if(OnCellDraw !=null)
            {
                var callArgv = BuildArgv(args, column);
                callArgv.rowRect = cellRect;
                if (OnCellDraw(callArgv))
                {
                    GUI.color = oldColor;
                    return;
                }
            }
            switch (column)
            {
                case 0:
                    {
                        if (GUI.Button(cellRect, item.data.id.ToString()))
                        {
                            if (EditorUtility.DisplayDialog("Tips", "delete this ?", "yes", "no"))
                            {
                                Remove(item);
                            }
                        }
                    }
                    break;
                case 1:
                    {
                        DefaultGUI.Label(cellRect,
                            item.displayName,
                            args.selected,
                            args.focused);
                    }
                    break;
            }
            GUI.color = oldColor;
        }
        //--------------------------------------------------------
        public void SetExpanded(ItemData data, bool bExpand)
        {
            foreach(var db in m_SourceAssets)
            {
                if (db ==data)
                    SetExpanded(db.id, bExpand);
            }
        }
        //--------------------------------------------------------
        public void Remove(TreeItemData item)
        {
            if(item!=null)
            {
                m_SourceAssets.Remove(item.data);
                if (OnRemoved != null) OnRemoved(item.data);
                Reload();
            }
        }
        //--------------------------------------------------------
        public void Remove(ItemData item)
        {
            if (item != null)
            {
                m_SourceAssets.Remove(item);
                if (OnRemoved != null) OnRemoved(item);
                Reload();
            }
        }
        //--------------------------------------------------------
        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            if (OnDragItem == null) return false;
            return OnDragItem((TreeItemData)args.draggedItem);
        }
        //--------------------------------------------------------
        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {

        }
        //--------------------------------------------------------
        public override void OnGUI(Rect rect)
        {
            if(scrollWhellIgnore)
            {
                if (Event.current.type == EventType.ScrollWheel && GetRect().Contains(Event.current.mousePosition))
                {
                    return;
                }
            }
            if(discardRect.size.sqrMagnitude>0 && discardRect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type <= EventType.MouseDrag)
                    return;
            }
            base.OnGUI(rect);
        }
    }
}
#endif