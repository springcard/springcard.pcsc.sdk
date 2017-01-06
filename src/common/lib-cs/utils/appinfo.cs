using System;
using System.Reflection;
using System.Diagnostics;
using System.IO;

namespace SpringCard.LibCs
{
	public class ApplicationInfo
	{
		private static Assembly mainAssembly;
		private static FileVersionInfo mainAssemblyInfo;
			
		private static void Prepare()
		{
			if (mainAssembly == null)
				mainAssembly = System.Reflection.Assembly.GetEntryAssembly();
			if (mainAssemblyInfo == null)
				mainAssemblyInfo = FileVersionInfo.GetVersionInfo(mainAssembly.Location);
		}
		
		public static string ExecutionDirectory
		{
			get
			{
				Prepare();
				return Path.GetDirectoryName(mainAssembly.Location);
			}
		}
		
		public static string IdentificationString
		{
			get
			{
				Prepare();
				return String.Format("{0} {1} {2}", mainAssemblyInfo.CompanyName, mainAssemblyInfo.ProductName, mainAssemblyInfo.ProductVersion);
			}			
		}
	}
}