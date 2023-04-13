using System.IO;

namespace WhiteSparrow.PackageRepoEditor
{
	public class ChangeCheckRequest : AbstractRequest
	{
		public readonly DirectoryInfo RepositoryDirectory;
		
		public bool HasUncommittedChanges { get; private set; }
		public bool HasCommitsToPush { get; private set; }
		
		public ChangeCheckRequest(DirectoryInfo repositoryDirectory)
		{
			RepositoryDirectory = repositoryDirectory;
		}
		protected override void StartRequest()
		{
			CommandLineProcess changeCheck = new CommandLineProcess(ProcessFile.GIT, "status", RepositoryDirectory.FullName);
			changeCheck.Start();
			changeCheck.WaitTillExit();

			HasUncommittedChanges = !changeCheck.Output.Contains("nothing to commit, working tree clean");
			HasCommitsToPush = changeCheck.Output.Contains("use \"git push\"");
		}
	}
}