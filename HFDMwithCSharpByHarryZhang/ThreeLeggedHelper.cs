namespace HFDMwithCSharpByHarryZhang
{
	public partial class Form_CollaborativeNotepad:
	System.Windows.Forms.Form
	{
		OAuth2ThreeLeggedBearerTokenAgent threeLeggedBearerTokenAgent;
		OAuth2ThreeLeggedRequests threeLeggedRequests;
		private void tabPage2_Enter(object sender,System.EventArgs e)
		{
			if(null==threeLeggedBearerTokenAgent)
			{
				threeLeggedBearerTokenAgent=new OAuth2ThreeLeggedBearerTokenAgent(apigeeHostUrl,clientId,clientSecret,scope,redirectUrl,sessionLogger);
			}
			if(null==threeLeggedRequests)
			{
				threeLeggedRequests=new OAuth2ThreeLeggedRequests(apigeeHostUrl,threeLeggedBearerTokenAgent,sessionLogger);
			}
			setTitleWithAdditionalInformation(apigeeHostUrl);
		}
		private void button_GetUserProfile_Click(object sender,System.EventArgs e)
		{
			PrependUiLogger(threeLeggedRequests.GetUserProfile());
		}
		private void button_GetUserHubs_Click(object sender,System.EventArgs e)
		{
			PrependUiLogger(threeLeggedRequests.GetUserHubs());
		}
		private void button_GetHubProjects_Click(object sender,System.EventArgs e)
		{
			PrependUiLogger(threeLeggedRequests.GetHubProjects(textBox_GetHubProjects_HubId.Text));
		}
		private void button_GetProjectFolderContents_Click(object sender,System.EventArgs e)
		{
			PrependUiLogger(threeLeggedRequests.GetProjectFolderContents(textBox_GetProjectFolderContents_ProjectId.Text,textBox_GetProjectFolderContents_FolderId.Text));
		}
		private void button_GetProjectItemDetails_Click(object sender,System.EventArgs e)
		{
			PrependUiLogger(threeLeggedRequests.GetProjectItemDetails(textBox_GetProjectItemDetails_ProjectId.Text,textBox_GetProjectItemDetails_ItemId.Text));
		}
		private void button_DownloadBucketObject_Click(object sender,System.EventArgs e)
		{
			PrependUiLogger(threeLeggedRequests.DownloadBucketObject(textBox_DownloadBucketObject_BucketKey.Text,textBox_DownloadBucketObject_ObjectId.Text));
		}
	}
}
