/*
 * Crée par SharpDevelop.
 * Utilisateur: johann
 * Date: 26/03/2012
 * Heure: 09:21
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
namespace PcscDiag2
{
  partial class DisconnectForm
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
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnOK = new System.Windows.Forms.Button();
      this.panel3 = new System.Windows.Forms.Panel();
      this.label1 = new System.Windows.Forms.Label();
      this.rbEject = new System.Windows.Forms.RadioButton();
      this.label8 = new System.Windows.Forms.Label();
      this.label9 = new System.Windows.Forms.Label();
      this.rbUnpower = new System.Windows.Forms.RadioButton();
      this.label10 = new System.Windows.Forms.Label();
      this.rbReset = new System.Windows.Forms.RadioButton();
      this.label11 = new System.Windows.Forms.Label();
      this.rbLeave = new System.Windows.Forms.RadioButton();
      this.panel1.SuspendLayout();
      this.panel3.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnCancel);
      this.panel1.Controls.Add(this.btnOK);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
      this.panel1.Location = new System.Drawing.Point(416, 0);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(98, 125);
      this.panel1.TabIndex = 10;
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(3, 43);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(85, 23);
      this.btnCancel.TabIndex = 11;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      this.btnCancel.Click += new System.EventHandler(this.BtnCancelClick);
      // 
      // btnOK
      // 
      this.btnOK.Location = new System.Drawing.Point(3, 14);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(85, 23);
      this.btnOK.TabIndex = 10;
      this.btnOK.Text = "OK";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new System.EventHandler(this.BtnOKClick);
      // 
      // panel3
      // 
      this.panel3.Controls.Add(this.label1);
      this.panel3.Controls.Add(this.rbEject);
      this.panel3.Controls.Add(this.label8);
      this.panel3.Controls.Add(this.label9);
      this.panel3.Controls.Add(this.rbUnpower);
      this.panel3.Controls.Add(this.label10);
      this.panel3.Controls.Add(this.rbReset);
      this.panel3.Controls.Add(this.label11);
      this.panel3.Controls.Add(this.rbLeave);
      this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
      this.panel3.Location = new System.Drawing.Point(0, 0);
      this.panel3.Name = "panel3";
      this.panel3.Size = new System.Drawing.Size(416, 131);
      this.panel3.TabIndex = 19;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(103, 99);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(215, 13);
      this.label1.TabIndex = 16;
      this.label1.Text = "Eject the card (if the reader is able to do so!)";
      // 
      // rbEject
      // 
      this.rbEject.AutoSize = true;
      this.rbEject.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbEject.Location = new System.Drawing.Point(10, 97);
      this.rbEject.Name = "rbEject";
      this.rbEject.Size = new System.Drawing.Size(54, 17);
      this.rbEject.TabIndex = 15;
      this.rbEject.TabStop = true;
      this.rbEject.Text = "Eject";
      this.rbEject.UseVisualStyleBackColor = true;
      // 
      // label8
      // 
      this.label8.AutoSize = true;
      this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label8.Location = new System.Drawing.Point(8, 9);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(97, 13);
      this.label8.TabIndex = 14;
      this.label8.Text = "Card disposition";
      // 
      // label9
      // 
      this.label9.AutoSize = true;
      this.label9.Location = new System.Drawing.Point(103, 76);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(219, 13);
      this.label9.TabIndex = 13;
      this.label9.Text = "Power down the card and reset it (Cold reset)";
      // 
      // rbUnpower
      // 
      this.rbUnpower.AutoSize = true;
      this.rbUnpower.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbUnpower.Location = new System.Drawing.Point(10, 74);
      this.rbUnpower.Name = "rbUnpower";
      this.rbUnpower.Size = new System.Drawing.Size(75, 17);
      this.rbUnpower.TabIndex = 12;
      this.rbUnpower.TabStop = true;
      this.rbUnpower.Text = "Unpower";
      this.rbUnpower.UseVisualStyleBackColor = true;
      // 
      // label10
      // 
      this.label10.AutoSize = true;
      this.label10.Location = new System.Drawing.Point(103, 53);
      this.label10.Name = "label10";
      this.label10.Size = new System.Drawing.Size(140, 13);
      this.label10.TabIndex = 11;
      this.label10.Text = "Reset the card (Warm reset)";
      // 
      // rbReset
      // 
      this.rbReset.AutoSize = true;
      this.rbReset.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbReset.Location = new System.Drawing.Point(10, 51);
      this.rbReset.Name = "rbReset";
      this.rbReset.Size = new System.Drawing.Size(58, 17);
      this.rbReset.TabIndex = 10;
      this.rbReset.TabStop = true;
      this.rbReset.Text = "Reset";
      this.rbReset.UseVisualStyleBackColor = true;
      // 
      // label11
      // 
      this.label11.AutoSize = true;
      this.label11.Location = new System.Drawing.Point(103, 30);
      this.label11.Name = "label11";
      this.label11.Size = new System.Drawing.Size(199, 13);
      this.label11.TabIndex = 9;
      this.label11.Text = "Do not do anything special on reconnect";
      // 
      // rbLeave
      // 
      this.rbLeave.AutoSize = true;
      this.rbLeave.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbLeave.Location = new System.Drawing.Point(10, 28);
      this.rbLeave.Name = "rbLeave";
      this.rbLeave.Size = new System.Drawing.Size(60, 17);
      this.rbLeave.TabIndex = 8;
      this.rbLeave.TabStop = true;
      this.rbLeave.Text = "Leave";
      this.rbLeave.UseVisualStyleBackColor = true;
      // 
      // DisconnectForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(514, 125);
      this.ControlBox = false;
      this.Controls.Add(this.panel3);
      this.Controls.Add(this.panel1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "DisconnectForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Option for Disconnect";
      this.Load += new System.EventHandler(this.DisconnectFormLoad);
      this.panel1.ResumeLayout(false);
      this.panel3.ResumeLayout(false);
      this.panel3.PerformLayout();
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.RadioButton rbEject;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label11;
    private System.Windows.Forms.Label label10;
    private System.Windows.Forms.Label label9;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.Panel panel3;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.RadioButton rbUnpower;
    private System.Windows.Forms.RadioButton rbReset;
    private System.Windows.Forms.RadioButton rbLeave;
  }
}
