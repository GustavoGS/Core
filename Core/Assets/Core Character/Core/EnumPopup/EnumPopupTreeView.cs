﻿using System.Collections.Generic;
using UnityEngine;
using UnityEditor.IMGUI.Controls;

public class EnumPopupTreeView : TreeView {

	#region Class members
	public System.Action<int> onEnumSelected;
	private TreeViewState treeViewState;
	private string[] enumNames;
	#endregion

	#region Class implementation
	public EnumPopupTreeView (TreeViewState treeViewState, string[] enumNames) : base (treeViewState) {
		this.treeViewState = treeViewState;
		this.enumNames = enumNames;
		Reload ();
		SingleClickedItem (0);
	}
	#endregion

	#region Class overrides
	protected override TreeViewItem BuildRoot () {
		List<TreeViewItem> items = new List<TreeViewItem> ();
		for (int i = 0; i < enumNames.Length; i++)
			items.Add (new TreeViewItem (i, 0, enumNames[i].ToTitleCase ()));

		TreeViewItem root = new TreeViewItem (0, -1, "Root");
		root.children = items;
		return root;
	}

	protected override void KeyEvent () {
		base.KeyEvent ();

		if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
			SingleClickedItem (treeViewState.lastClickedID);
	}

	protected override void SingleClickedItem (int id) {
		base.SingleClickedItem (id);

		if (onEnumSelected != null)
			onEnumSelected (id);
	}
	#endregion
}