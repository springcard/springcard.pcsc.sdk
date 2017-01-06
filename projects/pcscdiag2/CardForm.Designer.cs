/*
 * Crée par SharpDevelop.
 * Utilisateur: johann
 * Date: 19/03/2012
 * Heure: 14:16
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
namespace PcscDiag2
{
  partial class CardForm
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
    	System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CardForm));
    	this.pTop = new System.Windows.Forms.Panel();
    	this.eMode = new System.Windows.Forms.TextBox();
    	this.label29 = new System.Windows.Forms.Label();
    	this.eProtocol = new System.Windows.Forms.TextBox();
    	this.label28 = new System.Windows.Forms.Label();
    	this.eCardAtr = new System.Windows.Forms.TextBox();
    	this.label10 = new System.Windows.Forms.Label();
    	this.pRight = new System.Windows.Forms.Panel();
    	this.btnClose = new System.Windows.Forms.Button();
    	this.btnDisconnect = new System.Windows.Forms.Button();
    	this.btnReconnect = new System.Windows.Forms.Button();
    	this.btnConnect = new System.Windows.Forms.Button();
    	this.btnControl = new System.Windows.Forms.Button();
    	this.btnTransmit = new System.Windows.Forms.Button();
    	this.pMain = new System.Windows.Forms.Panel();
    	this.tabPages = new System.Windows.Forms.TabControl();
    	this.tabPageTransmit = new System.Windows.Forms.TabPage();
    	this.eTransmitErrorExplain = new System.Windows.Forms.TextBox();
    	this.eTransmitError = new System.Windows.Forms.TextBox();
    	this.label9 = new System.Windows.Forms.Label();
    	this.label23 = new System.Windows.Forms.Label();
    	this.eStatusWordExplain = new System.Windows.Forms.TextBox();
    	this.eStatusWord = new System.Windows.Forms.TextBox();
    	this.label21 = new System.Windows.Forms.Label();
    	this.label22 = new System.Windows.Forms.Label();
    	this.label20 = new System.Windows.Forms.Label();
    	this.label19 = new System.Windows.Forms.Label();
    	this.btnCApduBookmark = new System.Windows.Forms.Button();
    	this.btnCApduNext = new System.Windows.Forms.Button();
    	this.btnCApduPrev = new System.Windows.Forms.Button();
    	this.btnCApduClear = new System.Windows.Forms.Button();
    	this.label5 = new System.Windows.Forms.Label();
    	this.label6 = new System.Windows.Forms.Label();
    	this.label7 = new System.Windows.Forms.Label();
    	this.label8 = new System.Windows.Forms.Label();
    	this.label4 = new System.Windows.Forms.Label();
    	this.label3 = new System.Windows.Forms.Label();
    	this.label2 = new System.Windows.Forms.Label();
    	this.label1 = new System.Windows.Forms.Label();
    	this.hexBoxRApdu = new Be.Windows.Forms.HexBox();
    	this.hexBoxCApdu = new Be.Windows.Forms.HexBox();
    	this.tabPageControl = new System.Windows.Forms.TabPage();
    	this.eControlErrorExplain = new System.Windows.Forms.TextBox();
    	this.eControlError = new System.Windows.Forms.TextBox();
    	this.label24 = new System.Windows.Forms.Label();
    	this.label25 = new System.Windows.Forms.Label();
    	this.eResultByteExplain = new System.Windows.Forms.TextBox();
    	this.eResultByte = new System.Windows.Forms.TextBox();
    	this.label26 = new System.Windows.Forms.Label();
    	this.label27 = new System.Windows.Forms.Label();
    	this.btnCtrlBookmark = new System.Windows.Forms.Button();
    	this.btnCCtrlNext = new System.Windows.Forms.Button();
    	this.btnCCtrlPrev = new System.Windows.Forms.Button();
    	this.btnCtrlClear = new System.Windows.Forms.Button();
    	this.label11 = new System.Windows.Forms.Label();
    	this.label12 = new System.Windows.Forms.Label();
    	this.label13 = new System.Windows.Forms.Label();
    	this.label14 = new System.Windows.Forms.Label();
    	this.label15 = new System.Windows.Forms.Label();
    	this.label16 = new System.Windows.Forms.Label();
    	this.label17 = new System.Windows.Forms.Label();
    	this.label18 = new System.Windows.Forms.Label();
    	this.hexBoxRCtrl = new Be.Windows.Forms.HexBox();
    	this.hexBoxCCtrl = new Be.Windows.Forms.HexBox();
    	this.pTop.SuspendLayout();
    	this.pRight.SuspendLayout();
    	this.pMain.SuspendLayout();
    	this.tabPages.SuspendLayout();
    	this.tabPageTransmit.SuspendLayout();
    	this.tabPageControl.SuspendLayout();
    	this.SuspendLayout();
    	// 
    	// pTop
    	// 
    	this.pTop.BackColor = System.Drawing.SystemColors.Window;
    	this.pTop.Controls.Add(this.eMode);
    	this.pTop.Controls.Add(this.label29);
    	this.pTop.Controls.Add(this.eProtocol);
    	this.pTop.Controls.Add(this.label28);
    	this.pTop.Controls.Add(this.eCardAtr);
    	this.pTop.Controls.Add(this.label10);
    	this.pTop.Dock = System.Windows.Forms.DockStyle.Top;
    	this.pTop.Location = new System.Drawing.Point(0, 0);
    	this.pTop.Name = "pTop";
    	this.pTop.Size = new System.Drawing.Size(794, 43);
    	this.pTop.TabIndex = 0;
    	// 
    	// eMode
    	// 
    	this.eMode.BackColor = System.Drawing.SystemColors.Window;
    	this.eMode.Font = new System.Drawing.Font("Consolas", 8.25F);
    	this.eMode.Location = new System.Drawing.Point(691, 11);
    	this.eMode.Name = "eMode";
    	this.eMode.ReadOnly = true;
    	this.eMode.Size = new System.Drawing.Size(81, 20);
    	this.eMode.TabIndex = 7;
    	// 
    	// label29
    	// 
    	this.label29.AutoSize = true;
    	this.label29.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    	this.label29.ForeColor = System.Drawing.SystemColors.GrayText;
    	this.label29.Location = new System.Drawing.Point(648, 15);
    	this.label29.Name = "label29";
    	this.label29.Size = new System.Drawing.Size(41, 14);
    	this.label29.TabIndex = 6;
    	this.label29.Text = "Mode:";
    	// 
    	// eProtocol
    	// 
    	this.eProtocol.BackColor = System.Drawing.SystemColors.Window;
    	this.eProtocol.Font = new System.Drawing.Font("Consolas", 8.25F);
    	this.eProtocol.Location = new System.Drawing.Point(573, 11);
    	this.eProtocol.Name = "eProtocol";
    	this.eProtocol.ReadOnly = true;
    	this.eProtocol.Size = new System.Drawing.Size(69, 20);
    	this.eProtocol.TabIndex = 5;
    	// 
    	// label28
    	// 
    	this.label28.AutoSize = true;
    	this.label28.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    	this.label28.ForeColor = System.Drawing.SystemColors.GrayText;
    	this.label28.Location = new System.Drawing.Point(518, 15);
    	this.label28.Name = "label28";
    	this.label28.Size = new System.Drawing.Size(54, 14);
    	this.label28.TabIndex = 4;
    	this.label28.Text = "Protocol:";
    	// 
    	// eCardAtr
    	// 
    	this.eCardAtr.BackColor = System.Drawing.SystemColors.Window;
    	this.eCardAtr.Font = new System.Drawing.Font("Consolas", 8.25F);
    	this.eCardAtr.Location = new System.Drawing.Point(78, 11);
    	this.eCardAtr.Name = "eCardAtr";
    	this.eCardAtr.ReadOnly = true;
    	this.eCardAtr.Size = new System.Drawing.Size(434, 20);
    	this.eCardAtr.TabIndex = 3;
    	// 
    	// label10
    	// 
    	this.label10.AutoSize = true;
    	this.label10.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    	this.label10.ForeColor = System.Drawing.SystemColors.GrayText;
    	this.label10.Location = new System.Drawing.Point(15, 15);
    	this.label10.Name = "label10";
    	this.label10.Size = new System.Drawing.Size(56, 14);
    	this.label10.TabIndex = 2;
    	this.label10.Text = "Card ATR:";
    	// 
    	// pRight
    	// 
    	this.pRight.Controls.Add(this.btnClose);
    	this.pRight.Controls.Add(this.btnDisconnect);
    	this.pRight.Controls.Add(this.btnReconnect);
    	this.pRight.Controls.Add(this.btnConnect);
    	this.pRight.Controls.Add(this.btnControl);
    	this.pRight.Controls.Add(this.btnTransmit);
    	this.pRight.Dock = System.Windows.Forms.DockStyle.Right;
    	this.pRight.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    	this.pRight.Location = new System.Drawing.Point(674, 43);
    	this.pRight.Name = "pRight";
    	this.pRight.Padding = new System.Windows.Forms.Padding(3, 28, 3, 6);
    	this.pRight.Size = new System.Drawing.Size(120, 532);
    	this.pRight.TabIndex = 2;
    	// 
    	// btnClose
    	// 
    	this.btnClose.Dock = System.Windows.Forms.DockStyle.Bottom;
    	this.btnClose.FlatAppearance.BorderSize = 0;
    	this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
    	this.btnClose.Image = ((System.Drawing.Image)(resources.GetObject("btnClose.Image")));
    	this.btnClose.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
    	this.btnClose.Location = new System.Drawing.Point(3, 477);
    	this.btnClose.Name = "btnClose";
    	this.btnClose.Size = new System.Drawing.Size(114, 49);
    	this.btnClose.TabIndex = 15;
    	this.btnClose.Text = "Close";
    	this.btnClose.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
    	this.btnClose.UseVisualStyleBackColor = true;
    	this.btnClose.Click += new System.EventHandler(this.BtnCloseClick);
    	// 
    	// btnDisconnect
    	// 
    	this.btnDisconnect.Dock = System.Windows.Forms.DockStyle.Top;
    	this.btnDisconnect.Enabled = false;
    	this.btnDisconnect.FlatAppearance.BorderSize = 0;
    	this.btnDisconnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
    	this.btnDisconnect.Image = ((System.Drawing.Image)(resources.GetObject("btnDisconnect.Image")));
    	this.btnDisconnect.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
    	this.btnDisconnect.Location = new System.Drawing.Point(3, 224);
    	this.btnDisconnect.Name = "btnDisconnect";
    	this.btnDisconnect.Size = new System.Drawing.Size(114, 49);
    	this.btnDisconnect.TabIndex = 12;
    	this.btnDisconnect.Text = "Disconnect";
    	this.btnDisconnect.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
    	this.btnDisconnect.UseVisualStyleBackColor = true;
    	this.btnDisconnect.Click += new System.EventHandler(this.BtnDisconnectClick);
    	// 
    	// btnReconnect
    	// 
    	this.btnReconnect.Dock = System.Windows.Forms.DockStyle.Top;
    	this.btnReconnect.Enabled = false;
    	this.btnReconnect.FlatAppearance.BorderSize = 0;
    	this.btnReconnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
    	this.btnReconnect.Image = ((System.Drawing.Image)(resources.GetObject("btnReconnect.Image")));
    	this.btnReconnect.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
    	this.btnReconnect.Location = new System.Drawing.Point(3, 175);
    	this.btnReconnect.Name = "btnReconnect";
    	this.btnReconnect.Size = new System.Drawing.Size(114, 49);
    	this.btnReconnect.TabIndex = 13;
    	this.btnReconnect.Text = "Reconnect";
    	this.btnReconnect.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
    	this.btnReconnect.UseVisualStyleBackColor = true;
    	this.btnReconnect.Click += new System.EventHandler(this.BtnReconnectClick);
    	// 
    	// btnConnect
    	// 
    	this.btnConnect.Dock = System.Windows.Forms.DockStyle.Top;
    	this.btnConnect.Enabled = false;
    	this.btnConnect.FlatAppearance.BorderSize = 0;
    	this.btnConnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
    	this.btnConnect.Image = ((System.Drawing.Image)(resources.GetObject("btnConnect.Image")));
    	this.btnConnect.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
    	this.btnConnect.Location = new System.Drawing.Point(3, 126);
    	this.btnConnect.Name = "btnConnect";
    	this.btnConnect.Size = new System.Drawing.Size(114, 49);
    	this.btnConnect.TabIndex = 14;
    	this.btnConnect.Text = "Connect";
    	this.btnConnect.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
    	this.btnConnect.UseVisualStyleBackColor = true;
    	this.btnConnect.Click += new System.EventHandler(this.BtnConnectClick);
    	// 
    	// btnControl
    	// 
    	this.btnControl.Dock = System.Windows.Forms.DockStyle.Top;
    	this.btnControl.Enabled = false;
    	this.btnControl.FlatAppearance.BorderSize = 0;
    	this.btnControl.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
    	this.btnControl.Image = ((System.Drawing.Image)(resources.GetObject("btnControl.Image")));
    	this.btnControl.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
    	this.btnControl.Location = new System.Drawing.Point(3, 77);
    	this.btnControl.Name = "btnControl";
    	this.btnControl.Size = new System.Drawing.Size(114, 49);
    	this.btnControl.TabIndex = 11;
    	this.btnControl.Text = "Control";
    	this.btnControl.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
    	this.btnControl.UseVisualStyleBackColor = true;
    	this.btnControl.Visible = false;
    	this.btnControl.Click += new System.EventHandler(this.BtnControlClick);
    	// 
    	// btnTransmit
    	// 
    	this.btnTransmit.Dock = System.Windows.Forms.DockStyle.Top;
    	this.btnTransmit.Enabled = false;
    	this.btnTransmit.FlatAppearance.BorderSize = 0;
    	this.btnTransmit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
    	this.btnTransmit.Image = ((System.Drawing.Image)(resources.GetObject("btnTransmit.Image")));
    	this.btnTransmit.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
    	this.btnTransmit.Location = new System.Drawing.Point(3, 28);
    	this.btnTransmit.Name = "btnTransmit";
    	this.btnTransmit.Size = new System.Drawing.Size(114, 49);
    	this.btnTransmit.TabIndex = 10;
    	this.btnTransmit.Text = "Transmit";
    	this.btnTransmit.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
    	this.btnTransmit.UseVisualStyleBackColor = true;
    	this.btnTransmit.Visible = false;
    	this.btnTransmit.Click += new System.EventHandler(this.BtnTransmitClick);
    	// 
    	// pMain
    	// 
    	this.pMain.Controls.Add(this.tabPages);
    	this.pMain.Dock = System.Windows.Forms.DockStyle.Fill;
    	this.pMain.Location = new System.Drawing.Point(0, 43);
    	this.pMain.Name = "pMain";
    	this.pMain.Padding = new System.Windows.Forms.Padding(6);
    	this.pMain.Size = new System.Drawing.Size(674, 532);
    	this.pMain.TabIndex = 3;
    	// 
    	// tabPages
    	// 
    	this.tabPages.Controls.Add(this.tabPageTransmit);
    	this.tabPages.Controls.Add(this.tabPageControl);
    	this.tabPages.Dock = System.Windows.Forms.DockStyle.Fill;
    	this.tabPages.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    	this.tabPages.Location = new System.Drawing.Point(6, 6);
    	this.tabPages.Multiline = true;
    	this.tabPages.Name = "tabPages";
    	this.tabPages.SelectedIndex = 0;
    	this.tabPages.Size = new System.Drawing.Size(662, 520);
    	this.tabPages.TabIndex = 0;
    	this.tabPages.SelectedIndexChanged += new System.EventHandler(this.TabPagesSelectedIndexChanged);
    	// 
    	// tabPageTransmit
    	// 
    	this.tabPageTransmit.Controls.Add(this.eTransmitErrorExplain);
    	this.tabPageTransmit.Controls.Add(this.eTransmitError);
    	this.tabPageTransmit.Controls.Add(this.label9);
    	this.tabPageTransmit.Controls.Add(this.label23);
    	this.tabPageTransmit.Controls.Add(this.eStatusWordExplain);
    	this.tabPageTransmit.Controls.Add(this.eStatusWord);
    	this.tabPageTransmit.Controls.Add(this.label21);
    	this.tabPageTransmit.Controls.Add(this.label22);
    	this.tabPageTransmit.Controls.Add(this.label20);
    	this.tabPageTransmit.Controls.Add(this.label19);
    	this.tabPageTransmit.Controls.Add(this.btnCApduBookmark);
    	this.tabPageTransmit.Controls.Add(this.btnCApduNext);
    	this.tabPageTransmit.Controls.Add(this.btnCApduPrev);
    	this.tabPageTransmit.Controls.Add(this.btnCApduClear);
    	this.tabPageTransmit.Controls.Add(this.label5);
    	this.tabPageTransmit.Controls.Add(this.label6);
    	this.tabPageTransmit.Controls.Add(this.label7);
    	this.tabPageTransmit.Controls.Add(this.label8);
    	this.tabPageTransmit.Controls.Add(this.label4);
    	this.tabPageTransmit.Controls.Add(this.label3);
    	this.tabPageTransmit.Controls.Add(this.label2);
    	this.tabPageTransmit.Controls.Add(this.label1);
    	this.tabPageTransmit.Controls.Add(this.hexBoxRApdu);
    	this.tabPageTransmit.Controls.Add(this.hexBoxCApdu);
    	this.tabPageTransmit.Location = new System.Drawing.Point(4, 23);
    	this.tabPageTransmit.Name = "tabPageTransmit";
    	this.tabPageTransmit.Padding = new System.Windows.Forms.Padding(3);
    	this.tabPageTransmit.Size = new System.Drawing.Size(654, 493);
    	this.tabPageTransmit.TabIndex = 0;
    	this.tabPageTransmit.Text = "Transmit";
    	this.tabPageTransmit.UseVisualStyleBackColor = true;
    	// 
    	// eTransmitErrorExplain
    	// 
    	this.eTransmitErrorExplain.BackColor = System.Drawing.SystemColors.Control;
    	this.eTransmitErrorExplain.Location = new System.Drawing.Point(94, 465);
    	this.eTransmitErrorExplain.Name = "eTransmitErrorExplain";
    	this.eTransmitErrorExplain.ReadOnly = true;
    	this.eTransmitErrorExplain.Size = new System.Drawing.Size(553, 22);
    	this.eTransmitErrorExplain.TabIndex = 28;
    	// 
    	// eTransmitError
    	// 
    	this.eTransmitError.BackColor = System.Drawing.SystemColors.Control;
    	this.eTransmitError.Font = new System.Drawing.Font("Consolas", 8.25F);
    	this.eTransmitError.Location = new System.Drawing.Point(6, 465);
    	this.eTransmitError.Name = "eTransmitError";
    	this.eTransmitError.ReadOnly = true;
    	this.eTransmitError.Size = new System.Drawing.Size(82, 20);
    	this.eTransmitError.TabIndex = 27;
    	this.eTransmitError.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
    	// 
    	// label9
    	// 
    	this.label9.AutoSize = true;
    	this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    	this.label9.ForeColor = System.Drawing.SystemColors.GrayText;
    	this.label9.Location = new System.Drawing.Point(6, 449);
    	this.label9.Name = "label9";
    	this.label9.Size = new System.Drawing.Size(56, 13);
    	this.label9.TabIndex = 26;
    	this.label9.Text = "Error code";
    	// 
    	// label23
    	// 
    	this.label23.AutoSize = true;
    	this.label23.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    	this.label23.ForeColor = System.Drawing.SystemColors.GrayText;
    	this.label23.Location = new System.Drawing.Point(94, 449);
    	this.label23.Name = "label23";
    	this.label23.Size = new System.Drawing.Size(62, 13);
    	this.label23.TabIndex = 25;
    	this.label23.Text = "Explanation";
    	// 
    	// eStatusWordExplain
    	// 
    	this.eStatusWordExplain.BackColor = System.Drawing.SystemColors.Control;
    	this.eStatusWordExplain.Location = new System.Drawing.Point(94, 423);
    	this.eStatusWordExplain.Name = "eStatusWordExplain";
    	this.eStatusWordExplain.ReadOnly = true;
    	this.eStatusWordExplain.Size = new System.Drawing.Size(553, 22);
    	this.eStatusWordExplain.TabIndex = 20;
    	// 
    	// eStatusWord
    	// 
    	this.eStatusWord.BackColor = System.Drawing.SystemColors.Control;
    	this.eStatusWord.Font = new System.Drawing.Font("Consolas", 8.25F);
    	this.eStatusWord.Location = new System.Drawing.Point(6, 423);
    	this.eStatusWord.Name = "eStatusWord";
    	this.eStatusWord.ReadOnly = true;
    	this.eStatusWord.Size = new System.Drawing.Size(82, 20);
    	this.eStatusWord.TabIndex = 19;
    	this.eStatusWord.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
    	// 
    	// label21
    	// 
    	this.label21.AutoSize = true;
    	this.label21.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    	this.label21.ForeColor = System.Drawing.SystemColors.GrayText;
    	this.label21.Location = new System.Drawing.Point(6, 407);
    	this.label21.Name = "label21";
    	this.label21.Size = new System.Drawing.Size(66, 13);
    	this.label21.TabIndex = 18;
    	this.label21.Text = "Status Word";
    	// 
    	// label22
    	// 
    	this.label22.AutoSize = true;
    	this.label22.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    	this.label22.ForeColor = System.Drawing.SystemColors.GrayText;
    	this.label22.Location = new System.Drawing.Point(94, 407);
    	this.label22.Name = "label22";
    	this.label22.Size = new System.Drawing.Size(48, 13);
    	this.label22.TabIndex = 17;
    	this.label22.Text = "Meaning";
    	// 
    	// label20
    	// 
    	this.label20.AutoSize = true;
    	this.label20.Font = new System.Drawing.Font("Calibri Light", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    	this.label20.ForeColor = System.Drawing.SystemColors.ControlText;
    	this.label20.Location = new System.Drawing.Point(188, 233);
    	this.label20.Name = "label20";
    	this.label20.Size = new System.Drawing.Size(119, 15);
    	this.label20.TabIndex = 16;
    	this.label20.Text = "[DataOut], SW1, SW2";
    	// 
    	// label19
    	// 
    	this.label19.AutoSize = true;
    	this.label19.Font = new System.Drawing.Font("Calibri Light", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    	this.label19.ForeColor = System.Drawing.SystemColors.ControlText;
    	this.label19.Location = new System.Drawing.Point(162, 8);
    	this.label19.Name = "label19";
    	this.label19.Size = new System.Drawing.Size(179, 15);
    	this.label19.TabIndex = 15;
    	this.label19.Text = "CLA, INS, P1, P2, [Lc, DataIn], [Le]";
    	// 
    	// btnCApduBookmark
    	// 
    	this.btnCApduBookmark.Enabled = false;
    	this.btnCApduBookmark.FlatAppearance.BorderSize = 0;
    	this.btnCApduBookmark.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
    	this.btnCApduBookmark.Image = ((System.Drawing.Image)(resources.GetObject("btnCApduBookmark.Image")));
    	this.btnCApduBookmark.Location = new System.Drawing.Point(612, 183);
    	this.btnCApduBookmark.Name = "btnCApduBookmark";
    	this.btnCApduBookmark.Size = new System.Drawing.Size(35, 32);
    	this.btnCApduBookmark.TabIndex = 14;
    	this.btnCApduBookmark.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
    	this.btnCApduBookmark.UseVisualStyleBackColor = true;
    	this.btnCApduBookmark.Click += new System.EventHandler(this.BtnCApduBookmarkClick);
    	// 
    	// btnCApduNext
    	// 
    	this.btnCApduNext.FlatAppearance.BorderSize = 0;
    	this.btnCApduNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
    	this.btnCApduNext.Image = ((System.Drawing.Image)(resources.GetObject("btnCApduNext.Image")));
    	this.btnCApduNext.Location = new System.Drawing.Point(571, 183);
    	this.btnCApduNext.Name = "btnCApduNext";
    	this.btnCApduNext.Size = new System.Drawing.Size(35, 32);
    	this.btnCApduNext.TabIndex = 13;
    	this.btnCApduNext.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
    	this.btnCApduNext.UseVisualStyleBackColor = true;
    	this.btnCApduNext.Click += new System.EventHandler(this.BtnCApduNextClick);
    	// 
    	// btnCApduPrev
    	// 
    	this.btnCApduPrev.FlatAppearance.BorderSize = 0;
    	this.btnCApduPrev.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
    	this.btnCApduPrev.Image = ((System.Drawing.Image)(resources.GetObject("btnCApduPrev.Image")));
    	this.btnCApduPrev.Location = new System.Drawing.Point(530, 183);
    	this.btnCApduPrev.Name = "btnCApduPrev";
    	this.btnCApduPrev.Size = new System.Drawing.Size(35, 32);
    	this.btnCApduPrev.TabIndex = 12;
    	this.btnCApduPrev.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
    	this.btnCApduPrev.UseVisualStyleBackColor = true;
    	this.btnCApduPrev.Click += new System.EventHandler(this.BtnCApduPrevClick);
    	// 
    	// btnCApduClear
    	// 
    	this.btnCApduClear.FlatAppearance.BorderSize = 0;
    	this.btnCApduClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
    	this.btnCApduClear.Image = ((System.Drawing.Image)(resources.GetObject("btnCApduClear.Image")));
    	this.btnCApduClear.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
    	this.btnCApduClear.Location = new System.Drawing.Point(496, 183);
    	this.btnCApduClear.Name = "btnCApduClear";
    	this.btnCApduClear.Size = new System.Drawing.Size(35, 32);
    	this.btnCApduClear.TabIndex = 11;
    	this.btnCApduClear.UseVisualStyleBackColor = true;
    	this.btnCApduClear.Click += new System.EventHandler(this.BtnCApduClearClick);
    	// 
    	// label5
    	// 
    	this.label5.AutoSize = true;
    	this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    	this.label5.ForeColor = System.Drawing.SystemColors.GrayText;
    	this.label5.Location = new System.Drawing.Point(6, 251);
    	this.label5.Name = "label5";
    	this.label5.Size = new System.Drawing.Size(35, 13);
    	this.label5.TabIndex = 9;
    	this.label5.Text = "Offset";
    	// 
    	// label6
    	// 
    	this.label6.AutoSize = true;
    	this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    	this.label6.ForeColor = System.Drawing.SystemColors.GrayText;
    	this.label6.Location = new System.Drawing.Point(490, 251);
    	this.label6.Name = "label6";
    	this.label6.Size = new System.Drawing.Size(85, 13);
    	this.label6.TabIndex = 8;
    	this.label6.Text = "ASCII translation";
    	// 
    	// label7
    	// 
    	this.label7.AutoSize = true;
    	this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    	this.label7.ForeColor = System.Drawing.SystemColors.GrayText;
    	this.label7.Location = new System.Drawing.Point(90, 251);
    	this.label7.Name = "label7";
    	this.label7.Size = new System.Drawing.Size(97, 13);
    	this.label7.TabIndex = 7;
    	this.label7.Text = "Hexadecimal value";
    	// 
    	// label8
    	// 
    	this.label8.AutoSize = true;
    	this.label8.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    	this.label8.Location = new System.Drawing.Point(6, 233);
    	this.label8.Name = "label8";
    	this.label8.Size = new System.Drawing.Size(169, 15);
    	this.label8.TabIndex = 6;
    	this.label8.Text = "Response APDU from the card";
    	// 
    	// label4
    	// 
    	this.label4.AutoSize = true;
    	this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    	this.label4.ForeColor = System.Drawing.SystemColors.GrayText;
    	this.label4.Location = new System.Drawing.Point(6, 26);
    	this.label4.Name = "label4";
    	this.label4.Size = new System.Drawing.Size(35, 13);
    	this.label4.TabIndex = 5;
    	this.label4.Text = "Offset";
    	// 
    	// label3
    	// 
    	this.label3.AutoSize = true;
    	this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    	this.label3.ForeColor = System.Drawing.SystemColors.GrayText;
    	this.label3.Location = new System.Drawing.Point(490, 26);
    	this.label3.Name = "label3";
    	this.label3.Size = new System.Drawing.Size(85, 13);
    	this.label3.TabIndex = 4;
    	this.label3.Text = "ASCII translation";
    	// 
    	// label2
    	// 
    	this.label2.AutoSize = true;
    	this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    	this.label2.ForeColor = System.Drawing.SystemColors.GrayText;
    	this.label2.Location = new System.Drawing.Point(90, 26);
    	this.label2.Name = "label2";
    	this.label2.Size = new System.Drawing.Size(94, 13);
    	this.label2.TabIndex = 3;
    	this.label2.Text = "Hexadecimal entry";
    	// 
    	// label1
    	// 
    	this.label1.AutoSize = true;
    	this.label1.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    	this.label1.Location = new System.Drawing.Point(6, 8);
    	this.label1.Name = "label1";
    	this.label1.Size = new System.Drawing.Size(146, 15);
    	this.label1.TabIndex = 2;
    	this.label1.Text = "Enter the Command APDU";
    	// 
    	// hexBoxRApdu
    	// 
    	this.hexBoxRApdu.BackColor = System.Drawing.SystemColors.Control;
    	this.hexBoxRApdu.Font = new System.Drawing.Font("Consolas", 8.25F);
    	this.hexBoxRApdu.LineInfoForeColor = System.Drawing.Color.Empty;
    	this.hexBoxRApdu.LineInfoVisible = true;
    	this.hexBoxRApdu.Location = new System.Drawing.Point(6, 267);
    	this.hexBoxRApdu.Name = "hexBoxRApdu";
    	this.hexBoxRApdu.ReadOnly = true;
    	this.hexBoxRApdu.ShadowSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(60)))), ((int)(((byte)(188)))), ((int)(((byte)(255)))));
    	this.hexBoxRApdu.Size = new System.Drawing.Size(641, 135);
    	this.hexBoxRApdu.StringViewVisible = true;
    	this.hexBoxRApdu.TabIndex = 1;
    	this.hexBoxRApdu.UseFixedBytesPerLine = true;
    	this.hexBoxRApdu.VScrollBarVisible = true;
    	// 
    	// hexBoxCApdu
    	// 
    	this.hexBoxCApdu.Font = new System.Drawing.Font("Consolas", 8.25F);
    	this.hexBoxCApdu.LineInfoForeColor = System.Drawing.Color.Empty;
    	this.hexBoxCApdu.LineInfoVisible = true;
    	this.hexBoxCApdu.Location = new System.Drawing.Point(6, 42);
    	this.hexBoxCApdu.Name = "hexBoxCApdu";
    	this.hexBoxCApdu.ShadowSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(60)))), ((int)(((byte)(188)))), ((int)(((byte)(255)))));
    	this.hexBoxCApdu.Size = new System.Drawing.Size(641, 135);
    	this.hexBoxCApdu.StringViewVisible = true;
    	this.hexBoxCApdu.TabIndex = 0;
    	this.hexBoxCApdu.UseFixedBytesPerLine = true;
    	this.hexBoxCApdu.VScrollBarVisible = true;
    	this.hexBoxCApdu.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.HexBoxCApduKeyPress);
    	// 
    	// tabPageControl
    	// 
    	this.tabPageControl.Controls.Add(this.eControlErrorExplain);
    	this.tabPageControl.Controls.Add(this.eControlError);
    	this.tabPageControl.Controls.Add(this.label24);
    	this.tabPageControl.Controls.Add(this.label25);
    	this.tabPageControl.Controls.Add(this.eResultByteExplain);
    	this.tabPageControl.Controls.Add(this.eResultByte);
    	this.tabPageControl.Controls.Add(this.label26);
    	this.tabPageControl.Controls.Add(this.label27);
    	this.tabPageControl.Controls.Add(this.btnCtrlBookmark);
    	this.tabPageControl.Controls.Add(this.btnCCtrlNext);
    	this.tabPageControl.Controls.Add(this.btnCCtrlPrev);
    	this.tabPageControl.Controls.Add(this.btnCtrlClear);
    	this.tabPageControl.Controls.Add(this.label11);
    	this.tabPageControl.Controls.Add(this.label12);
    	this.tabPageControl.Controls.Add(this.label13);
    	this.tabPageControl.Controls.Add(this.label14);
    	this.tabPageControl.Controls.Add(this.label15);
    	this.tabPageControl.Controls.Add(this.label16);
    	this.tabPageControl.Controls.Add(this.label17);
    	this.tabPageControl.Controls.Add(this.label18);
    	this.tabPageControl.Controls.Add(this.hexBoxRCtrl);
    	this.tabPageControl.Controls.Add(this.hexBoxCCtrl);
    	this.tabPageControl.Location = new System.Drawing.Point(4, 23);
    	this.tabPageControl.Name = "tabPageControl";
    	this.tabPageControl.Padding = new System.Windows.Forms.Padding(3);
    	this.tabPageControl.Size = new System.Drawing.Size(654, 493);
    	this.tabPageControl.TabIndex = 1;
    	this.tabPageControl.Text = "Control";
    	this.tabPageControl.UseVisualStyleBackColor = true;
    	// 
    	// eControlErrorExplain
    	// 
    	this.eControlErrorExplain.BackColor = System.Drawing.SystemColors.Control;
    	this.eControlErrorExplain.Location = new System.Drawing.Point(94, 465);
    	this.eControlErrorExplain.Name = "eControlErrorExplain";
    	this.eControlErrorExplain.ReadOnly = true;
    	this.eControlErrorExplain.Size = new System.Drawing.Size(553, 22);
    	this.eControlErrorExplain.TabIndex = 37;
    	// 
    	// eControlError
    	// 
    	this.eControlError.BackColor = System.Drawing.SystemColors.Control;
    	this.eControlError.Font = new System.Drawing.Font("Consolas", 8.25F);
    	this.eControlError.Location = new System.Drawing.Point(6, 465);
    	this.eControlError.Name = "eControlError";
    	this.eControlError.ReadOnly = true;
    	this.eControlError.Size = new System.Drawing.Size(82, 20);
    	this.eControlError.TabIndex = 36;
    	this.eControlError.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
    	// 
    	// label24
    	// 
    	this.label24.AutoSize = true;
    	this.label24.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    	this.label24.ForeColor = System.Drawing.SystemColors.GrayText;
    	this.label24.Location = new System.Drawing.Point(6, 449);
    	this.label24.Name = "label24";
    	this.label24.Size = new System.Drawing.Size(56, 13);
    	this.label24.TabIndex = 35;
    	this.label24.Text = "Error code";
    	// 
    	// label25
    	// 
    	this.label25.AutoSize = true;
    	this.label25.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    	this.label25.ForeColor = System.Drawing.SystemColors.GrayText;
    	this.label25.Location = new System.Drawing.Point(94, 449);
    	this.label25.Name = "label25";
    	this.label25.Size = new System.Drawing.Size(62, 13);
    	this.label25.TabIndex = 34;
    	this.label25.Text = "Explanation";
    	// 
    	// eResultByteExplain
    	// 
    	this.eResultByteExplain.BackColor = System.Drawing.SystemColors.Control;
    	this.eResultByteExplain.Location = new System.Drawing.Point(94, 423);
    	this.eResultByteExplain.Name = "eResultByteExplain";
    	this.eResultByteExplain.ReadOnly = true;
    	this.eResultByteExplain.Size = new System.Drawing.Size(553, 22);
    	this.eResultByteExplain.TabIndex = 33;
    	// 
    	// eResultByte
    	// 
    	this.eResultByte.BackColor = System.Drawing.SystemColors.Control;
    	this.eResultByte.Font = new System.Drawing.Font("Consolas", 8.25F);
    	this.eResultByte.Location = new System.Drawing.Point(6, 423);
    	this.eResultByte.Name = "eResultByte";
    	this.eResultByte.ReadOnly = true;
    	this.eResultByte.Size = new System.Drawing.Size(82, 20);
    	this.eResultByte.TabIndex = 32;
    	this.eResultByte.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
    	// 
    	// label26
    	// 
    	this.label26.AutoSize = true;
    	this.label26.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    	this.label26.ForeColor = System.Drawing.SystemColors.GrayText;
    	this.label26.Location = new System.Drawing.Point(6, 407);
    	this.label26.Name = "label26";
    	this.label26.Size = new System.Drawing.Size(60, 13);
    	this.label26.TabIndex = 31;
    	this.label26.Text = "Result byte";
    	// 
    	// label27
    	// 
    	this.label27.AutoSize = true;
    	this.label27.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    	this.label27.ForeColor = System.Drawing.SystemColors.GrayText;
    	this.label27.Location = new System.Drawing.Point(94, 407);
    	this.label27.Name = "label27";
    	this.label27.Size = new System.Drawing.Size(48, 13);
    	this.label27.TabIndex = 30;
    	this.label27.Text = "Meaning";
    	// 
    	// btnCtrlBookmark
    	// 
    	this.btnCtrlBookmark.Enabled = false;
    	this.btnCtrlBookmark.FlatAppearance.BorderSize = 0;
    	this.btnCtrlBookmark.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
    	this.btnCtrlBookmark.Image = ((System.Drawing.Image)(resources.GetObject("btnCtrlBookmark.Image")));
    	this.btnCtrlBookmark.Location = new System.Drawing.Point(612, 183);
    	this.btnCtrlBookmark.Name = "btnCtrlBookmark";
    	this.btnCtrlBookmark.Size = new System.Drawing.Size(35, 32);
    	this.btnCtrlBookmark.TabIndex = 29;
    	this.btnCtrlBookmark.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
    	this.btnCtrlBookmark.UseVisualStyleBackColor = true;
    	this.btnCtrlBookmark.Click += new System.EventHandler(this.BtnCtrlBookmarkClick);
    	// 
    	// btnCCtrlNext
    	// 
    	this.btnCCtrlNext.FlatAppearance.BorderSize = 0;
    	this.btnCCtrlNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
    	this.btnCCtrlNext.Image = ((System.Drawing.Image)(resources.GetObject("btnCCtrlNext.Image")));
    	this.btnCCtrlNext.Location = new System.Drawing.Point(571, 183);
    	this.btnCCtrlNext.Name = "btnCCtrlNext";
    	this.btnCCtrlNext.Size = new System.Drawing.Size(35, 32);
    	this.btnCCtrlNext.TabIndex = 28;
    	this.btnCCtrlNext.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
    	this.btnCCtrlNext.UseVisualStyleBackColor = true;
    	this.btnCCtrlNext.Click += new System.EventHandler(this.BtnCtrlNextClick);
    	// 
    	// btnCCtrlPrev
    	// 
    	this.btnCCtrlPrev.FlatAppearance.BorderSize = 0;
    	this.btnCCtrlPrev.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
    	this.btnCCtrlPrev.Image = ((System.Drawing.Image)(resources.GetObject("btnCCtrlPrev.Image")));
    	this.btnCCtrlPrev.Location = new System.Drawing.Point(530, 183);
    	this.btnCCtrlPrev.Name = "btnCCtrlPrev";
    	this.btnCCtrlPrev.Size = new System.Drawing.Size(35, 32);
    	this.btnCCtrlPrev.TabIndex = 27;
    	this.btnCCtrlPrev.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
    	this.btnCCtrlPrev.UseVisualStyleBackColor = true;
    	this.btnCCtrlPrev.Click += new System.EventHandler(this.BtnCtrlPrevClick);
    	// 
    	// btnCtrlClear
    	// 
    	this.btnCtrlClear.FlatAppearance.BorderSize = 0;
    	this.btnCtrlClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
    	this.btnCtrlClear.Image = ((System.Drawing.Image)(resources.GetObject("btnCtrlClear.Image")));
    	this.btnCtrlClear.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
    	this.btnCtrlClear.Location = new System.Drawing.Point(496, 183);
    	this.btnCtrlClear.Name = "btnCtrlClear";
    	this.btnCtrlClear.Size = new System.Drawing.Size(35, 32);
    	this.btnCtrlClear.TabIndex = 26;
    	this.btnCtrlClear.UseVisualStyleBackColor = true;
    	this.btnCtrlClear.Click += new System.EventHandler(this.BtnCtrlClearClick);
    	// 
    	// label11
    	// 
    	this.label11.AutoSize = true;
    	this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    	this.label11.ForeColor = System.Drawing.SystemColors.GrayText;
    	this.label11.Location = new System.Drawing.Point(6, 251);
    	this.label11.Name = "label11";
    	this.label11.Size = new System.Drawing.Size(35, 13);
    	this.label11.TabIndex = 24;
    	this.label11.Text = "Offset";
    	// 
    	// label12
    	// 
    	this.label12.AutoSize = true;
    	this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    	this.label12.ForeColor = System.Drawing.SystemColors.GrayText;
    	this.label12.Location = new System.Drawing.Point(490, 251);
    	this.label12.Name = "label12";
    	this.label12.Size = new System.Drawing.Size(85, 13);
    	this.label12.TabIndex = 23;
    	this.label12.Text = "ASCII translation";
    	// 
    	// label13
    	// 
    	this.label13.AutoSize = true;
    	this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    	this.label13.ForeColor = System.Drawing.SystemColors.GrayText;
    	this.label13.Location = new System.Drawing.Point(90, 251);
    	this.label13.Name = "label13";
    	this.label13.Size = new System.Drawing.Size(97, 13);
    	this.label13.TabIndex = 22;
    	this.label13.Text = "Hexadecimal value";
    	// 
    	// label14
    	// 
    	this.label14.AutoSize = true;
    	this.label14.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    	this.label14.Location = new System.Drawing.Point(6, 233);
    	this.label14.Name = "label14";
    	this.label14.Size = new System.Drawing.Size(191, 15);
    	this.label14.TabIndex = 21;
    	this.label14.Text = "Escape Response from the reader:";
    	// 
    	// label15
    	// 
    	this.label15.AutoSize = true;
    	this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    	this.label15.ForeColor = System.Drawing.SystemColors.GrayText;
    	this.label15.Location = new System.Drawing.Point(6, 26);
    	this.label15.Name = "label15";
    	this.label15.Size = new System.Drawing.Size(35, 13);
    	this.label15.TabIndex = 20;
    	this.label15.Text = "Offset";
    	// 
    	// label16
    	// 
    	this.label16.AutoSize = true;
    	this.label16.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    	this.label16.ForeColor = System.Drawing.SystemColors.GrayText;
    	this.label16.Location = new System.Drawing.Point(490, 26);
    	this.label16.Name = "label16";
    	this.label16.Size = new System.Drawing.Size(85, 13);
    	this.label16.TabIndex = 19;
    	this.label16.Text = "ASCII translation";
    	// 
    	// label17
    	// 
    	this.label17.AutoSize = true;
    	this.label17.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    	this.label17.ForeColor = System.Drawing.SystemColors.GrayText;
    	this.label17.Location = new System.Drawing.Point(90, 26);
    	this.label17.Name = "label17";
    	this.label17.Size = new System.Drawing.Size(94, 13);
    	this.label17.TabIndex = 18;
    	this.label17.Text = "Hexadecimal entry";
    	// 
    	// label18
    	// 
    	this.label18.AutoSize = true;
    	this.label18.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    	this.label18.Location = new System.Drawing.Point(6, 8);
    	this.label18.Name = "label18";
    	this.label18.Size = new System.Drawing.Size(157, 15);
    	this.label18.TabIndex = 17;
    	this.label18.Text = "Enter the Escape Command:";
    	// 
    	// hexBoxRCtrl
    	// 
    	this.hexBoxRCtrl.BackColor = System.Drawing.SystemColors.Control;
    	this.hexBoxRCtrl.Font = new System.Drawing.Font("Consolas", 8.25F);
    	this.hexBoxRCtrl.LineInfoForeColor = System.Drawing.Color.Empty;
    	this.hexBoxRCtrl.LineInfoVisible = true;
    	this.hexBoxRCtrl.Location = new System.Drawing.Point(6, 267);
    	this.hexBoxRCtrl.Name = "hexBoxRCtrl";
    	this.hexBoxRCtrl.ReadOnly = true;
    	this.hexBoxRCtrl.ShadowSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(60)))), ((int)(((byte)(188)))), ((int)(((byte)(255)))));
    	this.hexBoxRCtrl.Size = new System.Drawing.Size(641, 135);
    	this.hexBoxRCtrl.StringViewVisible = true;
    	this.hexBoxRCtrl.TabIndex = 16;
    	this.hexBoxRCtrl.UseFixedBytesPerLine = true;
    	this.hexBoxRCtrl.VScrollBarVisible = true;
    	// 
    	// hexBoxCCtrl
    	// 
    	this.hexBoxCCtrl.Font = new System.Drawing.Font("Consolas", 8.25F);
    	this.hexBoxCCtrl.LineInfoForeColor = System.Drawing.Color.Empty;
    	this.hexBoxCCtrl.LineInfoVisible = true;
    	this.hexBoxCCtrl.Location = new System.Drawing.Point(6, 42);
    	this.hexBoxCCtrl.Name = "hexBoxCCtrl";
    	this.hexBoxCCtrl.ShadowSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(60)))), ((int)(((byte)(188)))), ((int)(((byte)(255)))));
    	this.hexBoxCCtrl.Size = new System.Drawing.Size(641, 135);
    	this.hexBoxCCtrl.StringViewVisible = true;
    	this.hexBoxCCtrl.TabIndex = 15;
    	this.hexBoxCCtrl.UseFixedBytesPerLine = true;
    	this.hexBoxCCtrl.VScrollBarVisible = true;
    	this.hexBoxCCtrl.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.HexBoxCCtrlKeyPress);
    	// 
    	// CardForm
    	// 
    	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
    	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
    	this.ClientSize = new System.Drawing.Size(794, 575);
    	this.Controls.Add(this.pMain);
    	this.Controls.Add(this.pRight);
    	this.Controls.Add(this.pTop);
    	this.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    	this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
    	this.MaximizeBox = false;
    	this.Name = "CardForm";
    	this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
    	this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
    	this.Text = "CardForm";
    	this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.CardFormFormClosed);
    	this.pTop.ResumeLayout(false);
    	this.pTop.PerformLayout();
    	this.pRight.ResumeLayout(false);
    	this.pMain.ResumeLayout(false);
    	this.tabPages.ResumeLayout(false);
    	this.tabPageTransmit.ResumeLayout(false);
    	this.tabPageTransmit.PerformLayout();
    	this.tabPageControl.ResumeLayout(false);
    	this.tabPageControl.PerformLayout();
    	this.ResumeLayout(false);

    }
    private System.Windows.Forms.Button btnCCtrlNext;
    private System.Windows.Forms.Label label29;
    private System.Windows.Forms.TextBox eMode;
    private System.Windows.Forms.Label label28;
    private System.Windows.Forms.TextBox eProtocol;
    private System.Windows.Forms.Label label27;
    private System.Windows.Forms.Label label26;
    private System.Windows.Forms.TextBox eResultByte;
    private System.Windows.Forms.TextBox eResultByteExplain;
    private System.Windows.Forms.Label label25;
    private System.Windows.Forms.Label label24;
    private System.Windows.Forms.TextBox eControlError;
    private System.Windows.Forms.TextBox eControlErrorExplain;
    private System.Windows.Forms.Button btnConnect;
    private System.Windows.Forms.Button btnReconnect;
    private System.Windows.Forms.Button btnDisconnect;
    private System.Windows.Forms.Button btnClose;
    private System.Windows.Forms.Label label23;
    private System.Windows.Forms.Label label9;
    private System.Windows.Forms.TextBox eTransmitError;
    private System.Windows.Forms.TextBox eTransmitErrorExplain;
    private System.Windows.Forms.Label label22;
    private System.Windows.Forms.Label label21;
    private System.Windows.Forms.TextBox eStatusWord;
    private System.Windows.Forms.TextBox eStatusWordExplain;
    private System.Windows.Forms.TabControl tabPages;
    private Be.Windows.Forms.HexBox hexBoxRApdu;
    private Be.Windows.Forms.HexBox hexBoxRCtrl;
    private System.Windows.Forms.Label label18;
    private System.Windows.Forms.Label label17;
    private System.Windows.Forms.Label label16;
    private System.Windows.Forms.Label label15;
    private System.Windows.Forms.Label label14;
    private System.Windows.Forms.Label label13;
    private System.Windows.Forms.Label label12;
    private System.Windows.Forms.Label label11;
    private System.Windows.Forms.Button btnControl;
    private System.Windows.Forms.Button btnCtrlClear;
    private System.Windows.Forms.Button btnCCtrlPrev;
    private System.Windows.Forms.Button btnCtrlBookmark;
    private System.Windows.Forms.Label label19;
    private System.Windows.Forms.Label label20;
    private System.Windows.Forms.Label label10;
    private System.Windows.Forms.TextBox eCardAtr;
    private System.Windows.Forms.Button btnCApduPrev;
    private System.Windows.Forms.Button btnCApduNext;
    private System.Windows.Forms.Button btnCApduBookmark;
    private System.Windows.Forms.Button btnCApduClear;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Button btnTransmit;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label3;
    private Be.Windows.Forms.HexBox hexBoxCApdu;
    private System.Windows.Forms.TabPage tabPageTransmit;
    private System.Windows.Forms.TabPage tabPageControl;
    private Be.Windows.Forms.HexBox hexBoxCCtrl;
    private System.Windows.Forms.Panel pMain;
    private System.Windows.Forms.Panel pRight;
    private System.Windows.Forms.Panel pTop;
  }
}
