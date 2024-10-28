using System;
using Plugins.WhiteSparrow.Shared_PackageRepoEditor.Editor.Requests;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UIElements;
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

            
            MetaDataContainer = new VisualElement();
            MetaDataContainer.AddToClassList("meta-data-container");
            this.Add(MetaDataContainer);
            
            PackageName = new Label();
            PackageDisplayName.AddToClassList("package-name");
            MetaDataContainer.Add(PackageName);
            
            PackagePath = new Label();
            PackageDisplayName.AddToClassList("package-path");
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
            PackagePath.text = $"{packageInfo.assetPath}";
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
                    menu.AddItem(EditorGUIUtility.TrTextContent($"Switch to remote package/{PackageInfo.versions.latestCompatible}"), false, TriggerSwitchToRemotePackage, new Tuple<PackageInfo, string>(PackageInfo, PackageInfo.versions.latestCompatible));
                    for (int i = 0; i < Mathf.Min(PackageInfo.versions.compatible.Length, 10); i++)
                    {
                        var compatibleVersion = PackageInfo.versions.compatible[i];
                        if (compatibleVersion == PackageInfo.versions.latestCompatible)
                            continue;
                        menu.AddItem(EditorGUIUtility.TrTextContent($"Switch to remote package/{compatibleVersion}"), false, TriggerSwitchToRemotePackage, new Tuple<PackageInfo, string>(PackageInfo, compatibleVersion));
                    }
                }
                else
                {
                    menu.AddDisabledItem(EditorGUIUtility.TrTextContent("Switch to remote package/No available versions"));
                }

                if (PackageInfo.repository != null)
                {
                    menu.AddItem(EditorGUIUtility.TrTextContent($"Switch to remote package/Git: {PackageInfo.repository.url.Replace("/", "|")}"), false, TriggerSwitchToGitPackage);
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
        }

        private void TriggerSwitchToGitPackage()
        {
            PackageSwitcherEditor.StartRequest(new ManifestSetGitRepositoryRequest(PackageInfo));
        }

    }

    public class PackageJsonSwitcherEditorItem : AbstractPackageSwitcherEditorItem
    {
        public FindLocalPackagesRequest.PackageRecord PackageRecord { get; private set; }

        public PackageJsonSwitcherEditorItem() : base()
        {
            this.AddToClassList("package-information-item__PackageRecord");
        }
        
        public PackageJsonSwitcherEditorItem(FindLocalPackagesRequest.PackageRecord packageRecord): this()
        {
            SetPackageRecord(packageRecord);
        }

        public void SetPackageRecord(FindLocalPackagesRequest.PackageRecord packageRecord)
        {
            PackageRecord = packageRecord;
            PackageDisplayName.text = packageRecord.PackageDisplayName;
            PackageVersion.text = packageRecord.PackageVersion;
            PackageName.text = $"{packageRecord.PackageName}@{packageRecord.PackageVersion}";
            PackagePath.text = $"{packageRecord.PackageFile}";
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
            PackageSwitcherEditor.StartRequest(new ManifestSetLocalRepositoryRequest(PackageRecord));
        }

        private void TriggerRevealInFinder()
        {
            EditorUtility.RevealInFinder(PackageRecord.PackageFile.FullName);
        }
    }
}