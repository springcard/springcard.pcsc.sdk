/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 21/11/2017
 * Time: 17:47
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SpringCard.LibCs.Windows.Forms
{
	public enum WizardCancelText
	{
		Cancel,
		Close,
		Exit,
		Terminate
	}
	
	public enum WizardCancelAccept
	{
		Accept,
		PromptConfirm,
		Deny		
	}
	
	public interface WizardParent
	{	
		void WizardizeChildForm(Form childForm);
		void DisableCancel();
		void EnableCancel(WizardCancelText cancelText = WizardCancelText.Cancel);
		void EnablePrevious();
		void DisablePrevious();
		void EnableNext();
		void DisableNext();
		void GotoPrevious();
		void GotoNext();
	}
	
	public interface WizardChild
	{
		void WizardizeChild(WizardParent parent);
		void AddNext(WizardChild brother);
		WizardChild GetNext();
		string GetTitle();
		string GetSubtitle();
		void Appear(object data);
		void Disappear();
		void PreviousClicked(ref bool accept);
		void NextClicked(ref bool accept);
		void CancelClicked(ref WizardCancelAccept accept);
	}
}
