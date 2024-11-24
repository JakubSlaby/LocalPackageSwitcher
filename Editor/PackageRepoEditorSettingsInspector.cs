using UnityEditor;
using UnityEngine;

namespace WhiteSparrow.PackageRepoEditor
{
	[CustomEditor(typeof(PackageRepoEditorSettings))]
	public class PackageRepoEditorSettingsInspector : Editor
	{
		// ReSharper disable once InconsistentNaming
		// ReSharper disable once MemberCanBePrivate.Global
		public new PackageRepoEditorSettings target => base.target as PackageRepoEditorSettings;

		private static class Styles
		{
			public static readonly GUIStyle WelcomeMessage;
			public static readonly GUIStyle WelcomeMessageLabel;
			public static readonly GUIStyle VersionLabel;
			public static readonly GUIStyle WelcomeMessageButton;

			static Styles()
			{
				WelcomeMessage = new GUIStyle("FrameBox");
				WelcomeMessage.padding = new RectOffset(10, 10, 12, 12);
				
				WelcomeMessageLabel = new GUIStyle(EditorStyles.label);
				WelcomeMessageLabel.richText = true;
				WelcomeMessageLabel.wordWrap = true;
				WelcomeMessageLabel.stretchWidth = true;
				WelcomeMessageButton = new GUIStyle(EditorStyles.miniButton);
				
				VersionLabel = new GUIStyle(EditorStyles.label);
				VersionLabel.richText = true;
				VersionLabel.margin = new RectOffset(EditorStyles.label.margin.left, EditorStyles.label.margin.right, 6, 20);
			}
		}

		private void OnEnable()
		{
			if (target.hideFlags.HasFlag(HideFlags.NotEditable))
			{
				target.hideFlags ^= HideFlags.NotEditable;
			}
		}

		public override void OnInspectorGUI()
		{
			using (new GUILayout.HorizontalScope(Styles.WelcomeMessage))
			{
				using (new GUILayout.VerticalScope())
				{
					GUILayout.Label("Thanks for using Local Package Switcher!\nIf you find any issues or have ideas for enhancements, please post them in GitHub Issues!", Styles.WelcomeMessageLabel);
					GUILayout.Label($"Version <b>{PackageSwitcherEditor.Version}</b>", Styles.VersionLabel);
					GUILayout.Label("Local package manager is intended for usage as a tool to easily swap between remote and local packages while developing them. More information about best practices in the Readme file or on the Github page.\n\n<b>Search Paths</b> - Define a path (absolute) on your machine to search for available local packages for easier switching.", Styles.WelcomeMessageLabel);
				}
				if (GUILayout.Button("GitHub", Styles.WelcomeMessageButton, GUILayout.MaxWidth(100)))
				{
					PackageSwitcherEditor.VisitGitHub();
				}
			}
			
			using (var check = new EditorGUI.ChangeCheckScope())
			{
				base.OnInspectorGUI();
				if (check.changed)
					target.Save();
			}
			
		}



		public const string SettingsPath = "Project/White Sparrow/Local Package Switcher";
		[SettingsProvider]
		static SettingsProvider CreateSettingsProvider()
		{
			return AssetSettingsProvider.CreateProviderFromObject(SettingsPath, PackageRepoEditorSettings.instance);
		}
	}
}