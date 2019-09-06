﻿#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace GGS_Framework
{
    public partial class ReorderableList
    {
        #region Class Members
        private IList list;
        private Type elementType;

        private TreeView treeView;
        private TreeViewState treeViewState;

        public bool drawBackground;

        public string title;
        public bool canSearch;
        private string searchVariableName;
        private SearchField searchBar;

        public bool draggable;

        #region Action Delegates
        public bool canAddAndRemove;
        public delegate void AddElementDelegate (int addIndex);
        public AddElementDelegate onElementAdd;
        public AddElementDelegate onAfterElementAdd;

        public bool canCopyAndPaste;
        public delegate void CopyElementDelegate (int copyIndex);
        public CopyElementDelegate onElementCopy;
        public CopyElementDelegate onAfterElementCopy;

        public delegate void PasteElementDelegate (int copyIndex, int[] pasteIndexes);
        public PasteElementDelegate onElementPaste;
        public PasteElementDelegate onAfterElementPaste;

        public delegate void RemoveElementDelegate (List<int> removedIndexes);
        public RemoveElementDelegate onAfterElementsRemove;

        public delegate void DrawElementDelegate (Rect rect, int index);
        public DrawElementDelegate onElementDraw;
        #endregion
        #endregion

        #region Class Accesors
        public int Count
        {
            get { return list.Count; }
        }

        public bool HasSearch
        {
            get { return treeView.hasSearch; }
        }

        private string SearchString
        {
            get { return treeView.searchString; }
            set { treeView.searchString = value; }
        }

        private int LatestIdOfSelection
        {
            get
            {
                IList<int> selection = GetSelection ();
                return selection[selection.Count - 1];
            }
        }
        #endregion

        #region Class Implementation
        public ReorderableList (IList list, Type elementType, string searchVariableName, string title = "Reorderable List")
        {
            this.list = list;
            this.elementType = elementType;

            treeViewState = new TreeViewState ();
            treeView = new TreeView (this, treeViewState);

            drawBackground = true;

            this.title = title;

            canSearch = true;
            searchBar = new SearchField ();
            searchBar.downOrUpArrowKeyPressed += treeView.SetFocusAndEnsureSelectedItem;
            this.searchVariableName = searchVariableName;

            draggable = true;
            canAddAndRemove = true;
            canCopyAndPaste = true;
        }

        public void Draw (Rect rect)
        {
            if (treeView.ItemCount != Count)
            {
                ReloadTree ();
                return;
            }

            Dictionary<string, Rect> rects = AdvancedRect.GetRects (rect, AdvancedRect.Orientation.Vertical,
                new AdvancedRect.Space (Styles.DefaultSpacing),
                new AdvancedRect.FixedItem ("Header", Styles.HeaderHeight, new AdvancedRect.Padding (Styles.DefaultPadding, AdvancedRect.Padding.Type.Horizontal)),
                new AdvancedRect.Space (Styles.DefaultSpacing),
                new AdvancedRect.ExpandedItem ("TreeView")
            );

            DrawHeader (rects["Header"]);

            if (drawBackground)
                Styles.Background.Draw (rects["TreeView"]);

            if (Count > 0)
                treeView.OnGUI (rects["TreeView"]);
            else
                AdvancedGUILabel.Draw (rects["TreeView"], new AdvancedGUILabelConfig ("Nothing in list.", FontStyle.Bold));
        }

        private void DrawHeader (Rect rect)
        {
            bool showAddButton = canAddAndRemove && !HasSearch;

            Dictionary<string, Rect> rects = AdvancedRect.GetRects (rect, AdvancedRect.Orientation.Horizontal,
                new AdvancedRect.ExpandedGroup ("SearchBar", AdvancedRect.Orientation.Horizontal,
                    new AdvancedRect.Space (Styles.AddButtonWidth),
                    new AdvancedRect.ExpandedItem ("Label")
                ),
                new AdvancedRect.FixedItem ("AddButton", Styles.AddButtonWidth, showAddButton)
            );

            Styles.HeaderBackground.Draw (rect);

            if (canSearch)
            {
                if (Count > 0)
                    DrawSearchBar (rects["SearchBar"]);
            }

            if (!canSearch || (!searchBar.HasFocus () && string.IsNullOrEmpty (SearchString)))
                GUI.Label (rects["Label"], title, Styles.Header);

            if (showAddButton)
                DrawAddButton (rects["AddButton"]);
        }

        private void DrawSearchBar (Rect rect)
        {
            Dictionary<string, Rect> rects = AdvancedRect.GetRects (rect, AdvancedRect.Orientation.Horizontal,
                new AdvancedRect.ExpandedItem ("Bar"),
                new AdvancedRect.FixedItem ("CancelButton", Styles.HeaderHeight, new AdvancedRect.Padding (Styles.SearchBarCancelButtonPadding, AdvancedRect.Padding.Type.All))
            );

            SearchString = searchBar.OnGUI (rects["Bar"], SearchString, Styles.SearchBar, GUIStyle.none, GUIStyle.none);

            if (!string.IsNullOrEmpty (SearchString))
            {
                if (GUI.Button (rects["CancelButton"], string.Empty, Styles.SearchBarCancelButton))
                {
                    SearchString = string.Empty;
                    GUI.FocusControl (null);
                }
            }
        }

        private void DrawAddButton (Rect rect)
        {
            if (GUI.Button (rect, string.Empty, Styles.AddButton))
                AddButtonPress ();
        }

        protected virtual void AddButtonPress ()
        {
            AddElement (Count);
        }

        private void DrawElement (Rect rect, int id)
        {
            Dictionary<string, Rect> rects = AdvancedRect.GetRects (rect, AdvancedRect.Orientation.Horizontal,
                new AdvancedRect.FixedItem ("DragIcon", Styles.DragIconWidth),
                new AdvancedRect.ExpandedItem ("Element")
            );

            Styles.DragIcon.Draw (rects["DragIcon"]);

            if (onElementDraw != null)
                onElementDraw (rects["Element"], id);
        }

        private void ContextClickElement (int id)
        {
            DoElementOptionsMenu ();
        }

        private void DoElementOptionsMenu ()
        {
            RepaintTree ();

            bool canInsertElementAbove = CanInsertElementAbove ();
            bool canInsertElementBelow = CanInsertElementBelow ();
            bool canCopyElement = CanCopyElement ();
            bool canPasteElement = CanPasteElement ();

            List<AdvancedGenericMenu.Item> items = new List<AdvancedGenericMenu.Item>
            {
                new AdvancedGenericMenu.Item ("Remove", false, canAddAndRemove),
                new AdvancedGenericMenu.Separator (canAddAndRemove),
                new AdvancedGenericMenu.Item ("Copy", false, canCopyElement),
                new AdvancedGenericMenu.Item ("Paste", false, canPasteElement),
                new AdvancedGenericMenu.Separator (canCopyElement || canPasteElement),
                new AdvancedGenericMenu.Item ("Insert Above", false, canInsertElementAbove),
                new AdvancedGenericMenu.Item ("Insert Below", false, canInsertElementBelow)
            };

            AdvancedGenericMenu.Draw<ElementOptions> (items.ToArray (), item =>
            {
                ElementOptions option = (ElementOptions) item;

                switch (option)
                {
                    case ElementOptions.Remove:
                        RemoveElementSelection ();
                        break;
                    case ElementOptions.Copy:
                        CopyElement ();
                        break;
                    case ElementOptions.Paste:
                        PasteElement ();
                        break;
                    case ElementOptions.InsertAbove:
                        InsertElementAbove ();
                        break;
                    case ElementOptions.InsertBelow:
                        InsertElementBelow ();
                        break;
                }
            });
        }

        private string GetDisplayNameOfElement (int elementIndex)
        {
            Type elementType = this.elementType;
            string name = string.Empty;

            if (string.IsNullOrEmpty (searchVariableName))
                name = list[elementIndex] as string;
            else if (elementType.GetField (searchVariableName) != null)
                name = elementType.GetField (searchVariableName).GetValue (list[elementIndex]) as string;
            else if (elementType.GetProperty (searchVariableName) != null)
                name = elementType.GetProperty (searchVariableName).GetValue (list[elementIndex], null) as string;

            if (string.IsNullOrEmpty (name))
                name = "Unnamed";

            return name;
        }

        #region Element Management
        private void PerformDrop (int insertIndex, List<int> draggedIds)
        {
            MoveElementSelection (insertIndex, draggedIds);
        }

        private void MoveElementSelection (int insertIndex, List<int> selectedIds)
        {
            if (insertIndex < 0)
                return;

            List<object> selection = new List<object> ();

            for (int i = 0; i < selectedIds.Count; i++)
                selection.Add (list[selectedIds[i]]);

            foreach (object item in selection)
                list.Remove (item);

            int itemsAboveInsertIndex = 0;
            foreach (int selectedElement in selectedIds)
            {
                if (selectedElement < insertIndex)
                    itemsAboveInsertIndex++;
            }

            insertIndex -= itemsAboveInsertIndex;

            selection.Reverse ();
            foreach (object item in selection)
                list.Insert (insertIndex, item);

            List<int> newSelection = new List<int> ();
            for (int i = insertIndex; i < insertIndex + selection.Count; i++)
                newSelection.Add (i);

            SetSelection (newSelection, TreeViewSelectionOptions.RevealAndFrame);

            ReloadTree ();
        }

        private void AddElement (int insertIndex)
        {
            if (onElementAdd == null)
            {
                object objectForAdd = CreateObjectInstanceForAdd (list);
                list.Insert (insertIndex, objectForAdd);
            }
            else
                onElementAdd (insertIndex);

            if (onAfterElementAdd != null)
                onAfterElementAdd (insertIndex);

            ReloadTree ();
            SetSelection (new List<int> { insertIndex });
        }

        private object CreateObjectInstanceForAdd (IList list)
        {
            // This is ugly but there are a lot of cases like null types and default constructors
            Type listType = list.GetType ();
            Type elementType = listType.GetElementType ();

            if (elementType == typeof (string))
                return "";
            else if (elementType != null && elementType.GetConstructor (Type.EmptyTypes) == null)
                Debug.LogErrorFormat ("Cannot add element. Type {0} has no default constructor. Implement a default constructor or implement your own add behaviour.", elementType);
            else if (listType.GetGenericArguments ()[0] != null)
                return Activator.CreateInstance (listType.GetGenericArguments ()[0]);
            else if (elementType != null)
                return Activator.CreateInstance (elementType);
            else
                Debug.LogError ("Cannot add element of type Null.");

            return null;
        }

        #region Insert
        private bool CanInsertElementAbove ()
        {
            if (!canAddAndRemove || HasSearch)
                return false;

            return (GetSelection ().Count == 1);
        }

        private void InsertElementAbove ()
        {
            AddElement (LatestIdOfSelection);
        }

        private bool CanInsertElementBelow ()
        {
            if (!canAddAndRemove || HasSearch)
                return false;

            return (GetSelection ().Count == 1);
        }

        private void InsertElementBelow ()
        {
            AddElement (LatestIdOfSelection + 1);
        }
        #endregion

        #region Copy
        private bool CanCopyElement ()
        {
            if (!canCopyAndPaste)
                return false;

            return (GetSelection ().Count == 1);
        }

        private void CopyElement ()
        {
            // TODO: Implement method

            if (onElementCopy == null)
            {
                // Implement copy
            }
            else
            {
                //rl.onElementCopy (0);
            }

            //rl.onAfterElementCopy (0);
        }
        #endregion

        #region Paste
        private bool CanPasteElement ()
        {
            if (!canCopyAndPaste)
                return false;

            return false;
        }

        protected void PasteElement ()
        {
            DoPasteElement ();
        }

        protected void DoPasteElement ()
        {
            // TODO: Implement paste

            AfterPasteElement ();
        }

        protected virtual void AfterPasteElement ()
        {

        }

        //private void PasteElement ()
        //{
        //    if (rl.onElementPaste == null)
        //    {
        //        // Implement paste
        //    }
        //    else
        //    {
        //        //rl.onElementPaste (0, null);
        //    }

        //    //rl.onAfterElementPaste (0, null);
        //}
        #endregion

        #region Remove
        protected virtual void RemoveElementSelection ()
        {
            DoRemoveElementSelection ();
        }

        private void DoRemoveElementSelection ()
        {
            List<int> selection = treeViewState.selectedIDs;

            // Sort elements by descending 
            if (selection.Count > 1)
                selection.Sort ((a, b) => -1 * a.CompareTo (b));

            foreach (int id in selection)
                list.RemoveAt (id);

            if (onAfterElementsRemove != null)
                onAfterElementsRemove (selection);

            ReloadTree ();
            SetSelection (null);
        }
        #endregion
        #endregion

        private void ReloadTree ()
        {
            treeView.Reload ();
        }

        private void RepaintTree ()
        {
            treeView.Repaint ();
        }

        private void SetSelection (IList<int> ids)
        {
            if (ids == null)
                ids = new List<int> ();

            treeView.SetSelection (ids);
        }

        private void SetSelection (IList<int> ids, TreeViewSelectionOptions options)
        {
            treeView.SetSelection (ids, options);
        }

        private IList<int> GetSelection ()
        {
            return treeView.GetSelection ();
        }
        #endregion
    }
}
#endif