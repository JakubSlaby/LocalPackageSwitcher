using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace WhiteSparrow.PackageRepoEditor
{
	public class ConvertToRepositoryRequest : AbstractRequest
	{
		public readonly PackageInfo PackageInfo;
		public readonly DirectoryInfo RepositoryDirectory;
		
		public ConvertToRepositoryRequest(PackageInfo packageInfo, DirectoryInfo repositoryDirectory)
		{
			PackageInfo = packageInfo;
			RepositoryDirectory = repositoryDirectory;
		}
		protected override void StartRequest()
		{
			if (!RepositoryDirectory.Exists)
			{
				Complete();
				return;
			}

			if (PackageInfo.repository == null || string.IsNullOrWhiteSpace(PackageInfo.repository.url) || PackageInfo.repository.type != "git")
			{
				Complete();
				return;
			}

			string url = PackageInfo.repository.url;
			if (url.StartsWith("git://"))
				url = "https://" + url.Substring("git://".Length);

			string repositoryName = url.Substring(url.LastIndexOf('/') + 1);
			repositoryName = repositoryName.Substring(0, repositoryName.LastIndexOf('.'));

			DirectoryInfo targetDirectory = new DirectoryInfo(Path.Combine(RepositoryDirectory.FullName, repositoryName));
			if (targetDirectory.Exists)
			{
				if (targetDirectory.GetFiles("*", SearchOption.TopDirectoryOnly).Length > 0 || targetDirectory.GetDirectories("*", SearchOption.TopDirectoryOnly).Length > 0)
				{
					EditorUtility.DisplayDialog("Target directory already exists",
						$"Cannot clone repository as the target directory already exists. {targetDirectory.FullName}", "OK");
					return;
				}
			}
			
			CommandLineProcess clone = new CommandLineProcess(ProcessFile.GIT, $"clone {url}", RepositoryDirectory.FullName);
			clone.Start();
			clone.WaitTillExit();
			
			if (clone.ExitCode != 0 && clone.HasError)
				Debug.LogError(clone.Error);

			if (clone.ExitCode != 0)
			{
				Complete();
				return;
			}
			
			
			var removeRequest = Client.Remove(PackageInfo.name);
			while (!removeRequest.IsCompleted)
				continue;
			
			
			if (removeRequest.Error != null)
				Debug.LogError(removeRequest.Error.message);
			
			Complete();
		}
	}
}