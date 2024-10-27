using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using WhiteSparrow.PackageRepoEditor;

namespace Plugins.WhiteSparrow.Shared_PackageRepoEditor.Editor.Requests
{
    public class FindLocalPackagesRequest : AbstractRequest
    {
        public class PackageRecord
        {
            public string PackageName;
            public string PackageDisplayName;
            public string PackageVersion;

            public string PackageNameAndVersion;
            
            public FileInfo PackageFile;

            public bool ReadFromJson(string packageJson)
            {
                var package = JObject.Parse(packageJson);

                this.PackageName = package["name"]?.Value<string>();
                this.PackageDisplayName = package["displayName"]?.Value<string>();
                this.PackageVersion = package["version"]?.Value<string>();

                if (string.IsNullOrWhiteSpace(this.PackageName))
                    return false;
                if (string.IsNullOrWhiteSpace(this.PackageVersion))
                    return false;
                
                if (string.IsNullOrWhiteSpace(this.PackageDisplayName))
                    this.PackageDisplayName = this.PackageName;
                return true;
            }

            public static PackageRecord ReadFromFile(string pathAbsolute)
            {
                FileInfo packageFile = new FileInfo(pathAbsolute);
                return ReadFromFile(packageFile);
            }
            public static PackageRecord ReadFromFile(FileInfo packageFile)
            {
                string packageJson = File.ReadAllText(packageFile.FullName);
                
                PackageRecord output = new PackageRecord();
                if (!output.ReadFromJson(packageJson))
                    return null;
                
                output.PackageFile = packageFile;
                
                return output;
            }
        }

        private string[] m_SearchDirectories;
        private CancellationTokenSource m_CancellationTokenSource;
        private ConcurrentBag<FileInfo> m_CandidatePackageFiles;
        private ConcurrentBag<PackageRecord> m_OutputRecords;

        private static string[] s_ProhibitedPathPatterns = new[]
        {
            Path.Combine("Library", "PackageCache")
        };
        
        public FindLocalPackagesRequest(string[] searchDirectories)
        {
            m_SearchDirectories = searchDirectories;
        }
        
        public PackageRecord[] Result { get; private set; }
        
        protected override void StartRequest()
        {
            if (m_SearchDirectories == null || m_SearchDirectories.Length == 0)
            {
                Complete();
                return;
            }

            m_OutputRecords = new ConcurrentBag<PackageRecord>();
            m_CandidatePackageFiles = new ConcurrentBag<FileInfo>();
            
            m_CancellationTokenSource = new CancellationTokenSource();
            var task = Task.Run(() => Parallel.ForEach(m_SearchDirectories, ParallelSearch), m_CancellationTokenSource.Token);
            task.ContinueWith(t => ProcessCandidates());
        }

        private void ProcessCandidates()
        {
            var candidates = m_CandidatePackageFiles.ToArray();
            var task = Task.Run(() => Parallel.ForEach(candidates, ParallelProcessCandidate), m_CancellationTokenSource.Token);
            task.ContinueWith(t => { EditorApplication.delayCall += AttemptComplete; });
        }

        private void AttemptComplete()
        {
            Result = m_OutputRecords.ToArray();
            Array.Sort(Result, (lhs, rhs) => EditorUtility.NaturalCompare(lhs.PackageDisplayName, rhs.PackageDisplayName));
            Complete();
        }

        private void ParallelSearch(string searchDirectory, ParallelLoopState state)
        {
            if (!Path.IsPathRooted(searchDirectory))
                return;
            
            DirectoryInfo directory = new DirectoryInfo(searchDirectory);
            if (!directory.Exists)
                return;
            FileInfo[] packageFiles = directory.GetFiles("package.json", SearchOption.AllDirectories);

            if (packageFiles.Length == 0)
                return;

            foreach (var packageFile in packageFiles)
            {
                m_CandidatePackageFiles.Add(packageFile);
            }
        }

        private void ParallelProcessCandidate(FileInfo packageFile)
        {
            string fullPath = packageFile.FullName;
            foreach (var prohibitedPath in s_ProhibitedPathPatterns)
            {
                if (fullPath.Contains(prohibitedPath))
                    return;
            }
            
            PackageRecord output =  PackageRecord.ReadFromFile(packageFile);
            if (output == null)
                return;
            
            m_OutputRecords.Add(output);
        }
    }
}