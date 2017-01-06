/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 02/03/2012
 * Time: 17:56
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace SpringCardApplication
{
	partial class SplashForm
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SplashForm));
			this.AutoCloseTimer = new System.Windows.Forms.Timer(this.components);
			this.lbVersion = new System.Windows.Forms.Label();
			this.lbVersionCode = new System.Windows.Forms.Label();
			this.lbCopyright = new System.Windows.Forms.Label();
			this.lbProductName = new System.Windows.Forms.Label();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// AutoCloseTimer
			// 
			this.AutoCloseTimer.Enabled = true;
			this.AutoCloseTimer.Interval = 2000;
			this.AutoCloseTimer.Tick += new System.EventHandler(this.SplashFormClose);
			// 
			// lbVersion
			// 
			this.lbVersion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
			this.lbVersion.ForeColor = System.Drawing.Color.DimGray;
			this.lbVersion.Location = new System.Drawing.Point(12, 380);
			this.lbVersion.Name = "lbVersion";
			this.lbVersion.Size = new System.Drawing.Size(686, 18);
			this.lbVersion.TabIndex = 1;
			this.lbVersion.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// lbVersionCode
			// 
			this.lbVersionCode.BackColor = System.Drawing.Color.White;
			this.lbVersionCode.Font = new System.Drawing.Font("Calibri Light", 27F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lbVersionCode.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(8)))), ((int)(((byte)(27)))));
			this.lbVersionCode.Location = new System.Drawing.Point(161, 38);
			this.lbVersionCode.Name = "lbVersionCode";
			this.lbVersionCode.Size = new System.Drawing.Size(103, 54);
			this.lbVersionCode.TabIndex = 2;
			this.lbVersionCode.Text = "xx.xx";
			// 
			// lbCopyright
			// 
			this.lbCopyright.BackColor = System.Drawing.Color.White;
			this.lbCopyright.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lbCopyright.ForeColor = System.Drawing.Color.Black;
			this.lbCopyright.Location = new System.Drawing.Point(13, 84);
			this.lbCopyright.Name = "lbCopyright";
			this.lbCopyright.Size = new System.Drawing.Size(671, 23);
			this.lbCopyright.TabIndex = 3;
			this.lbCopyright.Text = "Copyright";
			// 
			// lbProductName
			// 
			this.lbProductName.BackColor = System.Drawing.Color.White;
			this.lbProductName.Font = new System.Drawing.Font("Calibri", 27F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lbProductName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(174)))), ((int)(((byte)(141)))), ((int)(((byte)(128)))));
			this.lbProductName.Location = new System.Drawing.Point(6, 38);
			this.lbProductName.Name = "lbProductName";
			this.lbProductName.Size = new System.Drawing.Size(159, 54);
			this.lbProductName.TabIndex = 5;
			this.lbProductName.Text = "NfcBeam";
			// 
			// pictureBox1
			// 
			this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox1.Location = new System.Drawing.Point(0, 0);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(710, 400);
			this.pictureBox1.TabIndex = 7;
			this.pictureBox1.TabStop = false;
			// 
			// SplashForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(710, 400);
			this.Controls.Add(this.lbCopyright);
			this.Controls.Add(this.lbVersionCode);
			this.Controls.Add(this.lbVersion);
			this.Controls.Add(this.lbProductName);
			this.Controls.Add(this.pictureBox1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SplashForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "SplashForm";
			this.Click += new System.EventHandler(this.SplashFormClose);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);

		}
		private System.Windows.Forms.Label lbCopyright;
		private System.Windows.Forms.Label lbVersionCode;
		private System.Windows.Forms.Label lbVersion;
		private System.Windows.Forms.Timer AutoCloseTimer;
		private System.Windows.Forms.Label lbProductName;
		private System.Windows.Forms.PictureBox pictureBox1;
	}
}
