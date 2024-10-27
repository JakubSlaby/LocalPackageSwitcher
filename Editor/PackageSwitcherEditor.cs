using System;
using System.Collections.Generic;
using System.IO;
using Plugins.WhiteSparrow.Shared_PackageRepoEditor.Editor.Requests;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace WhiteSparrow.PackageRepoEditor
{
    public class PackageSwitcherEditor : EditorWindow
    {
        [MenuItem("Tools/White Sparrow/Package Switcher")]
        public static PackageSwitcherEditor ShowWindow()
        {
            PackageSwitcherEditor window = EditorWindow.GetWindow<PackageSwitcherEditor>();
            window.Show();
            return window;
        }

        private VisualElement m_ManifestPackageContainer;
        private VisualElement m_ManifestPackageContent;
        private VisualElement m_LocalPackagesContainer;
        private VisualElement m_LocalPackagesContent;

        private PackageSwitcherEditorStatusMessage m_ManifestPackagesStatusMessage;
        private PackageSwitcherEditorStatusMessage m_LocalPackagesStatusMessage;
        
        private void OnEnable()
        {
            InitializePaths();
            if (focusedWindow == this)
            {
                FetchPackages(true);
                FindLocalPackages(true);
            }
            else
            {
                m_PendingFetchPackagesRequest = true;
                m_PendingFindLocalPackagesRequest = true;
            }
        }

        private void OnFocus()
        {
            if(m_PendingFetchPackagesRequest)
                FetchPackages(true);
            if(m_PendingFindLocalPackagesRequest)
                FindLocalPackages(true);
        }


        private void CreateGUI()
        {
        
            m_ManifestPackagesStatusMessage = new PackageSwitcherEditorStatusMessage();
            m_LocalPackagesStatusMessage = new PackageSwitcherEditorStatusMessage();

            
            VisualElement root = rootVisualElement;
            root.AddToClassList("package-switcher-window");
            root.styleSheets.Add(Resources.Load<StyleSheet>("PackageRepoSwitcher/PackageRepoSwitcherStyleSheet"));

            ScrollView scroll = new ScrollView();
            root.Add(scroll);

            VisualElement container = scroll.contentContainer;
            container.AddToClassList("scroll-content-container");
            
            m_ManifestPackageContainer = new VisualElement();
            m_ManifestPackageContainer.AddToClassList("package-list-container");
            m_ManifestPackageContainer.AddToClassList("container-packages");
            container.Add(m_ManifestPackageContainer);
            
            m_LocalPackagesContainer = new VisualElement();
            m_LocalPackagesContainer.AddToClassList("package-list-container");
            m_LocalPackagesContainer.AddToClassList("container-local-packages");
            container.Add(m_LocalPackagesContainer);
            Label manifestPackagesTitle = new Label("Packages");
            manifestPackagesTitle.AddToClassList("title");
            m_ManifestPackageContainer.Add(manifestPackagesTitle);

            Label localPackagesTitle = new Label("Available Local Packages");
            localPackagesTitle.AddToClassList("title");
            m_LocalPackagesContainer.Add(localPackagesTitle);


            m_ManifestPackageContent = new VisualElement();
            m_ManifestPackageContent.AddToClassList("content");
            m_ManifestPackageContainer.Add(m_ManifestPackageContent);
            m_LocalPackagesContent = new VisualElement();
            m_LocalPackagesContent.AddToClassList("content");
            m_LocalPackagesContainer.Add(m_LocalPackagesContent);

            UpdatePackagesDisplay();
            UpdateLocalPackagesDisplay();
        }

        private static FetchPackagesRequest m_FetchPackagesRequest;
        private bool m_PendingFetchPackagesRequest = false;
        private void FetchPackages(bool force = false)
        {
            m_PendingFetchPackagesRequest = false;
            if (m_FetchPackagesRequest != null)
            {
                if (!force)
                    return;

                if (m_FetchPackagesRequest.IsComplete)
                {
                    m_FetchPackagesRequest.OnComplete -= OnFetchPackagesRequestComplete;
                    m_FetchPackagesRequest = null;
                }
            }

            m_FetchPackagesRequest = new FetchPackagesRequest();
            m_FetchPackagesRequest.OnComplete += OnFetchPackagesRequestComplete;
            m_FetchPackagesRequest.Start();
        }

        private void OnFetchPackagesRequestComplete()
        {
            m_FetchPackagesRequest.OnComplete -= OnFetchPackagesRequestComplete;
            UpdatePackagesDisplay();
        }

        private List<PackageInfoSwitcherEditorItem> m_ManifestPackageVisualItems =
            new List<PackageInfoSwitcherEditorItem>();
        
        private void UpdatePackagesDisplay()
        {
            if (m_ManifestPackageContent == null)
                return;
            
            m_ManifestPackageContent.Clear();
            m_ManifestPackageVisualItems.Clear();

            if (m_FetchPackagesRequest == null || !m_FetchPackagesRequest.IsComplete)
            {
                m_ManifestPackagesStatusMessage.Label.text = "Fetching packages...";
                m_ManifestPackageContent.Add(m_ManifestPackagesStatusMessage);
                return;
            }
            
            if (m_FetchPackagesRequest.IsComplete && m_FetchPackagesRequest.Error != null)
            {
                m_ManifestPackagesStatusMessage.Label.text = m_FetchPackagesRequest.Error;
                m_ManifestPackageContent.Add(m_ManifestPackagesStatusMessage);
                return;
            }
            
            if (m_FetchPackagesRequest.IsComplete && m_FetchPackagesRequest.Result.Length == 0)
            {
                m_ManifestPackagesStatusMessage.Label.text = "No packages found";
                m_ManifestPackageContent.Add(m_ManifestPackagesStatusMessage);
                return;
            }

            foreach (var packageInfo in m_FetchPackagesRequest.Result)
            {
                PackageInfoSwitcherEditorItem item = new PackageInfoSwitcherEditorItem(packageInfo);
                m_ManifestPackageVisualItems.Add(item);
                m_ManifestPackageContent.Add(item);
            }
        }

        private static FindLocalPackagesRequest m_FindLocalPackagesRequest;
        private bool m_PendingFindLocalPackagesRequest = false;
        private void FindLocalPackages(bool force)
        {
            m_PendingFindLocalPackagesRequest = false;
            if (m_FindLocalPackagesRequest != null)
            {
                if (!force)
                    return;

                if (m_FindLocalPackagesRequest.IsComplete)
                {
                    m_FindLocalPackagesRequest.OnComplete -= OnFindLocalPackagesRequestComplete;
                    m_FindLocalPackagesRequest = null;
                }
            }

            m_FindLocalPackagesRequest = new FindLocalPackagesRequest(PackageRepoEditorSettings.instance.searchPaths);
            m_FindLocalPackagesRequest.OnComplete += OnFindLocalPackagesRequestComplete;
            m_FindLocalPackagesRequest.Start();
        }

        private void OnFindLocalPackagesRequestComplete()
        {
            m_FindLocalPackagesRequest.OnComplete -= OnFindLocalPackagesRequestComplete;
            UpdateLocalPackagesDisplay();
        }

        private List<PackageJsonSwitcherEditorItem> m_LocalPackageVisualItems =
            new List<PackageJsonSwitcherEditorItem>();
        private void UpdateLocalPackagesDisplay()
        {
            if (m_LocalPackagesContent == null)
                return;
            m_LocalPackagesContent.Clear();
            m_LocalPackageVisualItems.Clear();
            
            if (m_FindLocalPackagesRequest == null || !m_FindLocalPackagesRequest.IsComplete)
            {
                m_LocalPackagesStatusMessage.Label.text = "Searching for packages...";
                m_LocalPackagesContent.Add(m_LocalPackagesStatusMessage);
                return;
            }
            
            if (m_FindLocalPackagesRequest.IsComplete && m_FindLocalPackagesRequest.Error != null)
            {
                m_LocalPackagesStatusMessage.Label.text = m_FindLocalPackagesRequest.Error;
                m_LocalPackagesContent.Add(m_LocalPackagesStatusMessage);
                return;
            }
            
            if (m_FindLocalPackagesRequest.IsComplete && m_FindLocalPackagesRequest.Result.Length == 0)
            {
                m_LocalPackagesStatusMessage.Label.text = "No packages found";
                m_LocalPackagesContent.Add(m_LocalPackagesStatusMessage);
                return;
            }

            foreach (var packageInfo in m_FindLocalPackagesRequest.Result)
            {
                PackageJsonSwitcherEditorItem item = new PackageJsonSwitcherEditorItem(packageInfo);
                m_LocalPackageVisualItems.Add(item);
                m_LocalPackagesContent.Add(item);
            }
        }



        private static FileInfo s_ManifestFile;

        public static FileInfo ManifestFile => s_ManifestFile ??= InitializeManifestFilePath();
        private static FileInfo InitializeManifestFilePath()
        {
            return new FileInfo(Path.Combine(Application.dataPath, "../Packages/manifest.json"));
        }

        private static DirectoryInfo s_ProjectDirectory;
        public static DirectoryInfo ProjectDirectory => s_ProjectDirectory ??= InitializeProjectDirectory();

        private static DirectoryInfo InitializeProjectDirectory()
        {
            return new DirectoryInfo(Path.Combine(Application.dataPath, "../"));
        }

        private static void InitializePaths()
        {
            s_ManifestFile ??= InitializeManifestFilePath();
            s_ProjectDirectory ??= InitializeProjectDirectory();
        }


        public static AbstractRequest CurrentRequest { get; private set; }
        public static T StartRequest<T>(T request) where T : AbstractRequest
        {
            if (CurrentRequest != null)
            {
                if (!CurrentRequest.IsComplete)
                {
                    Debug.LogError("Cannot start a new Package Switcher request. One already in progress.");
                    return null;
                }

                CurrentRequest = null;
            }

            CurrentRequest = request;
            request.OnComplete += OnCurrentRequestComplete;
            request.Start();

            return request;
        }

        private static void OnCurrentRequestComplete()
        {
            if (CurrentRequest != null)
            {
                if(CurrentRequest.Error != null)
                    Debug.LogError($"Package Switcher Request reported error: {CurrentRequest.Error}");
                CurrentRequest.OnComplete -= OnCurrentRequestComplete;
            }
            CurrentRequest = null;
        }
    }

    public class PackageSwitcherEditorStatusMessage : VisualElement
    {
        public Label Label { get; private set; }
        
        public PackageSwitcherEditorStatusMessage()
        {
            this.AddToClassList("package-switcher__status-message");
            
            Label = new Label();
            this.Add(Label);
        }

        public PackageSwitcherEditorStatusMessage(string message) : this()
        {
            this.Label.text = message;
        }
    }
}