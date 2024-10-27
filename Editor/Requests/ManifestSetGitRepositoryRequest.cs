using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
using WhiteSparrow.PackageRepoEditor;

namespace Plugins.WhiteSparrow.Shared_PackageRepoEditor.Editor.Requests
{
    public class ManifestSetGitRepositoryRequest : AbstractRequest
    {
        private PackageInfo m_PackageInfo;

        public ManifestSetGitRepositoryRequest(PackageInfo packageInfo)
        {
            m_PackageInfo = packageInfo;
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

            
            string targetValue = m_PackageInfo.repository.url.Replace("git://", "https://");
            string packageName = m_PackageInfo.name;
            
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

                if (!EditorUtility.DisplayDialog("Set package Git Url",
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