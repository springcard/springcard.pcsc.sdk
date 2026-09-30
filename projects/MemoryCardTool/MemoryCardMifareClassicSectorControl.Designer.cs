/*
 * Created by SharpDevelop.
 * User: jerome.i
 * Date: 20/06/2013
 * Time: 14:36
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace MemoryCardTool
{
	partial class MemoryCardMifareClassicSectorControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MemoryCardMifareClassicSectorControl));
			this.lbASCII = new System.Windows.Forms.Label();
			this.lbSector = new System.Windows.Forms.Label();
			this.HexBoxContent = new Be.Windows.Forms.HexBox();
			this.lbHex = new System.Windows.Forms.Label();
			this.btChangeAccesConditions = new System.Windows.Forms.Button();
			this.btTryDifferentKeys = new System.Windows.Forms.Button();
			this.btWriteToCard = new System.Windows.Forms.Button();
			this.btnUnlock = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// lbASCII
			// 
			this.lbASCII.AutoSize = true;
			this.lbASCII.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lbASCII.ForeColor = System.Drawing.SystemColors.GrayText;
			this.lbASCII.Location = new System.Drawing.Point(430, 11);
			this.lbASCII.Name = "lbASCII";
			this.lbASCII.Size = new System.Drawing.Size(34, 13);
			this.lbASCII.TabIndex = 30;
			this.lbASCII.Text = "ASCII";
			// 
			// lbSector
			// 
			this.lbSector.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lbSector.ForeColor = System.Drawing.SystemColors.GrayText;
			this.lbSector.Location = new System.Drawing.Point(38, 11);
			this.lbSector.Name = "lbSector";
			this.lbSector.Size = new System.Drawing.Size(24, 13);
			this.lbSector.TabIndex = 29;
			this.lbSector.Text = "10";
			this.lbSector.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// HexBoxContent
			// 
			this.HexBoxContent.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.HexBoxContent.LineInfoForeColor = System.Drawing.Color.Empty;
			this.HexBoxContent.LineInfoVisible = true;
			this.HexBoxContent.Location = new System.Drawing.Point(40, 27);
			this.HexBoxContent.Name = "HexBoxContent";
			this.HexBoxContent.ShadowSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(60)))), ((int)(((byte)(188)))), ((int)(((byte)(255)))));
			this.HexBoxContent.Size = new System.Drawing.Size(604, 51);
			this.HexBoxContent.StringViewVisible = true;
			this.HexBoxContent.TabIndex = 27;
			this.HexBoxContent.UseFixedBytesPerLine = true;
			this.HexBoxContent.VScrollBarVisible = true;
			this.HexBoxContent.KeyDown += new System.Windows.Forms.KeyEventHandler(this.HexBoxContentKeyDown);
			// 
			// lbHex
			// 
			this.lbHex.AutoSize = true;
			this.lbHex.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lbHex.ForeColor = System.Drawing.SystemColors.GrayText;
			this.lbHex.Location = new System.Drawing.Point(78, 11);
			this.lbHex.Name = "lbHex";
			this.lbHex.Size = new System.Drawing.Size(29, 13);
			this.lbHex.TabIndex = 28;
			this.lbHex.Text = "Hex.";
			// 
			// btChangeAccesConditions
			// 
			this.btChangeAccesConditions.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.btChangeAccesConditions.FlatAppearance.BorderSize = 0;
			this.btChangeAccesConditions.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.btChangeAccesConditions.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btChangeAccesConditions.Image = ((System.Drawing.Image)(resources.GetObject("btChangeAccesConditions.Image")));
			this.btChangeAccesConditions.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.btChangeAccesConditions.Location = new System.Drawing.Point(650, 57);
			this.btChangeAccesConditions.Name = "btChangeAccesConditions";
			this.btChangeAccesConditions.Size = new System.Drawing.Size(27, 22);
			this.btChangeAccesConditions.TabIndex = 32;
			this.btChangeAccesConditions.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.btChangeAccesConditions.UseVisualStyleBackColor = false;
			this.btChangeAccesConditions.Click += new System.EventHandler(this.BtChangeAccesConditionsClick);
			// 
			// btTryDifferentKeys
			// 
			this.btTryDifferentKeys.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.btTryDifferentKeys.FlatAppearance.BorderSize = 0;
			this.btTryDifferentKeys.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.btTryDifferentKeys.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btTryDifferentKeys.Image = ((System.Drawing.Image)(resources.GetObject("btTryDifferentKeys.Image")));
			this.btTryDifferentKeys.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.btTryDifferentKeys.Location = new System.Drawing.Point(650, 27);
			this.btTryDifferentKeys.Name = "btTryDifferentKeys";
			this.btTryDifferentKeys.Size = new System.Drawing.Size(27, 22);
			this.btTryDifferentKeys.TabIndex = 33;
			this.btTryDifferentKeys.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.btTryDifferentKeys.UseVisualStyleBackColor = false;
			// 
			// btWriteToCard
			// 
			this.btWriteToCard.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.btWriteToCard.FlatAppearance.BorderSize = 0;
			this.btWriteToCard.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.btWriteToCard.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btWriteToCard.ForeColor = System.Drawing.SystemColors.ControlText;
			this.btWriteToCard.Image = ((System.Drawing.Image)(resources.GetObject("btWriteToCard.Image")));
			this.btWriteToCard.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.btWriteToCard.Location = new System.Drawing.Point(650, 27);
			this.btWriteToCard.Name = "btWriteToCard";
			this.btWriteToCard.Size = new System.Drawing.Size(27, 22);
			this.btWriteToCard.TabIndex = 34;
			this.btWriteToCard.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.btWriteToCard.UseVisualStyleBackColor = false;
			this.btWriteToCard.Click += new System.EventHandler(this.BtWriteToCardClick);
			// 
			// btnUnlock
			// 
			this.btnUnlock.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.btnUnlock.FlatAppearance.BorderSize = 0;
			this.btnUnlock.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.btnUnlock.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnUnlock.Image = ((System.Drawing.Image)(resources.GetObject("btnUnlock.Image")));
			this.btnUnlock.Location = new System.Drawing.Point(650, 0);
			this.btnUnlock.Name = "btnUnlock";
			this.btnUnlock.Size = new System.Drawing.Size(27, 22);
			this.btnUnlock.TabIndex = 35;
			this.btnUnlock.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.btnUnlock.UseVisualStyleBackColor = false;
			this.btnUnlock.Visible = false;
			this.btnUnlock.Click += new System.EventHandler(this.BtnUnlockClick);
			// 
			// MemoryCardMifareClassicSectorControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Controls.Add(this.btnUnlock);
			this.Controls.Add(this.btWriteToCard);
			this.Controls.Add(this.btTryDifferentKeys);
			this.Controls.Add(this.btChangeAccesConditions);
			this.Controls.Add(this.lbASCII);
			this.Controls.Add(this.lbSector);
			this.Controls.Add(this.HexBoxContent);
			this.Controls.Add(this.lbHex);
			this.Name = "MemoryCardMifareClassicSectorControl";
			this.Size = new System.Drawing.Size(689, 94);
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		private System.Windows.Forms.Button btWriteToCard;
		private System.Windows.Forms.Button btTryDifferentKeys;
		private System.Windows.Forms.Button btChangeAccesConditions;
		private System.Windows.Forms.Label lbHex;
		private Be.Windows.Forms.HexBox HexBoxContent;
		private System.Windows.Forms.Label lbSector;
		private System.Windows.Forms.Label lbASCII;
		private System.Windows.Forms.Button btnUnlock;
	}
}
