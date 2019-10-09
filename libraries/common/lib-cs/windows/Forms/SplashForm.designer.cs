/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 02/03/2012
 * Time: 17:56
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace SpringCard.LibCs.Windows.Forms
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
            this.imgSplashRed = new System.Windows.Forms.PictureBox();
            this.AutoCloseTimer = new System.Windows.Forms.Timer(this.components);
            this.lbVersion = new System.Windows.Forms.Label();
            this.lbDisclaimer1 = new System.Windows.Forms.Label();
            this.lbDisclaimer3 = new System.Windows.Forms.Label();
            this.lbDisclaimer2 = new System.Windows.Forms.Label();
            this.lbProduct = new System.Windows.Forms.Label();
            this.lbCopyright = new System.Windows.Forms.Label();
            this.imgSplashMarroon = new System.Windows.Forms.PictureBox();
            this.imgSplashLight = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.imgSplashRed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgSplashMarroon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgSplashLight)).BeginInit();
            this.SuspendLayout();
            // 
            // imgSplashRed
            // 
            this.imgSplashRed.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imgSplashRed.Image = ((System.Drawing.Image)(resources.GetObject("imgSplashRed.Image")));
            this.imgSplashRed.Location = new System.Drawing.Point(0, 0);
            this.imgSplashRed.Name = "imgSplashRed";
            this.imgSplashRed.Size = new System.Drawing.Size(600, 400);
            this.imgSplashRed.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.imgSplashRed.TabIndex = 0;
            this.imgSplashRed.TabStop = false;
            this.imgSplashRed.Click += new System.EventHandler(this.SplashFormClose);
            // 
            // AutoCloseTimer
            // 
            this.AutoCloseTimer.Enabled = true;
            this.AutoCloseTimer.Interval = 3500;
            this.AutoCloseTimer.Tick += new System.EventHandler(this.SplashFormClose);
            // 
            // lbVersion
            // 
            this.lbVersion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(7)))), ((int)(((byte)(20)))));
            this.lbVersion.ForeColor = System.Drawing.Color.Black;
            this.lbVersion.Location = new System.Drawing.Point(444, 376);
            this.lbVersion.Name = "lbVersion";
            this.lbVersion.Size = new System.Drawing.Size(144, 22);
            this.lbVersion.TabIndex = 1;
            this.lbVersion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lbDisclaimer1
            // 
            this.lbDisclaimer1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
            this.lbDisclaimer1.Font = new System.Drawing.Font("Calibri Light", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbDisclaimer1.ForeColor = System.Drawing.Color.Black;
            this.lbDisclaimer1.Location = new System.Drawing.Point(0, 294);
            this.lbDisclaimer1.Name = "lbDisclaimer1";
            this.lbDisclaimer1.Size = new System.Drawing.Size(600, 23);
            this.lbDisclaimer1.TabIndex = 2;
            this.lbDisclaimer1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbDisclaimer3
            // 
            this.lbDisclaimer3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
            this.lbDisclaimer3.Font = new System.Drawing.Font("Calibri Light", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbDisclaimer3.ForeColor = System.Drawing.Color.Black;
            this.lbDisclaimer3.Location = new System.Drawing.Point(0, 346);
            this.lbDisclaimer3.Name = "lbDisclaimer3";
            this.lbDisclaimer3.Size = new System.Drawing.Size(600, 23);
            this.lbDisclaimer3.TabIndex = 3;
            this.lbDisclaimer3.Text = "See LICENSE.txt or the \"About\" box for details.";
            this.lbDisclaimer3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbDisclaimer2
            // 
            this.lbDisclaimer2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
            this.lbDisclaimer2.Font = new System.Drawing.Font("Calibri Light", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbDisclaimer2.ForeColor = System.Drawing.Color.Black;
            this.lbDisclaimer2.Location = new System.Drawing.Point(0, 320);
            this.lbDisclaimer2.Name = "lbDisclaimer2";
            this.lbDisclaimer2.Size = new System.Drawing.Size(600, 23);
            this.lbDisclaimer2.TabIndex = 4;
            this.lbDisclaimer2.Text = "This tool is an unsupported software.";
            this.lbDisclaimer2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbProduct
            // 
            this.lbProduct.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
            this.lbProduct.Font = new System.Drawing.Font("Calibri Light", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbProduct.ForeColor = System.Drawing.Color.Black;
            this.lbProduct.Location = new System.Drawing.Point(0, 130);
            this.lbProduct.Name = "lbProduct";
            this.lbProduct.Size = new System.Drawing.Size(600, 63);
            this.lbProduct.TabIndex = 5;
            this.lbProduct.Text = "Name of software";
            this.lbProduct.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbCopyright
            // 
            this.lbCopyright.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(7)))), ((int)(((byte)(20)))));
            this.lbCopyright.ForeColor = System.Drawing.Color.Black;
            this.lbCopyright.Location = new System.Drawing.Point(12, 376);
            this.lbCopyright.Name = "lbCopyright";
            this.lbCopyright.Size = new System.Drawing.Size(414, 22);
            this.lbCopyright.TabIndex = 6;
            this.lbCopyright.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // imgSplashMarroon
            // 
            this.imgSplashMarroon.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imgSplashMarroon.Image = ((System.Drawing.Image)(resources.GetObject("imgSplashMarroon.Image")));
            this.imgSplashMarroon.Location = new System.Drawing.Point(0, 0);
            this.imgSplashMarroon.Name = "imgSplashMarroon";
            this.imgSplashMarroon.Size = new System.Drawing.Size(600, 400);
            this.imgSplashMarroon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.imgSplashMarroon.TabIndex = 7;
            this.imgSplashMarroon.TabStop = false;
            // 
            // imgSplashLight
            // 
            this.imgSplashLight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imgSplashLight.Image = ((System.Drawing.Image)(resources.GetObject("imgSplashLight.Image")));
            this.imgSplashLight.Location = new System.Drawing.Point(0, 0);
            this.imgSplashLight.Name = "imgSplashLight";
            this.imgSplashLight.Size = new System.Drawing.Size(600, 400);
            this.imgSplashLight.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.imgSplashLight.TabIndex = 8;
            this.imgSplashLight.TabStop = false;
            // 
            // SplashForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 400);
            this.Controls.Add(this.lbCopyright);
            this.Controls.Add(this.lbProduct);
            this.Controls.Add(this.lbDisclaimer2);
            this.Controls.Add(this.lbDisclaimer3);
            this.Controls.Add(this.lbDisclaimer1);
            this.Controls.Add(this.lbVersion);
            this.Controls.Add(this.imgSplashLight);
            this.Controls.Add(this.imgSplashRed);
            this.Controls.Add(this.imgSplashMarroon);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SplashForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SplashForm";
            this.TopMost = true;
            this.Click += new System.EventHandler(this.SplashFormClose);
            ((System.ComponentModel.ISupportInitialize)(this.imgSplashRed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgSplashMarroon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgSplashLight)).EndInit();
            this.ResumeLayout(false);

		}
		private System.Windows.Forms.Label lbVersion;
		private System.Windows.Forms.Timer AutoCloseTimer;
		private System.Windows.Forms.PictureBox imgSplashRed;
        private System.Windows.Forms.Label lbDisclaimer1;
        private System.Windows.Forms.Label lbDisclaimer3;
        private System.Windows.Forms.Label lbDisclaimer2;
        private System.Windows.Forms.Label lbProduct;
        private System.Windows.Forms.Label lbCopyright;
        private System.Windows.Forms.PictureBox imgSplashMarroon;
        private System.Windows.Forms.PictureBox imgSplashLight;
    }
}
