namespace HFDMwithCSharpByHarryZhang
{
	public static class UrlSafeBase64Encoder
	{
		public static string ToUrlSafeBase64(this string text)
		{
			return System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(text)).TrimEnd(new char[]{'='}).Replace('+','-').Replace('/','_');
		}
	}
}
