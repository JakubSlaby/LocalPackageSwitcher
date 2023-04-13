using System.IO;
using WhiteSparrow.PackageRepoEditor;

namespace Plugins.Repositories.Shared_PackageRepoEditor.Editor.Requests
{
	public class RepositoryDirectoryValidationRequest : AbstractRequest
	{
		public enum ValidationResult
		{
			None,
			Success,
			DirectoryDoesntExist,
			NoValidGitIgnore
		}

		public readonly string DirectoryPath;
		public readonly DirectoryInfo RepositoryDirectory;

		public ValidationResult Result { get; private set; } = ValidationResult.None;
		
		public RepositoryDirectoryValidationRequest(string path, DirectoryInfo directoryInfo)
		{
			DirectoryPath = path;
			RepositoryDirectory = directoryInfo;
		}
		
		protected override void StartRequest()
		{
			if (!RepositoryDirectory.Exists)
			{
				Result = ValidationResult.DirectoryDoesntExist;
				CompleteError($"Target directory {RepositoryDirectory.FullName} doesn't exist.");
				return;
			}

			var gitIgnoreFiles = RepositoryDirectory.GetFiles(".gitignore", SearchOption.TopDirectoryOnly);
			bool hasCorrectGitIgnore = false;
			foreach (var gitIgnoreFile in gitIgnoreFiles)
			{
				var lines = File.ReadLines(gitIgnoreFile.FullName);
				foreach (var line in lines)
				{
					if (line == "*")
					{
						hasCorrectGitIgnore = true;
						break;
					}
				}

				if (hasCorrectGitIgnore)
					break;
			}

			if (!hasCorrectGitIgnore)
			{
				Result = ValidationResult.NoValidGitIgnore;
				CompleteError($"Target directory {RepositoryDirectory.FullName} doesn't have a valid .gitignore file.");
				return;
			}

			Result = ValidationResult.Success;
			Complete();
		}
	}
}