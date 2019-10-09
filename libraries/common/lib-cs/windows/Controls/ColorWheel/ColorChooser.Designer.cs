namespace SpringCard.LibCs.Windows.Controls.ColorWheel
{
    partial class ColorChooser
    {
        private System.Windows.Forms.Label Label3;
        private System.Windows.Forms.NumericUpDown nudSaturation;
        private System.Windows.Forms.Label Label7;
        private System.Windows.Forms.NumericUpDown nudBrightness;
        private System.Windows.Forms.NumericUpDown nudRed;
        private System.Windows.Forms.Panel pnlColor;
        private System.Windows.Forms.Label Label6;
        private System.Windows.Forms.Label Label1;
        private System.Windows.Forms.Label Label5;
        private System.Windows.Forms.Panel pnlSelectedColor;
        private System.Windows.Forms.Panel pnlBrightness;
        private System.Windows.Forms.NumericUpDown nudBlue;
        private System.Windows.Forms.Label Label4;
        private System.Windows.Forms.NumericUpDown nudGreen;
        private System.Windows.Forms.Label Label2;
        private System.Windows.Forms.NumericUpDown nudHue;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Label3 = new System.Windows.Forms.Label();
            this.nudSaturation = new System.Windows.Forms.NumericUpDown();
            this.Label7 = new System.Windows.Forms.Label();
            this.nudBrightness = new System.Windows.Forms.NumericUpDown();
            this.nudRed = new System.Windows.Forms.NumericUpDown();
            this.pnlColor = new System.Windows.Forms.Panel();
            this.Label6 = new System.Windows.Forms.Label();
            this.Label1 = new System.Windows.Forms.Label();
            this.Label5 = new System.Windows.Forms.Label();
            this.pnlSelectedColor = new System.Windows.Forms.Panel();
            this.pnlBrightness = new System.Windows.Forms.Panel();
            this.nudBlue = new System.Windows.Forms.NumericUpDown();
            this.Label4 = new System.Windows.Forms.Label();
            this.nudGreen = new System.Windows.Forms.NumericUpDown();
            this.Label2 = new System.Windows.Forms.Label();
            this.nudHue = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.nudSaturation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudBrightness)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudBlue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGreen)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudHue)).BeginInit();
            this.SuspendLayout();
            // 
            // Label3
            // 
            this.Label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label3.Location = new System.Drawing.Point(152, 280);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(48, 23);
            this.Label3.TabIndex = 45;
            this.Label3.Text = "Blue:";
            this.Label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // nudSaturation
            // 
            this.nudSaturation.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nudSaturation.Location = new System.Drawing.Point(96, 256);
            this.nudSaturation.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.nudSaturation.Name = "nudSaturation";
            this.nudSaturation.Size = new System.Drawing.Size(48, 22);
            this.nudSaturation.TabIndex = 42;
            this.nudSaturation.ValueChanged += new System.EventHandler(this.HandleHSVChange);
            // 
            // Label7
            // 
            this.Label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label7.Location = new System.Drawing.Point(16, 282);
            this.Label7.Name = "Label7";
            this.Label7.Size = new System.Drawing.Size(74, 21);
            this.Label7.TabIndex = 50;
            this.Label7.Text = "Brightness:";
            this.Label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // nudBrightness
            // 
            this.nudBrightness.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nudBrightness.Location = new System.Drawing.Point(96, 280);
            this.nudBrightness.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.nudBrightness.Name = "nudBrightness";
            this.nudBrightness.Size = new System.Drawing.Size(48, 22);
            this.nudBrightness.TabIndex = 47;
            this.nudBrightness.ValueChanged += new System.EventHandler(this.HandleHSVChange);
            // 
            // nudRed
            // 
            this.nudRed.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nudRed.Location = new System.Drawing.Point(208, 232);
            this.nudRed.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.nudRed.Name = "nudRed";
            this.nudRed.Size = new System.Drawing.Size(48, 22);
            this.nudRed.TabIndex = 38;
            this.nudRed.ValueChanged += new System.EventHandler(this.HandleRGBChange);
            // 
            // pnlColor
            // 
            this.pnlColor.Location = new System.Drawing.Point(8, 8);
            this.pnlColor.Name = "pnlColor";
            this.pnlColor.Size = new System.Drawing.Size(176, 176);
            this.pnlColor.TabIndex = 51;
            this.pnlColor.Visible = false;
            this.pnlColor.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ColorChooser_MouseUp);
            // 
            // Label6
            // 
            this.Label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label6.Location = new System.Drawing.Point(16, 256);
            this.Label6.Name = "Label6";
            this.Label6.Size = new System.Drawing.Size(72, 23);
            this.Label6.TabIndex = 49;
            this.Label6.Text = "Saturation:";
            this.Label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Label1
            // 
            this.Label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label1.Location = new System.Drawing.Point(152, 232);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(48, 23);
            this.Label1.TabIndex = 43;
            this.Label1.Text = "Red:";
            this.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Label5
            // 
            this.Label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label5.Location = new System.Drawing.Point(16, 232);
            this.Label5.Name = "Label5";
            this.Label5.Size = new System.Drawing.Size(72, 23);
            this.Label5.TabIndex = 48;
            this.Label5.Text = "Hue:";
            this.Label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlSelectedColor
            // 
            this.pnlSelectedColor.Location = new System.Drawing.Point(208, 200);
            this.pnlSelectedColor.Name = "pnlSelectedColor";
            this.pnlSelectedColor.Size = new System.Drawing.Size(48, 24);
            this.pnlSelectedColor.TabIndex = 53;
            this.pnlSelectedColor.Visible = false;
            // 
            // pnlBrightness
            // 
            this.pnlBrightness.Location = new System.Drawing.Point(208, 8);
            this.pnlBrightness.Name = "pnlBrightness";
            this.pnlBrightness.Size = new System.Drawing.Size(16, 176);
            this.pnlBrightness.TabIndex = 52;
            this.pnlBrightness.Visible = false;
            // 
            // nudBlue
            // 
            this.nudBlue.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nudBlue.Location = new System.Drawing.Point(208, 280);
            this.nudBlue.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.nudBlue.Name = "nudBlue";
            this.nudBlue.Size = new System.Drawing.Size(48, 22);
            this.nudBlue.TabIndex = 40;
            this.nudBlue.ValueChanged += new System.EventHandler(this.HandleRGBChange);
            // 
            // Label4
            // 
            this.Label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label4.Location = new System.Drawing.Point(152, 200);
            this.Label4.Name = "Label4";
            this.Label4.Size = new System.Drawing.Size(48, 24);
            this.Label4.TabIndex = 46;
            this.Label4.Text = "Color:";
            this.Label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // nudGreen
            // 
            this.nudGreen.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nudGreen.Location = new System.Drawing.Point(208, 256);
            this.nudGreen.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.nudGreen.Name = "nudGreen";
            this.nudGreen.Size = new System.Drawing.Size(48, 22);
            this.nudGreen.TabIndex = 39;
            this.nudGreen.ValueChanged += new System.EventHandler(this.HandleRGBChange);
            // 
            // Label2
            // 
            this.Label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label2.Location = new System.Drawing.Point(152, 256);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(48, 23);
            this.Label2.TabIndex = 44;
            this.Label2.Text = "Green:";
            this.Label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // nudHue
            // 
            this.nudHue.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nudHue.Location = new System.Drawing.Point(96, 232);
            this.nudHue.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.nudHue.Name = "nudHue";
            this.nudHue.Size = new System.Drawing.Size(48, 22);
            this.nudHue.TabIndex = 41;
            this.nudHue.ValueChanged += new System.EventHandler(this.HandleHSVChange);
            // 
            // ColorChooser
            // 
            this.Controls.Add(this.Label3);
            this.Controls.Add(this.nudSaturation);
            this.Controls.Add(this.Label7);
            this.Controls.Add(this.nudBrightness);
            this.Controls.Add(this.nudRed);
            this.Controls.Add(this.pnlColor);
            this.Controls.Add(this.Label6);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.Label5);
            this.Controls.Add(this.pnlSelectedColor);
            this.Controls.Add(this.pnlBrightness);
            this.Controls.Add(this.nudBlue);
            this.Controls.Add(this.Label4);
            this.Controls.Add(this.nudGreen);
            this.Controls.Add(this.Label2);
            this.Controls.Add(this.nudHue);
            this.Name = "ColorChooser";
            this.Size = new System.Drawing.Size(264, 315);
            this.Load += new System.EventHandler(this.ColorChooser_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.ColorChooser_Paint);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.HandleMouse);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.HandleMouse);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ColorChooser_MouseUp);
            ((System.ComponentModel.ISupportInitialize)(this.nudSaturation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudBrightness)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudBlue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGreen)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudHue)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion
    }
}
