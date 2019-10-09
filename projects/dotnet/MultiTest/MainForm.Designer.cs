/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 02/09/2014
 * Time: 11:46
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace MultiTest
{
	partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.pTop = new System.Windows.Forms.Panel();
            this.cbTestSuite = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lkAbout = new System.Windows.Forms.LinkLabel();
            this.lkRefresh = new System.Windows.Forms.LinkLabel();
            this.lbReader = new System.Windows.Forms.Label();
            this.cbReader = new System.Windows.Forms.ComboBox();
            this.pBottom = new System.Windows.Forms.Panel();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnRun = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.pMain = new System.Windows.Forms.Panel();
            this.richTextBoxResponses = new System.Windows.Forms.RichTextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.richTextBoxCommands = new System.Windows.Forms.RichTextBox();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.pTop.SuspendLayout();
            this.pBottom.SuspendLayout();
            this.pMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // pTop
            // 
            this.pTop.Controls.Add(this.cbTestSuite);
            this.pTop.Controls.Add(this.label1);
            this.pTop.Controls.Add(this.lkAbout);
            this.pTop.Controls.Add(this.lkRefresh);
            this.pTop.Controls.Add(this.lbReader);
            this.pTop.Controls.Add(this.cbReader);
            this.pTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pTop.Location = new System.Drawing.Point(0, 0);
            this.pTop.Name = "pTop";
            this.pTop.Size = new System.Drawing.Size(1008, 57);
            this.pTop.TabIndex = 0;
            // 
            // cbTestSuite
            // 
            this.cbTestSuite.FormattingEnabled = true;
            this.cbTestSuite.Location = new System.Drawing.Point(512, 25);
            this.cbTestSuite.Name = "cbTestSuite";
            this.cbTestSuite.Size = new System.Drawing.Size(209, 21);
            this.cbTestSuite.TabIndex = 14;
            this.cbTestSuite.SelectedIndexChanged += new System.EventHandler(this.cbTestSuite_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(509, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 13);
            this.label1.TabIndex = 13;
            this.label1.Text = "Test suite:";
            // 
            // lkAbout
            // 
            this.lkAbout.AutoSize = true;
            this.lkAbout.Location = new System.Drawing.Point(961, 9);
            this.lkAbout.Name = "lkAbout";
            this.lkAbout.Size = new System.Drawing.Size(35, 13);
            this.lkAbout.TabIndex = 12;
            this.lkAbout.TabStop = true;
            this.lkAbout.Text = "About";
            this.lkAbout.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lkAbout_LinkClicked);
            // 
            // lkRefresh
            // 
            this.lkRefresh.AutoSize = true;
            this.lkRefresh.Location = new System.Drawing.Point(397, 9);
            this.lkRefresh.Name = "lkRefresh";
            this.lkRefresh.Size = new System.Drawing.Size(92, 13);
            this.lkRefresh.TabIndex = 7;
            this.lkRefresh.TabStop = true;
            this.lkRefresh.Text = "Refresh reader list";
            this.lkRefresh.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.lkRefresh.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lkRefresh_LinkClicked);
            // 
            // lbReader
            // 
            this.lbReader.AutoSize = true;
            this.lbReader.Location = new System.Drawing.Point(9, 9);
            this.lbReader.Name = "lbReader";
            this.lbReader.Size = new System.Drawing.Size(81, 13);
            this.lbReader.TabIndex = 6;
            this.lbReader.Text = "PC/SC Reader:";
            // 
            // cbReader
            // 
            this.cbReader.FormattingEnabled = true;
            this.cbReader.Location = new System.Drawing.Point(12, 25);
            this.cbReader.Name = "cbReader";
            this.cbReader.Size = new System.Drawing.Size(477, 21);
            this.cbReader.TabIndex = 0;
            this.cbReader.SelectedIndexChanged += new System.EventHandler(this.cbReader_SelectedIndexChanged);
            // 
            // pBottom
            // 
            this.pBottom.Controls.Add(this.btnStop);
            this.pBottom.Controls.Add(this.btnStart);
            this.pBottom.Controls.Add(this.btnRun);
            this.pBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pBottom.Location = new System.Drawing.Point(0, 668);
            this.pBottom.Name = "pBottom";
            this.pBottom.Size = new System.Drawing.Size(1008, 39);
            this.pBottom.TabIndex = 1;
            // 
            // btnStop
            // 
            this.btnStop.Enabled = false;
            this.btnStop.Location = new System.Drawing.Point(270, 6);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(123, 23);
            this.btnStop.TabIndex = 2;
            this.btnStop.Text = "Stop loop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.BtnStopClick);
            // 
            // btnStart
            // 
            this.btnStart.Enabled = false;
            this.btnStart.Location = new System.Drawing.Point(141, 6);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(123, 23);
            this.btnStart.TabIndex = 1;
            this.btnStart.Text = "Start loop";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.BtnStartClick);
            // 
            // btnRun
            // 
            this.btnRun.Enabled = false;
            this.btnRun.Location = new System.Drawing.Point(12, 6);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(123, 23);
            this.btnRun.TabIndex = 0;
            this.btnRun.Text = "Run (single shot)";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.BtnRunClick);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 707);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1008, 22);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // pMain
            // 
            this.pMain.Controls.Add(this.richTextBoxResponses);
            this.pMain.Controls.Add(this.label3);
            this.pMain.Controls.Add(this.label2);
            this.pMain.Controls.Add(this.richTextBoxCommands);
            this.pMain.Controls.Add(this.treeView1);
            this.pMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pMain.Location = new System.Drawing.Point(0, 57);
            this.pMain.Name = "pMain";
            this.pMain.Size = new System.Drawing.Size(1008, 611);
            this.pMain.TabIndex = 4;
            // 
            // richTextBoxResponses
            // 
            this.richTextBoxResponses.BackColor = System.Drawing.SystemColors.Window;
            this.richTextBoxResponses.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBoxResponses.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxResponses.Location = new System.Drawing.Point(512, 235);
            this.richTextBoxResponses.Name = "richTextBoxResponses";
            this.richTextBoxResponses.ReadOnly = true;
            this.richTextBoxResponses.Size = new System.Drawing.Size(484, 370);
            this.richTextBoxResponses.TabIndex = 16;
            this.richTextBoxResponses.Text = "";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(509, 219);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(69, 13);
            this.label3.TabIndex = 15;
            this.label3.Text = "Response(s):";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(509, 6);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "Command(s):";
            // 
            // richTextBoxCommands
            // 
            this.richTextBoxCommands.BackColor = System.Drawing.SystemColors.Window;
            this.richTextBoxCommands.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBoxCommands.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxCommands.Location = new System.Drawing.Point(512, 23);
            this.richTextBoxCommands.Name = "richTextBoxCommands";
            this.richTextBoxCommands.ReadOnly = true;
            this.richTextBoxCommands.Size = new System.Drawing.Size(484, 179);
            this.richTextBoxCommands.TabIndex = 2;
            this.richTextBoxCommands.Text = "";
            // 
            // treeView1
            // 
            this.treeView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.treeView1.FullRowSelect = true;
            this.treeView1.HideSelection = false;
            this.treeView1.HotTracking = true;
            this.treeView1.Location = new System.Drawing.Point(12, 6);
            this.treeView1.Name = "treeView1";
            this.treeView1.ShowNodeToolTips = true;
            this.treeView1.ShowRootLines = false;
            this.treeView1.Size = new System.Drawing.Size(477, 599);
            this.treeView1.TabIndex = 1;
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeView1AfterSelect);
            this.treeView1.DoubleClick += new System.EventHandler(this.TreeView1DoubleClick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1008, 729);
            this.Controls.Add(this.pMain);
            this.Controls.Add(this.pBottom);
            this.Controls.Add(this.pTop);
            this.Controls.Add(this.statusStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MultiTest";
            this.Load += new System.EventHandler(this.MainFormLoad);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.pTop.ResumeLayout(false);
            this.pTop.PerformLayout();
            this.pBottom.ResumeLayout(false);
            this.pMain.ResumeLayout(false);
            this.pMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		private System.Windows.Forms.Button btnStart;
		private System.Windows.Forms.Button btnStop;
		private System.Windows.Forms.TreeView treeView1;
		private System.Windows.Forms.Panel pMain;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.Button btnRun;
		private System.Windows.Forms.Panel pBottom;
		private System.Windows.Forms.ComboBox cbReader;
		private System.Windows.Forms.Panel pTop;
        private System.Windows.Forms.LinkLabel lkAbout;
        private System.Windows.Forms.LinkLabel lkRefresh;
        private System.Windows.Forms.Label lbReader;
        private System.Windows.Forms.ComboBox cbTestSuite;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RichTextBox richTextBoxCommands;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RichTextBox richTextBoxResponses;
    }
}
