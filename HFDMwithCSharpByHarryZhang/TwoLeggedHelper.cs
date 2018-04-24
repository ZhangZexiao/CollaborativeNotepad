namespace HFDMwithCSharpByHarryZhang
{
	public partial class Form_CollaborativeNotepad:
	System.Windows.Forms.Form
	{
		OAuth2TwoLeggedRequests twoLeggedRequests;
		private void tabPage3_Enter(object sender,System.EventArgs e)
		{
			if(null==twoLeggedRequests)
			{
				twoLeggedRequests=new OAuth2TwoLeggedRequests(apigeeHostUrl,clientId,clientSecret,scope,sessionLogger);
			}
			setTitleWithAdditionalInformation(apigeeHostUrl);
		}
		private void button_CreateBucket_Click(object sender,System.EventArgs e)
		{
			PrependUiLogger(twoLeggedRequests.CreateBucket(textBox_CreateBucket_BucketKey.Text));
		}
		private void button_GetBucketDetails_Click(object sender,System.EventArgs e)
		{
			PrependUiLogger(twoLeggedRequests.GetBucketDetails(textBox_GetBucketDetails_BucketKey.Text));
		}
		private void button_GetBucketObjects_Click(object sender,System.EventArgs e)
		{
			PrependUiLogger(twoLeggedRequests.GetBucketObjects(textBox_GetBucketObjects_BucketKey.Text));
		}
		private void button_UploadFile_Click(object sender,System.EventArgs e)
		{
			PrependUiLogger(twoLeggedRequests.UploadFile(textBox_UploadFile_BucketKey.Text,textBox_UploadFile_FilePath.Text));
		}
		private void button_PostJob_Click(object sender,System.EventArgs e)
		{
			PrependUiLogger(twoLeggedRequests.PostJob(textBox_PostJob_ObjectId.Text.ToUrlSafeBase64()));
		}
		private void button_GetJobManifest_Click(object sender,System.EventArgs e)
		{
			PrependUiLogger(twoLeggedRequests.GetJobManifest(textBox_GetJobManifest_ObjectId.Text.ToUrlSafeBase64()));
		}
	}
}
