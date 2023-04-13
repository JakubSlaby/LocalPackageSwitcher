using System.IO;
using System.Threading;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace WhiteSparrow.PackageRepoEditor
{
	public class ConvertToPackageRequest : AbstractRequest
	{
		public readonly DirectoryInfo RepositoryDirectory;
		
		public ConvertToPackageRequest(DirectoryInfo repositoryDirectory)
		{
			RepositoryDirectory = repositoryDirectory;
		}

		private Timer m_Ticker;
		private AddRequest m_AddPackageRequest;

		protected override void StartRequest()
		{
			var packageFiles = RepositoryDirectory.GetFiles("package.json");
			if (packageFiles.Length == 0)
			{
				Debug.LogError($"No package manifest in the repository under {RepositoryDirectory.FullName} - Cannot convert to package");
				return;
			}

			if (packageFiles.Length > 1)
			{
				Debug.LogError($"More than one package manifest in the repository under {RepositoryDirectory.FullName} - Cannot convert to package");
				return;
			}

			CommandLineProcess gitUrlProcess = new CommandLineProcess(ProcessFile.GIT, "config --get remote.origin.url", RepositoryDirectory.FullName);
			gitUrlProcess.Start();
			gitUrlProcess.WaitTillExit();
			
			Debug.Log(gitUrlProcess.Output);
			if (gitUrlProcess.HasError)
			{
				Debug.LogError(gitUrlProcess.Error);
				return;
			}

			m_AddPackageRequest = Client.Add(gitUrlProcess.Output.Trim());
			while (!m_AddPackageRequest.IsCompleted)
				continue;

			if (m_AddPackageRequest.Error != null)
			{
				Debug.LogError(m_AddPackageRequest.Error.message);
			}
			else
			{
				Directory.Delete(RepositoryDirectory.FullName, true);
			}
			Complete();
		}
	}
}