using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
