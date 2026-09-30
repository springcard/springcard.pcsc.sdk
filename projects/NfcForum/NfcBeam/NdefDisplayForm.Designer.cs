/*
 * Crée par SharpDevelop.
 * Utilisateur: johann
 * Date: 01/10/2012
 * Heure: 15:46
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
namespace SpringCardApplication
{
  partial class NdefDisplayForm
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NdefDisplayForm));
      this.panel1 = new System.Windows.Forms.Panel();
      this.label1 = new System.Windows.Forms.Label();
      this.hexBox = new Be.Windows.Forms.HexBox();
      this.label4 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.panel2 = new System.Windows.Forms.Panel();
      this.pRight = new System.Windows.Forms.Panel();
      this.btnClose = new System.Windows.Forms.Button();
      this.pDecoded = new System.Windows.Forms.Panel();
      this.label5 = new System.Windows.Forms.Label();
      this.lbDecodedType = new System.Windows.Forms.Label();
      this.panel1.SuspendLayout();
      this.panel2.SuspendLayout();
      this.pRight.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.lbDecodedType);
      this.panel1.Controls.Add(this.label5);
      this.panel1.Controls.Add(this.label1);
      this.panel1.Controls.Add(this.hexBox);
      this.panel1.Controls.Add(this.label4);
      this.panel1.Controls.Add(this.label3);
      this.panel1.Controls.Add(this.label2);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
      this.panel1.Location = new System.Drawing.Point(0, 0);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(667, 212);
      this.panel1.TabIndex = 0;
      // 
      // label1
      // 
      this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.Location = new System.Drawing.Point(12, 9);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(100, 20);
      this.label1.TabIndex = 10;
      this.label1.Text = "Raw data";
      // 
      // hexBox
      // 
      this.hexBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.hexBox.LineInfoVisible = true;
      this.hexBox.Location = new System.Drawing.Point(12, 45);
      this.hexBox.Name = "hexBox";
      this.hexBox.ReadOnly = true;
      this.hexBox.ShadowSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(60)))), ((int)(((byte)(188)))), ((int)(((byte)(255)))));
      this.hexBox.Size = new System.Drawing.Size(641, 135);
      this.hexBox.StringViewVisible = true;
      this.hexBox.TabIndex = 9;
      this.hexBox.UseFixedBytesPerLine = true;
      this.hexBox.VScrollBarVisible = true;
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label4.ForeColor = System.Drawing.SystemColors.GrayText;
      this.label4.Location = new System.Drawing.Point(10, 29);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(35, 13);
      this.label4.TabIndex = 8;
      this.label4.Text = "Offset";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label3.ForeColor = System.Drawing.SystemColors.GrayText;
      this.label3.Location = new System.Drawing.Point(494, 29);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(85, 13);
      this.label3.TabIndex = 7;
      this.label3.Text = "ASCII translation";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label2.ForeColor = System.Drawing.SystemColors.GrayText;
      this.label2.Location = new System.Drawing.Point(94, 29);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(94, 13);
      this.label2.TabIndex = 6;
      this.label2.Text = "Hexadecimal entry";
      // 
      // panel2
      // 
      this.panel2.Controls.Add(this.pRight);
      this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel2.Location = new System.Drawing.Point(0, 489);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(667, 64);
      this.panel2.TabIndex = 4;
      // 
      // pRight
      // 
      this.pRight.Controls.Add(this.btnClose);
      this.pRight.Dock = System.Windows.Forms.DockStyle.Right;
      this.pRight.Location = new System.Drawing.Point(547, 0);
      this.pRight.Name = "pRight";
      this.pRight.Padding = new System.Windows.Forms.Padding(3, 28, 3, 6);
      this.pRight.Size = new System.Drawing.Size(120, 64);
      this.pRight.TabIndex = 4;
      // 
      // btnClose
      // 
      this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnClose.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.btnClose.FlatAppearance.BorderSize = 0;
      this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnClose.Image = ((System.Drawing.Image)(resources.GetObject("btnClose.Image")));
      this.btnClose.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
      this.btnClose.Location = new System.Drawing.Point(3, 9);
      this.btnClose.Name = "btnClose";
      this.btnClose.Size = new System.Drawing.Size(114, 49);
      this.btnClose.TabIndex = 15;
      this.btnClose.Text = "Close";
      this.btnClose.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
      this.btnClose.UseVisualStyleBackColor = true;
      // 
      // pDecoded
      // 
      this.pDecoded.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pDecoded.Location = new System.Drawing.Point(0, 212);
      this.pDecoded.Name = "pDecoded";
      this.pDecoded.Padding = new System.Windows.Forms.Padding(12);
      this.pDecoded.Size = new System.Drawing.Size(667, 277);
      this.pDecoded.TabIndex = 5;
      // 
      // label5
      // 
      this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label5.Location = new System.Drawing.Point(12, 189);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(100, 20);
      this.label5.TabIndex = 11;
      this.label5.Text = "Decoded data:";
      // 
      // lbDecodedType
      // 
      this.lbDecodedType.Location = new System.Drawing.Point(107, 189);
      this.lbDecodedType.Name = "lbDecodedType";
      this.lbDecodedType.Size = new System.Drawing.Size(546, 23);
      this.lbDecodedType.TabIndex = 12;
      this.lbDecodedType.Text = "?";
      // 
      // NdefDisplayForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(667, 553);
      this.Controls.Add(this.pDecoded);
      this.Controls.Add(this.panel2);
      this.Controls.Add(this.panel1);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "NdefDisplayForm";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "New NDEF content detected";
      this.panel1.ResumeLayout(false);
      this.panel1.PerformLayout();
      this.panel2.ResumeLayout(false);
      this.pRight.ResumeLayout(false);
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label lbDecodedType;
    private System.Windows.Forms.Panel pDecoded;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.Button btnClose;
    private System.Windows.Forms.Panel pRight;
    private Be.Windows.Forms.HexBox hexBox;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Panel panel1;
  }
}
