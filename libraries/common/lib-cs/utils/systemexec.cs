/**
 *
 * \ingroup LibCs
 *
 * \copyright
 *   Copyright (c) 2008-2018 SpringCard - www.springcard.com
 *   All right reserved
 *
 * \author
 *   Johann.D et al. / SpringCard
 *
 */
/*
 * Read LICENSE.txt for license details and restrictions.
 */
using System;
using System.Diagnostics;

namespace SpringCard.LibCs
{
	/**
	 * \brief Utilities to run and control external programs
	 */
	public class SystemExec
	{
		/**
		 * \breif Run an external program and returns its stdout as a single string 
		 */
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