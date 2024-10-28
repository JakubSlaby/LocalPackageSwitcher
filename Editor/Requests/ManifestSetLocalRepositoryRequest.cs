using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using WhiteSparrow.PackageRepoEditor;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Plugins.WhiteSparrow.Shared_PackageRepoEditor.Editor.Requests
{
    public class ManifestSetLocalRepositoryRequest : AbstractManifestPackageUpdateRequest
    {
        private PackageInfo m_PackageInfo;
        private FindLocalPackagesRequest.PackageRecord m_LocalPackageRecord;

        public ManifestSetLocalRepositoryRequest(PackageInfo packageInfo)
        {
            m_PackageInfo = packageInfo;
        }
        
        public ManifestSetLocalRepositoryRequest(FindLocalPackagesRequest.PackageRecord localPackageRecord)
        {
            m_LocalPackageRecord = localPackageRecord;
        }
        
        protected override void StartRequest()
        {
            string packageName = null;
            if (m_PackageInfo != null)
            {
                packageName = m_PackageInfo.packageId;
            }
            else if (m_LocalPackageRecord != null)
            {
                packageName = m_LocalPackageRecord.PackageName;
            }

            if (m_LocalPackageRecord == null)
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

                m_LocalPackageRecord = FindLocalPackagesRequest.PackageRecord.ReadFromFile(packageFile);
            }

            if (m_LocalPackageRecord == null)
            {
                Complete();
                return;
            }
            
            string relativePath = Path.GetRelativePath(PackageSwitcherEditor.ManifestFile.Directory.FullName,
                m_LocalPackageRecord.PackageFile.Directory.FullName).Replace("\\", "/");
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