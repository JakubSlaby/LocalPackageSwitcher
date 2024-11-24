using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace WhiteSparrow.PackageRepoEditor.Requests
{
    public class ManifestSetLocalRepositoryRequest : AbstractManifestPackageUpdateRequest
    {
        private PackageInfo m_PackageInfo;
        private PackageJsonInfo m_LocalPackageJsonInfo;

        public ManifestSetLocalRepositoryRequest(PackageInfo packageInfo)
        {
            m_PackageInfo = packageInfo;
        }
        
        public ManifestSetLocalRepositoryRequest(PackageJsonInfo localPackageJsonInfo)
        {
            m_LocalPackageJsonInfo = localPackageJsonInfo;
        }
        
        protected override void StartRequest()
        {
            string packageName = null;
            if (m_PackageInfo != null)
            {
                packageName = m_PackageInfo.name;
            }
            else if (m_LocalPackageJsonInfo != null)
            {
                packageName = m_LocalPackageJsonInfo.PackageName;
            }

            if (m_LocalPackageJsonInfo == null)
            {
                var targetOpenFile = EditorUtility.OpenFilePanel("Open package file", Application.dataPath, "json");

                if (string.IsNullOrWhiteSpace(targetOpenFile))
                {
                    Complete();
                    return;
                }

                FileInfo packageFile = new FileInfo(targetOpenFile);
                if(!packageFile.Exists)
                {
                    Complete();
                    return;
                }

                if (packageFile.Name != "package.json")
                {
                    Complete();
                    return;
                }

                m_LocalPackageJsonInfo = PackageJsonInfo.ReadFromFile(packageFile);
            }

            if (m_LocalPackageJsonInfo == null)
            {
                Complete();
                return;
            }
            
            string relativePath = Path.GetRelativePath(PackageSwitcherEditor.ManifestFile.Directory.FullName,
                m_LocalPackageJsonInfo.PackageFile.Directory.FullName).Replace("\\", "/");
            string targetValue = $"file:{relativePath}";

            if (!LoadAndUpdateManifest(packageName, targetValue))
            {
                Complete();
                return;
            }
            
            
            Complete();
            Client.Resolve();
        }
    }
}