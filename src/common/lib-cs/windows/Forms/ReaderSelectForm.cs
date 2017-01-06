/**h* SpringCardApplication/ReaderSelectForm
 *
 * NAME
 *   SpringCard API for NFC Forum
 * 
 * COPYRIGHT
 *   Copyright (c) SpringCard SAS, 2012-2013
 *   See LICENSE.TXT for information
 *
 **/
using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;
using SpringCard.PCSC;
using SpringCard.LibCs;

namespace SpringCardApplication
{

	/**c* SpringCardApplication/ReaderSelectForm
	 *
	 * NAME
	 *   DesfireFormatForm
	 * 
	 * DESCRIPTION
	 *   Enables to specify the contactless reader to be used
	 *   with this application
	 * 
	 *
	 **/
	public partial class ReaderSelectForm : Form
	{
		string _selected_reader = null;
		string _preselect_mask = null;
		
		public ReaderSelectForm()
		{
			InitializeComponent();
			LoadReaderList();

			string last_reader = AppConfig.ReadString("reader_name");
			
			if (!String.IsNullOrEmpty(last_reader) && (lvReaders.Items.Count > 0))
			{
				for (int i=0; i<lvReaders.Items.Count; i++)
				{
					if (lvReaders.Items[i].Text.Equals(last_reader))
					{
						lvReaders.Items[i].Selected = true;
						
						_selected_reader = last_reader;
						cbRemember.Checked = AppConfig.ReadBoolean("reader_reconnect");
						break;
					}
				}
			}
		}
		
		public ReaderSelectForm(Color headerColor) : this()
		{
			panel1.BackColor = headerColor;
		}
		
		public string SelectedReader
		{
			get
			{
				return _selected_reader;
			}
			set
			{
				lvReaders.SelectedItems.Clear();
				for (int i=0; i<lvReaders.Items.Count; i++)
					if (lvReaders.Items[i].Text.Equals(value))
						lvReaders.Items[i].Selected = true;
			}
		}
		
		public void Preselect(string preselect_mask)
		{
			_preselect_mask = preselect_mask;
		}

		void LoadReaderList()
		{
			string[] readers = SCARD.Readers;
			
			lvReaders.BeginUpdate();
			lvReaders.Items.Clear();
			if (readers != null)
				for (int i=0; i<readers.Length; i++)
					lvReaders.Items.Add(readers[i]);
			lvReaders.EndUpdate();
		}
		
		void LvReadersDoubleClick(object sender, EventArgs e)
		{
			if (lvReaders.SelectedItems.Count == 1)
			{
				BtnOKClick(sender, e);
			}
		}
		
		void BtnCancelClick(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}
		
		void BtnOKClick(object sender, EventArgs e)
		{
			AppConfig.WriteBoolean("reader_reconnect", cbRemember.Checked);
			bool canQuit = true;
			try
			{
				_selected_reader = lvReaders.SelectedItems[0].Text;
				AppConfig.WriteString("reader_name", _selected_reader);
			}
			catch(ArgumentOutOfRangeException exp)
			{
				MessageBox.Show("No reader selected!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				canQuit = false;
			}
				
			if(canQuit)
			{
				DialogResult = DialogResult.OK;
				Close();		
			}
		}
		
		void ReaderSelectFormShown(object sender, EventArgs e)
		{
			LoadReaderList();
			
			if (_selected_reader != null)
			{
				for (int i=0; i<lvReaders.Items.Count; i++)
				{
					if (lvReaders.Items[i].Text.Equals(_selected_reader))
					{
						lvReaders.SelectedItems.Clear();
						lvReaders.Items[i].Selected = true;
						btnOK.Enabled = true;
						btnOK.Focus();
						return;
					}
				}
			}
			
			if (_preselect_mask != null)
			{
				string[] mask = _preselect_mask.Split('|');
				
				for (int i=0; i<lvReaders.Items.Count; i++)
				{
					bool f = true;
					for (int j=0; j<mask.Length; j++)
					{
						if (!lvReaders.Items[i].Text.Contains(mask[j]))
						{
							f = false;
							break;
						}
					}
					if (f)
					{
						lvReaders.SelectedItems.Clear();
						lvReaders.Items[i].Selected = true;
						btnOK.Enabled = true;
						btnOK.Focus();
						return;
					}
				}
			}
		}
		
		void LvReadersSelectedIndexChanged(object sender, EventArgs e)
		{
			for (int i=0; i<lvReaders.Items.Count; i++)
				lvReaders.Items[i].ImageIndex = (lvReaders.Items[i].Selected) ? 0 : -1;

			UpdateDisplay();
		}
		
		void UpdateDisplay()
		{
			btnOK.Enabled = (lvReaders.SelectedItems.Count == 1);
		}
		
		void BtnRefreshLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			LoadReaderList();
		}
		
		void ImgRefreshClick(object sender, EventArgs e)
		{
			LoadReaderList();	
		}
	}
}
