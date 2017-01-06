/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 02/03/2012
 * Time: 17:56
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
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
			SetStyle(lbProductName, ControlStyles.SupportsTransparentBackColor, true);
			InitializeComponent();			
			FileVersionInfo i = FileVersionInfo.GetVersionInfo(System.AppDomain.CurrentDomain.FriendlyName);
			lbVersion.Text = i.ProductVersion;
			lbVersionCode.Text = String.Format("{0:00}.{1:00}", i.ProductMajorPart, i.ProductMinorPart);
			lbCopyright.Text = i.LegalCopyright;
		}
		
		void SplashFormClose(object sender, EventArgs e)
		{
			Close();		
		}

		public static bool SetStyle(Control c, ControlStyles Style, bool value)
		{
		    bool retval = false;
		    Type typeTB = typeof(Control);
		    System.Reflection.MethodInfo misSetStyle = typeTB.GetMethod("SetStyle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		    if (misSetStyle != null && c != null) { misSetStyle.Invoke(c, new object[] { Style, value }); retval = true; }
		    return retval;
		}
		
		protected override CreateParams CreateParams {
			get {
				const int CS_DROPSHADOW = 0x20000;
				CreateParams cp = base.CreateParams;
				cp.ClassStyle |= CS_DROPSHADOW;
				return cp;
			}
		}
	}
}
