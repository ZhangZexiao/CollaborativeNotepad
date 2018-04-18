using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace HFDMwithCSharpByHarryZhang
{
	public partial class Form_CollaborativeNotepad:
	Form
	{
		private CefSharp.WinForms.ChromiumWebBrowser cefBrowser;
		private bool initializeCefBrowser()
		{
			cefBrowser=new CefSharp.WinForms.ChromiumWebBrowser("www.autodesk.com")
			{
				Dock=DockStyle.Fill
			};
			cefBrowser.ConsoleMessage+=CefBrowser_ConsoleMessage;
			tabPage4.Controls.Clear();
			tabPage4.Controls.Add(cefBrowser);
			tabPage4.Controls.Add(groupBox_Viewer);
			comboBox_Viewer_Environment.SelectedIndex=0;
			return true;
		}
		private delegate void Delegate_PrependUiLogger(string text);
		private void CefBrowser_ConsoleMessage(object sender,CefSharp.ConsoleMessageEventArgs e)
		{
			this.Invoke(new Delegate_PrependUiLogger(PrependUiLogger),new object[]{e.Message});
		}
		private void button_Viewer_Click(object sender,EventArgs e)
		{
			var env=comboBox_Viewer_Environment.Text;
			var accessToken=twoLeggedRequests.GetBearerToken();
			var documentId=textBox_Viewer_ObjectId.Text.ToUrlSafeBase64();
			cefBrowser.Load("file:///viewer.html?env="+env+"&accessToken="+accessToken+"&documentId="+documentId);
		}
		private void tabPage4_Enter(object sender,EventArgs e)
		{
			if(null==twoLeggedRequests)
			{
				twoLeggedRequests=new OAuth2TwoLeggedRequests(apigeeHostUrl,clientId,clientSecret,scope,sessionLogger);
			}
			setTitleWithAdditionalInformation(apigeeHostUrl);
		}
	}
}
