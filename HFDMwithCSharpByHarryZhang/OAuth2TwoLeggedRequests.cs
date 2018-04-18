using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
namespace HFDMwithCSharpByHarryZhang
{
	class OAuth2TwoLeggedRequests
	{
		private System.IO.StreamWriter sessionLogger;
		private Lynx.CollaborationClient.AuthenticationToken twoLeggedBearerToken;
		private string apigeeHostUrl,clientId,clientSecret,scope;
		private Timer timer;
		public OAuth2TwoLeggedRequests(string apigeeHostUrl,string clientId,string clientSecret,string scope,System.IO.StreamWriter sessionLogger)
		{
			this.sessionLogger=sessionLogger;
			this.apigeeHostUrl=apigeeHostUrl;
			this.clientId=clientId;
			this.clientSecret=clientSecret;
			this.scope=scope;
			twoLeggedBearerToken=getTwoLeggedBearerToken(clientId,clientSecret,scope);
			timer=new Timer(twoLeggedBearerToken.m_expiresIn*1000);
			timer.Elapsed+=Timer_Elapsed;
			timer.Enabled=true;
		}
		private void Timer_Elapsed(object sender,ElapsedEventArgs e)
		{
			sessionLogger.Log("refresh two legged bearer token");
			twoLeggedBearerToken=getTwoLeggedBearerToken(clientId,clientSecret,scope);
			timer.Enabled=false;
			timer.Interval=twoLeggedBearerToken.m_expiresIn*1000;
			timer.Enabled=true;
		}
		private Lynx.CollaborationClient.AuthenticationToken getTwoLeggedBearerToken(string clientId,string clientSecret,string scope)
		{
			var authTokenRequest=new Lynx.CollaborationClient.AuthTokenRequest(clientId,clientSecret,"","",scope);
			return getTwoLeggedBearerToken(authTokenRequest);
		}
		private Lynx.CollaborationClient.AuthenticationToken getTwoLeggedBearerToken(Lynx.CollaborationClient.AuthTokenRequest request)
		{
			//POST https://developer.api.autodesk.com/authentication/v1/authenticate
			string endpoint=apigeeHostUrl+"/authentication/v1/authenticate";
			sessionLogger.Log("oauth2, get two legged bearer token, request endpoint: "+endpoint);
			string body="client_id="+request.m_clientId+"&client_secret="+request.m_clientSecret+"&grant_type=client_credentials"+"&scope="+Nancy.Helpers.HttpUtility.UrlEncode(request.m_scope);
			sessionLogger.Log("oauth2, get two legged bearer token, request body(form url encoded): "+body);
			string response;
			using(var client=new System.Net.WebClient())
			{
				client.Headers[System.Net.HttpRequestHeader.ContentType]="application/x-www-form-urlencoded";
				response=client.UploadString(endpoint,"POST",body);
			}
			var jsonObject=Newtonsoft.Json.Linq.JObject.Parse(response);
			sessionLogger.Log("oauth2, get two legged bearer token, response: "+jsonObject.ToString());
			return new Lynx.CollaborationClient.AuthenticationToken()
			{
				m_accessToken=jsonObject["access_token"].Value<string>(),m_tokenType=jsonObject["token_type"].Value<string>(),m_expiresIn=jsonObject["expires_in"].Value<uint>()
			};
		}
		private string downloadString(string endpoint)
		{
			string result;
			using(var client=new System.Net.WebClient())
			{
				client.Headers[System.Net.HttpRequestHeader.Authorization]="Bearer "+twoLeggedBearerToken.m_accessToken;
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
		public string GetBearerToken()
		{
			return twoLeggedBearerToken.m_accessToken;
		}
		public string CreateBucket(string bucketKey)
		{
			//POST https://developer-stg.api.autodesk.com/oss/v2/buckets
			string result;
			using(var client=new System.Net.WebClient())
			{
				client.Headers[System.Net.HttpRequestHeader.Authorization]="Bearer "+twoLeggedBearerToken.m_accessToken;
				client.Headers[System.Net.HttpRequestHeader.ContentType]="application/json";
				var jsonObject=new
				{
					bucketKey=bucketKey,policyKey="transient"
				};
				var jsonString=Newtonsoft.Json.JsonConvert.SerializeObject(jsonObject);
				try
				{
					result=client.UploadString(apigeeHostUrl+"/oss/v2/buckets","POST",jsonString);
				}
				catch(Exception e)
				{
					result= $"{{\"exception\":\"{e.Message}\"}}";
					sessionLogger.Log(result.ToPrettyJsonString());
				}
			}
			return result.ToPrettyJsonString();
		}
		public string GetBucketDetails(string bucketKey)
		{
			//GET https://developer.api.autodesk.com/oss/v2/buckets/mybucket/details
			return downloadString("/oss/v2/buckets/"+bucketKey+"/details").ToPrettyJsonString();
		}
		public string GetBucketObjects(string bucketKey)
		{
			//GET buckets/:bucketKey/objects
			return downloadString("/oss/v2/buckets/"+bucketKey+"/objects").ToPrettyJsonString();
		}
		public string UploadFile(string bucketKey,string filePath)
		{
			//PUT https://developer.api.autodesk.com/oss/v2/buckets/mybucket/objects/skyscpr1.3ds
			string result;
			using(var client=new System.Net.WebClient())
			{
				client.Headers[System.Net.HttpRequestHeader.Authorization]="Bearer "+twoLeggedBearerToken.m_accessToken;
				try
				{
					var bytes=client.UploadFile(apigeeHostUrl+"/oss/v2/buckets/"+bucketKey+"/objects/"+System.IO.Path.GetFileName(filePath),"PUT",filePath);
					result=System.Text.Encoding.ASCII.GetString(bytes);
				}
				catch(Exception e)
				{
					result= $"{{\"exception\":\"{e.Message}\"}}";
					sessionLogger.Log(result.ToPrettyJsonString());
				}
			}
			return result.ToPrettyJsonString();
		}
		public string PostJob(string urlSafeBase64ObjectId)
		{
			//POST https://developer.api.autodesk.com/modelderivative/v2/designdata/job
			string result;
			using(var client=new System.Net.WebClient())
			{
				client.Headers[System.Net.HttpRequestHeader.Authorization]="Bearer "+twoLeggedBearerToken.m_accessToken;
				client.Headers[System.Net.HttpRequestHeader.ContentType]="application/json";
				var jsonObject=new
				{
					input=new
					{
						urn=urlSafeBase64ObjectId
					}
					,output=new
					{
						formats=new[]
						{
							new
							{
								type="svf",views=new[]
								{
									"2d","3d"
								}
							}
						}
					}
				};
				var jsonString=Newtonsoft.Json.JsonConvert.SerializeObject(jsonObject);
				sessionLogger.Log("post model derivative job, request: "+jsonString);
				try
				{
					result=client.UploadString(apigeeHostUrl+"/modelderivative/v2/designdata/job","POST",jsonString);
				}
				catch(Exception e)
				{
					result= $"{{\"exception\":\"{e.Message}\"}}";
					sessionLogger.Log(result.ToPrettyJsonString());
				}
			}
			return result.ToPrettyJsonString();
		}
		public string GetJobManifest(string urlSafeBase64ObjectId)
		{
			//GET https://developer.api.autodesk.com/modelderivative/v2/designdata/:urn/manifest
			return downloadString("/modelderivative/v2/designdata/"+urlSafeBase64ObjectId+"/manifest").ToPrettyJsonString();
		}
	}
}
