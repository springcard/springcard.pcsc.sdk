using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace SpringCard.LibCs
{
	public partial class AboutForm : Form
	{
		public bool Flat = false;
		
		public AboutForm()
		{
			InitializeComponent();
			FileVersionInfo i = FileVersionInfo.GetVersionInfo(System.AppDomain.CurrentDomain.FriendlyName);
			lbCompanyProduct.Text = i.CompanyName + " " + i.ProductName;
			lbVersion.Text = "Version " + i.ProductVersion;
			lbCopyright.Text = "Copyright "  + i.LegalCopyright;
			lbTrademarks.Text = i.LegalTrademarks;			
		}
		
		public AboutForm(Color headerColor) : this()
		{
			pTop.BackColor = headerColor;
		}
		
		void AboutFormLoad(object sender, EventArgs e)
		{
			if (Flat)
			{
				btnOKNormal.Visible = false;
				btnOKFlat.Visible = true;
				AcceptButton = btnOKFlat;
				CancelButton = btnOKFlat;
			}
			else
			{
				btnOKFlat.Visible = false;
				btnOKNormal.Visible = true;
				AcceptButton = btnOKNormal;
				CancelButton = btnOKNormal;				
			}	
		}		
		
		void ImgTopClick(object sender, EventArgs e)
		{
		  System.Diagnostics.Process.Start("http://www.springcard.com");
		}
		
		void BtnOKClick(object sender, EventArgs e)
		{
		  Close();
		}
				
		void Lbl_MITLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			string str_path = Application.StartupPath + @"\LICENCE-MIT.txt";
			System.Diagnostics.Process.Start(str_path);
		}
				
		
		void Lbl_GNULinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			string str_path = Application.StartupPath + @"\LICENCE-GPL.txt";
			System.Diagnostics.Process.Start(str_path);
		}
		
		void Lbl_springcardLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			string str_path = Application.StartupPath + @"\LICENCE-SPRINGCARD.txt";
			System.Diagnostics.Process.Start(str_path);
		}
		
		void LinkSpringCardLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start("http://www.springcard.com");			
		}
		
		void LinkLabel1LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start("http://www.flaticon.com");			
		}
	}
}
