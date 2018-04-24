namespace HFDMwithCSharpByHarryZhang
{
	public partial class Form_CollaborativeNotepad:
	System.Windows.Forms.Form
	{
		private bool createHfdmWorkspace(ref Lynx.PropertySets.HFDM hfdm,ref Lynx.PropertySets.Workspace workspace,PropertySetsServerUrl propertySetsServerUrl,System.Func<string>getBearerToken)
		{
			sessionLogger.Log("create hfdm and workspace: "+propertySetsServerUrl.ToString());
			hfdm=LynxPropertySetsCSharp.CreateHFDM();
			using(var eventWaitHandle=new System.Threading.EventWaitHandle(false,System.Threading.EventResetMode.ManualReset))
			{
				hfdm.connect(propertySetsServerUrl.url,(error)=>{if(error!=null){sessionLogger.Log("error: "+error.what());}eventWaitHandle.Set();},getBearerToken);
				eventWaitHandle.WaitOne();
			}
			sessionLogger.Log("hfdm is connected: "+hfdm.isConnected());
			if(hfdm.isConnected())
			{
				workspace=hfdm.createWorkspace();
			}
			return hfdm.isConnected()&&null!=workspace;
		}
		private bool connectPropertySetsServer()
		{
			if(currentPropertySetsServerUrl.isBearerTokenRequired.GetValueOrDefault(true))
			{
				threeLeggedBearerTokenAgent=new OAuth2ThreeLeggedBearerTokenAgent(apigeeHostUrl,clientId,clientSecret,scope,redirectUrl,sessionLogger);
				return createHfdmWorkspace(ref hfdm,ref workspace,currentPropertySetsServerUrl,threeLeggedBearerTokenAgent.GetBearerToken);
			}
			else
			{
				return createHfdmWorkspace(ref hfdm,ref workspace,currentPropertySetsServerUrl,null);
			}
		}
		private bool disconnectPropertySetsServer()
		{
			if(null!=workspace)
			{
				if(workspace.hasPendingChanges())
				{
					commitPendingChanges(workspace);
				}
				workspace.Dispose();
				workspace=null;
			}
			if(null!=hfdm)
			{
				if(hfdm.isConnected())
				{
					hfdm.disconnect();
				}
				hfdm.Dispose();
				hfdm=null;
			}
			return true;
		}
		private void button_Connect_Click(object sender,System.EventArgs e)
		{
			if(disconnectPropertySetsServer())
			{
				setTitleWithCompilationMode();
			}
			button_Connect.Enabled=false;
			if(connectPropertySetsServer())
			{
				setTitleWithAdditionalInformation(currentPropertySetsServerUrl.url);
			}
			else
			{
				System.Windows.Forms.MessageBox.Show("Unable to connect "+currentPropertySetsServerUrl.url);
				disconnectPropertySetsServer();
				button_Connect.Enabled=true;
			}
		}
	}
}
