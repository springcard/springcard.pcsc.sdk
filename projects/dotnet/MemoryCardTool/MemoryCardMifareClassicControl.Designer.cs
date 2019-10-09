/*
 * Created by SharpDevelop.
 * User: jerome.i
 * Date: 20/06/2013
 * Time: 14:30
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace MemoryCardTool
{
	partial class MemoryCardMifareClassicControl
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the control.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			this.SuspendLayout();
			// 
			// MemoryCardWithSectorsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Name = "MemoryCardWithSectorsControl";
			this.Size = new System.Drawing.Size(690, 523);
			this.ResumeLayout(false);
		}
	}
}
