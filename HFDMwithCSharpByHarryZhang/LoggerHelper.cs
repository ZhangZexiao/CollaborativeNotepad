namespace HFDMwithCSharpByHarryZhang
{
	public partial class Form_CollaborativeNotepad:
	System.Windows.Forms.Form
	{
		System.IO.StreamWriter sessionLogger;
		private void PrependUiLogger(string text)
		{
			text=System.Environment.NewLine+text+System.Environment.NewLine;
			richTextBox_Logger.Text=text+richTextBox_Logger.Text;
			//write log file
			sessionLogger.Log(text);
			if(checkBox__LogSettings_SwtichToLogTabPage.Checked)
			{
				//jump to logger
				tabControl1.SelectedIndex=5;
			}
		}
	}
}
