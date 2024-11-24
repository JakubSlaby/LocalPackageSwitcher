using System;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace WhiteSparrow.PackageRepoEditor.Requests
{
	public class FetchPackagesRequest : AbstractRequest
	{
		public bool OnlineMode { get; private set; }
		public PackageInfo[] Result { get; private set; }

		public FetchPackagesRequest(bool onlineMode)
		{
			OnlineMode = onlineMode;
		}
		
		protected override void StartRequest()
		{
			PackageManagerRequest.Wrap(Client.List(!OnlineMode), OnListRequestCompleted);
		}

		private void OnListRequestCompleted(ListRequest request)
		{
			if (request.Error != null)
			{
				CompleteError(request.Error.message);
				return;
			}
			
			List<PackageInfo> output = new List<PackageInfo>();
			foreach (var packageInfo in request.Result)
			{
				if (packageInfo.packageId.StartsWith("com.unity", StringComparison.OrdinalIgnoreCase))
					continue;
				
				output.Add(packageInfo);
			}

			Result = output.ToArray();
			Complete();
		}
	}
}