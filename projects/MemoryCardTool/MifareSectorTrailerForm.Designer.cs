/*
 * Created by SharpDevelop.
 * User: herve.t
 * Date: 19/01/2016
 * Time: 10:33
 *
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace MemoryCardTool
{
	partial class MifareSectorTrailerForm
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.GroupBox grpAccessConditionDataBlocks;
		private System.Windows.Forms.ListView lvBlock0;
		private System.Windows.Forms.ColumnHeader C1C2C3;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ListView lvBlock1;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.ColumnHeader columnHeader5;
		private System.Windows.Forms.ColumnHeader columnHeader6;
		private System.Windows.Forms.ColumnHeader columnHeader7;
		private System.Windows.Forms.ListView lvBlock2;
		private System.Windows.Forms.ColumnHeader columnHeader8;
		private System.Windows.Forms.ColumnHeader columnHeader9;
		private System.Windows.Forms.ColumnHeader columnHeader10;
		private System.Windows.Forms.ColumnHeader columnHeader11;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ListView lvRemarks1;
		private System.Windows.Forms.ColumnHeader columnHeader12;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.ListView lvAccessConditions;
		private System.Windows.Forms.ColumnHeader columnHeader13;
		private System.Windows.Forms.ColumnHeader columnHeader14;
		private System.Windows.Forms.ColumnHeader columnHeader15;
		private System.Windows.Forms.ColumnHeader columnHeader16;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.ColumnHeader columnHeader17;
		private System.Windows.Forms.ColumnHeader columnHeader18;
		private System.Windows.Forms.ColumnHeader columnHeader19;
		private System.Windows.Forms.ColumnHeader columnHeader20;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.ComboBox cbNewKeyA;
		private System.Windows.Forms.ComboBox cbNewKeyB;

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

		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ListViewItem listViewItem121 = new System.Windows.Forms.ListViewItem("Transport configuration");
            System.Windows.Forms.ListViewItem listViewItem122 = new System.Windows.Forms.ListViewItem("");
            System.Windows.Forms.ListViewItem listViewItem123 = new System.Windows.Forms.ListViewItem("Recommended for a Data block");
            System.Windows.Forms.ListViewItem listViewItem124 = new System.Windows.Forms.ListViewItem("Recommended for a Value block");
            System.Windows.Forms.ListViewItem listViewItem125 = new System.Windows.Forms.ListViewItem("");
            System.Windows.Forms.ListViewItem listViewItem126 = new System.Windows.Forms.ListViewItem("");
            System.Windows.Forms.ListViewItem listViewItem127 = new System.Windows.Forms.ListViewItem("No write access!");
            System.Windows.Forms.ListViewItem listViewItem128 = new System.Windows.Forms.ListViewItem("No access at all!");
            System.Windows.Forms.ListViewItem listViewItem129 = new System.Windows.Forms.ListViewItem(new string[] {
            "000",
            "Data",
            "Rd,Wr,In,De",
            "Rd,Wr,In,De"}, 0);
            System.Windows.Forms.ListViewItem listViewItem130 = new System.Windows.Forms.ListViewItem(new string[] {
            "010",
            "Data",
            "Rd",
            "Rd"}, 0);
            System.Windows.Forms.ListViewItem listViewItem131 = new System.Windows.Forms.ListViewItem(new string[] {
            "100",
            "Data",
            "Rd",
            "Rd,Wr"}, 0);
            System.Windows.Forms.ListViewItem listViewItem132 = new System.Windows.Forms.ListViewItem(new string[] {
            "110",
            "Value",
            "Rd,De",
            "Rd,Wr,In,De"}, 0);
            System.Windows.Forms.ListViewItem listViewItem133 = new System.Windows.Forms.ListViewItem(new string[] {
            "001",
            "Value",
            "Rd,De",
            "Rd,De"}, 0);
            System.Windows.Forms.ListViewItem listViewItem134 = new System.Windows.Forms.ListViewItem(new string[] {
            "011",
            "Data",
            "-",
            "Rd,Wr"}, 0);
            System.Windows.Forms.ListViewItem listViewItem135 = new System.Windows.Forms.ListViewItem(new string[] {
            "101",
            "Data",
            "-",
            "Rd"}, 0);
            System.Windows.Forms.ListViewItem listViewItem136 = new System.Windows.Forms.ListViewItem(new string[] {
            "111",
            "Data",
            "-",
            "-"}, 0);
            System.Windows.Forms.ListViewItem listViewItem137 = new System.Windows.Forms.ListViewItem(new string[] {
            "000",
            "Data",
            "Rd,Wr,In,De",
            "Rd,Wr,In,De"}, 0);
            System.Windows.Forms.ListViewItem listViewItem138 = new System.Windows.Forms.ListViewItem(new string[] {
            "010",
            "Data",
            "Rd",
            "Rd"}, 0);
            System.Windows.Forms.ListViewItem listViewItem139 = new System.Windows.Forms.ListViewItem(new string[] {
            "100",
            "Data",
            "Rd",
            "Rd,Wr"}, 0);
            System.Windows.Forms.ListViewItem listViewItem140 = new System.Windows.Forms.ListViewItem(new string[] {
            "110",
            "Value",
            "Rd,De",
            "Rd,Wr,In,De"}, 0);
            System.Windows.Forms.ListViewItem listViewItem141 = new System.Windows.Forms.ListViewItem(new string[] {
            "001",
            "Value",
            "Rd,De",
            "Rd,De"}, 0);
            System.Windows.Forms.ListViewItem listViewItem142 = new System.Windows.Forms.ListViewItem(new string[] {
            "011",
            "Data",
            "-",
            "Rd,Wr"}, 0);
            System.Windows.Forms.ListViewItem listViewItem143 = new System.Windows.Forms.ListViewItem(new string[] {
            "101",
            "Data",
            "-",
            "Rd"}, 0);
            System.Windows.Forms.ListViewItem listViewItem144 = new System.Windows.Forms.ListViewItem(new string[] {
            "111",
            "Data",
            "-",
            "-"}, 0);
            System.Windows.Forms.ListViewItem listViewItem145 = new System.Windows.Forms.ListViewItem(new string[] {
            "000",
            "Data",
            "Rd,Wr,In,De",
            "Rd,Wr,In,De"}, 0);
            System.Windows.Forms.ListViewItem listViewItem146 = new System.Windows.Forms.ListViewItem(new string[] {
            "010",
            "Data",
            "Rd",
            "Rd"}, 0);
            System.Windows.Forms.ListViewItem listViewItem147 = new System.Windows.Forms.ListViewItem(new string[] {
            "100",
            "Rd",
            "Rd,Wr",
            "Rd,Wr"}, 0);
            System.Windows.Forms.ListViewItem listViewItem148 = new System.Windows.Forms.ListViewItem(new string[] {
            "110",
            "Value",
            "Rd,De",
            "Rd,Wr,In,De"}, 0);
            System.Windows.Forms.ListViewItem listViewItem149 = new System.Windows.Forms.ListViewItem(new string[] {
            "001",
            "Value",
            "Rd,De",
            "Rd,De"}, 0);
            System.Windows.Forms.ListViewItem listViewItem150 = new System.Windows.Forms.ListViewItem(new string[] {
            "011",
            "Data",
            "-",
            "Rd,Wr"}, 0);
            System.Windows.Forms.ListViewItem listViewItem151 = new System.Windows.Forms.ListViewItem(new string[] {
            "101",
            "Data",
            "-",
            "Rd"}, 0);
            System.Windows.Forms.ListViewItem listViewItem152 = new System.Windows.Forms.ListViewItem(new string[] {
            "111",
            "Data",
            "-",
            "-"}, 0);
            System.Windows.Forms.ListViewItem listViewItem153 = new System.Windows.Forms.ListViewItem(new string[] {
            "000",
            "-",
            "A",
            "A",
            "-",
            "A",
            "A",
            "No key B"}, 0);
            System.Windows.Forms.ListViewItem listViewItem154 = new System.Windows.Forms.ListViewItem(new string[] {
            "010",
            "-",
            "-",
            "A",
            "-",
            "A",
            "-",
            "No key B"}, 0);
            System.Windows.Forms.ListViewItem listViewItem155 = new System.Windows.Forms.ListViewItem(new string[] {
            "100",
            "-",
            "B",
            "A,B",
            "-",
            "-",
            "B",
            ""}, 0);
            System.Windows.Forms.ListViewItem listViewItem156 = new System.Windows.Forms.ListViewItem(new string[] {
            "110",
            "-",
            "-",
            "A,B",
            "-",
            "-",
            "-",
            ""}, 0);
            System.Windows.Forms.ListViewItem listViewItem157 = new System.Windows.Forms.ListViewItem(new string[] {
            "001",
            "-",
            "A",
            "A",
            "A",
            "A",
            "A",
            "No key B, transport configuration"}, 0);
            System.Windows.Forms.ListViewItem listViewItem158 = new System.Windows.Forms.ListViewItem(new string[] {
            "011",
            "-",
            "B",
            "A,B",
            "B",
            "-",
            "B",
            "Recommended"}, 0);
            System.Windows.Forms.ListViewItem listViewItem159 = new System.Windows.Forms.ListViewItem(new string[] {
            "101",
            "-",
            "-",
            "A,B",
            "B",
            "-",
            "-",
            "Keys are locked"}, 0);
            System.Windows.Forms.ListViewItem listViewItem160 = new System.Windows.Forms.ListViewItem(new string[] {
            "111",
            "-",
            "-",
            "A,B",
            "-",
            "-",
            "-",
            "Trailer is locked"}, 0);
            this.grpAccessConditionDataBlocks = new System.Windows.Forms.GroupBox();
            this.lvRemarks1 = new System.Windows.Forms.ListView();
            this.columnHeader12 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lvBlock2 = new System.Windows.Forms.ListView();
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader10 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader11 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lvBlock1 = new System.Windows.Forms.ListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lvBlock0 = new System.Windows.Forms.ListView();
            this.C1C2C3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lvAccessConditions = new System.Windows.Forms.ListView();
            this.columnHeader13 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader14 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader15 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader16 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader17 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader18 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader19 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader20 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.cbNewKeyB = new System.Windows.Forms.ComboBox();
            this.cbNewKeyA = new System.Windows.Forms.ComboBox();
            this.pTop = new System.Windows.Forms.Panel();
            this.lbTitle = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.txtOldContent = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.txtOldAC = new System.Windows.Forms.TextBox();
            this.txtOldKeyB = new System.Windows.Forms.TextBox();
            this.txtOldKey = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.txtNewContent = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.txtNewAC = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.btnRead = new System.Windows.Forms.Button();
            this.btnWrite = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.grpAccessConditionDataBlocks.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.pTop.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpAccessConditionDataBlocks
            // 
            this.grpAccessConditionDataBlocks.Controls.Add(this.lvRemarks1);
            this.grpAccessConditionDataBlocks.Controls.Add(this.label3);
            this.grpAccessConditionDataBlocks.Controls.Add(this.label2);
            this.grpAccessConditionDataBlocks.Controls.Add(this.label1);
            this.grpAccessConditionDataBlocks.Controls.Add(this.lvBlock2);
            this.grpAccessConditionDataBlocks.Controls.Add(this.lvBlock1);
            this.grpAccessConditionDataBlocks.Controls.Add(this.lvBlock0);
            this.grpAccessConditionDataBlocks.Location = new System.Drawing.Point(7, 40);
            this.grpAccessConditionDataBlocks.Name = "grpAccessConditionDataBlocks";
            this.grpAccessConditionDataBlocks.Size = new System.Drawing.Size(986, 219);
            this.grpAccessConditionDataBlocks.TabIndex = 2;
            this.grpAccessConditionDataBlocks.TabStop = false;
            this.grpAccessConditionDataBlocks.Text = "Access condition for the data blocks ";
            this.grpAccessConditionDataBlocks.UseCompatibleTextRendering = true;
            // 
            // lvRemarks1
            // 
            this.lvRemarks1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader12});
            this.lvRemarks1.FullRowSelect = true;
            this.lvRemarks1.GridLines = true;
            this.lvRemarks1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvRemarks1.HideSelection = false;
            this.lvRemarks1.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem121,
            listViewItem122,
            listViewItem123,
            listViewItem124,
            listViewItem125,
            listViewItem126,
            listViewItem127,
            listViewItem128});
            this.lvRemarks1.Location = new System.Drawing.Point(784, 46);
            this.lvRemarks1.MultiSelect = false;
            this.lvRemarks1.Name = "lvRemarks1";
            this.lvRemarks1.Size = new System.Drawing.Size(193, 165);
            this.lvRemarks1.TabIndex = 10;
            this.lvRemarks1.UseCompatibleStateImageBehavior = false;
            this.lvRemarks1.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader12
            // 
            this.columnHeader12.Text = "Remark";
            this.columnHeader12.Width = 188;
            // 
            // label3
            // 
            this.label3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label3.Location = new System.Drawing.Point(524, 20);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(260, 23);
            this.label3.TabIndex = 9;
            this.label3.Text = "Bloc 2";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            this.label2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label2.Location = new System.Drawing.Point(265, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(260, 23);
            this.label2.TabIndex = 8;
            this.label2.Text = "Bloc 1";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label1.Location = new System.Drawing.Point(6, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(260, 23);
            this.label1.TabIndex = 7;
            this.label1.Text = "Bloc 0";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lvBlock2
            // 
            this.lvBlock2.CheckBoxes = true;
            this.lvBlock2.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader8,
            this.columnHeader9,
            this.columnHeader10,
            this.columnHeader11});
            this.lvBlock2.FullRowSelect = true;
            this.lvBlock2.GridLines = true;
            this.lvBlock2.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvBlock2.HideSelection = false;
            listViewItem129.StateImageIndex = 0;
            listViewItem129.Tag = "000";
            listViewItem130.StateImageIndex = 0;
            listViewItem130.Tag = "010";
            listViewItem131.StateImageIndex = 0;
            listViewItem131.Tag = "100";
            listViewItem132.StateImageIndex = 0;
            listViewItem132.Tag = "110";
            listViewItem133.StateImageIndex = 0;
            listViewItem133.Tag = "001";
            listViewItem134.StateImageIndex = 0;
            listViewItem134.Tag = "011";
            listViewItem135.StateImageIndex = 0;
            listViewItem135.Tag = "101";
            listViewItem136.StateImageIndex = 0;
            listViewItem136.Tag = "111";
            this.lvBlock2.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem129,
            listViewItem130,
            listViewItem131,
            listViewItem132,
            listViewItem133,
            listViewItem134,
            listViewItem135,
            listViewItem136});
            this.lvBlock2.Location = new System.Drawing.Point(524, 46);
            this.lvBlock2.MultiSelect = false;
            this.lvBlock2.Name = "lvBlock2";
            this.lvBlock2.Size = new System.Drawing.Size(261, 165);
            this.lvBlock2.TabIndex = 6;
            this.lvBlock2.UseCompatibleStateImageBehavior = false;
            this.lvBlock2.View = System.Windows.Forms.View.Details;
            this.lvBlock2.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.lvBlock2_ItemChecked);
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "C1,C2,C3";
            // 
            // columnHeader9
            // 
            this.columnHeader9.Text = "Type";
            this.columnHeader9.Width = 40;
            // 
            // columnHeader10
            // 
            this.columnHeader10.Text = "Key A";
            this.columnHeader10.Width = 72;
            // 
            // columnHeader11
            // 
            this.columnHeader11.Text = "Key B";
            this.columnHeader11.Width = 72;
            // 
            // lvBlock1
            // 
            this.lvBlock1.CheckBoxes = true;
            this.lvBlock1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7});
            this.lvBlock1.FullRowSelect = true;
            this.lvBlock1.GridLines = true;
            this.lvBlock1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvBlock1.HideSelection = false;
            listViewItem137.StateImageIndex = 0;
            listViewItem137.Tag = "000";
            listViewItem138.StateImageIndex = 0;
            listViewItem138.Tag = "010";
            listViewItem139.StateImageIndex = 0;
            listViewItem139.Tag = "100";
            listViewItem140.StateImageIndex = 0;
            listViewItem140.Tag = "110";
            listViewItem141.StateImageIndex = 0;
            listViewItem141.Tag = "001";
            listViewItem142.StateImageIndex = 0;
            listViewItem142.Tag = "011";
            listViewItem143.StateImageIndex = 0;
            listViewItem143.Tag = "101";
            listViewItem144.StateImageIndex = 0;
            listViewItem144.Tag = "111";
            this.lvBlock1.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem137,
            listViewItem138,
            listViewItem139,
            listViewItem140,
            listViewItem141,
            listViewItem142,
            listViewItem143,
            listViewItem144});
            this.lvBlock1.Location = new System.Drawing.Point(265, 46);
            this.lvBlock1.MultiSelect = false;
            this.lvBlock1.Name = "lvBlock1";
            this.lvBlock1.Size = new System.Drawing.Size(260, 165);
            this.lvBlock1.TabIndex = 5;
            this.lvBlock1.UseCompatibleStateImageBehavior = false;
            this.lvBlock1.View = System.Windows.Forms.View.Details;
            this.lvBlock1.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.lvBlock1_ItemChecked);
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "C1,C2,C3";
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Type";
            this.columnHeader5.Width = 40;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Key A";
            this.columnHeader6.Width = 72;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "Key B";
            this.columnHeader7.Width = 72;
            // 
            // lvBlock0
            // 
            this.lvBlock0.CheckBoxes = true;
            this.lvBlock0.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.C1C2C3,
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.lvBlock0.FullRowSelect = true;
            this.lvBlock0.GridLines = true;
            this.lvBlock0.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvBlock0.HideSelection = false;
            listViewItem145.StateImageIndex = 0;
            listViewItem145.Tag = "0000";
            listViewItem146.StateImageIndex = 0;
            listViewItem146.Tag = "010";
            listViewItem147.StateImageIndex = 0;
            listViewItem147.Tag = "100";
            listViewItem148.StateImageIndex = 0;
            listViewItem148.Tag = "110";
            listViewItem149.StateImageIndex = 0;
            listViewItem149.Tag = "001";
            listViewItem150.StateImageIndex = 0;
            listViewItem150.Tag = "011";
            listViewItem151.StateImageIndex = 0;
            listViewItem151.Tag = "101";
            listViewItem152.StateImageIndex = 0;
            listViewItem152.Tag = "111";
            this.lvBlock0.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem145,
            listViewItem146,
            listViewItem147,
            listViewItem148,
            listViewItem149,
            listViewItem150,
            listViewItem151,
            listViewItem152});
            this.lvBlock0.Location = new System.Drawing.Point(6, 46);
            this.lvBlock0.MultiSelect = false;
            this.lvBlock0.Name = "lvBlock0";
            this.lvBlock0.Size = new System.Drawing.Size(260, 165);
            this.lvBlock0.TabIndex = 4;
            this.lvBlock0.UseCompatibleStateImageBehavior = false;
            this.lvBlock0.View = System.Windows.Forms.View.Details;
            this.lvBlock0.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.LvBlock0ItemChecked);
            // 
            // C1C2C3
            // 
            this.C1C2C3.Text = "C1,C2,C3";
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Type";
            this.columnHeader1.Width = 40;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Key A";
            this.columnHeader2.Width = 72;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Key B";
            this.columnHeader3.Width = 72;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.lvAccessConditions);
            this.groupBox1.Location = new System.Drawing.Point(7, 265);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(986, 217);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Access condition for the sector trailer block itself";
            // 
            // label4
            // 
            this.label4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label4.Location = new System.Drawing.Point(318, 20);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(120, 23);
            this.label4.TabIndex = 12;
            this.label4.Text = "Access to Key B";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label5
            // 
            this.label5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label5.Location = new System.Drawing.Point(185, 20);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(134, 23);
            this.label5.TabIndex = 11;
            this.label5.Text = "Access to Access Bits";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label6
            // 
            this.label6.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label6.Location = new System.Drawing.Point(70, 20);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(116, 23);
            this.label6.TabIndex = 10;
            this.label6.Text = "Access to Key A";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lvAccessConditions
            // 
            this.lvAccessConditions.CheckBoxes = true;
            this.lvAccessConditions.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader13,
            this.columnHeader14,
            this.columnHeader15,
            this.columnHeader16,
            this.columnHeader17,
            this.columnHeader18,
            this.columnHeader19,
            this.columnHeader20});
            this.lvAccessConditions.FullRowSelect = true;
            this.lvAccessConditions.GridLines = true;
            this.lvAccessConditions.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvAccessConditions.HideSelection = false;
            listViewItem153.StateImageIndex = 0;
            listViewItem153.Tag = "0000";
            listViewItem154.StateImageIndex = 0;
            listViewItem154.Tag = "010";
            listViewItem155.StateImageIndex = 0;
            listViewItem155.Tag = "100";
            listViewItem156.StateImageIndex = 0;
            listViewItem156.Tag = "110";
            listViewItem157.StateImageIndex = 0;
            listViewItem157.Tag = "001";
            listViewItem158.StateImageIndex = 0;
            listViewItem158.Tag = "011";
            listViewItem159.StateImageIndex = 0;
            listViewItem159.Tag = "101";
            listViewItem160.StateImageIndex = 0;
            listViewItem160.Tag = "111";
            this.lvAccessConditions.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem153,
            listViewItem154,
            listViewItem155,
            listViewItem156,
            listViewItem157,
            listViewItem158,
            listViewItem159,
            listViewItem160});
            this.lvAccessConditions.Location = new System.Drawing.Point(6, 46);
            this.lvAccessConditions.MultiSelect = false;
            this.lvAccessConditions.Name = "lvAccessConditions";
            this.lvAccessConditions.Size = new System.Drawing.Size(633, 165);
            this.lvAccessConditions.TabIndex = 5;
            this.lvAccessConditions.UseCompatibleStateImageBehavior = false;
            this.lvAccessConditions.View = System.Windows.Forms.View.Details;
            this.lvAccessConditions.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.lvAccessConditions_ItemChecked);
            // 
            // columnHeader13
            // 
            this.columnHeader13.Text = "C1,C2,C3";
            // 
            // columnHeader14
            // 
            this.columnHeader14.Text = "Rd";
            this.columnHeader14.Width = 46;
            // 
            // columnHeader15
            // 
            this.columnHeader15.Text = "Wr";
            this.columnHeader15.Width = 72;
            // 
            // columnHeader16
            // 
            this.columnHeader16.Text = "Re";
            this.columnHeader16.Width = 72;
            // 
            // columnHeader17
            // 
            this.columnHeader17.Text = "Wr";
            // 
            // columnHeader18
            // 
            this.columnHeader18.Text = "Re";
            // 
            // columnHeader19
            // 
            this.columnHeader19.Text = "Wr";
            // 
            // columnHeader20
            // 
            this.columnHeader20.Text = "Remark";
            this.columnHeader20.Width = 190;
            // 
            // cbNewKeyB
            // 
            this.cbNewKeyB.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbNewKeyB.FormattingEnabled = true;
            this.cbNewKeyB.Location = new System.Drawing.Point(427, 27);
            this.cbNewKeyB.MaxLength = 12;
            this.cbNewKeyB.Name = "cbNewKeyB";
            this.cbNewKeyB.Size = new System.Drawing.Size(135, 21);
            this.cbNewKeyB.TabIndex = 12;
            this.cbNewKeyB.SelectedIndexChanged += new System.EventHandler(this.CbKeyBSelectedIndexChanged);
            this.cbNewKeyB.KeyUp += new System.Windows.Forms.KeyEventHandler(this.CbKeyBKeyUp);
            // 
            // cbNewKeyA
            // 
            this.cbNewKeyA.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbNewKeyA.FormattingEnabled = true;
            this.cbNewKeyA.Location = new System.Drawing.Point(54, 27);
            this.cbNewKeyA.MaxLength = 12;
            this.cbNewKeyA.Name = "cbNewKeyA";
            this.cbNewKeyA.Size = new System.Drawing.Size(135, 21);
            this.cbNewKeyA.TabIndex = 11;
            this.cbNewKeyA.SelectedIndexChanged += new System.EventHandler(this.CbKeyASelectedIndexChanged);
            this.cbNewKeyA.KeyUp += new System.Windows.Forms.KeyEventHandler(this.CbKeyAKeyUp);
            // 
            // pTop
            // 
            this.pTop.Controls.Add(this.lbTitle);
            this.pTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pTop.Location = new System.Drawing.Point(0, 0);
            this.pTop.Name = "pTop";
            this.pTop.Size = new System.Drawing.Size(1001, 34);
            this.pTop.TabIndex = 5;
            // 
            // lbTitle
            // 
            this.lbTitle.AutoSize = true;
            this.lbTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbTitle.Location = new System.Drawing.Point(12, 9);
            this.lbTitle.Name = "lbTitle";
            this.lbTitle.Size = new System.Drawing.Size(172, 13);
            this.lbTitle.TabIndex = 0;
            this.lbTitle.Text = "Trailer of sector ... (block ...)";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnRead);
            this.groupBox3.Controls.Add(this.txtOldContent);
            this.groupBox3.Controls.Add(this.label15);
            this.groupBox3.Controls.Add(this.txtOldAC);
            this.groupBox3.Controls.Add(this.txtOldKeyB);
            this.groupBox3.Controls.Add(this.txtOldKey);
            this.groupBox3.Controls.Add(this.label14);
            this.groupBox3.Controls.Add(this.label12);
            this.groupBox3.Controls.Add(this.label13);
            this.groupBox3.Location = new System.Drawing.Point(7, 488);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(986, 82);
            this.groupBox3.TabIndex = 6;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Current settings";
            // 
            // txtOldContent
            // 
            this.txtOldContent.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtOldContent.Location = new System.Drawing.Point(93, 53);
            this.txtOldContent.Name = "txtOldContent";
            this.txtOldContent.ReadOnly = true;
            this.txtOldContent.Size = new System.Drawing.Size(469, 20);
            this.txtOldContent.TabIndex = 21;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.Location = new System.Drawing.Point(10, 56);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(77, 13);
            this.label15.TabIndex = 20;
            this.label15.Text = "Complete bloc:";
            // 
            // txtOldAC
            // 
            this.txtOldAC.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtOldAC.Location = new System.Drawing.Point(281, 26);
            this.txtOldAC.Name = "txtOldAC";
            this.txtOldAC.ReadOnly = true;
            this.txtOldAC.Size = new System.Drawing.Size(85, 20);
            this.txtOldAC.TabIndex = 19;
            // 
            // txtOldKeyB
            // 
            this.txtOldKeyB.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtOldKeyB.Location = new System.Drawing.Point(427, 26);
            this.txtOldKeyB.Name = "txtOldKeyB";
            this.txtOldKeyB.ReadOnly = true;
            this.txtOldKeyB.Size = new System.Drawing.Size(135, 20);
            this.txtOldKeyB.TabIndex = 18;
            // 
            // txtOldKey
            // 
            this.txtOldKey.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtOldKey.Location = new System.Drawing.Point(54, 26);
            this.txtOldKey.Name = "txtOldKey";
            this.txtOldKey.ReadOnly = true;
            this.txtOldKey.Size = new System.Drawing.Size(135, 20);
            this.txtOldKey.TabIndex = 17;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.Location = new System.Drawing.Point(211, 29);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(64, 13);
            this.label14.TabIndex = 16;
            this.label14.Text = "Access bits:";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(383, 29);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(38, 13);
            this.label12.TabIndex = 15;
            this.label12.Text = "Key B:";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(10, 29);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(38, 13);
            this.label13.TabIndex = 14;
            this.label13.Text = "Key A:";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.btnWrite);
            this.groupBox4.Controls.Add(this.txtNewContent);
            this.groupBox4.Controls.Add(this.label16);
            this.groupBox4.Controls.Add(this.txtNewAC);
            this.groupBox4.Controls.Add(this.label17);
            this.groupBox4.Controls.Add(this.label18);
            this.groupBox4.Controls.Add(this.label19);
            this.groupBox4.Controls.Add(this.cbNewKeyB);
            this.groupBox4.Controls.Add(this.cbNewKeyA);
            this.groupBox4.Location = new System.Drawing.Point(7, 578);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(986, 82);
            this.groupBox4.TabIndex = 22;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "New settings";
            // 
            // txtNewContent
            // 
            this.txtNewContent.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtNewContent.Location = new System.Drawing.Point(93, 53);
            this.txtNewContent.Name = "txtNewContent";
            this.txtNewContent.Size = new System.Drawing.Size(469, 20);
            this.txtNewContent.TabIndex = 21;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label16.Location = new System.Drawing.Point(10, 56);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(77, 13);
            this.label16.TabIndex = 20;
            this.label16.Text = "Complete bloc:";
            // 
            // txtNewAC
            // 
            this.txtNewAC.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtNewAC.Location = new System.Drawing.Point(281, 26);
            this.txtNewAC.Name = "txtNewAC";
            this.txtNewAC.Size = new System.Drawing.Size(85, 20);
            this.txtNewAC.TabIndex = 19;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label17.Location = new System.Drawing.Point(211, 29);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(64, 13);
            this.label17.TabIndex = 16;
            this.label17.Text = "Access bits:";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label18.Location = new System.Drawing.Point(383, 29);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(38, 13);
            this.label18.TabIndex = 15;
            this.label18.Text = "Key B:";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label19.Location = new System.Drawing.Point(10, 29);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(38, 13);
            this.label19.TabIndex = 14;
            this.label19.Text = "Key A:";
            // 
            // btnRead
            // 
            this.btnRead.Location = new System.Drawing.Point(902, 50);
            this.btnRead.Name = "btnRead";
            this.btnRead.Size = new System.Drawing.Size(75, 23);
            this.btnRead.TabIndex = 22;
            this.btnRead.Text = "Read again";
            this.btnRead.UseVisualStyleBackColor = true;
            this.btnRead.Click += new System.EventHandler(this.btnRead_Click);
            // 
            // btnWrite
            // 
            this.btnWrite.Location = new System.Drawing.Point(902, 50);
            this.btnWrite.Name = "btnWrite";
            this.btnWrite.Size = new System.Drawing.Size(75, 23);
            this.btnWrite.TabIndex = 23;
            this.btnWrite.Text = "Write";
            this.btnWrite.UseVisualStyleBackColor = true;
            this.btnWrite.Click += new System.EventHandler(this.btnWrite_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(909, 666);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 23;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // MifareSectorTrailerForm
            // 
            this.ClientSize = new System.Drawing.Size(1001, 696);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.pTop);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.grpAccessConditionDataBlocks);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MifareSectorTrailerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Mifare Classic Sector Trailer";
            this.Shown += new System.EventHandler(this.MifareSectorTrailerDefinitionFormShown);
            this.grpAccessConditionDataBlocks.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.pTop.ResumeLayout(false);
            this.pTop.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);

		}

        private System.Windows.Forms.Panel pTop;
        private System.Windows.Forms.Label lbTitle;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox txtOldAC;
        private System.Windows.Forms.TextBox txtOldKeyB;
        private System.Windows.Forms.TextBox txtOldKey;
        private System.Windows.Forms.TextBox txtOldContent;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox txtNewContent;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TextBox txtNewAC;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Button btnRead;
        private System.Windows.Forms.Button btnWrite;
        private System.Windows.Forms.Button btnClose;
    }
}
