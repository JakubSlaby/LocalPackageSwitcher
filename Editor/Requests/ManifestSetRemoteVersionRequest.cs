
using UnityEditor.PackageManager;

namespace Plugins.WhiteSparrow.Shared_PackageRepoEditor.Editor.Requests
{
    public class ManifestSetRemoteVersionRequest : AbstractManifestPackageUpdateRequest
    {
        private PackageInfo m_PackageInfo;
        private FindLocalPackagesRequest.PackageRecord m_PackageRecord;
        private string m_Version;
        
        public ManifestSetRemoteVersionRequest(PackageInfo packageInfo, string version)
        {
            m_PackageInfo = packageInfo;
            m_Version = version;
        }
        
        public ManifestSetRemoteVersionRequest(FindLocalPackagesRequest.PackageRecord packageRecord, string version)
        {
            m_PackageRecord = packageRecord;
            m_Version = version;
        }
        
        protected override void StartRequest()
        {
            string packageName = null;
            if (m_PackageRecord != null)
                packageName = m_PackageRecord.PackageName;
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