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
    	this.label3 = new System.Windows.Forms.Label();
    	this.label4 = new System.Windows.Forms.Label();
    	this.eTechnical = new System.Windows.Forms.TextBox();
    	this.label5 = new System.Windows.Forms.Label();
    	this.panel1.SuspendLayout();
    	this.SuspendLayout();
    	// 
    	// panel1
    	// 
    	this.panel1.Controls.Add(this.btnClose);
    	this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
    	this.panel1.Location = new System.Drawing.Point(0, 427);
    	this.panel1.Name = "panel1";
    	this.panel1.Size = new System.Drawing.Size(533, 49);
    	this.panel1.TabIndex = 14;
    	// 
    	// btnClose
    	// 
    	this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
    	this.btnClose.Location = new System.Drawing.Point(221, 7);
    	this.btnClose.Name = "btnClose";
    	this.btnClose.Size = new System.Drawing.Size(85, 34);
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
    	this.eAtr.Location = new System.Drawing.Point(12, 26);
    	this.eAtr.Name = "eAtr";
    	this.eAtr.Size = new System.Drawing.Size(500, 19);
    	this.eAtr.TabIndex = 0;
    	// 
    	// label2
    	// 
    	this.label2.Location = new System.Drawing.Point(12, 185);
    	this.label2.Name = "label2";
    	this.label2.Size = new System.Drawing.Size(500, 23);
    	this.label2.TabIndex = 4;
    	this.label2.Text = "Possibly recognized card(s):";
    	// 
    	// eDescription
    	// 
    	this.eDescription.Font = new System.Drawing.Font("Lucida Console", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    	this.eDescription.Location = new System.Drawing.Point(12, 201);
    	this.eDescription.Multiline = true;
    	this.eDescription.Name = "eDescription";
    	this.eDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
    	this.eDescription.Size = new System.Drawing.Size(500, 103);
    	this.eDescription.TabIndex = 3;
    	this.eDescription.WordWrap = false;
    	// 
    	// eAnalysis
    	// 
    	this.eAnalysis.Font = new System.Drawing.Font("Lucida Console", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    	this.eAnalysis.Location = new System.Drawing.Point(12, 71);
    	this.eAnalysis.Multiline = true;
    	this.eAnalysis.Name = "eAnalysis";
    	this.eAnalysis.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
    	this.eAnalysis.Size = new System.Drawing.Size(500, 103);
    	this.eAnalysis.TabIndex = 1;
    	this.eAnalysis.WordWrap = false;
    	// 
    	// label3
    	// 
    	this.label3.Location = new System.Drawing.Point(12, 55);
    	this.label3.Name = "label3";
    	this.label3.Size = new System.Drawing.Size(100, 23);
    	this.label3.TabIndex = 16;
    	this.label3.Text = "Technical analysis:";
    	// 
    	// label4
    	// 
    	this.label4.Location = new System.Drawing.Point(12, 307);
    	this.label4.Name = "label4";
    	this.label4.Size = new System.Drawing.Size(500, 32);
    	this.label4.TabIndex = 17;
    	this.label4.Text = "Card recognition is based on based on smartcard_list.txt, a file publied by Ludov" +
	"ic Rousseau under the GPL license.";
    	// 
    	// eTechnical
    	// 
    	this.eTechnical.Font = new System.Drawing.Font("Lucida Console", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    	this.eTechnical.Location = new System.Drawing.Point(12, 362);
    	this.eTechnical.Multiline = true;
    	this.eTechnical.Name = "eTechnical";
    	this.eTechnical.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
    	this.eTechnical.Size = new System.Drawing.Size(500, 59);
    	this.eTechnical.TabIndex = 18;
    	this.eTechnical.WordWrap = false;
    	// 
    	// label5
    	// 
    	this.label5.Location = new System.Drawing.Point(12, 346);
    	this.label5.Name = "label5";
    	this.label5.Size = new System.Drawing.Size(500, 23);
    	this.label5.TabIndex = 19;
    	this.label5.Text = "Technical information from PC/SC v2:";
    	// 
    	// CardInfoForm
    	// 
    	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
    	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
    	this.ClientSize = new System.Drawing.Size(533, 476);
    	this.Controls.Add(this.eTechnical);
    	this.Controls.Add(this.label5);
    	this.Controls.Add(this.label4);
    	this.Controls.Add(this.eAnalysis);
    	this.Controls.Add(this.eDescription);
    	this.Controls.Add(this.label2);
    	this.Controls.Add(this.eAtr);
    	this.Controls.Add(this.label1);
    	this.Controls.Add(this.panel1);
    	this.Controls.Add(this.label3);
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
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.TextBox eTechnical;
    private System.Windows.Forms.Label label5;
  }
}
