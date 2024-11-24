using UnityEditor.PackageManager;

namespace WhiteSparrow.PackageRepoEditor.Requests
{
    public class ManifestSetRemoteVersionRequest : AbstractManifestPackageUpdateRequest
    {
        private PackageInfo m_PackageInfo;
        private PackageJsonInfo m_PackageJsonInfo;
        private string m_Version;
        
        public ManifestSetRemoteVersionRequest(PackageInfo packageInfo, string version)
        {
            m_PackageInfo = packageInfo;
            m_Version = version;
        }
        
        public ManifestSetRemoteVersionRequest(PackageJsonInfo packageJsonInfo, string version)
        {
            m_PackageJsonInfo = packageJsonInfo;
            m_Version = version;
        }
        
        protected override void StartRequest()
        {
            string packageName = null;
            if (m_PackageJsonInfo != null)
                packageName = m_PackageJsonInfo.PackageName;
            else if (m_PackageInfo != null)
                packageName = m_PackageInfo.name;

            if (packageName == null)
            {
                Complete();
                return;
            }

            if (!LoadAndUpdateManifest(packageName, m_Version))
            {
                Complete();
                return;
            }
            
            Complete();
            Client.Resolve();
        }
    }
}