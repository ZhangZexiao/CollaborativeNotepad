namespace HFDMwithCSharpByHarryZhang
{
	class BearerToken
	{
		public string AccessToken
		{
			get;
			set;
		}
		public string TokenType
		{
			get;
			set;
		}
		public uint ExpiresIn
		{
			get;
			set;
		}
		public string RefreshToken
		{
			get;
			set;
		}
	}
}
