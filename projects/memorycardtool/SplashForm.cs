/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 02/03/2012
 * Time: 17:56
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Windows.Forms;
using System.Diagnostics;

namespace SpringCardApplication
{
	/// <summary>
	/// Description of SplashForm.
	/// </summary>
	public partial class SplashForm : Form
	{
		public SplashForm()
		{
			InitializeComponent();
			System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
			FileVersionInfo i = FileVersionInfo.GetVersionInfo(assembly.Location);
			//FileVersionInfo i = FileVersionInfo.GetVersionInfo(System.AppDomain.CurrentDomain.FriendlyName);
			lbVersion.Text = i.ProductVersion;
		}
		
		void SplashFormClose(object sender, EventArgs e)
		{
			Close();		
		}
	}
}
