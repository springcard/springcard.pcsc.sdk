using System;
using System.Diagnostics;

namespace SpringCard.LibCs
{
	public class SystemExec
	{
		public static string ExecuteToString(string FileName, string Arguments = null)
		{
			Process p = new Process();
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.RedirectStandardError = false;
			p.StartInfo.FileName = FileName;
			p.StartInfo.Arguments = Arguments;
			p.Start();
			string result = p.StandardOutput.ReadToEnd();
			p.WaitForExit();
			return result;
		}
		
	}
}