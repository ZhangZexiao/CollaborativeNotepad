namespace HFDMwithCSharpByHarryZhang
{
	public partial class Form_CollaborativeNotepad:
	System.Windows.Forms.Form
	{
		Lynx.PropertySets.HFDM hfdm;
		Lynx.PropertySets.Workspace workspace;
		const string KEYPRESS="KeyPress";
		const string KEYPRESSCOUNT="KeyPressCount";
		private string getKeyPressName(int i)
		{
			return KEYPRESS+i.ToString("D8");
		}
		private bool isEmptyBranchGuid(string newBranchGuid)
		{
			return string.IsNullOrWhiteSpace(newBranchGuid);
		}
		private string getBranchGuidFromUserInterface()
		{
			return comboBox_BranchGuid.Text;
		}
		private void setBranchGuidOnUserInterface(string newBranchGuid)
		{
			comboBox_BranchGuid.Text=newBranchGuid;
		}
		private void button_Checkout_Click(object sender,System.EventArgs e)
		{
			if((hfdm?.isConnected()).GetValueOrDefault(false)&&!(workspace?.isCheckedOut()).GetValueOrDefault(true))
			{
				if(isEmptyBranchGuid(getBranchGuidFromUserInterface()))
				{
					setBranchGuidOnUserInterface(createRepository(hfdm)[0].branchNode.getGuid());
					sessionLogger.Log("***created*** branch guid: "+getBranchGuidFromUserInterface());
				}
				if(!string.IsNullOrEmpty(currentPropertySetsServerUrl.inspectorUrl))
				{
					var inspectorUrl=currentPropertySetsServerUrl.inspectorUrl+getBranchGuidFromUserInterface();
					System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(inspectorUrl));
					cefBrowser.Load(inspectorUrl);
					sessionLogger.Log("inspector url: "+inspectorUrl);
				}
				checkoutWorkspace(workspace,getBranchGuidFromUserInterface());
				joinBranch(hfdm,workspace,getBranchGuidFromUserInterface());
			}
		}
		private void richTextBox_Notepad_KeyPress(object sender,System.Windows.Forms.KeyPressEventArgs e)
		{
			//get key press count
			Lynx.PropertySets.Int32Property keyPressCountProperty=workspace.resolvePathInt32Property(KEYPRESSCOUNT);
			if(null==keyPressCountProperty)
			{
				keyPressCountProperty=LynxPropertySetsCSharp.GetPropertyFactory().createInt32Property();
				keyPressCountProperty.setValue(0);
				workspace.insert(KEYPRESSCOUNT,keyPressCountProperty);
			}
			var count=keyPressCountProperty.getValue();
			sessionLogger.Log("key press count: "+count);
			//add a string property
			var stringProperty=LynxPropertySetsCSharp.GetPropertyFactory().createStringProperty();
			stringProperty.setValue(System.Text.Encoding.ASCII.GetString(System.BitConverter.GetBytes(e.KeyChar)));
			workspace.insert(getKeyPressName(count),stringProperty);
			//update key press count
			count++;
			keyPressCountProperty.setValue(count);
			//commit
			if(workspace.hasPendingChanges())
			{
				commitPendingChanges(workspace);
			}
			prettyPrintWorkspace(workspace);
		}
		private void prettyPrintWorkspace(Lynx.PropertySets.Workspace workspace)
		{
			sessionLogger.Log("workspace active branch guid: "+workspace.getActiveBranch().getGuid());
			sessionLogger.Log("workspace active branch head guid: "+workspace.getActiveBranch().getHEAD().getGuid());
			System.IO.MemoryStream memoryStream=new System.IO.MemoryStream();
			workspace.prettyPrint(memoryStream);
			sessionLogger.Log("workspace root pretty print: "+System.Text.Encoding.ASCII.GetString(memoryStream.ToArray()));
		}
		private bool joinBranch(Lynx.PropertySets.HFDM hfdm,Lynx.PropertySets.Workspace workspace,string branchGuid)
		{
			bool result=true;
			sessionLogger.Log("join branch: "+branchGuid);
			using(var eventWaitHandle=new System.Threading.EventWaitHandle(false,System.Threading.EventResetMode.ManualReset))
			{
				hfdm.join(branchGuid,(error,repository)=>{if(error!=null){System.Windows.Forms.MessageBox.Show(error.what());sessionLogger.Log("error: "+error.what());result=false;}else{checkoutWorkspace(workspace,branchGuid);}eventWaitHandle.Set();});
				eventWaitHandle.WaitOne();
			}
			return result;
		}
		private LynxRepositoryInfoVector createRepository(Lynx.PropertySets.HFDM hfdm)
		{
			LynxRepositoryInfoVector repositoryInfoVector=null;
			sessionLogger.Log("create repository.");
			using(var eventWaitHandle=new System.Threading.EventWaitHandle(false,System.Threading.EventResetMode.ManualReset))
			{
				hfdm.createRepository(false,(error,lynxRepositoryInfoVector)=>{if(error!=null){System.Windows.Forms.MessageBox.Show(error.what());sessionLogger.Log("error: "+error.what());}else if(lynxRepositoryInfoVector.Count<1){const string errorMessage="Create repository failed.";System.Windows.Forms.MessageBox.Show(errorMessage);sessionLogger.Log("error: "+errorMessage);}else{repositoryInfoVector=lynxRepositoryInfoVector;}eventWaitHandle.Set();});
				eventWaitHandle.WaitOne();
			}
			return repositoryInfoVector;
		}
		private void synchronizeWorkspace(Lynx.PropertySets.Workspace workspace)
		{
			if(!workspace.isSynchronized())
			{
				sessionLogger.Log("synchronize workspace.");
				using(var eventWaitHandle=new System.Threading.EventWaitHandle(false,System.Threading.EventResetMode.ManualReset))
				{
					workspace.synchronize((error,commitNodeVector)=>{if(error!=null){System.Windows.Forms.MessageBox.Show(error.what());sessionLogger.Log("error: "+error.what());}else{sessionLogger.Log("workspace synchronized: "+commitNodeVector.Count);}eventWaitHandle.Set();});
					eventWaitHandle.WaitOne();
				}
			}
		}
		private void commitPendingChanges(Lynx.PropertySets.Workspace workspace)
		{
			sessionLogger.Log("commit pending changes.");
			using(var eventWaitHandle=new System.Threading.EventWaitHandle(false,System.Threading.EventResetMode.ManualReset))
			{
				workspace.commit(System.DateTime.Now.ToString(),(error,commitNode)=>{if(error!=null){System.Windows.Forms.MessageBox.Show(error.what());sessionLogger.Log("error: "+error.what());}eventWaitHandle.Set();});
				eventWaitHandle.WaitOne();
			}
		}
		private bool checkoutWorkspace(Lynx.PropertySets.Workspace workspace,string branchGuid)
		{
			sessionLogger.Log("checkout workspace guid: "+branchGuid);
			if(!workspace.checkout(branchGuid))
			{
				const string errorMessage="Checkout workspace failed.";
				System.Windows.Forms.MessageBox.Show(errorMessage);
				sessionLogger.Log("error: "+errorMessage);
				return false;
			}
			//BUG: refreshRichTextBox called three times!!!
			//workspace.setAutoUpdate(true,(error)=>{if(error!=null){System.Windows.Forms.MessageBox.Show(error.what());sessionLogger.Log("error: "+error.what());}refreshRichTextBox();});
			return true;
		}
	}
}
