/*
 * Crée par SharpDevelop.
 * Utilisateur: johann
 * Date: 15/03/2012
 * Heure: 12:00
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
namespace SpringCardApplication
{
  partial class TypeSelectButton
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
      this.lbText = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // lbText
      // 
      this.lbText.Dock = System.Windows.Forms.DockStyle.Fill;
      this.lbText.ForeColor = System.Drawing.Color.White;
      this.lbText.Location = new System.Drawing.Point(0, 0);
      this.lbText.Name = "lbText";
      this.lbText.Padding = new System.Windows.Forms.Padding(9);
      this.lbText.Size = new System.Drawing.Size(159, 48);
      this.lbText.TabIndex = 0;
      this.lbText.Text = "Text";
      this.lbText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.lbText.Click += new System.EventHandler(this.LbTextClick);
      this.lbText.MouseEnter += new System.EventHandler(this.LbTextMouseEnter);
      this.lbText.MouseLeave += new System.EventHandler(this.LbTextMouseLeave);
      // 
      // NfcTagTypeButton
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.lbText);
      this.Name = "NfcTagTypeButton";
      this.Size = new System.Drawing.Size(159, 48);
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.Label lbText;
    
  }
}
