using System.IO;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace WhiteSparrow.PackageRepoEditor
{
    public class PackageJsonInfo
    {
        public string PackageName;
        public string PackageDisplayName;
        public string PackageVersion;

        public string PackageNameAndVersion;

        public string Homepage;
        public string RepositoryUrl;
        public string BugsUrl;
        
        public FileInfo PackageFile;

        public bool ReadFromJson(string packageJson)
        {
            var package = JObject.Parse(packageJson);

            this.PackageName = package["name"]?.Value<string>();
            this.PackageDisplayName = package["displayName"]?.Value<string>();
            this.PackageVersion = package["version"]?.Value<string>();
            this.RepositoryUrl = package["repository"]?["url"]?.Value<string>();
            this.BugsUrl = package["bugs"]?["url"]?.Value<string>();
            this.Homepage = package["homepage"]?.Value<string>();

            if (string.IsNullOrWhiteSpace(this.PackageName))
                return false;
            if (string.IsNullOrWhiteSpace(this.PackageVersion))
                return false;
            
            if (string.IsNullOrWhiteSpace(this.PackageDisplayName))
                this.PackageDisplayName = this.PackageName;
            return true;
        }

        public static PackageJsonInfo ReadFromFile(string pathAbsolute)
        {
            FileInfo packageFile = new FileInfo(pathAbsolute);
            return ReadFromFile(packageFile);
        }
        public static PackageJsonInfo ReadFromFile(FileInfo packageFile)
        {
            string packageJson = File.ReadAllText(packageFile.FullName);
            
            PackageJsonInfo output = new PackageJsonInfo();
            if (!output.ReadFromJson(packageJson))
                return null;
            
            output.PackageFile = packageFile;
            
            return output;
        }

        public static PackageJsonInfo ReadFromAsset(string assetPath)
        {
            TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
            if (asset == null)
                return null;

            PackageJsonInfo output = new PackageJsonInfo();
            if (!output.ReadFromJson(asset.text))
                return null;
            return output;
        }

        public static PackageJsonInfo ReadFromAssetGuid(string assetGuid)
        {
            return ReadFromAsset(AssetDatabase.GUIDToAssetPath(assetGuid));
        }
    }
}