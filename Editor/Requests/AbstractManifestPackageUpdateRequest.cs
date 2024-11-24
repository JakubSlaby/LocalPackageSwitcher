using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace WhiteSparrow.PackageRepoEditor.Requests
{
    public abstract class AbstractManifestPackageUpdateRequest : AbstractRequest
    {
        protected virtual bool LoadAndUpdateManifest(string packageName, string targetValue)
        {
            var manifestFile = PackageSwitcherEditor.ManifestFile;
            if (!manifestFile.Exists)
            {
                Debug.LogError($"Package Manifest doesn't exist at path {manifestFile.FullName}");
                return false;
            }
            
            string manifestContent = File.ReadAllText(manifestFile.FullName);
            var manifest = JObject.Parse(manifestContent);
            
            // string targetValue = m_PackageInfo.repositorytory.url.Replace("git://", "https://");
            // string packageName = m_PackageInfo.name;

            var dependencies = manifest["dependencies"];
            if (dependencies == null)
                return false;
            
            var packageToken = dependencies[packageName];
            if (packageToken != null)
            {
                string existingValue = packageToken.Value<string>();
                if (existingValue == targetValue)
                {
                    EditorUtility.DisplayDialog("Set package value", $"Already set to value: {targetValue}", "ok");
                    return false;
                }

                if (!EditorUtility.DisplayDialog("Set package value",
                        $"{packageName} is already indexed with value:\n{existingValue}\n\n you are trying to change it to:\n{targetValue}",
                        "Continue", "Cancel"))
                {
                    return false;
                }
                
                dependencies[packageName] = targetValue;
            }
            else
            {
                dependencies[packageName] = targetValue;
            }

            File.WriteAllText(manifestFile.FullName, manifest.ToString(Formatting.Indented));
            
            return true;
        }
    }
}