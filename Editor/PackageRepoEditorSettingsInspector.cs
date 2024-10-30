using System;
using System.IO;
using Plugins.WhiteSparrow.Shared_PackageRepoEditor.Editor.Requests;
using UnityEditor;
using UnityEngine;

namespace WhiteSparrow.PackageRepoEditor
{
	[CustomEditor(typeof(PackageRepoEditorSettings))]
	public class PackageRepoEditorSettingsInspector : Editor
	{
		// ReSharper disable once InconsistentNaming
		// ReSharper disable once MemberCanBePrivate.Global
		public new PackageRepoEditorSettings target => base.target as PackageRepoEditorSettings;

		private void OnEnable()
		{
			if (target.hideFlags.HasFlag(HideFlags.NotEditable))
			{
				target.hideFlags ^= HideFlags.NotEditable;
			}
		}

		public override void OnInspectorGUI()
		{
			using (var check = new EditorGUI.ChangeCheckScope())
			{
				base.OnInspectorGUI();
				if (check.changed)
					target.Save();
			}
			
		}



		public const string SettingsPath = "Project/White Sparrow/Package Repo Switcher";
		[SettingsProvider]
		static SettingsProvider CreateSettingsProvider()
		{
			// return new SettingsProvider("Preferences/Package Repo Switcher", SettingsScope.User);
			return AssetSettingsProvider.CreateProviderFromObject(SettingsPath, PackageRepoEditorSettings.instance);
		}
	}
}