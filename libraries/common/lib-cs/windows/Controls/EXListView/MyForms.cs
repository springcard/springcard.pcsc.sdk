using System;
using System.Windows.Forms;
using System.Collections;
using EXControls;
using System.Drawing;
using System.Threading;

class MyForm : Form {
    
	private EXListView lstv;
	private Button btn;
	private Button btn2;
	private StatusStrip statusstrip1;
	private ToolStripStatusLabel toolstripstatuslabel1;
	private delegate void del_do_update(ProgressBar pb);
	private delegate void del_do_changetxt(LinkLabel l, string text);
           
	public MyForm() {
		statusstrip1 = new StatusStrip();
		toolstripstatuslabel1 = new ToolStripStatusLabel();
		btn = new Button();
		btn2 = new Button();
		InitializeComponent();
	}
    
	private void InitializeComponent() {
		//imglst_genre
		ImageList imglst_genre = new ImageList();
		imglst_genre.ColorDepth = ColorDepth.Depth32Bit;
		imglst_genre.Images.Add(Image.FromFile("music.png"));
		imglst_genre.Images.Add(Image.FromFile("love.png"));
		imglst_genre.Images.Add(Image.FromFile("comedy.png"));
		imglst_genre.Images.Add(Image.FromFile("drama.png"));
		imglst_genre.Images.Add(Image.FromFile("horror.ico"));
		imglst_genre.Images.Add(Image.FromFile("family.ico"));
		//excmbx_genre
		EXComboBox excmbx_genre = new EXComboBox();
		excmbx_genre.DropDownStyle = ComboBoxStyle.DropDownList;
		excmbx_genre.MyHighlightBrush = Brushes.Goldenrod;
		excmbx_genre.ItemHeight = 20;
		excmbx_genre.Items.Add(new EXComboBox.EXImageItem(imglst_genre.Images[0], "Music"));
		excmbx_genre.Items.Add(new EXComboBox.EXImageItem(imglst_genre.Images[1], "Romantic"));
		excmbx_genre.Items.Add(new EXComboBox.EXImageItem(imglst_genre.Images[2], "Comedy"));
		excmbx_genre.Items.Add(new EXComboBox.EXImageItem(imglst_genre.Images[3], "Drama"));
		excmbx_genre.Items.Add(new EXComboBox.EXImageItem(imglst_genre.Images[4], "Horror"));
		excmbx_genre.Items.Add(new EXComboBox.EXImageItem(imglst_genre.Images[5], "Family"));
		excmbx_genre.Items.Add(new EXComboBox.EXMultipleImagesItem(new ArrayList(new object[] {Image.FromFile("love.png"), Image.FromFile("comedy.png")}), "Romantic comedy"));
		//excmbx_rate
		EXComboBox excmbx_rate = new EXComboBox();
		excmbx_rate.MyHighlightBrush = Brushes.Goldenrod;
		excmbx_rate.DropDownStyle = ComboBoxStyle.DropDownList;
		ImageList imglst_rate = new ImageList();
		imglst_rate.ColorDepth = ColorDepth.Depth32Bit;
		imglst_rate.Images.Add(Image.FromFile("rate.png"));
		for (int i = 1; i < 6; i++) {
			ArrayList _arlst1 = new ArrayList();
			for (int j = 0; j < i; j++) {
				_arlst1.Add(imglst_rate.Images[0]);
			}
			excmbx_rate.Items.Add(new EXComboBox.EXMultipleImagesItem("", _arlst1, i.ToString()));
		}
		//lstv
		lstv = new EXListView();
		lstv.MySortBrush = SystemBrushes.ControlLight;
		lstv.MyHighlightBrush = Brushes.Goldenrod;
		lstv.GridLines = true;
		lstv.Location = new Point(10, 40);
		lstv.Size = new Size(500, 400);
		lstv.ControlPadding = 4;
		lstv.MouseMove += new MouseEventHandler(lstv_MouseMove);
		lstv.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
		//add SmallImageList to ListView - images will be shown in ColumnHeaders
		ImageList colimglst = new ImageList();
		colimglst.Images.Add("down", Image.FromFile("down.png"));
		colimglst.Images.Add("up", Image.FromFile("up.png"));
		colimglst.ColorDepth = ColorDepth.Depth32Bit;
		colimglst.ImageSize = new Size(20, 20); // this will affect the row height
		lstv.SmallImageList = colimglst;
		//add columns and items
		lstv.Columns.Add(new EXEditableColumnHeader("Movie", 20));
		lstv.Columns.Add(new EXColumnHeader("Progress", 120));
		lstv.Columns.Add(new EXEditableColumnHeader("Genre", excmbx_genre, 60));
		lstv.Columns.Add(new EXEditableColumnHeader("Rate", excmbx_rate, 100));
		lstv.Columns.Add(new EXColumnHeader("Status", 80));
		EXBoolColumnHeader boolcol = new EXBoolColumnHeader("Conclusion", 80);
		boolcol.Editable = true;
		boolcol.TrueImage = Image.FromFile("true.png");
		boolcol.FalseImage = Image.FromFile("false.png");
		lstv.Columns.Add(boolcol);
		lstv.BeginUpdate();
		for (int i = 0; i < 100; i++) {
			//movie
			EXListViewItem item = new EXListViewItem(i.ToString());
			EXControlListViewSubItem cs = new EXControlListViewSubItem();
			ProgressBar b = new ProgressBar();
			b.Tag = item;
			b.Minimum = 0;
			b.Maximum = 1000;
			b.Step = 1;
			item.SubItems.Add(cs);
			lstv.AddControlToSubItem(b, cs);
			//genre
			item.SubItems.Add(new EXMultipleImagesListViewSubItem(new ArrayList(new object[] {imglst_genre.Images[1], imglst_genre.Images[2]}), "Romantic comedy"));
			//rate
			item.SubItems.Add(new EXMultipleImagesListViewSubItem(new ArrayList(new object[] {imglst_rate.Images[0]}), "1"));
			//cancel and resume
			EXControlListViewSubItem cs1 = new EXControlListViewSubItem();
			LinkLabel llbl = new LinkLabel();
			llbl.Text = "Start";
			llbl.Tag = cs;
			llbl.LinkClicked += new LinkLabelLinkClickedEventHandler(llbl_LinkClicked);
			item.SubItems.Add(cs1);
			lstv.AddControlToSubItem(llbl, cs1);
			//conclusion
			item.SubItems.Add(new EXBoolListViewSubItem(true));
			lstv.Items.Add(item);
		}
		lstv.EndUpdate();
		//statusstrip1
		statusstrip1.Items.AddRange(new ToolStripItem[] {toolstripstatuslabel1});
		//btn
		btn.Location = new Point(10, 450);
		btn.Text = "Remove Control";
		btn.AutoSize = true;
		btn.Click += new EventHandler(btn_Click);
		//btn2
		btn2.Location = new Point(btn.Right + 20, 450);
		btn2.Text = "Remove Image";
		btn2.AutoSize = true;
		btn2.Click += new EventHandler(btn2_Click);
		//this
		this.ClientSize = new Size(520, 510);
		this.Controls.Add(statusstrip1);
		Label lbl = new Label();
		lbl.Text = "Doubleclick on the subitems to edit...";
		lbl.Bounds = new Rectangle(10, 10, 480, 20);
		this.Controls.Add(lbl);
		this.Controls.Add(lstv);
		this.Controls.Add(btn);
		this.Controls.Add(btn2);
	}

	private void llbl_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
		LinkLabel l = (LinkLabel) sender;
		if (l.Text == "Downloading") return;
		EXControlListViewSubItem subitem = l.Tag as EXControlListViewSubItem;
		ProgressBar p = subitem.MyControl as ProgressBar;
		Thread th = new Thread(new ParameterizedThreadStart(UpdateProgressBarMethod));
		th.IsBackground = true;
		th.Start(p);
		((LinkLabel) sender).Text = "Downloading";
	}

	private void lstv_MouseMove(object sender, MouseEventArgs e) {
		ListViewHitTestInfo lstvinfo = lstv.HitTest(e.X, e.Y);
		ListViewItem.ListViewSubItem subitem = lstvinfo.SubItem;
		if (subitem == null) return;
		if (subitem is EXListViewSubItemAB) {
			toolstripstatuslabel1.Text = ((EXListViewSubItemAB) subitem).MyValue;
		}
	}

	private void ChangeTextMethod(LinkLabel l, string text) {
		l.Text = text;
	}
	
	private void UpdateProgressBarMethod(object pb) {
		ProgressBar pp = (ProgressBar) pb;
		if (pp.Value == pp.Maximum) pp.Value = 0;
		del_do_update delupdate = new del_do_update(do_update);
		for (int i = pp.Value; i < pp.Maximum; i++) {
			pp.BeginInvoke(delupdate, new object[] {pp});
			Thread.Sleep(10);
		}
		ListViewItem item = (ListViewItem) pp.Tag;
		LinkLabel l = ((LinkLabel) ((EXControlListViewSubItem) item.SubItems[4]).MyControl);
		del_do_changetxt delchangetxt = new del_do_changetxt(ChangeTextMethod);
		l.BeginInvoke(delchangetxt, new object[] {l, "OK"});
	}

	private void do_update(ProgressBar p) {
		p.PerformStep();
	}

	private void btn_Click(object sender, EventArgs e) {
		lstv.RemoveControlFromSubItem((EXControlListViewSubItem) lstv.Items[1].SubItems[4]);
	}

	private void btn2_Click(object sender, EventArgs e) {
		((EXMultipleImagesListViewSubItem) lstv.Items[1].SubItems[2]).MyImages.Clear();
		lstv.Invalidate(lstv.Items[1].SubItems[2].Bounds);
	}

	public static void Main() {
		Application.EnableVisualStyles();
		Application.Run(new MyForm());
	}

}
