﻿using UnityEditor;
using UnityEngine;

namespace GGS_Framework.Editor
{
	[CustomPropertyDrawer (typeof (EnumPopupAttribute))]
	public class EnumPopupDrawer : PropertyDrawer
	{
		#region Class overrides
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
		{
			EnumPopup.Popup (position, property, label.text);
		}
		#endregion
	}
}