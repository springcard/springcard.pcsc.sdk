/*
 * Crée par SharpDevelop.
 * Utilisateur: johann
 * Date: 20/03/2012
 * Heure: 12:15
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
namespace PcscDiag2
{
  partial class CardInfoForm
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
    	this.panel1 = new System.Windows.Forms.Panel();
    	this.btnClose = new System.Windows.Forms.Button();
    	this.label1 = new System.Windows.Forms.Label();
    	this.eAtr = new System.Windows.Forms.TextBox();
    	this.label2 = new System.Windows.Forms.Label();
    	this.eDescription = new System.Windows.Forms.TextBox();
    	this.eAnalysis = new System.Windows.Forms.TextBox();
    	this.panel1.SuspendLayout();
    	this.SuspendLayout();
    	// 
    	// panel1
    	// 
    	this.panel1.Controls.Add(this.btnClose);
    	this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
    	this.panel1.Location = new System.Drawing.Point(0, 377);
    	this.panel1.Name = "panel1";
    	this.panel1.Size = new System.Drawing.Size(550, 38);
    	this.panel1.TabIndex = 14;
    	// 
    	// btnClose
    	// 
    	this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
    	this.btnClose.Location = new System.Drawing.Point(335, 6);
    	this.btnClose.Name = "btnClose";
    	this.btnClose.Size = new System.Drawing.Size(85, 23);
    	this.btnClose.TabIndex = 10;
    	this.btnClose.Text = "Close";
    	this.btnClose.UseVisualStyleBackColor = true;
    	this.btnClose.Click += new System.EventHandler(this.BtnCloseClick);
    	// 
    	// label1
    	// 
    	this.label1.Location = new System.Drawing.Point(12, 9);
    	this.label1.Name = "label1";
    	this.label1.Size = new System.Drawing.Size(100, 23);
    	this.label1.TabIndex = 15;
    	this.label1.Text = "ATR:";
    	// 
    	// eAtr
    	// 
    	this.eAtr.Font = new System.Drawing.Font("Lucida Console", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    	this.eAtr.Location = new System.Drawing.Point(12, 35);
    	this.eAtr.Name = "eAtr";
    	this.eAtr.Size = new System.Drawing.Size(500, 19);
    	this.eAtr.TabIndex = 16;
    	// 
    	// label2
    	// 
    	this.label2.Location = new System.Drawing.Point(12, 88);
    	this.label2.Name = "label2";
    	this.label2.Size = new System.Drawing.Size(500, 23);
    	this.label2.TabIndex = 17;
    	this.label2.Text = "Recognized card(s):";
    	// 
    	// eDescription
    	// 
    	this.eDescription.Font = new System.Drawing.Font("Lucida Console", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    	this.eDescription.Location = new System.Drawing.Point(12, 104);
    	this.eDescription.Multiline = true;
    	this.eDescription.Name = "eDescription";
    	this.eDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
    	this.eDescription.Size = new System.Drawing.Size(500, 103);
    	this.eDescription.TabIndex = 18;
    	this.eDescription.WordWrap = false;
    	// 
    	// eAnalysis
    	// 
    	this.eAnalysis.Font = new System.Drawing.Font("Lucida Console", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    	this.eAnalysis.Location = new System.Drawing.Point(12, 234);
    	this.eAnalysis.Multiline = true;
    	this.eAnalysis.Name = "eAnalysis";
    	this.eAnalysis.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
    	this.eAnalysis.Size = new System.Drawing.Size(500, 103);
    	this.eAnalysis.TabIndex = 19;
    	this.eAnalysis.WordWrap = false;
    	// 
    	// CardInfoForm
    	// 
    	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
    	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
    	this.ClientSize = new System.Drawing.Size(550, 415);
    	this.Controls.Add(this.eAnalysis);
    	this.Controls.Add(this.eDescription);
    	this.Controls.Add(this.label2);
    	this.Controls.Add(this.eAtr);
    	this.Controls.Add(this.label1);
    	this.Controls.Add(this.panel1);
    	this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
    	this.MaximizeBox = false;
    	this.MinimizeBox = false;
    	this.Name = "CardInfoForm";
    	this.ShowIcon = false;
    	this.ShowInTaskbar = false;
    	this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
    	this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
    	this.Text = "CardInfoForm";
    	this.panel1.ResumeLayout(false);
    	this.ResumeLayout(false);
    	this.PerformLayout();

    }
    private System.Windows.Forms.Button btnClose;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox eAtr;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox eDescription;
    private System.Windows.Forms.TextBox eAnalysis;
  }
}
