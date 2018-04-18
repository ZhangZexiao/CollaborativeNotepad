using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace HFDMwithCSharpByHarryZhang
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			CefSharp.Cef.EnableHighDPISupport();
			CefSharp.Cef.Initialize(new CefSharp.CefSettings(){IgnoreCertificateErrors=true});
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form_CollaborativeNotepad());
		}
	}
}
