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
		readonly string Form_CollaborativeNotepad_Title;
		void setTitleWithAdditionalInformation(string information)
		{
			setTitleWithCompilationMode();
			this.Text+=" - "+information;
		}
		void setTitleWithCompilationMode()
		{
			this.Text=Form_CollaborativeNotepad_Title;
#if DEBUG
			this.Text+=" - "+"DEBUG";
#else
			this.Text+=" - "+"RELEASE";
#endif
		}
	}
}
