using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WhiteSparrow.PackageRepoEditor
{
	public class FetchRepositoriesRequest : AbstractRequest
	{
		private CommandLineProcess m_FindRepositoriesProcess;

		private string m_Output;
		public string Output => m_Output;
		
		private string m_Error;
		public string Error => m_Error;

		public DirectoryInfo[] Result { get; private set; }

		private readonly DirectoryInfo m_WorkingDirectory;
		
		public FetchRepositoriesRequest(DirectoryInfo repositoryDirectory)
		{
			m_WorkingDirectory = repositoryDirectory;
		}
		
		
		protected override void StartRequest()
		{
			if (!m_WorkingDirectory.Exists)
			{
				Complete();
				return;
			}

			List<DirectoryInfo> resultDirectories = new List<DirectoryInfo>();

			StringBuilder output = new StringBuilder();
			output.AppendLine($"Working directory: {m_WorkingDirectory}");
			var allGitDirectories = m_WorkingDirectory.GetDirectories(".git", SearchOption.AllDirectories);
			foreach (var gitDirectory in allGitDirectories)
			{
				var packageFiles = gitDirectory.Parent?.GetFiles("package.json", SearchOption.AllDirectories) ?? Array.Empty<FileInfo>();
				if (packageFiles.Length == 0)
					continue;
				
				output.AppendLine(gitDirectory.FullName);
				resultDirectories.Add(gitDirectory.Parent);
			}

			m_Output = output.ToString();
			Result = resultDirectories.ToArray();
			Complete();
		}

	}
}