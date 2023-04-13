using System;
using System.IO;
using System.Threading;
using UnityEditor;
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
				CompleteError($"No package manifest in the repository under {RepositoryDirectory.FullName} - Cannot convert to package");
				return;
			}

			if (packageFiles.Length > 1)
			{
				CompleteError($"More than one package manifest in the repository under {RepositoryDirectory.FullName} - Cannot convert to package");
				return;
			}

			ChangeCheckRequest changeCheck = new ChangeCheckRequest(RepositoryDirectory);
			changeCheck.Start();
			if (changeCheck.HasUncommittedChanges || changeCheck.HasCommitsToPush)
			{
				if(!EditorUtility.DisplayDialog("Changes in repo", $"Repository {RepositoryDirectory.Name} has changes.\nUncommitted changes: {changeCheck.HasUncommittedChanges}\nChanges to push: {changeCheck.HasCommitsToPush}", "Discard all changes", "Abort"))
					return;

				if (!EditorUtility.DisplayDialog("Changes in repo", "Are you sure you want to discard all changes?","Yes, Discard all changes", "No, abort"))
					return;
			}
			
			
			
			CommandLineProcess gitUrlProcess = new CommandLineProcess(ProcessFile.GIT, "config --get remote.origin.url", RepositoryDirectory.FullName);
			gitUrlProcess.Start();
			gitUrlProcess.WaitTillExit();
			
			if (gitUrlProcess.HasError)
			{
				CompleteError(gitUrlProcess.Error);
				return;
			}

			m_AddPackageRequest = Client.Add(gitUrlProcess.Output.Trim());
			while (!m_AddPackageRequest.IsCompleted)
				continue;

			if (m_AddPackageRequest.Error != null)
			{
				CompleteError(m_AddPackageRequest.Error.message);
				return;
			}

			string assetPath = "Assets" + RepositoryDirectory.FullName.Substring(Application.dataPath.Length);
			AssetDatabase.DeleteAsset(assetPath);
			
			Complete();
		}
	}
}