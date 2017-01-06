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
	partial class MifareSectorTrailerDefinitionForm
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Button btnCancel;
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
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.ComboBox cbKeyA;
		private System.Windows.Forms.ComboBox cbKeyB;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.TextBox txtAccessBits;
		private System.Windows.Forms.TextBox txtSectorTrailer;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label11;

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
			System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("Transport configuration");
			System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem("");
			System.Windows.Forms.ListViewItem listViewItem3 = new System.Windows.Forms.ListViewItem("Recommended for a Data block");
			System.Windows.Forms.ListViewItem listViewItem4 = new System.Windows.Forms.ListViewItem("Recommended for a Value block");
			System.Windows.Forms.ListViewItem listViewItem5 = new System.Windows.Forms.ListViewItem("");
			System.Windows.Forms.ListViewItem listViewItem6 = new System.Windows.Forms.ListViewItem("");
			System.Windows.Forms.ListViewItem listViewItem7 = new System.Windows.Forms.ListViewItem("No write access!");
			System.Windows.Forms.ListViewItem listViewItem8 = new System.Windows.Forms.ListViewItem("No access at all!");
			System.Windows.Forms.ListViewItem listViewItem9 = new System.Windows.Forms.ListViewItem(new string[] {
			"000",
			"Data",
			"Rd,Wr,In,De",
			"Rd,Wr,In,De"}, 0);
			System.Windows.Forms.ListViewItem listViewItem10 = new System.Windows.Forms.ListViewItem(new string[] {
			"010",
			"Data",
			"Rd",
			"Rd"}, 0);
			System.Windows.Forms.ListViewItem listViewItem11 = new System.Windows.Forms.ListViewItem(new string[] {
			"100",
			"Data",
			"Rd",
			"Rd,Wr"}, 0);
			System.Windows.Forms.ListViewItem listViewItem12 = new System.Windows.Forms.ListViewItem(new string[] {
			"110",
			"Value",
			"Rd,De",
			"Rd,Wr,In,De"}, 0);
			System.Windows.Forms.ListViewItem listViewItem13 = new System.Windows.Forms.ListViewItem(new string[] {
			"001",
			"Value",
			"Rd,De",
			"Rd,De"}, 0);
			System.Windows.Forms.ListViewItem listViewItem14 = new System.Windows.Forms.ListViewItem(new string[] {
			"011",
			"Data",
			"-",
			"Rd,Wr"}, 0);
			System.Windows.Forms.ListViewItem listViewItem15 = new System.Windows.Forms.ListViewItem(new string[] {
			"101",
			"Data",
			"-",
			"Rd"}, 0);
			System.Windows.Forms.ListViewItem listViewItem16 = new System.Windows.Forms.ListViewItem(new string[] {
			"111",
			"Data",
			"-",
			"-"}, 0);
			System.Windows.Forms.ListViewItem listViewItem17 = new System.Windows.Forms.ListViewItem(new string[] {
			"000",
			"Data",
			"Rd,Wr,In,De",
			"Rd,Wr,In,De"}, 0);
			System.Windows.Forms.ListViewItem listViewItem18 = new System.Windows.Forms.ListViewItem(new string[] {
			"010",
			"Data",
			"Rd",
			"Rd"}, 0);
			System.Windows.Forms.ListViewItem listViewItem19 = new System.Windows.Forms.ListViewItem(new string[] {
			"100",
			"Data",
			"Rd",
			"Rd,Wr"}, 0);
			System.Windows.Forms.ListViewItem listViewItem20 = new System.Windows.Forms.ListViewItem(new string[] {
			"110",
			"Value",
			"Rd,De",
			"Rd,Wr,In,De"}, 0);
			System.Windows.Forms.ListViewItem listViewItem21 = new System.Windows.Forms.ListViewItem(new string[] {
			"001",
			"Value",
			"Rd,De",
			"Rd,De"}, 0);
			System.Windows.Forms.ListViewItem listViewItem22 = new System.Windows.Forms.ListViewItem(new string[] {
			"011",
			"Data",
			"-",
			"Rd,Wr"}, 0);
			System.Windows.Forms.ListViewItem listViewItem23 = new System.Windows.Forms.ListViewItem(new string[] {
			"101",
			"Data",
			"-",
			"Rd"}, 0);
			System.Windows.Forms.ListViewItem listViewItem24 = new System.Windows.Forms.ListViewItem(new string[] {
			"111",
			"Data",
			"-",
			"-"}, 0);
			System.Windows.Forms.ListViewItem listViewItem25 = new System.Windows.Forms.ListViewItem(new string[] {
			"000",
			"Data",
			"Rd,Wr,In,De",
			"Rd,Wr,In,De"}, 0);
			System.Windows.Forms.ListViewItem listViewItem26 = new System.Windows.Forms.ListViewItem(new string[] {
			"010",
			"Data",
			"Rd",
			"Rd"}, 0);
			System.Windows.Forms.ListViewItem listViewItem27 = new System.Windows.Forms.ListViewItem(new string[] {
			"100",
			"Rd",
			"Rd,Wr",
			"Rd,Wr"}, 0);
			System.Windows.Forms.ListViewItem listViewItem28 = new System.Windows.Forms.ListViewItem(new string[] {
			"110",
			"Value",
			"Rd,De",
			"Rd,Wr,In,De"}, 0);
			System.Windows.Forms.ListViewItem listViewItem29 = new System.Windows.Forms.ListViewItem(new string[] {
			"001",
			"Value",
			"Rd,De",
			"Rd,De"}, 0);
			System.Windows.Forms.ListViewItem listViewItem30 = new System.Windows.Forms.ListViewItem(new string[] {
			"011",
			"Data",
			"-",
			"Rd,Wr"}, 0);
			System.Windows.Forms.ListViewItem listViewItem31 = new System.Windows.Forms.ListViewItem(new string[] {
			"101",
			"Data",
			"-",
			"Rd"}, 0);
			System.Windows.Forms.ListViewItem listViewItem32 = new System.Windows.Forms.ListViewItem(new string[] {
			"111",
			"Data",
			"-",
			"-"}, 0);
			System.Windows.Forms.ListViewItem listViewItem33 = new System.Windows.Forms.ListViewItem(new string[] {
			"000",
			"-",
			"A",
			"A",
			"-",
			"A",
			"A",
			"No key B"}, 0);
			System.Windows.Forms.ListViewItem listViewItem34 = new System.Windows.Forms.ListViewItem(new string[] {
			"010",
			"-",
			"-",
			"A",
			"-",
			"A",
			"-",
			"No key B"}, 0);
			System.Windows.Forms.ListViewItem listViewItem35 = new System.Windows.Forms.ListViewItem(new string[] {
			"100",
			"-",
			"B",
			"A,B",
			"-",
			"-",
			"B",
			""}, 0);
			System.Windows.Forms.ListViewItem listViewItem36 = new System.Windows.Forms.ListViewItem(new string[] {
			"110",
			"-",
			"-",
			"A,B",
			"-",
			"-",
			"-",
			""}, 0);
			System.Windows.Forms.ListViewItem listViewItem37 = new System.Windows.Forms.ListViewItem(new string[] {
			"001",
			"-",
			"A",
			"A",
			"A",
			"A",
			"A",
			"No key B, transport configuration"}, 0);
			System.Windows.Forms.ListViewItem listViewItem38 = new System.Windows.Forms.ListViewItem(new string[] {
			"011",
			"-",
			"B",
			"A,B",
			"B",
			"-",
			"B",
			"Recommended"}, 0);
			System.Windows.Forms.ListViewItem listViewItem39 = new System.Windows.Forms.ListViewItem(new string[] {
			"101",
			"-",
			"-",
			"A,B",
			"B",
			"-",
			"-",
			"Keys are locked"}, 0);
			System.Windows.Forms.ListViewItem listViewItem40 = new System.Windows.Forms.ListViewItem(new string[] {
			"111",
			"-",
			"-",
			"A,B",
			"-",
			"-",
			"-",
			"Trailer is locked"}, 0);
			this.btnOk = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.grpAccessConditionDataBlocks = new System.Windows.Forms.GroupBox();
			this.lvRemarks1 = new System.Windows.Forms.ListView();
			this.columnHeader12 = new System.Windows.Forms.ColumnHeader();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.lvBlock2 = new System.Windows.Forms.ListView();
			this.columnHeader8 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader9 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader10 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader11 = new System.Windows.Forms.ColumnHeader();
			this.lvBlock1 = new System.Windows.Forms.ListView();
			this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader6 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader7 = new System.Windows.Forms.ColumnHeader();
			this.lvBlock0 = new System.Windows.Forms.ListView();
			this.C1C2C3 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.lvAccessConditions = new System.Windows.Forms.ListView();
			this.columnHeader13 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader14 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader15 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader16 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader17 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader18 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader19 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader20 = new System.Windows.Forms.ColumnHeader();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.txtAccessBits = new System.Windows.Forms.TextBox();
			this.txtSectorTrailer = new System.Windows.Forms.TextBox();
			this.label10 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.cbKeyB = new System.Windows.Forms.ComboBox();
			this.label7 = new System.Windows.Forms.Label();
			this.cbKeyA = new System.Windows.Forms.ComboBox();
			this.grpAccessConditionDataBlocks.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnOk
			// 
			this.btnOk.Location = new System.Drawing.Point(907, 452);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(75, 23);
			this.btnOk.TabIndex = 0;
			this.btnOk.Text = "Ok";
			this.btnOk.UseVisualStyleBackColor = true;
			this.btnOk.Click += new System.EventHandler(this.BtnOkClick);
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(826, 452);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 1;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.BtnCancelClick);
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
			this.grpAccessConditionDataBlocks.Location = new System.Drawing.Point(5, 4);
			this.grpAccessConditionDataBlocks.Name = "grpAccessConditionDataBlocks";
			this.grpAccessConditionDataBlocks.Size = new System.Drawing.Size(986, 219);
			this.grpAccessConditionDataBlocks.TabIndex = 2;
			this.grpAccessConditionDataBlocks.TabStop = false;
			this.grpAccessConditionDataBlocks.Text = "Access condition for data blocks ";
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
			listViewItem1,
			listViewItem2,
			listViewItem3,
			listViewItem4,
			listViewItem5,
			listViewItem6,
			listViewItem7,
			listViewItem8});
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
			listViewItem9.StateImageIndex = 0;
			listViewItem9.Tag = "000";
			listViewItem10.StateImageIndex = 0;
			listViewItem10.Tag = "010";
			listViewItem11.StateImageIndex = 0;
			listViewItem11.Tag = "100";
			listViewItem12.StateImageIndex = 0;
			listViewItem12.Tag = "110";
			listViewItem13.StateImageIndex = 0;
			listViewItem13.Tag = "001";
			listViewItem14.StateImageIndex = 0;
			listViewItem14.Tag = "011";
			listViewItem15.StateImageIndex = 0;
			listViewItem15.Tag = "101";
			listViewItem16.StateImageIndex = 0;
			listViewItem16.Tag = "111";
			this.lvBlock2.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
			listViewItem9,
			listViewItem10,
			listViewItem11,
			listViewItem12,
			listViewItem13,
			listViewItem14,
			listViewItem15,
			listViewItem16});
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
			listViewItem17.StateImageIndex = 0;
			listViewItem17.Tag = "000";
			listViewItem18.StateImageIndex = 0;
			listViewItem18.Tag = "010";
			listViewItem19.StateImageIndex = 0;
			listViewItem19.Tag = "100";
			listViewItem20.StateImageIndex = 0;
			listViewItem20.Tag = "110";
			listViewItem21.StateImageIndex = 0;
			listViewItem21.Tag = "001";
			listViewItem22.StateImageIndex = 0;
			listViewItem22.Tag = "011";
			listViewItem23.StateImageIndex = 0;
			listViewItem23.Tag = "101";
			listViewItem24.StateImageIndex = 0;
			listViewItem24.Tag = "111";
			this.lvBlock1.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
			listViewItem17,
			listViewItem18,
			listViewItem19,
			listViewItem20,
			listViewItem21,
			listViewItem22,
			listViewItem23,
			listViewItem24});
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
			listViewItem25.StateImageIndex = 0;
			listViewItem25.Tag = "0000";
			listViewItem26.StateImageIndex = 0;
			listViewItem26.Tag = "010";
			listViewItem27.StateImageIndex = 0;
			listViewItem27.Tag = "100";
			listViewItem28.StateImageIndex = 0;
			listViewItem28.Tag = "110";
			listViewItem29.StateImageIndex = 0;
			listViewItem29.Tag = "001";
			listViewItem30.StateImageIndex = 0;
			listViewItem30.Tag = "011";
			listViewItem31.StateImageIndex = 0;
			listViewItem31.Tag = "101";
			listViewItem32.StateImageIndex = 0;
			listViewItem32.Tag = "111";
			this.lvBlock0.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
			listViewItem25,
			listViewItem26,
			listViewItem27,
			listViewItem28,
			listViewItem29,
			listViewItem30,
			listViewItem31,
			listViewItem32});
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
			this.groupBox1.Location = new System.Drawing.Point(5, 229);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(644, 217);
			this.groupBox1.TabIndex = 3;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Access condition for sector trailer ";
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
			listViewItem33.StateImageIndex = 0;
			listViewItem33.Tag = "0000";
			listViewItem34.StateImageIndex = 0;
			listViewItem34.Tag = "010";
			listViewItem35.StateImageIndex = 0;
			listViewItem35.Tag = "100";
			listViewItem36.StateImageIndex = 0;
			listViewItem36.Tag = "110";
			listViewItem37.StateImageIndex = 0;
			listViewItem37.Tag = "001";
			listViewItem38.StateImageIndex = 0;
			listViewItem38.Tag = "011";
			listViewItem39.StateImageIndex = 0;
			listViewItem39.Tag = "101";
			listViewItem40.StateImageIndex = 0;
			listViewItem40.Tag = "111";
			this.lvAccessConditions.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
			listViewItem33,
			listViewItem34,
			listViewItem35,
			listViewItem36,
			listViewItem37,
			listViewItem38,
			listViewItem39,
			listViewItem40});
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
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.txtAccessBits);
			this.groupBox2.Controls.Add(this.txtSectorTrailer);
			this.groupBox2.Controls.Add(this.label10);
			this.groupBox2.Controls.Add(this.label11);
			this.groupBox2.Controls.Add(this.label9);
			this.groupBox2.Controls.Add(this.label8);
			this.groupBox2.Controls.Add(this.cbKeyB);
			this.groupBox2.Controls.Add(this.label7);
			this.groupBox2.Controls.Add(this.cbKeyA);
			this.groupBox2.Location = new System.Drawing.Point(655, 229);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(336, 217);
			this.groupBox2.TabIndex = 4;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Keys && calculated content of sector trailer ";
			// 
			// txtAccessBits
			// 
			this.txtAccessBits.Location = new System.Drawing.Point(111, 126);
			this.txtAccessBits.Name = "txtAccessBits";
			this.txtAccessBits.ReadOnly = true;
			this.txtAccessBits.Size = new System.Drawing.Size(216, 20);
			this.txtAccessBits.TabIndex = 18;
			// 
			// txtSectorTrailer
			// 
			this.txtSectorTrailer.Location = new System.Drawing.Point(111, 154);
			this.txtSectorTrailer.Name = "txtSectorTrailer";
			this.txtSectorTrailer.ReadOnly = true;
			this.txtSectorTrailer.Size = new System.Drawing.Size(216, 20);
			this.txtSectorTrailer.TabIndex = 17;
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label10.Location = new System.Drawing.Point(6, 156);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(98, 13);
			this.label10.TabIndex = 16;
			this.label10.Text = "Complete block:";
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label11.Location = new System.Drawing.Point(6, 129);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(76, 13);
			this.label11.TabIndex = 15;
			this.label11.Text = "Access bits:";
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label9.Location = new System.Drawing.Point(29, 94);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(252, 13);
			this.label9.TabIndex = 14;
			this.label9.Text = "**** Calculated content of sector trailer ****";
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label8.Location = new System.Drawing.Point(6, 52);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(38, 13);
			this.label8.TabIndex = 13;
			this.label8.Text = "Key B:";
			// 
			// cbKeyB
			// 
			this.cbKeyB.FormattingEnabled = true;
			this.cbKeyB.Location = new System.Drawing.Point(111, 49);
			this.cbKeyB.MaxLength = 12;
			this.cbKeyB.Name = "cbKeyB";
			this.cbKeyB.Size = new System.Drawing.Size(168, 21);
			this.cbKeyB.TabIndex = 12;
			this.cbKeyB.SelectedIndexChanged += new System.EventHandler(this.CbKeyBSelectedIndexChanged);
			this.cbKeyB.KeyUp += new System.Windows.Forms.KeyEventHandler(this.CbKeyBKeyUp);
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label7.Location = new System.Drawing.Point(6, 25);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(38, 13);
			this.label7.TabIndex = 0;
			this.label7.Text = "Key A:";
			// 
			// cbKeyA
			// 
			this.cbKeyA.FormattingEnabled = true;
			this.cbKeyA.Location = new System.Drawing.Point(111, 22);
			this.cbKeyA.MaxLength = 12;
			this.cbKeyA.Name = "cbKeyA";
			this.cbKeyA.Size = new System.Drawing.Size(168, 21);
			this.cbKeyA.TabIndex = 11;
			this.cbKeyA.SelectedIndexChanged += new System.EventHandler(this.CbKeyASelectedIndexChanged);
			this.cbKeyA.KeyUp += new System.Windows.Forms.KeyEventHandler(this.CbKeyAKeyUp);
			// 
			// MifareSectorTrailerDefinitionForm
			// 
			this.AcceptButton = this.btnOk;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(995, 480);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.grpAccessConditionDataBlocks);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOk);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MifareSectorTrailerDefinitionForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Mifare Classic sector trailer definition";
			this.Shown += new System.EventHandler(this.MifareSectorTrailerDefinitionFormShown);
			this.grpAccessConditionDataBlocks.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.ResumeLayout(false);

		}
	}
}
