﻿#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace GGS_Framework
{
	public static partial class AdvancedGenericDropdown
	{
		#region Class Implementation
		public static void Show (Option[] elements, Action<Option> onOptionSelected)
		{
			DoMenu (Rect.zero, elements, element => onOptionSelected (element));
		}

		public static void Show (Rect rect, Option[] elements, Action<Option> onOptionSelected)
		{
			DoMenu (rect, elements, element => onOptionSelected (element));
		}

		public static void Show<EnumType> (Option[] elements, Action<object> onOptionSelected)
		{
			DoMenu (Rect.zero, typeof (EnumType), elements, element => onOptionSelected (element));
		}

		public static void Show<EnumType> (Rect rect, Option[] elements, Action<object> onOptionSelected)
		{
			DoMenu (rect, typeof (EnumType), elements, element => onOptionSelected (element));
		}

		private static void DoMenu (Rect rect, Option[] elements, Action<Option> onOptionSelected)
		{
			GenericMenu menu = new GenericMenu ();

			for (int i = 0; i < elements.Length; i++)
			{
				Option element = elements[i];

				if (!element.Use)
					continue;

				if (element is Separator)
					menu.AddSeparator (element.Path);
				else
					menu.AddItem (new GUIContent (element.Path), element.Selected, () => onOptionSelected (element));
			}

			if (rect != Rect.zero)
				menu.DropDown (rect);
			else
				menu.ShowAsContext ();
		}

		private static void DoMenu (Rect rect, Type enumType, Option[] elements, Action<object> onOptionSelected)
		{
			DoMenu (rect, elements, option =>
			{
				string trimmedOptionValue = option.GetValue ().Replace (" ", string.Empty);
				object enumObject = Enum.Parse (enumType, trimmedOptionValue);
				onOptionSelected (enumObject);
			});
		}
		#endregion
	}
}
#endif