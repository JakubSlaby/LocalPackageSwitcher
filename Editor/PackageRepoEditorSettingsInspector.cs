using System;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace WhiteSparrow.PackageRepoEditor
{
	[CustomEditor(typeof(PackageRepoEditorSettings))]
	public class PackageRepoEditorSettingsInspector : Editor
	{
		// ReSharper disable once InconsistentNaming
		// ReSharper disable once MemberCanBePrivate.Global
		public new PackageRepoEditorSettings target => base.target as PackageRepoEditorSettings;
		
		public override void OnInspectorGUI()
		{
			using (var check = new EditorGUI.ChangeCheckScope())
			{
				base.OnInspectorGUI();
				if (check.changed)
					target.Save();
			}

			EditorGUILayout.TextField(Application.persistentDataPath);
			EditorGUILayout.TextField(Application.dataPath);
			
			GUILayout.Space(20);
			RepositoryListGUI();
			GUILayout.Space(20);
			PackageListGUI();

		}

		private static FetchRepositoriesRequest s_RepositoryFetchProcess;
		private static ConvertToPackageRequest s_RepositoryConvertRequest;
		private void RepositoryListGUI()
		{
			if (s_RepositoryFetchProcess == null)
			{
				
				
				s_RepositoryFetchProcess = new FetchRepositoriesRequest(GetRepositoryDirectory());
				s_RepositoryFetchProcess.Start();
			}

			if (!s_RepositoryFetchProcess.IsComplete)
			{
				EditorGUILayout.HelpBox("... fetching repositories", MessageType.Info, true);
				return;
			}

			if (GUILayout.Button("Try again"))
			{
				s_RepositoryFetchProcess = null;
				Repaint();
				return;
			}
			
			if(!string.IsNullOrWhiteSpace(s_RepositoryFetchProcess.Output))
				EditorGUILayout.HelpBox(s_RepositoryFetchProcess.Output, MessageType.Info, true);
			if(!string.IsNullOrWhiteSpace(s_RepositoryFetchProcess.Error))
				EditorGUILayout.HelpBox(s_RepositoryFetchProcess.Error, MessageType.Error, true);

			foreach (var repositoryDirectory in s_RepositoryFetchProcess.Result)
			{
				using (new EditorGUILayout.HorizontalScope())
				{
					GUILayout.Label(repositoryDirectory.FullName);
					if (GUILayout.Button("Convert to package"))
					{
						ConvertRepositoryToPackage(repositoryDirectory);
					}
				}
			}
		}

		private void ConvertRepositoryToPackage(DirectoryInfo repositoryDirectory)
		{
			s_RepositoryConvertRequest = new ConvertToPackageRequest(repositoryDirectory);
			s_RepositoryConvertRequest.Start();
			s_RepositoryConvertRequest.OnComplete += OnRepositoryConvertCompleted;
		}

		private void OnRepositoryConvertCompleted()
		{
			
		}

		private static ListRequest s_RequestList;
		private static ConvertToRepositoryRequest s_PackageConvertRequest; 
		
		private void PackageListGUI()
		{
			if (s_RequestList == null)
			{
				s_RequestList = Client.List();
				EditorApplication.update += OnEditorUpdate;
			}

			if (s_RequestList.Status == StatusCode.InProgress)
			{
				EditorGUILayout.HelpBox("... fetching packages", MessageType.Info, true);
				return;
			}

			if (GUILayout.Button("Refresh"))
			{
				s_RequestList = null;
				Repaint();
				return;
			}
			
			foreach (var packageInfo in s_RequestList.Result)
			{
				using (new EditorGUILayout.HorizontalScope())
				{
					if (packageInfo.packageId.StartsWith("com.unity", StringComparison.OrdinalIgnoreCase))
						continue;
					GUILayout.Label(packageInfo.displayName);
					GUILayout.FlexibleSpace();
					GUILayout.Label(packageInfo.packageId);
					if (packageInfo.repository != null && packageInfo.repository.type == "git" && GUILayout.Button("Convert to repository"))
					{
						ConvertToRepository(packageInfo);
					}
				}
			}
		}

		private void ConvertToRepository(PackageInfo packageInfo)
		{
			s_PackageConvertRequest = new ConvertToRepositoryRequest(packageInfo, GetRepositoryDirectory());
			s_PackageConvertRequest.Start();
		}

		private void OnEditorUpdate()
		{
			
		}

		private void OnDestroy()
		{
			EditorApplication.update -= OnEditorUpdate;
		}

		private DirectoryInfo GetRepositoryDirectory()
		{
			string workingDirectory = null;
			if (Path.IsPathRooted(target.RepositoriesPath))
				workingDirectory = target.RepositoriesPath;
			else
			{
				string relativePath = target.RepositoriesPath.TrimStart('/');
				if (relativePath.StartsWith("Assets/"))
					relativePath = relativePath.Substring("Assets/".Length);
				workingDirectory = Path.Combine(Application.dataPath, relativePath);
			}

			return new DirectoryInfo(workingDirectory);
		}

		[SettingsProvider]
		static SettingsProvider CreateSettingsProvider()
		{
			return AssetSettingsProvider.CreateProviderFromObject("Project/WhiteSparrow/Package Repo Switcher", PackageRepoEditorSettings.instance);
		}
		
		// Example of how to prompt the settings window
		[MenuItem("Tools/Package Repository Switcher")]
		internal static void ShowWindow()
		{
			SettingsService.OpenProjectSettings("Project/WhiteSparrow/Package Repo Switcher");
		}
	}
}