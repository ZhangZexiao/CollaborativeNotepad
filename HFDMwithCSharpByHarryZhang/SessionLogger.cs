using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace HFDMwithCSharpByHarryZhang
{
	public static class SessionLogger
	{
		public static System.IO.StreamWriter CreateSessionLogger()
		{
			//https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings
			//create only one session log file one day!
			//append mode, save all sessions!
			var sessionLogger=new System.IO.StreamWriter("CollaborativeNotepad"+System.DateTime.Now.ToString("yyyyMMdd")+".log",true);
			//session start time
			sessionLogger.WriteLine(new string('*',30)+System.DateTime.Now.ToString()+new string('*',30));
			return sessionLogger;
		}
	}
	public static class StreamWriterExtensions
	{
		public static void Log(this System.IO.StreamWriter logStream,string log)
		{
			logStream.WriteLine(System.DateTime.Now.ToString()+": "+log);
			logStream.Flush();
		}
	}
}
