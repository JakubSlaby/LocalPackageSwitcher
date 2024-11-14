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
            window.FetchPackages(true);
            window.FindLocalPackages(true);
            return window;
        }

        private PackageSwitcherEditorListContainer m_ManifestPackageContainer;
        private PackageSwitcherEditorListContainer m_LocalPackagesContainer;

        private PackageSwitcherEditorStatusMessage m_ManifestPackagesStatusMessage;
        private PackageSwitcherEditorStatusMessage m_LocalPackagesStatusMessage;

        private Button m_RefreshPackages;
        private Button m_RefreshLocalPackages;
        
        private void OnEnable()
        {
            this.titleContent = EditorGUIUtility.TrTextContentWithIcon("Package Switcher", "d_Package Manager");
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

            OnRequestComplete += OnItemRequestComplete;
        }

        private void OnDestroy()
        {
	        
	        OnRequestComplete -= OnItemRequestComplete;
        }

        private void OnItemRequestComplete()
        {
	        FetchPackages(true);
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
            
            m_ManifestPackageContainer = new PackageSwitcherEditorListContainer();
            m_ManifestPackageContainer.Title.text = "Packages";
            m_RefreshPackages = new Button();
            m_RefreshPackages.text = "refresh";
            m_RefreshPackages.clickable.clicked += () =>
            {
				FetchPackages(true);
            };
            m_ManifestPackageContainer.TitleMetadataContainer.Add(m_RefreshPackages);
            container.Add(m_ManifestPackageContainer);
            
            m_LocalPackagesContainer = new PackageSwitcherEditorListContainer();
            m_LocalPackagesContainer.Title.text = "Available Local Packages";
            m_RefreshLocalPackages = new Button();
            m_RefreshLocalPackages.text = "refresh";
            m_RefreshLocalPackages.clickable.clicked += () =>
            {
                FindLocalPackages(true);
            };
            m_LocalPackagesContainer.TitleMetadataContainer.Add(m_RefreshLocalPackages);
            container.Add(m_LocalPackagesContainer);


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
                if (!m_FetchPackagesRequest.IsComplete)
	                return;

                if (m_FetchPackagesRequest.IsComplete)
                {
                    m_FetchPackagesRequest.OnComplete -= OnFetchPackagesRequestComplete;
                    m_FetchPackagesRequest = null;
                }
            }

            UpdatePackagesDisplay();
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
            if (m_ManifestPackageContainer == null)
                return;
            
            m_ManifestPackageContainer.contentContainer.Clear();
            m_ManifestPackageVisualItems.Clear();

            if (m_FetchPackagesRequest == null || !m_FetchPackagesRequest.IsComplete)
            {
                m_ManifestPackagesStatusMessage.SetMessage("Fetching packages...");
                m_ManifestPackageContainer.Add(m_ManifestPackagesStatusMessage);
                return;
            }
            
            if (m_FetchPackagesRequest.IsComplete && m_FetchPackagesRequest.Error != null)
            {
                m_ManifestPackagesStatusMessage.Label.text = m_FetchPackagesRequest.Error;
                m_ManifestPackageContainer.Add(m_ManifestPackagesStatusMessage);
                return;
            }
            
            if (m_FetchPackagesRequest.IsComplete && m_FetchPackagesRequest.Result.Length == 0)
            {
                m_ManifestPackagesStatusMessage.SetMessage("No packages found");
                m_ManifestPackageContainer.Add(m_ManifestPackagesStatusMessage);
                return;
            }

            foreach (var packageInfo in m_FetchPackagesRequest.Result)
            {
                PackageInfoSwitcherEditorItem item = new PackageInfoSwitcherEditorItem(packageInfo);
                m_ManifestPackageVisualItems.Add(item);
                m_ManifestPackageContainer.Add(item);
            }

            UpdateListChildrenClasses();
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

                if (!m_FindLocalPackagesRequest.IsComplete)
                    return;

                if (m_FindLocalPackagesRequest.IsComplete)
                {
                    m_FindLocalPackagesRequest.OnComplete -= OnFindLocalPackagesRequestComplete;
                    m_FindLocalPackagesRequest = null;
                }
            }
	        UpdateLocalPackagesDisplay();

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
            if (m_LocalPackagesContainer == null)
                return;
            m_LocalPackagesContainer.contentContainer.Clear();
            m_LocalPackageVisualItems.Clear();
            
            if (m_FindLocalPackagesRequest == null || !m_FindLocalPackagesRequest.IsComplete)
            {
                m_LocalPackagesStatusMessage.SetMessage("Searching for packages...");
                m_LocalPackagesContainer.Add(m_LocalPackagesStatusMessage);
                return;
            }
            
            if (m_FindLocalPackagesRequest.IsComplete && m_FindLocalPackagesRequest.Error != null)
            {
                m_LocalPackagesStatusMessage.Label.text = m_FindLocalPackagesRequest.Error;
                m_LocalPackagesContainer.Add(m_LocalPackagesStatusMessage);
                return;
            }
            
            if (m_FindLocalPackagesRequest.IsComplete && m_FindLocalPackagesRequest.Result.Length == 0)
            {
	            if (PackageRepoEditorSettings.instance == null ||
	                PackageRepoEditorSettings.instance.searchPaths == null ||
	                PackageRepoEditorSettings.instance.searchPaths.Length == 0)
	            {
		            m_LocalPackagesStatusMessage.SetMessage("No local package directories defined.", "configure in settings", ShowSwitcherSettingsWindow);
	            }
	            else
	            {
					m_LocalPackagesStatusMessage.SetMessage("No packages found");
	            }
	            m_LocalPackagesContainer.Add(m_LocalPackagesStatusMessage);
	            
                return;
            }

            foreach (var packageInfo in m_FindLocalPackagesRequest.Result)
            {
                PackageJsonSwitcherEditorItem item = new PackageJsonSwitcherEditorItem(packageInfo);
                m_LocalPackageVisualItems.Add(item);
                m_LocalPackagesContainer.Add(item);
            }

            UpdateListChildrenClasses();
        }

        private void ShowSwitcherSettingsWindow(EventBase obj)
        {
	        SettingsService.OpenProjectSettings(PackageRepoEditorSettingsInspector.SettingsPath);
        }

        private void UpdateListChildrenClasses()
        {
            bool odd = false;
            if (m_ManifestPackageContainer != null)
            {
                var children = m_ManifestPackageContainer.contentContainer.Children();
                foreach (var child in children)
                {
                    child.EnableInClassList("odd", odd = !odd);
                }
            }
            if (m_LocalPackagesContainer != null)
            {
                var children = m_LocalPackagesContainer.contentContainer.Children();
                foreach (var child in children)
                {
                    child.EnableInClassList("odd", odd = !odd);
                }
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
        private static event Action OnRequestComplete;
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
            OnRequestComplete?.Invoke();
        }

    }

    public class PackageSwitcherEditorStatusMessage : VisualElement
    {
        public Label Label { get; private set; }
        public Button ContextButton { get; private set; }
        private Action<EventBase> m_ContextButtonAction;
        
        public PackageSwitcherEditorStatusMessage()
        {
            this.AddToClassList("package-switcher__status-message");
            
            Label = new Label();
            this.Add(Label);
            
            ContextButton = new Button();
            ContextButton.clickable.clickedWithEventInfo += OnContextButtonClicked;
            ContextButton.text = "Settings";
            this.Add(ContextButton);
        }

        private void OnContextButtonClicked(EventBase eventBase)
        {
	        m_ContextButtonAction?.Invoke(eventBase);
        }

        public PackageSwitcherEditorStatusMessage(string message) : this()
        {
	        SetMessage(message);
        }

        public PackageSwitcherEditorStatusMessage(string message, string buttonText, Action buttonCallback) : this(message, buttonText, e => buttonCallback())
        {
	        
        }
        public PackageSwitcherEditorStatusMessage(string message, string buttonText, Action<EventBase> buttonCallback) : this()
        {
	        SetMessage(message, buttonText, buttonCallback);
        }

        public void SetMessage(string message)
        {
	        SetMessage(message, null, null);
        }
        public void SetMessage(string message, string buttonText, Action<EventBase> buttonCallback)
        {
	        this.Label.text = message;
            if (ContextButton.parent != null)
            {
                m_ContextButtonAction = null;
                ContextButton.RemoveFromHierarchy();
            }

            if (buttonCallback != null)
            {
	            ContextButton.text = buttonText;
	            m_ContextButtonAction = buttonCallback;
	            this.Add(ContextButton);
            }
        }
    }

    public class PackageSwitcherEditorListContainer : VisualElement
    {
        public VisualElement TitleRow { get; private set; }
        public Label Title { get; private set; }
        public VisualElement TitleMetadataContainer { get; private set; }
        
        private VisualElement m_Content;
        public override VisualElement contentContainer => m_Content;

        public PackageSwitcherEditorListContainer()
        {
            this.AddToClassList("package-list-container");
            this.AddToClassList("container-local-packages");

            TitleRow = new VisualElement();
            TitleRow.AddToClassList("title-row");
            hierarchy.Add(TitleRow);
            
            Title = new Label("Available Local Packages");
            Title.AddToClassList("title");
            TitleRow.Add(Title);

            TitleMetadataContainer = new VisualElement();
            TitleMetadataContainer.AddToClassList("title-metadata-container");
            TitleRow.Add(TitleMetadataContainer);
            
            m_Content = new VisualElement();
            m_Content.AddToClassList("content");
            hierarchy.Add(m_Content);
        }
    }
}