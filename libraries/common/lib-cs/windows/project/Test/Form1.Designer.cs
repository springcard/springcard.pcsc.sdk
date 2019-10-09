namespace Test
{
    partial class Form1
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

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnAboutClassic = new System.Windows.Forms.Button();
            this.btnAboutModern = new System.Windows.Forms.Button();
            this.btnAboutRed = new System.Windows.Forms.Button();
            this.btnAboutMarroon = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnAboutClassic
            // 
            this.btnAboutClassic.Location = new System.Drawing.Point(12, 12);
            this.btnAboutClassic.Name = "btnAboutClassic";
            this.btnAboutClassic.Size = new System.Drawing.Size(75, 23);
            this.btnAboutClassic.TabIndex = 0;
            this.btnAboutClassic.Text = "Classic";
            this.btnAboutClassic.UseVisualStyleBackColor = true;
            this.btnAboutClassic.Click += new System.EventHandler(this.btnAboutClassic_Click);
            // 
            // btnAboutModern
            // 
            this.btnAboutModern.Location = new System.Drawing.Point(12, 41);
            this.btnAboutModern.Name = "btnAboutModern";
            this.btnAboutModern.Size = new System.Drawing.Size(75, 23);
            this.btnAboutModern.TabIndex = 1;
            this.btnAboutModern.Text = "Modern";
            this.btnAboutModern.UseVisualStyleBackColor = true;
            this.btnAboutModern.Click += new System.EventHandler(this.btnAboutModern_Click);
            // 
            // btnAboutRed
            // 
            this.btnAboutRed.Location = new System.Drawing.Point(12, 70);
            this.btnAboutRed.Name = "btnAboutRed";
            this.btnAboutRed.Size = new System.Drawing.Size(75, 23);
            this.btnAboutRed.TabIndex = 2;
            this.btnAboutRed.Text = "Red";
            this.btnAboutRed.UseVisualStyleBackColor = true;
            this.btnAboutRed.Click += new System.EventHandler(this.btnAboutRed_Click);
            // 
            // btnAboutMarroon
            // 
            this.btnAboutMarroon.Location = new System.Drawing.Point(12, 99);
            this.btnAboutMarroon.Name = "btnAboutMarroon";
            this.btnAboutMarroon.Size = new System.Drawing.Size(75, 23);
            this.btnAboutMarroon.TabIndex = 3;
            this.btnAboutMarroon.Text = "Marroon";
            this.btnAboutMarroon.UseVisualStyleBackColor = true;
            this.btnAboutMarroon.Click += new System.EventHandler(this.btnAboutMarroon_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnAboutMarroon);
            this.Controls.Add(this.btnAboutRed);
            this.Controls.Add(this.btnAboutModern);
            this.Controls.Add(this.btnAboutClassic);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnAboutClassic;
        private System.Windows.Forms.Button btnAboutModern;
        private System.Windows.Forms.Button btnAboutRed;
        private System.Windows.Forms.Button btnAboutMarroon;
    }
}

