﻿using System;
using System.Collections.Generic;
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

			GUILayout.Space(20);
			RepositoryListGUI();
			GUILayout.Space(20);
			PackageListGUI();

		}

		private static FetchRepositoriesRequest s_RepositoryFetchRequest;
		private static ConvertToPackageRequest s_RepositoryConvertRequest;
		private void RepositoryListGUI()
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				GUILayout.Label("Repositories", EditorStyles.whiteLargeLabel);
				GUILayout.FlexibleSpace();
				using (new EditorGUI.DisabledScope(s_RepositoryFetchRequest == null || !s_RepositoryFetchRequest.IsComplete))
				{
					if (GUILayout.Button("Refresh"))
					{
						s_RepositoryFetchRequest = null;
						Repaint();
						return;
					}
				}
			}
			
			if (s_RepositoryFetchRequest == null)
			{
				s_RepositoryFetchRequest = new FetchRepositoriesRequest(GetRepositoryDirectory());
				s_RepositoryFetchRequest.Start();
			}

			if (!s_RepositoryFetchRequest.IsComplete)
			{
				EditorGUILayout.HelpBox("... fetching repositories", MessageType.Info, true);
				return;
			}

			if (!string.IsNullOrWhiteSpace(s_RepositoryFetchRequest.Error))
			{
				EditorGUILayout.HelpBox(s_RepositoryFetchRequest.Error, MessageType.Error, true);
				return;
			}

			if (s_RepositoryFetchRequest.Result == null || s_RepositoryFetchRequest.Result.Length == 0)
			{
				GUILayout.Label("No package repositories");
				return;
			}
			
			foreach (var record in s_RepositoryFetchRequest.Result)
			{
				using (new EditorGUILayout.HorizontalScope())
				{
					GUILayout.Label(record.RelativeUri);
					GUILayout.FlexibleSpace();
					if(record.HasUncommittedChanges)
						GUILayout.Label("uncommitted changes");
					if(record.HasChangesToPush)
						GUILayout.Label("changes to push");
						
					if (GUILayout.Button("Convert to package"))
					{
						s_RepositoryConvertRequest = new ConvertToPackageRequest(record.Directory);
						s_RepositoryConvertRequest.Start();
						s_RepositoryConvertRequest.OnComplete += OnRequestCompleted;
					}
				}
			}
		}


		private static FetchPackagesRequest s_PackageListRequest;
		private static ConvertToRepositoryRequest s_PackageConvertRequest; 
		
		private void PackageListGUI()
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				GUILayout.Label("Packages", EditorStyles.whiteLargeLabel);
				GUILayout.FlexibleSpace();
				using (new EditorGUI.DisabledScope(s_PackageListRequest == null || !s_PackageListRequest.IsComplete))
				{
					if (GUILayout.Button("Refresh"))
					{
						s_PackageListRequest = null;
						Repaint();
						return;
					}
				}
			}
			
			if (s_PackageListRequest == null)
			{
				s_PackageListRequest = new FetchPackagesRequest();
				s_PackageListRequest.Start();
			}
			
			if (!s_PackageListRequest.IsComplete)
			{
				EditorGUILayout.HelpBox("... fetching packages", MessageType.Info, true);
				return;
			}

			if (s_PackageListRequest.Error != null)
			{
				EditorGUILayout.HelpBox(s_PackageListRequest.Error, MessageType.Error, true);
				return;
			}

			if (s_PackageListRequest.Result == null || s_PackageListRequest.Result.Length == 0)
			{
				GUILayout.Label("No custom packages");
				return;
			}

			foreach (var packageInfo in s_PackageListRequest.Result)
			{
				using (new EditorGUILayout.HorizontalScope())
				{
					GUILayout.Label(packageInfo.displayName);
					GUILayout.FlexibleSpace();
					GUILayout.Label(packageInfo.packageId);
					if (packageInfo.repository != null && packageInfo.repository.type == "git" && GUILayout.Button("Convert to repository"))
					{
						s_PackageConvertRequest = new ConvertToRepositoryRequest(packageInfo, GetRepositoryDirectory());
						s_PackageConvertRequest.Start();
						s_PackageConvertRequest.OnComplete += OnRequestCompleted;
					}
				}
			}
		}
		
		

		private void OnRequestCompleted()
		{
			s_RepositoryFetchRequest = null;
			s_RepositoryConvertRequest = null;
			s_PackageConvertRequest = null;
			s_PackageListRequest = null;
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