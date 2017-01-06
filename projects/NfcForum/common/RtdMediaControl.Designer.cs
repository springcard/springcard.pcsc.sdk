/*
 * Crée par SharpDevelop.
 * Utilisateur: johann
 * Date: 05/10/2012
 * Heure: 12:12
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
namespace SpringCardApplication
{
  partial class RtdMediaControl
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
      this.lb = new System.Windows.Forms.Label();
      this.cbType = new System.Windows.Forms.ComboBox();
      this.eContent = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // lb
      // 
      this.lb.Location = new System.Drawing.Point(3, 3);
      this.lb.Name = "lb";
      this.lb.Size = new System.Drawing.Size(295, 23);
      this.lb.TabIndex = 36;
      this.lb.Text = "MIME type (only text types are supported):";
      this.lb.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // cbType
      // 
      this.cbType.FormattingEnabled = true;
      this.cbType.Location = new System.Drawing.Point(6, 26);
      this.cbType.Name = "cbType";
      this.cbType.Size = new System.Drawing.Size(443, 21);
      this.cbType.TabIndex = 40;
      // 
      // eContent
      // 
      this.eContent.Location = new System.Drawing.Point(6, 79);
      this.eContent.MaxLength = 240;
      this.eContent.Multiline = true;
      this.eContent.Name = "eContent";
      this.eContent.Size = new System.Drawing.Size(443, 95);
      this.eContent.TabIndex = 41;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(3, 56);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(132, 23);
      this.label1.TabIndex = 42;
      this.label1.Text = "Content:";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // RtdMediaControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.eContent);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.cbType);
      this.Controls.Add(this.lb);
      this.Name = "RtdMediaControl";
      this.Size = new System.Drawing.Size(458, 189);
      this.ResumeLayout(false);
      this.PerformLayout();
    }
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox eContent;
    private System.Windows.Forms.ComboBox cbType;
    private System.Windows.Forms.Label lb;
  }
}
