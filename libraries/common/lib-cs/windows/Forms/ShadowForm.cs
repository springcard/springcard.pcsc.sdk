using System;
using System.Drawing;
using System.Windows.Forms;

namespace SpringCard.LibCs.Windows
{
	/// <summary>
	/// Description of ShadowForm.
	/// </summary>
	public partial class ShadowForm : Form
	{
		public ShadowForm()
		{
			InitializeComponent();
		}

		private void _Shade(Form parent)
		{
			this.Owner = parent;
			Point p = new Point(0, 0);
			p = parent.PointToScreen(p);
			this.Top = p.Y;
			this.Left = p.X;
			this.Height = parent.ClientRectangle.Height;
			this.Width = parent.ClientRectangle.Width;
			this.Show();
		}
		
		static ShadowForm instance;
		
		public static void Shade(Form parent)
		{
			if (instance != null)
				return;
			
			instance = new ShadowForm();
			instance._Shade(parent);			
		}
		
		public static void Unshade()
		{
			if (instance == null)
				return;
			
			instance.Close();
			instance.Dispose();
			instance = null;			
		}
	}
}
