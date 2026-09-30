
namespace PcscDiag2
{
    partial class Benchmark
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            this.label1 = new System.Windows.Forms.Label();
            this.eCount = new System.Windows.Forms.NumericUpDown();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.eTimeElapsed = new System.Windows.Forms.TextBox();
            this.eBenchmark = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.ePcToCard = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.eCardToPc = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.btnStop = new System.Windows.Forms.Button();
            this.eProgress = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.eCount)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Number of runs:";
            // 
            // eCount
            // 
            this.eCount.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.eCount.Location = new System.Drawing.Point(100, 21);
            this.eCount.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.eCount.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.eCount.Name = "eCount";
            this.eCount.Size = new System.Drawing.Size(86, 20);
            this.eCount.TabIndex = 2;
            this.eCount.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(388, 18);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 3;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(388, 147);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 4;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 83);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Time elapsed:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 106);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(64, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Benchmark:";
            // 
            // eTimeElapsed
            // 
            this.eTimeElapsed.Location = new System.Drawing.Point(100, 80);
            this.eTimeElapsed.Name = "eTimeElapsed";
            this.eTimeElapsed.ReadOnly = true;
            this.eTimeElapsed.Size = new System.Drawing.Size(86, 20);
            this.eTimeElapsed.TabIndex = 7;
            // 
            // eBenchmark
            // 
            this.eBenchmark.Location = new System.Drawing.Point(100, 103);
            this.eBenchmark.Name = "eBenchmark";
            this.eBenchmark.ReadOnly = true;
            this.eBenchmark.Size = new System.Drawing.Size(86, 20);
            this.eBenchmark.TabIndex = 8;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(192, 83);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(69, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "(milliseconds)";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(192, 106);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(110, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "(transactions/second)";
            // 
            // ePcToCard
            // 
            this.ePcToCard.Location = new System.Drawing.Point(100, 126);
            this.ePcToCard.Name = "ePcToCard";
            this.ePcToCard.ReadOnly = true;
            this.ePcToCard.Size = new System.Drawing.Size(86, 20);
            this.ePcToCard.TabIndex = 12;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 129);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(61, 13);
            this.label6.TabIndex = 11;
            this.label6.Text = "PC to Card:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(192, 129);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(78, 13);
            this.label7.TabIndex = 13;
            this.label7.Text = "(bytes/second)";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(192, 152);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(78, 13);
            this.label8.TabIndex = 16;
            this.label8.Text = "(bytes/second)";
            // 
            // eCardToPc
            // 
            this.eCardToPc.Location = new System.Drawing.Point(100, 149);
            this.eCardToPc.Name = "eCardToPc";
            this.eCardToPc.ReadOnly = true;
            this.eCardToPc.Size = new System.Drawing.Size(86, 20);
            this.eCardToPc.TabIndex = 15;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(12, 152);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(61, 13);
            this.label9.TabIndex = 14;
            this.label9.Text = "Card to PC:";
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(388, 44);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 23);
            this.btnStop.TabIndex = 17;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // eProgress
            // 
            this.eProgress.Location = new System.Drawing.Point(100, 44);
            this.eProgress.Name = "eProgress";
            this.eProgress.ReadOnly = true;
            this.eProgress.Size = new System.Drawing.Size(86, 20);
            this.eProgress.TabIndex = 19;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(12, 47);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(51, 13);
            this.label10.TabIndex = 18;
            this.label10.Text = "Progress:";
            // 
            // Benchmark
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(494, 181);
            this.Controls.Add(this.eProgress);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.eCardToPc);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.ePcToCard);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.eBenchmark);
            this.Controls.Add(this.eTimeElapsed);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.eCount);
            this.Controls.Add(this.label1);
            this.Name = "Benchmark";
            this.Text = "BenchmarkForm";
            ((System.ComponentModel.ISupportInitialize)(this.eCount)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown eCount;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox eTimeElapsed;
        private System.Windows.Forms.TextBox eBenchmark;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox ePcToCard;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox eCardToPc;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.TextBox eProgress;
        private System.Windows.Forms.Label label10;
    }
}