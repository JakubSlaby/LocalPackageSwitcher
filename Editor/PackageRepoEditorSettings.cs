using System;
using UnityEditor;
using UnityEngine;

namespace WhiteSparrow.PackageRepoEditor
{
	[FilePath("ProjectSettings/PackageRepoEditorSettings.asset", FilePathAttribute.Location.ProjectFolder)]
	public class PackageRepoEditorSettings : ScriptableSingleton<PackageRepoEditorSettings>
	{
		public string RepositoriesPath = "Plugins/Repositories";
		
		private void OnEnable()
		{
			this.hideFlags = HideFlags.DontSave;
		}

		internal void Save()
		{
			this.Save(true);
		}
	}
}