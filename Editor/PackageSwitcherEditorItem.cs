using System;
using System.Text;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;
using WhiteSparrow.PackageRepoEditor.Requests;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace WhiteSparrow.PackageRepoEditor
{
    public class AbstractPackageSwitcherEditorItem : VisualElement
    {
        public Label PackageDisplayName { get; private set; }
        public Label PackageVersion { get; private set; }
        public Label PackageName { get; private set; }
        public Label PackagePath { get; private set; }
        
        public VisualElement TitleContainer { get; private set; }
        public VisualElement TagsContainer { get; private set; }
        public VisualElement MetaDataContainer { get; private set; }
        public VisualElement ActionsContainer { get; private set; }
        
        public Button GenericActionButton { get; private set; }
        
        public AbstractPackageSwitcherEditorItem()
        {
            this.AddToClassList("package-information-item");

            var primaryRow = new VisualElement();
            primaryRow.AddToClassList("primary-row");
            primaryRow.AddManipulator(new Clickable(OnPrimaryRowClicked));
            this.Add(primaryRow);
            
            TitleContainer = new VisualElement();
            TitleContainer.AddToClassList("title-container");
            primaryRow.Add(TitleContainer);

            PackageDisplayName = new Label();
            PackageDisplayName.AddToClassList("package-display-name");
            TitleContainer.Add(PackageDisplayName);
            
            PackageVersion =  new Label();
            PackageVersion.AddToClassList("package-version");
            TitleContainer.Add(PackageVersion);
            
            TagsContainer = new VisualElement();
            TagsContainer.AddToClassList("package-tags-container");
            primaryRow.Add(TagsContainer);

            
            MetaDataContainer = new VisualElement();
            MetaDataContainer.AddToClassList("meta-data-container");
            this.Add(MetaDataContainer);
            
            PackageName = new Label();
            PackageName.AddToClassList("package-name");
            PackageName.enableRichText = true;
            MetaDataContainer.Add(PackageName);
            
            PackagePath = new Label();
            PackagePath.AddToClassList("package-path");
            PackagePath.enableRichText = true;
            MetaDataContainer.Add(PackagePath);

            ActionsContainer = new VisualElement();
            ActionsContainer.AddToClassList("actions-container");
            primaryRow.Add(ActionsContainer);

            GenericActionButton = new Button();
            GenericActionButton.text = "...";
            GenericActionButton.AddToClassList("actions-generic-button");
            GenericActionButton.clickable.clicked += OnGenericActionMenu;
            ActionsContainer.Add(GenericActionButton);
        }

        private void OnPrimaryRowClicked(EventBase obj)
        {
            this.ToggleInClassList("toggle-active");
        }

        private void OnGenericActionMenu()
        {
            GenericMenu genericMenu = new GenericMenu();
            BuildActionsMenu(genericMenu);
            if (genericMenu.GetItemCount() == 0)
                return;
            genericMenu.ShowAsContext();
        }

        protected virtual void BuildActionsMenu(GenericMenu menu)
        {
            
        }

        public virtual void UpdateInfo()
        {
        }

        public void SetTag(string tag, bool enabled)
        {
	        var children = TagsContainer.Children();
	        Label target = null;
	        foreach (var child in children)
	        {
		        if (child is not Label label)
			        continue;
		        if (label.text != tag)
			        continue;
		        target = label;
		        break;
	        }
	        if (!enabled)
	        {
		        if(target != null)
			        target.RemoveFromHierarchy();
		        return;
	        }

	        if (target != null)
		        return;

	        target = new Label();
	        target.text = tag;
	        target.AddToClassList("package-tag");
	        target.AddToClassList($"tag-{tag.Replace(" ", "")}");
	        TagsContainer.Add(target);
        }
    }

    public class PackageInfoSwitcherEditorItem : AbstractPackageSwitcherEditorItem
    {
        public PackageInfo PackageInfo { get; private set; }

        public PackageInfoSwitcherEditorItem() : base()
        {
            this.AddToClassList("package-information-item__PackageInfo");
        }

        public PackageInfoSwitcherEditorItem(PackageInfo packageInfo) : this()
        {
            SetPackageInfo(packageInfo);
        }

        public void SetPackageInfo(PackageInfo packageInfo)
        {
            PackageInfo = packageInfo;
            PackageDisplayName.text = packageInfo.displayName;
            PackageVersion.text = packageInfo.version;
            PackageName.text = $"{packageInfo.name}@{packageInfo.version}";

            StringBuilder extraInfoBuilder = new StringBuilder();
            extraInfoBuilder.AppendLine($"asset path: {packageInfo.assetPath}");
            if (packageInfo.source == PackageSource.Local)
            {
                extraInfoBuilder.AppendLine($"absolute path: {packageInfo.resolvedPath}");
            }
            PackagePath.text = extraInfoBuilder.ToString().Trim();


            UpdateInfo();
        }

        public override void UpdateInfo()
        {
            base.UpdateInfo();
		    SetTag("Local", PackageInfo.source == PackageSource.Local);
		    SetTag("Git", PackageInfo.source == PackageSource.Git);
        }

        protected override void BuildActionsMenu(GenericMenu menu)
        {
            menu.AddItem(EditorGUIUtility.TrTextContent("Open in package manager"), false, TriggerOpenInPackageManager);
            menu.AddSeparator("/");
            if (PackageInfo.source == PackageSource.Local)
            {
                menu.AddDisabledItem(EditorGUIUtility.TrTextContent("Switch to local package"));

                if (PackageInfo.versions.compatible.Length > 0)
                {
                    menu.AddItem(EditorGUIUtility.TrTextContent($"Switch to remote package/npm/{PackageInfo.versions.latestCompatible}"), false, TriggerSwitchToRemotePackage, new Tuple<PackageInfo, string>(PackageInfo, PackageInfo.versions.latestCompatible));
                    for (int i = 0; i < Mathf.Min(PackageInfo.versions.compatible.Length, 10); i++)
                    {
                        var compatibleVersion = PackageInfo.versions.compatible[^(i + 1)];
                        if (compatibleVersion == PackageInfo.versions.latestCompatible)
                            continue;
                        menu.AddItem(EditorGUIUtility.TrTextContent($"Switch to remote package/npm/{compatibleVersion}"), false, TriggerSwitchToRemotePackage, new Tuple<PackageInfo, string>(PackageInfo, compatibleVersion));
                    }
                }
                else
                {
                    menu.AddDisabledItem(EditorGUIUtility.TrTextContent("Switch to remote package/No available versions"));
                }

                if (PackageInfo.repository != null)
                {
                    menu.AddItem(EditorGUIUtility.TrTextContent($"Switch to remote package/git/{PackageInfo.repository.url.Replace("/", "\\")}"), false, TriggerSwitchToGitPackage);
                }
            }
            else
            {
                menu.AddItem(EditorGUIUtility.TrTextContent("Switch to local package"), false, TriggerSwitchToLocalPackage, PackageInfo);
                menu.AddDisabledItem(EditorGUIUtility.TrTextContent("Switch to remote package"));
            }
        }

        private void TriggerOpenInPackageManager()
        {
            Window.Open(PackageInfo.name);
        }


        private void TriggerSwitchToLocalPackage(object userData)
        {
            PackageSwitcherEditor.StartRequest(new ManifestSetLocalRepositoryRequest(PackageInfo));
        }

        private void TriggerSwitchToRemotePackage(object userData)
        {
            if (userData is not Tuple<PackageInfo, string> tuple)
                return;
            
            PackageSwitcherEditor.StartRequest(new ManifestSetRemoteVersionRequest(tuple.Item1, tuple.Item2));
        }

        private void TriggerSwitchToGitPackage()
        {
            PackageSwitcherEditor.StartRequest(new ManifestSetGitRepositoryRequest(PackageInfo));
        }

    }

    public class PackageJsonSwitcherEditorItem : AbstractPackageSwitcherEditorItem
    {
        public PackageJsonInfo PackageJsonInfo { get; private set; }

        public PackageJsonSwitcherEditorItem() : base()
        {
            this.AddToClassList("package-information-item__PackageRecord");
        }
        
        public PackageJsonSwitcherEditorItem(PackageJsonInfo packageJsonInfo): this()
        {
            SetPackageRecord(packageJsonInfo);
        }

        public void SetPackageRecord(PackageJsonInfo packageJsonInfo)
        {
            PackageJsonInfo = packageJsonInfo;
            PackageDisplayName.text = packageJsonInfo.PackageDisplayName;
            PackageVersion.text = packageJsonInfo.PackageVersion;
            PackageName.text = $"{packageJsonInfo.PackageName}@{packageJsonInfo.PackageVersion}";
            PackagePath.text = $"{packageJsonInfo.PackageFile.Directory.FullName}";
            
            UpdateInfo();
        }

        protected override void BuildActionsMenu(GenericMenu menu)
        {
#if UNITY_EDITOR_OSX
            menu.AddItem(EditorGUIUtility.TrTextContent("Reveal in Finder"), false, TriggerRevealInFinder);
#else
            menu.AddItem(EditorGUIUtility.TrTextContent("Show in Explorer"), false, TriggerRevealInFinder);
#endif
            menu.AddItem(EditorGUIUtility.TrTextContent("Add as package"), false, TriggerAddAsPackage);
        }

        private void TriggerAddAsPackage()
        {
            PackageSwitcherEditor.StartRequest(new ManifestSetLocalRepositoryRequest(PackageJsonInfo));
        }

        private void TriggerRevealInFinder()
        {
            EditorUtility.RevealInFinder(PackageJsonInfo.PackageFile.FullName);
        }

        public void SetRelatedPackage(PackageInfo info)
        {
            SetTag("In Use", info.source == PackageSource.Local && info.resolvedPath == PackageJsonInfo.PackageFile.Directory.FullName);
            UpdateInfo();
        }
    }
}