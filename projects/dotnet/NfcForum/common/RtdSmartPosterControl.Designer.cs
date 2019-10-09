/*
 * Crée par SharpDevelop.
 * Utilisateur: johann
 * Date: 15/03/2012
 * Heure: 09:35
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
namespace SpringCardApplication.Controls
{
  partial class RtdSmartPosterControl
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
      this.nfcRtdUriControl1 = new RtdUriControl();
      this.panel1 = new System.Windows.Forms.Panel();
      this.cbMime = new System.Windows.Forms.ComboBox();
      this.label3 = new System.Windows.Forms.Label();
      this.SIZE_txt = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.ACT_Combo = new System.Windows.Forms.ComboBox();
      this.nfcRtdTextControl1 = new RtdTextControl();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // nfcRtdUriControl1
      // 
      this.nfcRtdUriControl1.AutoScroll = true;
      this.nfcRtdUriControl1.BackColor = System.Drawing.SystemColors.Window;
      this.nfcRtdUriControl1.Dock = System.Windows.Forms.DockStyle.Top;
      this.nfcRtdUriControl1.ForeColor = System.Drawing.Color.Black;
      this.nfcRtdUriControl1.Location = new System.Drawing.Point(0, 0);
      this.nfcRtdUriControl1.Name = "nfcRtdUriControl1";
      this.nfcRtdUriControl1.Padding = new System.Windows.Forms.Padding(3);
      this.nfcRtdUriControl1.Size = new System.Drawing.Size(513, 52);
      this.nfcRtdUriControl1.TabIndex = 36;
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.cbMime);
      this.panel1.Controls.Add(this.label3);
      this.panel1.Controls.Add(this.SIZE_txt);
      this.panel1.Controls.Add(this.label2);
      this.panel1.Controls.Add(this.label1);
      this.panel1.Controls.Add(this.ACT_Combo);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
      this.panel1.Location = new System.Drawing.Point(0, 104);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(513, 103);
      this.panel1.TabIndex = 38;
      // 
      // cbMime
      // 
      this.cbMime.FormattingEnabled = true;
      this.cbMime.Location = new System.Drawing.Point(6, 76);
      this.cbMime.Name = "cbMime";
      this.cbMime.Size = new System.Drawing.Size(443, 21);
      this.cbMime.TabIndex = 39;
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(3, 50);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(156, 23);
      this.label3.TabIndex = 38;
      this.label3.Text = "MIME type of the target:";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // SIZE_txt
      // 
      this.SIZE_txt.Location = new System.Drawing.Point(345, 26);
      this.SIZE_txt.Name = "SIZE_txt";
      this.SIZE_txt.Size = new System.Drawing.Size(104, 20);
      this.SIZE_txt.TabIndex = 37;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(219, 3);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(230, 23);
      this.label2.TabIndex = 36;
      this.label2.Text = "Estimated size of the target (in bytes):";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(3, 3);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(156, 23);
      this.label1.TabIndex = 35;
      this.label1.Text = "Action to perform:";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // ACT_Combo
      // 
      this.ACT_Combo.FormattingEnabled = true;
      this.ACT_Combo.Items.AddRange(new object[] {
                  "None",
                  "Do the action",
                  "Save for later",
                  "Open for editing"});
      this.ACT_Combo.Location = new System.Drawing.Point(6, 26);
      this.ACT_Combo.Name = "ACT_Combo";
      this.ACT_Combo.Size = new System.Drawing.Size(202, 21);
      this.ACT_Combo.TabIndex = 34;
      // 
      // nfcRtdTextControl1
      // 
      this.nfcRtdTextControl1.AutoScroll = true;
      this.nfcRtdTextControl1.BackColor = System.Drawing.SystemColors.Window;
      this.nfcRtdTextControl1.Dock = System.Windows.Forms.DockStyle.Top;
      this.nfcRtdTextControl1.ForeColor = System.Drawing.Color.Black;
      this.nfcRtdTextControl1.Location = new System.Drawing.Point(0, 52);
      this.nfcRtdTextControl1.Name = "nfcRtdTextControl1";
      this.nfcRtdTextControl1.Padding = new System.Windows.Forms.Padding(3);
      this.nfcRtdTextControl1.Size = new System.Drawing.Size(513, 52);
      this.nfcRtdTextControl1.TabIndex = 39;
      // 
      // RtdSmartPosterControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.Controls.Add(this.panel1);
      this.Controls.Add(this.nfcRtdTextControl1);
      this.Controls.Add(this.nfcRtdUriControl1);
      this.Name = "RtdSmartPosterControl";
      this.Padding = new System.Windows.Forms.Padding(0);
      this.Size = new System.Drawing.Size(513, 207);
      this.panel1.ResumeLayout(false);
      this.panel1.PerformLayout();
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Panel panel1;
    private RtdTextControl nfcRtdTextControl1;
    private RtdUriControl nfcRtdUriControl1;
    private System.Windows.Forms.ComboBox ACT_Combo;
    private System.Windows.Forms.ComboBox cbMime;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox SIZE_txt;
  }
}
