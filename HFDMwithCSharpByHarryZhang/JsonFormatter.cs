namespace HFDMwithCSharpByHarryZhang
{
	static class JsonFormatter
	{
		public static string ToPrettyJsonString(this string uglyJsonString)
		{
			return Newtonsoft.Json.Linq.JObject.Parse(uglyJsonString).ToString();
		}
	}
}
