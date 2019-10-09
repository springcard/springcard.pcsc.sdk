/*
 * Crée par SharpDevelop.
 * Utilisateur: johann
 * Date: 26/03/2012
 * Heure: 09:32
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
namespace PcscDiag2
{
  partial class ReconnectForm
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
      this.panel2 = new System.Windows.Forms.Panel();
      this.label12 = new System.Windows.Forms.Label();
      this.rbRaw = new System.Windows.Forms.RadioButton();
      this.label7 = new System.Windows.Forms.Label();
      this.rbT0orT1 = new System.Windows.Forms.RadioButton();
      this.label4 = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.rbT1 = new System.Windows.Forms.RadioButton();
      this.label6 = new System.Windows.Forms.Label();
      this.rbT0 = new System.Windows.Forms.RadioButton();
      this.pShareMode = new System.Windows.Forms.Panel();
      this.label3 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.rbExclusive = new System.Windows.Forms.RadioButton();
      this.label1 = new System.Windows.Forms.Label();
      this.rbShared = new System.Windows.Forms.RadioButton();
      this.panel1 = new System.Windows.Forms.Panel();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnOK = new System.Windows.Forms.Button();
      this.panel3 = new System.Windows.Forms.Panel();
      this.label8 = new System.Windows.Forms.Label();
      this.label9 = new System.Windows.Forms.Label();
      this.rbUnpower = new System.Windows.Forms.RadioButton();
      this.label10 = new System.Windows.Forms.Label();
      this.rbReset = new System.Windows.Forms.RadioButton();
      this.label11 = new System.Windows.Forms.Label();
      this.rbLeave = new System.Windows.Forms.RadioButton();
      this.panel2.SuspendLayout();
      this.pShareMode.SuspendLayout();
      this.panel1.SuspendLayout();
      this.panel3.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel2
      // 
      this.panel2.Controls.Add(this.label12);
      this.panel2.Controls.Add(this.rbRaw);
      this.panel2.Controls.Add(this.label7);
      this.panel2.Controls.Add(this.rbT0orT1);
      this.panel2.Controls.Add(this.label4);
      this.panel2.Controls.Add(this.label5);
      this.panel2.Controls.Add(this.rbT1);
      this.panel2.Controls.Add(this.label6);
      this.panel2.Controls.Add(this.rbT0);
      this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
      this.panel2.Location = new System.Drawing.Point(0, 179);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(416, 125);
      this.panel2.TabIndex = 2;
      // 
      // label12
      // 
      this.label12.AutoSize = true;
      this.label12.Location = new System.Drawing.Point(103, 99);
      this.label12.Name = "label12";
      this.label12.Size = new System.Drawing.Size(304, 13);
      this.label12.TabIndex = 20;
      this.label12.Text = "Raw protocol (valid on some readers for contact memory cards)";
      // 
      // rbRaw
      // 
      this.rbRaw.AutoSize = true;
      this.rbRaw.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbRaw.Location = new System.Drawing.Point(10, 97);
      this.rbRaw.Name = "rbRaw";
      this.rbRaw.Size = new System.Drawing.Size(50, 17);
      this.rbRaw.TabIndex = 3;
      this.rbRaw.TabStop = true;
      this.rbRaw.Text = "Raw";
      this.rbRaw.UseVisualStyleBackColor = true;
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(103, 76);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(111, 13);
      this.label7.TabIndex = 14;
      this.label7.Text = "Use either T=0 or T=1";
      // 
      // rbT0orT1
      // 
      this.rbT0orT1.AutoSize = true;
      this.rbT0orT1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbT0orT1.Location = new System.Drawing.Point(10, 74);
      this.rbT0orT1.Name = "rbT0orT1";
      this.rbT0orT1.Size = new System.Drawing.Size(80, 17);
      this.rbT0orT1.TabIndex = 2;
      this.rbT0orT1.TabStop = true;
      this.rbT0orT1.Text = "T=0 | T=1";
      this.rbT0orT1.UseVisualStyleBackColor = true;
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label4.Location = new System.Drawing.Point(8, 9);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(54, 13);
      this.label4.TabIndex = 12;
      this.label4.Text = "Protocol";
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(103, 53);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(115, 13);
      this.label5.TabIndex = 11;
      this.label5.Text = "Force the T=1 protocol";
      // 
      // rbT1
      // 
      this.rbT1.AutoSize = true;
      this.rbT1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbT1.Location = new System.Drawing.Point(10, 51);
      this.rbT1.Name = "rbT1";
      this.rbT1.Size = new System.Drawing.Size(47, 17);
      this.rbT1.TabIndex = 1;
      this.rbT1.TabStop = true;
      this.rbT1.Text = "T=1";
      this.rbT1.UseVisualStyleBackColor = true;
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(103, 30);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(115, 13);
      this.label6.TabIndex = 9;
      this.label6.Text = "Force the T=0 protocol";
      // 
      // rbT0
      // 
      this.rbT0.AutoSize = true;
      this.rbT0.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbT0.Location = new System.Drawing.Point(10, 28);
      this.rbT0.Name = "rbT0";
      this.rbT0.Size = new System.Drawing.Size(47, 17);
      this.rbT0.TabIndex = 0;
      this.rbT0.TabStop = true;
      this.rbT0.Text = "T=0";
      this.rbT0.UseVisualStyleBackColor = true;
      // 
      // pShareMode
      // 
      this.pShareMode.Controls.Add(this.label3);
      this.pShareMode.Controls.Add(this.label2);
      this.pShareMode.Controls.Add(this.rbExclusive);
      this.pShareMode.Controls.Add(this.label1);
      this.pShareMode.Controls.Add(this.rbShared);
      this.pShareMode.Dock = System.Windows.Forms.DockStyle.Top;
      this.pShareMode.Location = new System.Drawing.Point(0, 0);
      this.pShareMode.Name = "pShareMode";
      this.pShareMode.Size = new System.Drawing.Size(416, 75);
      this.pShareMode.TabIndex = 0;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label3.Location = new System.Drawing.Point(8, 9);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(82, 13);
      this.label3.TabIndex = 12;
      this.label3.Text = "Access mode";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(103, 53);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(296, 13);
      this.label2.TabIndex = 11;
      this.label2.Text = "This application will not share this card with other applications";
      // 
      // rbExclusive
      // 
      this.rbExclusive.AutoSize = true;
      this.rbExclusive.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbExclusive.Location = new System.Drawing.Point(10, 51);
      this.rbExclusive.Name = "rbExclusive";
      this.rbExclusive.Size = new System.Drawing.Size(79, 17);
      this.rbExclusive.TabIndex = 1;
      this.rbExclusive.TabStop = true;
      this.rbExclusive.Text = "Exclusive";
      this.rbExclusive.UseVisualStyleBackColor = true;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(103, 30);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(278, 13);
      this.label1.TabIndex = 9;
      this.label1.Text = "This application will share this card with other applications";
      // 
      // rbShared
      // 
      this.rbShared.AutoSize = true;
      this.rbShared.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbShared.Location = new System.Drawing.Point(10, 28);
      this.rbShared.Name = "rbShared";
      this.rbShared.Size = new System.Drawing.Size(65, 17);
      this.rbShared.TabIndex = 0;
      this.rbShared.TabStop = true;
      this.rbShared.Text = "Shared";
      this.rbShared.UseVisualStyleBackColor = true;
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnCancel);
      this.panel1.Controls.Add(this.btnOK);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
      this.panel1.Location = new System.Drawing.Point(416, 0);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(98, 304);
      this.panel1.TabIndex = 3;
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(3, 43);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(85, 23);
      this.btnCancel.TabIndex = 1;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      this.btnCancel.Click += new System.EventHandler(this.BtnCancelClick);
      // 
      // btnOK
      // 
      this.btnOK.Location = new System.Drawing.Point(3, 14);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(85, 23);
      this.btnOK.TabIndex = 0;
      this.btnOK.Text = "OK";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new System.EventHandler(this.BtnOKClick);
      // 
      // panel3
      // 
      this.panel3.Controls.Add(this.label8);
      this.panel3.Controls.Add(this.label9);
      this.panel3.Controls.Add(this.rbUnpower);
      this.panel3.Controls.Add(this.label10);
      this.panel3.Controls.Add(this.rbReset);
      this.panel3.Controls.Add(this.label11);
      this.panel3.Controls.Add(this.rbLeave);
      this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
      this.panel3.Location = new System.Drawing.Point(0, 75);
      this.panel3.Name = "panel3";
      this.panel3.Size = new System.Drawing.Size(416, 104);
      this.panel3.TabIndex = 1;
      // 
      // label8
      // 
      this.label8.AutoSize = true;
      this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label8.Location = new System.Drawing.Point(8, 9);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(104, 13);
      this.label8.TabIndex = 14;
      this.label8.Text = "Card initialization";
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
      this.rbUnpower.TabIndex = 2;
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
      this.rbReset.TabIndex = 1;
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
      this.rbLeave.TabIndex = 0;
      this.rbLeave.TabStop = true;
      this.rbLeave.Text = "Leave";
      this.rbLeave.UseVisualStyleBackColor = true;
      // 
      // ReconnectForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(514, 304);
      this.ControlBox = false;
      this.Controls.Add(this.panel2);
      this.Controls.Add(this.panel3);
      this.Controls.Add(this.pShareMode);
      this.Controls.Add(this.panel1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "ReconnectForm";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Options for Reconnect";
      this.Load += new System.EventHandler(this.ReconnectFormLoad);
      this.panel2.ResumeLayout(false);
      this.panel2.PerformLayout();
      this.pShareMode.ResumeLayout(false);
      this.pShareMode.PerformLayout();
      this.panel1.ResumeLayout(false);
      this.panel3.ResumeLayout(false);
      this.panel3.PerformLayout();
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.RadioButton rbRaw;
    private System.Windows.Forms.Label label12;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.RadioButton rbT0orT1;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.RadioButton rbT1;
    private System.Windows.Forms.RadioButton rbLeave;
    private System.Windows.Forms.Label label11;
    private System.Windows.Forms.RadioButton rbReset;
    private System.Windows.Forms.Label label10;
    private System.Windows.Forms.RadioButton rbUnpower;
    private System.Windows.Forms.Label label9;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.Panel panel3;
    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.RadioButton rbShared;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.RadioButton rbExclusive;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Panel pShareMode;
    private System.Windows.Forms.RadioButton rbT0;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.Label label5;
  }
}
