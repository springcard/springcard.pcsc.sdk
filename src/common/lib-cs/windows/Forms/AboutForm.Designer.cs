/*
 * Created by SharpDevelop.
 * User: jerome.i
 * Date: 02/03/2012
 * Time: 10:27
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace SpringCard.LibCs
{
	partial class AboutForm
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
			if (disposing) 
			{
				if (components != null)
					components.Dispose();
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutForm));
			this.pBottom = new System.Windows.Forms.Panel();
			this.btnOKNormal = new System.Windows.Forms.Button();
			this.linkSpringCard = new System.Windows.Forms.LinkLabel();
			this.btnOKFlat = new System.Windows.Forms.Button();
			this.pMain = new System.Windows.Forms.Panel();
			this.linkLabel1 = new System.Windows.Forms.LinkLabel();
			this.lbTrademarks = new System.Windows.Forms.Label();
			this.lbCopyright = new System.Windows.Forms.Label();
			this.lbVersion = new System.Windows.Forms.Label();
			this.lbCompanyProduct = new System.Windows.Forms.Label();
			this.pTop = new System.Windows.Forms.Panel();
			this.pLogo = new System.Windows.Forms.Panel();
			this.imgTop = new System.Windows.Forms.PictureBox();
			this.pBottom.SuspendLayout();
			this.pMain.SuspendLayout();
			this.pTop.SuspendLayout();
			this.pLogo.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.imgTop)).BeginInit();
			this.SuspendLayout();
			// 
			// pBottom
			// 
			this.pBottom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.pBottom.Controls.Add(this.btnOKNormal);
			this.pBottom.Controls.Add(this.linkSpringCard);
			this.pBottom.Controls.Add(this.btnOKFlat);
			resources.ApplyResources(this.pBottom, "pBottom");
			this.pBottom.Name = "pBottom";
			// 
			// btnOKNormal
			// 
			this.btnOKNormal.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			resources.ApplyResources(this.btnOKNormal, "btnOKNormal");
			this.btnOKNormal.ForeColor = System.Drawing.Color.Black;
			this.btnOKNormal.Name = "btnOKNormal";
			this.btnOKNormal.UseVisualStyleBackColor = false;
			// 
			// btnOKNormal
			// 
			this.btnOKNormal.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.btnOKNormal.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnOKNormal.ForeColor = System.Drawing.Color.Black;
			this.btnOKNormal.Location = new System.Drawing.Point(294, 14);
			this.btnOKNormal.Name = "btnOKNormal";
			this.btnOKNormal.Size = new System.Drawing.Size(95, 35);
			this.btnOKNormal.TabIndex = 14;
			this.btnOKNormal.Text = "OK";
			this.btnOKNormal.UseVisualStyleBackColor = false;
			// 
			// linkSpringCard
			// 
			this.linkSpringCard.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(113)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
			resources.ApplyResources(this.linkSpringCard, "linkSpringCard");
			this.linkSpringCard.DisabledLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(113)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
			this.linkSpringCard.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(113)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
			this.linkSpringCard.Name = "linkSpringCard";
			this.linkSpringCard.TabStop = true;
			this.linkSpringCard.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(113)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
			this.linkSpringCard.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkSpringCardLinkClicked);
			// 
			// btnOKFlat
			// 
			this.btnOKFlat.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(179)))), ((int)(((byte)(185)))));
			resources.ApplyResources(this.btnOKFlat, "btnOKFlat");
			this.btnOKFlat.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(113)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
			this.btnOKFlat.Name = "btnOKFlat";
			this.btnOKFlat.UseVisualStyleBackColor = false;
			this.btnOKFlat.Click += new System.EventHandler(this.BtnOKClick);
			// 
			// pMain
			// 
			this.pMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
			this.pMain.Controls.Add(this.linkLabel1);
			this.pMain.Controls.Add(this.lbTrademarks);
			this.pMain.Controls.Add(this.lbCopyright);
			this.pMain.Controls.Add(this.lbVersion);
			this.pMain.Controls.Add(this.lbCompanyProduct);
			resources.ApplyResources(this.pMain, "pMain");
			this.pMain.Name = "pMain";
			// 
			// linkLabel1
			// 
			this.linkLabel1.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(113)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
			resources.ApplyResources(this.linkLabel1, "linkLabel1");
			this.linkLabel1.DisabledLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(113)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
			this.linkLabel1.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(113)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
			this.linkLabel1.Name = "linkLabel1";
			this.linkLabel1.TabStop = true;
			this.linkLabel1.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(113)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
			this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel1LinkClicked);
			// 
			// lbTrademarks
			// 
			this.lbTrademarks.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			resources.ApplyResources(this.lbTrademarks, "lbTrademarks");
			this.lbTrademarks.ForeColor = System.Drawing.Color.Black;
			this.lbTrademarks.Name = "lbTrademarks";
			// 
			// lbCopyright
			// 
			resources.ApplyResources(this.lbCopyright, "lbCopyright");
			this.lbCopyright.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.lbCopyright.ForeColor = System.Drawing.Color.Black;
			this.lbCopyright.Name = "lbCopyright";
			// 
			// lbVersion
			// 
			resources.ApplyResources(this.lbVersion, "lbVersion");
			this.lbVersion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.lbVersion.ForeColor = System.Drawing.Color.Black;
			this.lbVersion.Name = "lbVersion";
			// 
			// lbCompanyProduct
			// 
			resources.ApplyResources(this.lbCompanyProduct, "lbCompanyProduct");
			this.lbCompanyProduct.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.lbCompanyProduct.ForeColor = System.Drawing.Color.Black;
			this.lbCompanyProduct.Name = "lbCompanyProduct";
			// 
			// pTop
			// 
			this.pTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
			this.pTop.Controls.Add(this.pLogo);
			resources.ApplyResources(this.pTop, "pTop");
			this.pTop.Name = "pTop";
			// 
			// pLogo
			// 
			this.pLogo.Controls.Add(this.imgTop);
			resources.ApplyResources(this.pLogo, "pLogo");
			this.pLogo.Name = "pLogo";
			// 
			// imgTop
			// 
			resources.ApplyResources(this.imgTop, "imgTop");
			this.imgTop.Name = "imgTop";
			this.imgTop.TabStop = false;
			this.imgTop.Click += new System.EventHandler(this.ImgTopClick);
			// 
			// AboutForm
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.Controls.Add(this.pMain);
			this.Controls.Add(this.pBottom);
			this.Controls.Add(this.pTop);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AboutForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Load += new System.EventHandler(this.AboutFormLoad);
			this.pBottom.ResumeLayout(false);
			this.pBottom.PerformLayout();
			this.pMain.ResumeLayout(false);
			this.pMain.PerformLayout();
			this.pTop.ResumeLayout(false);
			this.pLogo.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.imgTop)).EndInit();
			this.ResumeLayout(false);

		}
		private System.Windows.Forms.LinkLabel linkLabel1;
		private System.Windows.Forms.LinkLabel linkSpringCard;
		private System.Windows.Forms.PictureBox imgTop;
		private System.Windows.Forms.Panel pLogo;
		private System.Windows.Forms.Panel pTop;
		private System.Windows.Forms.Label lbTrademarks;
		private System.Windows.Forms.Label lbCompanyProduct;
		private System.Windows.Forms.Label lbVersion;
		private System.Windows.Forms.Label lbCopyright;
		private System.Windows.Forms.Panel pMain;
		private System.Windows.Forms.Button btnOKFlat;
		private System.Windows.Forms.Panel pBottom;
		private System.Windows.Forms.Button btnOKNormal;
	
	}
}
