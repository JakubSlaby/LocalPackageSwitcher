using UnityEditor;

namespace WhiteSparrow.PackageRepoEditor
{
	[FilePath("ProjectSettings/PackageRepoEditorSettings.asset", FilePathAttribute.Location.ProjectFolder)]
	public class PackageRepoEditorSettings : ScriptableSingleton<PackageRepoEditorSettings>
	{
		public string RepositoriesPath = "Plugins/Repositories";
		
		internal void Save()
		{
			this.Save(true);
		}
	}
}