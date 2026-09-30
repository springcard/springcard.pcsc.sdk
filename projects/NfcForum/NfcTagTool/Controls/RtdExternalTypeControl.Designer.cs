namespace SpringCardApplication.Controls
{
    partial class RtdExternalTypeControl
    {
        /// <summary> 
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur de composants

        /// <summary> 
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas 
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.eType = new System.Windows.Forms.TextBox();
            this.lb = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.eData = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // eType
            // 
            this.eType.Location = new System.Drawing.Point(6, 29);
            this.eType.MaxLength = 1000;
            this.eType.Name = "eType";
            this.eType.Size = new System.Drawing.Size(443, 20);
            this.eType.TabIndex = 34;
            // 
            // lb
            // 
            this.lb.Location = new System.Drawing.Point(3, 6);
            this.lb.Name = "lb";
            this.lb.Size = new System.Drawing.Size(446, 23);
            this.lb.TabIndex = 35;
            this.lb.Text = "Type (in the form urn:nfc:ext:your company:your service):";
            this.lb.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(6, 52);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(446, 23);
            this.label1.TabIndex = 36;
            this.label1.Text = "Data";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // eData
            // 
            this.eData.Location = new System.Drawing.Point(6, 76);
            this.eData.MaxLength = 1000;
            this.eData.Name = "eData";
            this.eData.Size = new System.Drawing.Size(443, 20);
            this.eData.TabIndex = 37;
            // 
            // RtdExternalTypeControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.eData);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.eType);
            this.Controls.Add(this.lb);
            this.Name = "RtdExternalTypeControl";
            this.Size = new System.Drawing.Size(657, 150);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox eType;
        private System.Windows.Forms.Label lb;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox eData;
    }
}
