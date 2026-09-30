/*
 * Created by SharpDevelop.
 * User: jerome.i
 * Date: 19/06/2013
 * Time: 11:48
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace MemoryCardTool
{
	partial class MemoryCardControl
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
			this.lbP2 = new System.Windows.Forms.Label();
			this.lbASCII = new System.Windows.Forms.Label();
			this.lbP1 = new System.Windows.Forms.Label();
			this.HexBoxContent = new Be.Windows.Forms.HexBox();
			this.lbHex = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// lbP2
			// 
			this.lbP2.AutoSize = true;
			this.lbP2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lbP2.ForeColor = System.Drawing.SystemColors.GrayText;
			this.lbP2.Location = new System.Drawing.Point(86, 8);
			this.lbP2.Name = "lbP2";
			this.lbP2.Size = new System.Drawing.Size(20, 13);
			this.lbP2.TabIndex = 26;
			this.lbP2.Text = "P2";
			// 
			// lbASCII
			// 
			this.lbASCII.AutoSize = true;
			this.lbASCII.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lbASCII.ForeColor = System.Drawing.SystemColors.GrayText;
			this.lbASCII.Location = new System.Drawing.Point(218, 8);
			this.lbASCII.Name = "lbASCII";
			this.lbASCII.Size = new System.Drawing.Size(34, 13);
			this.lbASCII.TabIndex = 24;
			this.lbASCII.Text = "ASCII";
			// 
			// lbP1
			// 
			this.lbP1.AutoSize = true;
			this.lbP1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lbP1.ForeColor = System.Drawing.SystemColors.GrayText;
			this.lbP1.Location = new System.Drawing.Point(63, 8);
			this.lbP1.Name = "lbP1";
			this.lbP1.Size = new System.Drawing.Size(20, 13);
			this.lbP1.TabIndex = 23;
			this.lbP1.Text = "P1";
			// 
			// HexBoxContent
			// 
			this.HexBoxContent.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.HexBoxContent.LineInfoForeColor = System.Drawing.Color.Empty;
			this.HexBoxContent.LineInfoVisible = true;
			this.HexBoxContent.Location = new System.Drawing.Point(60, 24);
			this.HexBoxContent.Name = "HexBoxContent";
			this.HexBoxContent.ShadowSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(60)))), ((int)(((byte)(188)))), ((int)(((byte)(255)))));
			this.HexBoxContent.Size = new System.Drawing.Size(656, 310);
			this.HexBoxContent.StringViewVisible = true;
			this.HexBoxContent.TabIndex = 21;
			this.HexBoxContent.UseFixedBytesPerLine = true;
			this.HexBoxContent.VScrollBarVisible = true;
			this.HexBoxContent.KeyDown += new System.Windows.Forms.KeyEventHandler(this.HexBoxContentKeyDown);
			// 
			// lbHex
			// 
			this.lbHex.AutoSize = true;
			this.lbHex.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lbHex.ForeColor = System.Drawing.SystemColors.GrayText;
			this.lbHex.Location = new System.Drawing.Point(119, 8);
			this.lbHex.Name = "lbHex";
			this.lbHex.Size = new System.Drawing.Size(29, 13);
			this.lbHex.TabIndex = 22;
			this.lbHex.Text = "Hex.";
			// 
			// MemoryCardControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Controls.Add(this.lbP2);
			this.Controls.Add(this.lbASCII);
			this.Controls.Add(this.lbP1);
			this.Controls.Add(this.HexBoxContent);
			this.Controls.Add(this.lbHex);
			this.Name = "MemoryCardControl";
			this.Size = new System.Drawing.Size(747, 389);
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		private System.Windows.Forms.Label lbHex;
		private Be.Windows.Forms.HexBox HexBoxContent;
		private System.Windows.Forms.Label lbP1;
		private System.Windows.Forms.Label lbASCII;
		private System.Windows.Forms.Label lbP2;
	}
}
