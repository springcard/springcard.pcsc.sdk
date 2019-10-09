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
  partial class RtdTextControl
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
      this.eValue = new System.Windows.Forms.TextBox();
      this.lb = new System.Windows.Forms.Label();
      this.lbLang = new System.Windows.Forms.Label();
      this.eLang = new System.Windows.Forms.TextBox();
      this.SuspendLayout();
      // 
      // eValue
      // 
      this.eValue.Location = new System.Drawing.Point(6, 26);
      this.eValue.MaxLength = 240;
      this.eValue.Name = "eValue";
      this.eValue.Size = new System.Drawing.Size(443, 20);
      this.eValue.TabIndex = 34;
      // 
      // lb
      // 
      this.lb.Location = new System.Drawing.Point(3, 3);
      this.lb.Name = "lb";
      this.lb.Size = new System.Drawing.Size(132, 23);
      this.lb.TabIndex = 35;
      this.lb.Text = "Text:";
      this.lb.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lbLang
      // 
      this.lbLang.Location = new System.Drawing.Point(455, 3);
      this.lbLang.Name = "lbLang";
      this.lbLang.Size = new System.Drawing.Size(47, 23);
      this.lbLang.TabIndex = 36;
      this.lbLang.Text = "Lang:";
      this.lbLang.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // eLang
      // 
      this.eLang.Location = new System.Drawing.Point(455, 26);
      this.eLang.MaxLength = 8;
      this.eLang.Name = "eLang";
      this.eLang.Size = new System.Drawing.Size(47, 20);
      this.eLang.TabIndex = 37;
      // 
      // RtdTextControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.Controls.Add(this.eLang);
      this.Controls.Add(this.lbLang);
      this.Controls.Add(this.eValue);
      this.Controls.Add(this.lb);
      this.Name = "RtdTextControl";
      this.Size = new System.Drawing.Size(506, 52);
      this.ResumeLayout(false);
      this.PerformLayout();
    }
    private System.Windows.Forms.TextBox eLang;
    private System.Windows.Forms.Label lbLang;
    private System.Windows.Forms.Label lb;
    private System.Windows.Forms.TextBox eValue;
  }
}
