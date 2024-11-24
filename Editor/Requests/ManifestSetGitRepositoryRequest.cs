using UnityEditor.PackageManager;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace WhiteSparrow.PackageRepoEditor.Requests
{
    public class ManifestSetGitRepositoryRequest : AbstractManifestPackageUpdateRequest
    {
        private PackageInfo m_PackageInfo;

        public ManifestSetGitRepositoryRequest(PackageInfo packageInfo)
        {
            m_PackageInfo = packageInfo;
        }
        protected override void StartRequest()
        {
            string targetValue = m_PackageInfo.repository.url.Replace("git://", "https://");
            string packageName = m_PackageInfo.name;

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