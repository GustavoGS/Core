﻿namespace GGS_Framework
{
	public static partial class ExtensionMethods
	{
		#region Class Implementation
		public static T CloneObject<T> (this T obj) where T : class
		{
			if (obj == null)
				return null;

			System.Reflection.MethodInfo instance = obj.GetType ().GetMethod ("MemberwiseClone", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			if (instance != null)
				return (T) instance.Invoke (obj, null);
			else
				return null;
		}
		#endregion
	}
}