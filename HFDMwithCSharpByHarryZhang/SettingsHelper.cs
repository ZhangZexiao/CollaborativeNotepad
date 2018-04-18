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
		string clientId="PC4fujKFEJIT2XsBvKyUhG3oBAHXfxKW";
		string clientSecret="GAO7oBn9KJeaDmNu";
		string scope="data:read data:write data:create bucket:create bucket:read bucket:update bucket:delete";
		string apigeeHostUrl="https://developer-stg.api.autodesk.com";
		string redirectUrl="http://localhost:5989";
		private void initializeSettings()
		{
			comboBox_ApigeeHostUrls.SelectedIndex=0;
			comboBox_ClientId.SelectedIndex=0;
			comboBox_ClientSecret.SelectedIndex=0;
			comboBox_Scope.SelectedIndex=0;
			comboBox_RedirectUrl.SelectedIndex=0;
		}
		private void applyCurrentSettings()
		{
			clientId=comboBox_ClientId.Text;
			clientSecret=comboBox_ClientSecret.Text;
			scope=comboBox_Scope.Text;
			apigeeHostUrl=comboBox_ApigeeHostUrls.Text;
			redirectUrl=comboBox_RedirectUrl.Text;
			threeLeggedBearerTokenAgent=null;
			threeLeggedRequests=null;
			twoLeggedRequests=null;
		}
		private bool isSettingsChanged()
		{
			return!(clientId==comboBox_ClientId.Text&&clientSecret==comboBox_ClientSecret.Text&&scope==comboBox_Scope.Text&&apigeeHostUrl==comboBox_ApigeeHostUrls.Text&&redirectUrl==comboBox_RedirectUrl.Text);
		}
		private void tabPage5_Leave(object sender,EventArgs e)
		{
			if(isSettingsChanged())
			{
				applyCurrentSettings();
			}
		}
	}
}
