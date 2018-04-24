namespace HFDMwithCSharpByHarryZhang
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[System.STAThread]
		static void Main()
		{
			CefSharp.Cef.EnableHighDPISupport();
			CefSharp.Cef.Initialize(new CefSharp.CefSettings(){IgnoreCertificateErrors=true});
			System.Windows.Forms.Application.EnableVisualStyles();
			System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
			System.Windows.Forms.Application.Run(new Form_CollaborativeNotepad());
		}
	}
}
