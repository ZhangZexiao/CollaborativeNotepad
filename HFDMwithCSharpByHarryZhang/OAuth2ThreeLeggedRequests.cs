using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace HFDMwithCSharpByHarryZhang
{
	class OAuth2ThreeLeggedRequests
	{
		private Lynx.CollaborationClient.BearerTokenExpirationHandler bearerTokenAgent;
		private string apigeeHostUrl;
		private System.IO.StreamWriter sessionLogger;
		public OAuth2ThreeLeggedRequests(string apigeeHostUrl,Lynx.CollaborationClient.BearerTokenExpirationHandler bearerTokenAgent,System.IO.StreamWriter sessionLogger)
		{
			this.bearerTokenAgent=bearerTokenAgent;
			this.apigeeHostUrl=apigeeHostUrl;
			this.sessionLogger=sessionLogger;
		}
		private string downloadString(string endpoint)
		{
			string result;
			using(var client=new System.Net.WebClient())
			{
				client.Headers[System.Net.HttpRequestHeader.Authorization]="Bearer "+bearerTokenAgent.getBearerToken();
				try
				{
					result=client.DownloadString(apigeeHostUrl+endpoint);
				}
				catch(Exception e)
				{
					result= $"{{\"exception\":\"{e.Message}\", \"endpoint\":\"{apigeeHostUrl + endpoint}\"}}";
					sessionLogger.Log(result.ToPrettyJsonString());
				}
			}
			return result;
		}
		public string GetUserProfile()
		{
			//GET https://developer.api.autodesk.com/userprofile/v1/users/@me
			return downloadString("/userprofile/v1/users/@me").ToPrettyJsonString();
		}
		public string GetUserHubs()
		{
			//GET https://developer.api.autodesk.com/project/v1/hubs
			return downloadString("/project/v1/hubs").ToPrettyJsonString();
		}
		public string GetHubProjects(string hubId)
		{
			//GET https://developer.api.autodesk.com/project/v1/hubs/a.cGVyc29uYWw6cGUyOWNjZjMy/projects
			return downloadString("/project/v1/hubs/"+hubId+"/projects").ToPrettyJsonString();
		}
		public string GetProjectFolderContents(string projectId,string folderId)
		{
			//GET https://developer.api.autodesk.com/data/v1/projects/a.cGVyc29uYWw6cGUyOWNjZjMyI0QyMDE2MDUyNDEyOTI5NzY/folders/urn:adsk.wipprod:fs.folder:co.uvDiLQ5DRYidDQ_EFW1OOg/contents
			return downloadString("/data/v1/projects/"+projectId+"/folders/"+folderId+"/contents").ToPrettyJsonString();
		}
		public string GetProjectItemDetails(string projectId,string itemId)
		{
			//GET https://developer.api.autodesk.com/data/v1/projects/a.cGVyc29uYWw6cGUyOWNjZjMyI0QyMDE2MDUyNDEyOTI5NzY/items/urn:adsk.wipprod:dm.lineage:6bVr4EVDSaOpykczeQYR2Q
			return downloadString("/data/v1/projects/"+projectId+"/items/"+itemId).ToPrettyJsonString();
		}
		public string DownloadBucketObject(string bucketKey,string objectName)
		{
			//GET https://developer-stg.api.autodesk.com/oss/v2/buckets/wip.dm.stg/objects/a5d72f9a-359a-4a6f-a226-6b740b04d290.rvt
			using(var client=new System.Net.WebClient())
			{
				client.Headers[System.Net.HttpRequestHeader.Authorization]="Bearer "+bearerTokenAgent.getBearerToken();
				try
				{
					client.DownloadFile(apigeeHostUrl+"/oss/v2/buckets/"+bucketKey+"/objects/"+objectName,objectName);
					return new System.IO.FileInfo(objectName).FullName;
				}
				catch(Exception e)
				{
					var endpoint=apigeeHostUrl+"/oss/v2/buckets/"+bucketKey+"/objects/"+objectName;
					var result= $"{{\"exception\":\"{e.Message}\",\"endpoint\":\"{endpoint}\"}}";
					sessionLogger.Log(result.ToPrettyJsonString());
					return result.ToPrettyJsonString();
				}
			}
		}
	}
}
