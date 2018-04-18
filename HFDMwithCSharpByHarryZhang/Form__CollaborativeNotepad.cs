using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
namespace HFDMwithCSharpByHarryZhang
{
	public partial class Form_CollaborativeNotepad:
	Form
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
