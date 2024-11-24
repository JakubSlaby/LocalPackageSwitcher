using UnityEditor;

namespace WhiteSparrow.PackageRepoEditor
{
	[FilePath("WhiteSparrow/PackageSwitcherSettings.asset", FilePathAttribute.Location.PreferencesFolder)]
	public class PackageRepoEditorSettings : ScriptableSingleton<PackageRepoEditorSettings>
	{
		public string[] searchPaths;
		
		internal void Save()
		{
			this.Save(true);
		}
	}
}