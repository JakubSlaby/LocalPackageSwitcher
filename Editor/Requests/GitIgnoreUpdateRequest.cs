using System;
using System.IO;
using System.Text;
using WhiteSparrow.PackageRepoEditor;

namespace Plugins.Repositories.Shared_PackageRepoEditor.Editor.Requests
{
	public class GitIgnoreUpdateRequest : AbstractRequest
	{
		public readonly DirectoryInfo RepositoryDirectory;

		
		public GitIgnoreUpdateRequest(DirectoryInfo directoryInfo)
		{
			RepositoryDirectory = directoryInfo;
		}
		
		protected override void StartRequest()
		{
			var gitIgnoreFiles = RepositoryDirectory.GetFiles(".gitignore", SearchOption.TopDirectoryOnly);

			FileInfo gitIgnore = new FileInfo(gitIgnoreFiles.Length > 0 ? gitIgnoreFiles[0].FullName : Path.Combine(RepositoryDirectory.FullName, ".gitignore"));

			try
			{
				byte[] content = new UTF8Encoding(true).GetBytes("*");
				using (var fileStream = File.OpenWrite(gitIgnore.FullName))
				{
					fileStream.Write(content, 0, content.Length);
				}
				Complete();
			}
			catch (Exception e)
			{
				CompleteError(e.Message);
			}
		}
	}
}