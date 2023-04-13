using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace WhiteSparrow.PackageRepoEditor
{
	public class FetchRepositoriesRequest : AbstractRequest
	{
		public class OutputRecord
		{
			public DirectoryInfo Directory;
			public string RelativeUri;
			public bool HasUncommittedChanges;
			public bool HasChangesToPush;
		}
		
		private CommandLineProcess m_FindRepositoriesProcess;

		private string m_Output;
		public string Output => m_Output;
		
		private string m_Error;
		public string Error => m_Error;

		public OutputRecord[] Result { get; private set; }

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

			List<OutputRecord> resultDirectories = new List<OutputRecord>();

			StringBuilder output = new StringBuilder();
			output.AppendLine($"Working directory: {m_WorkingDirectory}");
			var allGitDirectories = m_WorkingDirectory.GetDirectories(".git", SearchOption.AllDirectories);
			foreach (var gitDirectory in allGitDirectories)
			{
				var packageFiles = gitDirectory.Parent?.GetFiles("package.json", SearchOption.AllDirectories) ?? Array.Empty<FileInfo>();
				if (packageFiles.Length == 0)
					continue;
				
				output.AppendLine(gitDirectory.FullName);

				DirectoryInfo repositoryDirectory = gitDirectory.Parent;
				if(repositoryDirectory == null)
					continue;

				ChangeCheckRequest changeCheck = new ChangeCheckRequest(repositoryDirectory);
				changeCheck.Start();
				
				OutputRecord record = new OutputRecord()
				{
					Directory = repositoryDirectory,
					RelativeUri = new Uri(m_WorkingDirectory.FullName).MakeRelativeUri(new Uri(repositoryDirectory.FullName)).ToString(),
					HasUncommittedChanges = changeCheck.HasUncommittedChanges,
					HasChangesToPush = changeCheck.HasCommitsToPush
				};
				
				resultDirectories.Add(record);
			}

			m_Output = output.ToString();
			Result = resultDirectories.ToArray();
			Complete();
		}

	}
}