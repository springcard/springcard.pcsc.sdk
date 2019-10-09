/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 21/11/2017
 * Time: 17:47
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SpringCard.LibCs.Windows.Forms
{
	public partial class WizardParentForm : Form, WizardParent
	{
		List<WizardChild> breadcrumbs = new List<WizardChild>();
		object data;
		
		public WizardParentForm(string title, WizardChild firstChild, object data = null)
		{
			InitializeComponent();
			Text = title;
			breadcrumbs.Add(firstChild);
			this.data = data;
		}
		
		void InitializeChild(WizardChild aChild)
		{
			aChild.WizardizeChild(this);
		}
		
		void Disappear(WizardChild aChild)
		{
			Logger.Debug("Hiding " + aChild.GetType().FullName);
			aChild.Disappear();
			lbTitle.Text = "";
			lbSubTitle.Text = "";
			btnPrev.Enabled = false;
			btnNext.Enabled = false;
			btnCancel.Enabled = false;
		}
		
		void Appear(WizardChild aChild)
		{
			InitializeChild(aChild);
			Logger.Debug("Showing " + aChild.GetType().FullName);
			aChild.Appear(data);
			lbTitle.Text = aChild.GetTitle();
			lbSubTitle.Text = aChild.GetSubtitle();
		}
	
		public void GotoPrevious()
		{			
			if (breadcrumbs.Count < 2)
				return;
			
			WizardChild currentChild = breadcrumbs[breadcrumbs.Count - 1];
			WizardChild previousChild = breadcrumbs[breadcrumbs.Count - 2];
			
			Disappear(currentChild);
			breadcrumbs.RemoveAt(breadcrumbs.Count - 1);
			Appear(previousChild);
		}
		
		public void GotoNext()
		{
			if (breadcrumbs.Count < 1)
				return;
			
			WizardChild currentChild = breadcrumbs[breadcrumbs.Count - 1];
			WizardChild nextChild = currentChild.GetNext();
			if (nextChild == null)
				return;
			
			Disappear(currentChild);
			breadcrumbs.Add(nextChild);
			Appear(nextChild);
		}
		
		public void DisableCancel()
		{
			btnCancel.Enabled = false;
		}
		
		public void EnableCancel(WizardCancelText cancelText = WizardCancelText.Cancel)
		{		
			switch (cancelText)
			{
				case WizardCancelText.Cancel :
					btnCancel.Text = T._("Cancel");
					break;			
				case WizardCancelText.Close :
					btnCancel.Text = T._("Close");
					break;			
				case WizardCancelText.Exit :
					btnCancel.Text = T._("Exit");
					break;								
				case WizardCancelText.Terminate :
					btnCancel.Text = T._("Terminate");
					break;			
			}
			
			btnCancel.Enabled = true;			
		}

		public void DisablePrevious()
		{
			btnPrev.Enabled = false;
		}
		
		public void EnablePrevious()
		{
			btnPrev.Enabled = true;
		}

		public void DisableNext()
		{
			btnNext.Enabled = false;
		}
		
		public void EnableNext()
		{
			btnNext.Enabled = true;
		}
		
		public void WizardizeChildForm(Form childForm)
		{
			childForm.TopLevel = false;
			childForm.FormBorderStyle = FormBorderStyle.None;
			childForm.AutoScroll = true;
			pMain.Controls.Add(childForm);
			childForm.Dock = DockStyle.Fill;			
		}

		void WizardParentFormShown(object sender, EventArgs e)
		{
			Logger.Debug("WizardParentFormShown");
			Appear(breadcrumbs[0]);
		}

		void BtnPrevClick(object sender, EventArgs e)
		{
			Logger.Debug("[<- Previous]");
			
			bool Accept = true;
			
			WizardChild currentChild = breadcrumbs[breadcrumbs.Count - 1];
			currentChild.PreviousClicked(ref Accept);
			
			if (Accept)
				GotoPrevious();
		}
		
		void BtnNextClick(object sender, EventArgs e)
		{
			Logger.Debug("[-> Next]");
			
			bool Accept = true;
			
			WizardChild currentChild = breadcrumbs[breadcrumbs.Count - 1];
			currentChild.NextClicked(ref Accept);
			
			if (Accept)
				GotoNext();	
		}
		
		void BtnCancelClick(object sender, EventArgs e)
		{
			WizardCancelAccept Accept = WizardCancelAccept.Accept;

			Logger.Debug("[Cancel]");
			
			WizardChild currentChild = breadcrumbs[breadcrumbs.Count - 1];
			currentChild.CancelClicked(ref Accept);
			
			if (Accept == WizardCancelAccept.Deny)
				return;
			
			Close();
		}
		
		void WizardParentFormFormClosing(object sender, FormClosingEventArgs e)
		{

		}
		
		void WizardParentFormKeyDown(object sender, KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.PageUp :
				case Keys.Left :
					if (btnPrev.Enabled)
						BtnPrevClick(sender, null);
					break;
					
				case Keys.PageDown :
				case Keys.Right :
					if (btnNext.Enabled)
						BtnNextClick(sender, null);
					break;					
			}
		}
		
	}
}
