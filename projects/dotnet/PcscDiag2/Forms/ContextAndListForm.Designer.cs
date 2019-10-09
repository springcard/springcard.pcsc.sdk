/*
 * Crée par SharpDevelop.
 * Utilisateur: johann
 * Date: 13/04/2012
 * Heure: 10:04
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
namespace PcscDiag2
{
  partial class ContextAndListForm
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
      this.eGroupSpecific = new System.Windows.Forms.TextBox();
      this.label9 = new System.Windows.Forms.Label();
      this.rbGroupAll = new System.Windows.Forms.RadioButton();
      this.label4 = new System.Windows.Forms.Label();
      this.rbGroupSpecific = new System.Windows.Forms.RadioButton();
      this.label6 = new System.Windows.Forms.Label();
      this.rbGroupDefault = new System.Windows.Forms.RadioButton();
      this.label3 = new System.Windows.Forms.Label();
      this.rbContextSystem = new System.Windows.Forms.RadioButton();
      this.label1 = new System.Windows.Forms.Label();
      this.rbContextUser = new System.Windows.Forms.RadioButton();
      this.panel1 = new System.Windows.Forms.Panel();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnOK = new System.Windows.Forms.Button();
      this.pShareMode = new System.Windows.Forms.Panel();
      this.label2 = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.rbSystem = new System.Windows.Forms.RadioButton();
      this.label7 = new System.Windows.Forms.Label();
      this.rbUser = new System.Windows.Forms.RadioButton();
      this.panel2.SuspendLayout();
      this.panel1.SuspendLayout();
      this.pShareMode.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel2
      // 
      this.panel2.Controls.Add(this.eGroupSpecific);
      this.panel2.Controls.Add(this.label9);
      this.panel2.Controls.Add(this.rbGroupAll);
      this.panel2.Controls.Add(this.label4);
      this.panel2.Controls.Add(this.rbGroupSpecific);
      this.panel2.Controls.Add(this.label6);
      this.panel2.Controls.Add(this.rbGroupDefault);
      this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
      this.panel2.Location = new System.Drawing.Point(0, 79);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(416, 102);
      this.panel2.TabIndex = 1;
      // 
      // eGroupSpecific
      // 
      this.eGroupSpecific.Enabled = false;
      this.eGroupSpecific.Location = new System.Drawing.Point(103, 73);
      this.eGroupSpecific.Name = "eGroupSpecific";
      this.eGroupSpecific.Size = new System.Drawing.Size(307, 20);
      this.eGroupSpecific.TabIndex = 17;
      // 
      // label9
      // 
      this.label9.AutoSize = true;
      this.label9.Location = new System.Drawing.Point(103, 30);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(156, 13);
      this.label9.TabIndex = 16;
      this.label9.Text = "Returns the full list of all readers";
      // 
      // rbGroupAll
      // 
      this.rbGroupAll.AutoSize = true;
      this.rbGroupAll.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbGroupAll.Location = new System.Drawing.Point(10, 28);
      this.rbGroupAll.Name = "rbGroupAll";
      this.rbGroupAll.Size = new System.Drawing.Size(39, 17);
      this.rbGroupAll.TabIndex = 0;
      this.rbGroupAll.TabStop = true;
      this.rbGroupAll.Text = "All";
      this.rbGroupAll.UseVisualStyleBackColor = true;
      this.rbGroupAll.CheckedChanged += new System.EventHandler(this.RbGroupAllCheckedChanged);
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label4.Location = new System.Drawing.Point(8, 9);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(114, 13);
      this.label4.TabIndex = 12;
      this.label4.Text = "Reader list filtering";
      // 
      // rbGroupSpecific
      // 
      this.rbGroupSpecific.AutoSize = true;
      this.rbGroupSpecific.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbGroupSpecific.Location = new System.Drawing.Point(10, 74);
      this.rbGroupSpecific.Name = "rbGroupSpecific";
      this.rbGroupSpecific.Size = new System.Drawing.Size(75, 17);
      this.rbGroupSpecific.TabIndex = 2;
      this.rbGroupSpecific.TabStop = true;
      this.rbGroupSpecific.Text = "Specific:";
      this.rbGroupSpecific.UseVisualStyleBackColor = true;
      this.rbGroupSpecific.CheckedChanged += new System.EventHandler(this.RbGroupAllCheckedChanged);
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(103, 53);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(216, 13);
      this.label6.TabIndex = 9;
      this.label6.Text = "Default group to which all readers are added";
      // 
      // rbGroupDefault
      // 
      this.rbGroupDefault.AutoSize = true;
      this.rbGroupDefault.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbGroupDefault.Location = new System.Drawing.Point(10, 51);
      this.rbGroupDefault.Name = "rbGroupDefault";
      this.rbGroupDefault.Size = new System.Drawing.Size(66, 17);
      this.rbGroupDefault.TabIndex = 1;
      this.rbGroupDefault.TabStop = true;
      this.rbGroupDefault.Text = "Default";
      this.rbGroupDefault.UseVisualStyleBackColor = true;
      this.rbGroupDefault.CheckedChanged += new System.EventHandler(this.RbGroupAllCheckedChanged);
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label3.Location = new System.Drawing.Point(8, 9);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(129, 13);
      this.label3.TabIndex = 12;
      this.label3.Text = "PC/SC context scope";
      // 
      // rbContextSystem
      // 
      this.rbContextSystem.AutoSize = true;
      this.rbContextSystem.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbContextSystem.Location = new System.Drawing.Point(10, 51);
      this.rbContextSystem.Name = "rbContextSystem";
      this.rbContextSystem.Size = new System.Drawing.Size(65, 17);
      this.rbContextSystem.TabIndex = 1;
      this.rbContextSystem.TabStop = true;
      this.rbContextSystem.Text = "System";
      this.rbContextSystem.UseVisualStyleBackColor = true;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(103, 30);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(189, 13);
      this.label1.TabIndex = 9;
      this.label1.Text = "Access to readers within user\'s domain";
      // 
      // rbContextUser
      // 
      this.rbContextUser.AutoSize = true;
      this.rbContextUser.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbContextUser.Location = new System.Drawing.Point(10, 28);
      this.rbContextUser.Name = "rbContextUser";
      this.rbContextUser.Size = new System.Drawing.Size(51, 17);
      this.rbContextUser.TabIndex = 0;
      this.rbContextUser.TabStop = true;
      this.rbContextUser.Text = "User";
      this.rbContextUser.UseVisualStyleBackColor = true;
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnCancel);
      this.panel1.Controls.Add(this.btnOK);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
      this.panel1.Location = new System.Drawing.Point(416, 0);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(98, 183);
      this.panel1.TabIndex = 2;
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
      // pShareMode
      // 
      this.pShareMode.Controls.Add(this.label2);
      this.pShareMode.Controls.Add(this.label5);
      this.pShareMode.Controls.Add(this.rbSystem);
      this.pShareMode.Controls.Add(this.label7);
      this.pShareMode.Controls.Add(this.rbUser);
      this.pShareMode.Dock = System.Windows.Forms.DockStyle.Top;
      this.pShareMode.Location = new System.Drawing.Point(0, 0);
      this.pShareMode.Name = "pShareMode";
      this.pShareMode.Size = new System.Drawing.Size(416, 79);
      this.pShareMode.TabIndex = 0;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label2.Location = new System.Drawing.Point(8, 9);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(88, 13);
      this.label2.TabIndex = 12;
      this.label2.Text = "Context scope";
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(103, 53);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(220, 13);
      this.label5.TabIndex = 11;
      this.label5.Text = "Perform PC/SC actions within system\'s scope";
      // 
      // rbSystem
      // 
      this.rbSystem.AutoSize = true;
      this.rbSystem.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbSystem.Location = new System.Drawing.Point(10, 51);
      this.rbSystem.Name = "rbSystem";
      this.rbSystem.Size = new System.Drawing.Size(65, 17);
      this.rbSystem.TabIndex = 1;
      this.rbSystem.TabStop = true;
      this.rbSystem.Text = "System";
      this.rbSystem.UseVisualStyleBackColor = true;
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(103, 30);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(190, 13);
      this.label7.TabIndex = 9;
      this.label7.Text = "Restrict PC/SC actions to user\'s scope";
      // 
      // rbUser
      // 
      this.rbUser.AutoSize = true;
      this.rbUser.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbUser.Location = new System.Drawing.Point(10, 28);
      this.rbUser.Name = "rbUser";
      this.rbUser.Size = new System.Drawing.Size(51, 17);
      this.rbUser.TabIndex = 0;
      this.rbUser.TabStop = true;
      this.rbUser.Text = "User";
      this.rbUser.UseVisualStyleBackColor = true;
      // 
      // ContextAndListForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(514, 183);
      this.ControlBox = false;
      this.Controls.Add(this.panel2);
      this.Controls.Add(this.pShareMode);
      this.Controls.Add(this.panel1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "ContextAndListForm";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Global PC/SC options";
      this.Load += new System.EventHandler(this.ContextAndListFormLoad);
      this.panel2.ResumeLayout(false);
      this.panel2.PerformLayout();
      this.panel1.ResumeLayout(false);
      this.pShareMode.ResumeLayout(false);
      this.pShareMode.PerformLayout();
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.RadioButton rbUser;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.RadioButton rbSystem;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Panel pShareMode;
    private System.Windows.Forms.TextBox eGroupSpecific;
    private System.Windows.Forms.RadioButton rbGroupAll;
    private System.Windows.Forms.RadioButton rbGroupSpecific;
    private System.Windows.Forms.RadioButton rbGroupDefault;
    private System.Windows.Forms.RadioButton rbContextSystem;
    private System.Windows.Forms.RadioButton rbContextUser;
    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label label9;
    private System.Windows.Forms.Panel panel2;
  }
}
