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
			this.imgSplash = new System.Windows.Forms.PictureBox();
			this.AutoCloseTimer = new System.Windows.Forms.Timer(this.components);
			this.lbVersion = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.imgSplash)).BeginInit();
			this.SuspendLayout();
			// 
			// imgSplash
			// 
			this.imgSplash.Dock = System.Windows.Forms.DockStyle.Fill;
			this.imgSplash.Image = ((System.Drawing.Image)(resources.GetObject("imgSplash.Image")));
			this.imgSplash.Location = new System.Drawing.Point(0, 0);
			this.imgSplash.Name = "imgSplash";
			this.imgSplash.Size = new System.Drawing.Size(600, 400);
			this.imgSplash.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.imgSplash.TabIndex = 0;
			this.imgSplash.TabStop = false;
			this.imgSplash.Click += new System.EventHandler(this.SplashFormClose);
			// 
			// AutoCloseTimer
			// 
			this.AutoCloseTimer.Enabled = true;
			this.AutoCloseTimer.Interval = 3000;
			this.AutoCloseTimer.Tick += new System.EventHandler(this.SplashFormClose);
			// 
			// lbVersion
			// 
			this.lbVersion.BackColor = System.Drawing.Color.LightGray;
			this.lbVersion.ForeColor = System.Drawing.Color.DarkGray;
			this.lbVersion.Location = new System.Drawing.Point(12, 380);
			this.lbVersion.Name = "lbVersion";
			this.lbVersion.Size = new System.Drawing.Size(576, 16);
			this.lbVersion.TabIndex = 1;
			this.lbVersion.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// SplashForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(600, 400);
			this.Controls.Add(this.lbVersion);
			this.Controls.Add(this.imgSplash);
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
			((System.ComponentModel.ISupportInitialize)(this.imgSplash)).EndInit();
			this.ResumeLayout(false);

		}
		private System.Windows.Forms.Label lbVersion;
		private System.Windows.Forms.Timer AutoCloseTimer;
		private System.Windows.Forms.PictureBox imgSplash;
	}
}
