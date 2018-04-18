using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace HFDMwithCSharpByHarryZhang
{
	public class OAuth2ThreeLeggedBearerTokenAgent
	{
		private System.IO.StreamWriter sessionLogger;
		public OAuth2ThreeLeggedBearerTokenAgent(System.IO.StreamWriter _sessionLogger)
		{
			sessionLogger=_sessionLogger;
		}
		//https://stackoverflow.com/questions/47375304/nancy-self-host-returns-404
		public class AuthorizationCodeHelper
		{
			private static string authorizationCode;
			public class AuthorizationCodeHelperModule:
			Nancy.NancyModule
			{
				public AuthorizationCodeHelperModule()
				{
					Get["/"]=parameters=>
					{
						authorizationCode=Request.Query["code"];
						eventWaitHandler.Set();
						return authorizationCode;
					};
				}
			}
			private string url,redirectUrl;
			private System.IO.StreamWriter sessionLogger;
			public AuthorizationCodeHelper(string apigeeHost,string clientId,string scope,string redirectUrl,System.IO.StreamWriter sessionLogger)
			{
				//https://developer-stg.autodesk.com/en/docs/oauth/v2/reference/http/authorize-GET/
				url=apigeeHost+"/authentication/v1/authorize?"+"response_type=code"+"&client_id="+clientId+"&redirect_uri="+Nancy.Helpers.HttpUtility.UrlEncode(redirectUrl)+"&scope="+Nancy.Helpers.HttpUtility.UrlEncode(scope);
				this.redirectUrl=redirectUrl;
				this.sessionLogger=sessionLogger;
			}
			private static System.Threading.EventWaitHandle eventWaitHandler;
			public string GetAuthorizationCode()
			{
				using(var host=new Nancy.Hosting.Self.NancyHost(new Nancy.Hosting.Self.HostConfiguration(){UrlReservations={CreateAutomatically=true}},new Uri(redirectUrl)))
				{
					host.Start();
					eventWaitHandler=new System.Threading.EventWaitHandle(false,System.Threading.EventResetMode.ManualReset);
					sessionLogger.Log("launch operating system default web browser: "+url);
					System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url));
					eventWaitHandler.WaitOne();
					sessionLogger.Log("authorization code: "+authorizationCode);
					System.Threading.Thread.Sleep(1000);
				}
				return authorizationCode;
			}
		}
		private Lynx.CollaborationClient.AuthenticationToken getThreeLeggedBearerToken(string apigeeHost,Lynx.CollaborationClient.AuthTokenRequest request)
		{
			//https://developer-stg.autodesk.com/en/docs/oauth/v2/reference/http/gettoken-POST/
			string endpoint=apigeeHost+"/authentication/v1/gettoken";
			sessionLogger.Log("oauth2, get three legged bearer token, request endpoint: "+endpoint);
			string body="client_id="+request.m_clientId+"&client_secret="+request.m_clientSecret+"&grant_type=authorization_code"+"&code="+request.m_code+"&redirect_uri="+request.m_callbackUrl;
			sessionLogger.Log("oauth2, get three legged bearer token, request body(form url encoded): "+body);
			string response;
			using(var client=new System.Net.WebClient())
			{
				client.Headers[System.Net.HttpRequestHeader.ContentType]="application/x-www-form-urlencoded";
				response=client.UploadString(endpoint,"POST",body);
			}
			var jsonObject=Newtonsoft.Json.Linq.JObject.Parse(response);
			sessionLogger.Log("oauth2, get three legged bearer token, response: "+jsonObject.ToString());
			return new Lynx.CollaborationClient.AuthenticationToken()
			{
				m_accessToken=jsonObject["access_token"].Value<string>(),m_tokenType=jsonObject["token_type"].Value<string>(),m_expiresIn=jsonObject["expires_in"].Value<uint>(),m_refreshToken=jsonObject["refresh_token"].Value<string>()
			};
		}
		public Lynx.CollaborationClient.BearerTokenExpirationHandler CreateThreeLeggedBearerTokenAgent(string apigeeHostUrl,string clientId,string clientSecret,string scope,string redirectUrl)
		{
			var authTokenRequest=new Lynx.CollaborationClient.AuthTokenRequest(clientId,clientSecret,new AuthorizationCodeHelper(apigeeHostUrl,clientId,scope,redirectUrl,sessionLogger).GetAuthorizationCode(),redirectUrl);
			var bearerTokenExpirationHandler=new Lynx.CollaborationClient.BearerTokenExpirationHandler(apigeeHostUrl,authTokenRequest);
			bearerTokenExpirationHandler.setTokenResult(getThreeLeggedBearerToken(apigeeHostUrl,authTokenRequest));
			return bearerTokenExpirationHandler;
		}
	}
}
