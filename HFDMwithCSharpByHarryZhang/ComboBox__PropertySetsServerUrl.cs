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
		class PropertySetsServerUrl
		{
			public string url
			{
				get;
				set;
			}
			public string inspectorUrl
			{
				get;
				set;
			}
			public bool?isBearerTokenRequired
			{
				get;
				set;
			}
			public override string ToString()
			{
				return string.Join(";",new string[]{url,inspectorUrl,isBearerTokenRequired.ToString()});
			}
		}
		//http://ecs-master-opt.ecs.ads.autodesk.com:3501/HFDMInspector.html?environment=stable&branchGuid=urn%3Aadsk.lynx%3Abranch%3A4e9d5a86-ac9b-4a54-a424-8c45836c5df9
		//http://ecs-master-opt.ecs.ads.autodesk.com:3501/HFDMInspector.html?environment=staging
		//http://ecs-master-opt.ecs.ads.autodesk.com:3501/HFDMInspector.html?environment=production
		//http://ecs-master-opt.ecs.ads.autodesk.com:3501/HFDMInspector.html?environment=samples
		PropertySetsServerUrl[]PropertySetsServerUrls=
		{
			new PropertySetsServerUrl
			{
				url="https://dev-noauth.hfdm.autodesk.com/v1/",isBearerTokenRequired=false
			}
			,new PropertySetsServerUrl
			{
				url="http://ecs-master-opt.ecs.ads.autodesk.com:3000",isBearerTokenRequired=false,inspectorUrl="http://ecs-master-opt.ecs.ads.autodesk.com:3501/HFDMInspector.html?branchGuid=urn:adsk.lynx:branch:"
			}
			,new PropertySetsServerUrl
			{
				url="https://developer-stg.api.autodesk.com/lynx/stable/v1/pss",isBearerTokenRequired=true,inspectorUrl="http://ecs-master-opt.ecs.ads.autodesk.com:3501/HFDMInspector.html?environment=stable&branchGuid=urn%3Aadsk.lynx%3Abranch%3A"
			}
			,new PropertySetsServerUrl
			{
				url="https://developer-stg.api.autodesk.com/hfdm-stable"
			}
			,new PropertySetsServerUrl
			{
				url="https://developer-stg.api.autodesk.com/hfdm"
			}
			,new PropertySetsServerUrl
			{
				url="https://developer-stg.api.autodesk.com/lynx/leo/v1/pss"
			}
		};
		PropertySetsServerUrl currentPropertySetsServerUrl;
		private void comboBox_PropertySetsServerUrls_SelectedIndexChanged(object sender,EventArgs e)
		{
			currentPropertySetsServerUrl=(sender as ComboBox).SelectedItem as PropertySetsServerUrl;
			button_Connect.Enabled=true;
		}
		private void initialize_ComboBox_PropertySetsServerUrls()
		{
			comboBox_PropertySetsServerUrls.DataSource=PropertySetsServerUrls;
			comboBox_PropertySetsServerUrls.DisplayMember="url";
		}
		private void write_RichTextBox_Logger_PropertySetsServerUrls()
		{
			PropertySetsServerUrls.ToList().ForEach(url=>richTextBox_Logger.AppendText(url.ToString()+System.Environment.NewLine));
		}
	}
}
