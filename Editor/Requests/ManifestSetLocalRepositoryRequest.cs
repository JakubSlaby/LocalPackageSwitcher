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
    public class ManifestSetLocalRepositoryRequest : AbstractRequest
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
            var manifestFile = PackageSwitcherEditor.ManifestFile;
            if (!manifestFile.Exists)
            {
                Debug.LogError($"Package Manifest doesn't exist at path {manifestFile.FullName}");
                return;
            }

            string manifestContent = File.ReadAllText(manifestFile.FullName);
            var manifest = JObject.Parse(manifestContent);

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
            
            string relativePath = Path.GetRelativePath(manifestFile.Directory.FullName,
                m_LocalPackageRecord.PackageFile.Directory.FullName).Replace("\\", "/");

            string targetValue = $"file:{relativePath}";
            
            var packageToken = manifest["dependencies"]?[packageName];
            if (packageToken != null)
            {
                string existingValue = packageToken.Value<string>();
                if (existingValue == targetValue)
                {
                    EditorUtility.DisplayDialog("Add package", "Already added", "ok");
                    Complete();
                    return;
                }

                if (!EditorUtility.DisplayDialog("Change package path",
                        $"{packageName} is already indexed with value:\n{existingValue}\n\n you are trying to change it to:\n{targetValue}",
                        "Continue", "Cancel"))
                {
                    Complete();
                    return;
                }
                
                manifest["dependencies"][packageName] = targetValue;
            }
            else
            {
                manifest["dependencies"][packageName] = targetValue;
            }
            
            File.WriteAllText(manifestFile.FullName, manifest.ToString(Formatting.Indented));
            Complete();
            Client.Resolve();
        }
    }
}