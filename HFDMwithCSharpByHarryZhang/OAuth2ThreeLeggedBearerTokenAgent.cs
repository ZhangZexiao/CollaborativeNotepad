using Newtonsoft.Json.Linq;
namespace HFDMwithCSharpByHarryZhang
{
	public class OAuth2ThreeLeggedBearerTokenAgent
	{
		private System.IO.StreamWriter sessionLogger;
		private void Log(string log)
		{
			sessionLogger?.Log(log);
		}
		private BearerToken threeLeggedBearerToken;
		private string apigeeHostUrl,clientId,clientSecret;
		private System.Timers.Timer timer;
		public OAuth2ThreeLeggedBearerTokenAgent(string apigeeHostUrl,string clientId,string clientSecret,string scope,string redirectUrl,System.IO.StreamWriter sessionLogger)
		{
			this.sessionLogger=sessionLogger;
			this.apigeeHostUrl=apigeeHostUrl;
			this.clientId=clientId;
			this.clientSecret=clientSecret;
			threeLeggedBearerToken=getThreeLeggedBearerToken(apigeeHostUrl,clientId,clientSecret,scope,redirectUrl);
			timer=new System.Timers.Timer(threeLeggedBearerToken.ExpiresIn*1000);
			timer.Elapsed+=Timer_Elapsed;
			timer.Enabled=true;
		}
		private void Timer_Elapsed(object sender,System.Timers.ElapsedEventArgs e)
		{
			Log("refresh three legged bearer token");
			threeLeggedBearerToken=refreshThreeLeggedBearerToken();
			timer.Enabled=false;
			timer.Interval=threeLeggedBearerToken.ExpiresIn*1000;
			timer.Enabled=true;
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
						//error CS0656: Missing compiler required member 'Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo.Create'
						authorizationCode=Request.Query["code"];
						eventWaitHandler.Set();
						return authorizationCode;
					};
				}
			}
			private string url,redirectUrl;
			private System.IO.StreamWriter sessionLogger;
			private void Log(string log)
			{
				sessionLogger?.Log(log);
			}
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
				using(var host=new Nancy.Hosting.Self.NancyHost(new Nancy.Hosting.Self.HostConfiguration(){UrlReservations={CreateAutomatically=true}},new System.Uri(redirectUrl)))
				{
					host.Start();
					eventWaitHandler=new System.Threading.EventWaitHandle(false,System.Threading.EventResetMode.ManualReset);
					Log("launch operating system default web browser: "+url);
					System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url));
					eventWaitHandler.WaitOne();
					Log("authorization code: "+authorizationCode);
					System.Threading.Thread.Sleep(1000);
				}
				return authorizationCode;
			}
		}
		private BearerToken getThreeLeggedBearerToken(string apigeeHostUrl,string clientId,string clientSecret,string scope,string redirectUrl)
		{
			//https://developer-stg.autodesk.com/en/docs/oauth/v2/reference/http/gettoken-POST/
			string endpoint=apigeeHostUrl+"/authentication/v1/gettoken";
			Log("oauth2, get three legged bearer token, request endpoint: "+endpoint);
			string body="client_id="+clientId+"&client_secret="+clientSecret+"&grant_type=authorization_code"+"&code="+new AuthorizationCodeHelper(apigeeHostUrl,clientId,scope,redirectUrl,sessionLogger).GetAuthorizationCode()+"&redirect_uri="+redirectUrl;
			Log("oauth2, get three legged bearer token, request body(form url encoded): "+body);
			string response;
			using(var client=new System.Net.WebClient())
			{
				client.Headers[System.Net.HttpRequestHeader.ContentType]="application/x-www-form-urlencoded";
				response=client.UploadString(endpoint,"POST",body);
			}
			var jsonObject=Newtonsoft.Json.Linq.JObject.Parse(response);
			Log("oauth2, get three legged bearer token, response: "+jsonObject.ToString());
			return new BearerToken()
			{
				AccessToken=jsonObject["access_token"].Value<string>(),TokenType=jsonObject["token_type"].Value<string>(),ExpiresIn=jsonObject["expires_in"].Value<uint>(),RefreshToken=jsonObject["refresh_token"].Value<string>()
			};
		}
		private BearerToken refreshThreeLeggedBearerToken()
		{
			//POST https://developer.api.autodesk.com/authentication/v1/refreshtoken
			string endpoint=apigeeHostUrl+"/authentication/v1/refreshtoken";
			Log("oauth2, refresh three legged bearer token, request endpoint: "+endpoint);
			string body="client_id="+clientId+"&client_secret="+clientSecret+"&grant_type=refresh_token"+"&refresh_token="+threeLeggedBearerToken.RefreshToken;
			Log("oauth2, refresh three legged bearer token, request body(form url encoded): "+body);
			string response;
			using(var client=new System.Net.WebClient())
			{
				client.Headers[System.Net.HttpRequestHeader.ContentType]="application/x-www-form-urlencoded";
				response=client.UploadString(endpoint,"POST",body);
			}
			var jsonObject=Newtonsoft.Json.Linq.JObject.Parse(response);
			Log("oauth2, refresh three legged bearer token, response: "+jsonObject.ToString());
			return new BearerToken()
			{
				AccessToken=jsonObject["access_token"].Value<string>(),TokenType=jsonObject["token_type"].Value<string>(),ExpiresIn=jsonObject["expires_in"].Value<uint>(),RefreshToken=jsonObject["refresh_token"].Value<string>()
			};
		}
		public string GetBearerToken()
		{
			return threeLeggedBearerToken.AccessToken;
		}
	}
}
