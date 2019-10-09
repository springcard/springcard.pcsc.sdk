/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 22/03/2017
 * Time: 16:28
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace PcscHce
{
	partial class MainForm
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.RichTextBox richTextBox1;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button button4;
		private System.Windows.Forms.Button button6;
		private System.Windows.Forms.Button button5;
		private System.Windows.Forms.Button button7;
		
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
			this.panel2 = new System.Windows.Forms.Panel();
			this.button6 = new System.Windows.Forms.Button();
			this.button5 = new System.Windows.Forms.Button();
			this.button4 = new System.Windows.Forms.Button();
			this.button3 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.button1 = new System.Windows.Forms.Button();
			this.panel3 = new System.Windows.Forms.Panel();
			this.richTextBox1 = new System.Windows.Forms.RichTextBox();
			this.button7 = new System.Windows.Forms.Button();
			this.panel2.SuspendLayout();
			this.panel3.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.White;
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(777, 55);
			this.panel1.TabIndex = 0;
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.button7);
			this.panel2.Controls.Add(this.button6);
			this.panel2.Controls.Add(this.button5);
			this.panel2.Controls.Add(this.button4);
			this.panel2.Controls.Add(this.button3);
			this.panel2.Controls.Add(this.button2);
			this.panel2.Controls.Add(this.button1);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel2.Location = new System.Drawing.Point(0, 328);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(777, 51);
			this.panel2.TabIndex = 1;
			// 
			// button6
			// 
			this.button6.Location = new System.Drawing.Point(417, 16);
			this.button6.Name = "button6";
			this.button6.Size = new System.Drawing.Size(75, 23);
			this.button6.TabIndex = 5;
			this.button6.Text = "Stop";
			this.button6.UseVisualStyleBackColor = true;
			this.button6.Click += new System.EventHandler(this.Button6Click);
			// 
			// button5
			// 
			this.button5.Location = new System.Drawing.Point(336, 16);
			this.button5.Name = "button5";
			this.button5.Size = new System.Drawing.Size(75, 23);
			this.button5.TabIndex = 4;
			this.button5.Text = "Start";
			this.button5.UseVisualStyleBackColor = true;
			this.button5.Click += new System.EventHandler(this.Button5Click);
			// 
			// button4
			// 
			this.button4.Location = new System.Drawing.Point(255, 16);
			this.button4.Name = "button4";
			this.button4.Size = new System.Drawing.Size(75, 23);
			this.button4.TabIndex = 3;
			this.button4.Text = "APDU";
			this.button4.UseVisualStyleBackColor = true;
			this.button4.Click += new System.EventHandler(this.Button4Click);
			// 
			// button3
			// 
			this.button3.Location = new System.Drawing.Point(174, 16);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(75, 23);
			this.button3.TabIndex = 2;
			this.button3.Text = "Deselect";
			this.button3.UseVisualStyleBackColor = true;
			this.button3.Click += new System.EventHandler(this.Button3Click);
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(93, 16);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(75, 23);
			this.button2.TabIndex = 1;
			this.button2.Text = "Select";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.Button2Click);
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(12, 16);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 0;
			this.button1.Text = "Create";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.Button1Click);
			// 
			// panel3
			// 
			this.panel3.Controls.Add(this.richTextBox1);
			this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel3.Location = new System.Drawing.Point(0, 55);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(777, 273);
			this.panel3.TabIndex = 2;
			// 
			// richTextBox1
			// 
			this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.richTextBox1.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.richTextBox1.Location = new System.Drawing.Point(0, 0);
			this.richTextBox1.Name = "richTextBox1";
			this.richTextBox1.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
			this.richTextBox1.Size = new System.Drawing.Size(777, 273);
			this.richTextBox1.TabIndex = 0;
			this.richTextBox1.Text = "";
			// 
			// button7
			// 
			this.button7.Location = new System.Drawing.Point(498, 16);
			this.button7.Name = "button7";
			this.button7.Size = new System.Drawing.Size(75, 23);
			this.button7.TabIndex = 6;
			this.button7.Text = "Login";
			this.button7.UseVisualStyleBackColor = true;
			this.button7.Click += new System.EventHandler(this.Button7Click);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(777, 379);
			this.Controls.Add(this.panel3);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.panel1);
			this.Name = "MainForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "pcsc-hce";
			this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			this.panel2.ResumeLayout(false);
			this.panel3.ResumeLayout(false);
			this.ResumeLayout(false);

		}
	}
}
