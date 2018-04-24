namespace HFDMwithCSharpByHarryZhang
{
	public partial class Form_CollaborativeNotepad:
	System.Windows.Forms.Form
	{
		public Form_CollaborativeNotepad()
		{
			InitializeComponent();
			//initialize title
			Form_CollaborativeNotepad_Title=this.Text;
			setTitleWithCompilationMode();
			//initialize logger
			sessionLogger=SessionLogger.CreateSessionLogger();
			//initialize combo box
			initialize_ComboBox_PropertySetsServerUrls();
			write_RichTextBox_Logger_PropertySetsServerUrls();
			//initialize settings
			initializeSettings();
			applyCurrentSettings();
			//initialize cef browser
			initializeCefBrowser();
		}
	}
}
