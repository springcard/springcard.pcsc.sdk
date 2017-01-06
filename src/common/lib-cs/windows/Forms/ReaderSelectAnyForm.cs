/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 04/24/2015
 * Time: 13:57
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Windows.Forms;
using System.IO.Ports;
using SpringCard.PCSC;
using SpringCard.LibCs;

namespace SpringCard.LibCs
{
	/// <summary>
	/// Description of ReaderSelectAnyForm.
	/// </summary>
	public partial class ReaderSelectAnyForm : Form
	{
		public ReaderSelectAnyForm()
		{
			InitializeComponent();
			
			LoadReaderListPCSC();
			LoadSerialPortNames();			
			
			cbRemember.Checked = AppConfig.ReadBoolean("reader_reconnect");
			if (AppConfig.ReadString("reader_mode") == "serial") tabControl1.SelectedIndex = 1; else
			if (AppConfig.ReadString("reader_mode") == "network") tabControl1.SelectedIndex = 2; else tabControl1.SelectedIndex = 0;

			try
			{
				cbCcidOverSerialCommName.Text = AppConfig.ReadString("serial_port");
			}
			catch
			{
				
			}
			cbCcidOverSerialLPCD.Checked = AppConfig.ReadBoolean("serial_use_lpcd");			

			try
			{
				eCcidOverTcpAddr.Text = AppConfig.ReadString("network_addr");
			}
			catch
			{
				
			}
			
			UpdateDisplay();
		}
		
		void LoadReaderListPCSC()
		{
			string[] readers = SCARD.Readers;
			
			lvReaders.BeginUpdate();
			lvReaders.Items.Clear();
			if (readers != null)
				for (int i=0; i<readers.Length; i++)
					lvReaders.Items.Add(readers[i]);
			lvReaders.EndUpdate();			
		}
		
		void LoadSerialPortNames()
		{
			string[] PortNames = SerialPort.GetPortNames();
			
			cbCcidOverSerialCommName.Items.Clear();
			if (PortNames != null)
			{
				for (int i=0; i<PortNames.Length; i++)
					cbCcidOverSerialCommName.Items.Add(PortNames[i]);
			}
		}
		
		void UpdateDisplay()
		{
			if (tabControl1.SelectedIndex == 0)
			{
				btnOK.Enabled = (lvReaders.SelectedItems.Count == 1);
			}
			else if (tabControl1.SelectedIndex == 1)
				btnOK.Enabled = (cbCcidOverSerialCommName.Text != "");
			else if (tabControl1.SelectedIndex == 2) {
				btnOK.Enabled = (eCcidOverTcpAddr.Text != "");
			} else {
				btnOK.Enabled = false;
			}
			
			imgRefresh.Visible = btnRefresh.Visible = (tabControl1.SelectedIndex == 0);
		}
			
		void BtnOKClick(object sender, EventArgs e)
		{
			AppConfig.WriteBoolean("reader_reconnect", cbRemember.Checked);
			
			if (tabControl1.SelectedIndex == 0)
			{
				AppConfig.WriteString("reader_mode", "pc/sc");
				AppConfig.WriteString("reader_name", lvReaders.SelectedItems[0].Text);
			}
			else if (tabControl1.SelectedIndex == 1)
			{
				AppConfig.WriteString("reader_mode", "serial");
				AppConfig.WriteString("serial_port", cbCcidOverSerialCommName.Text);
				AppConfig.WriteBoolean("serial_use_lpcd", cbCcidOverSerialLPCD.Checked);
			}
			else if (tabControl1.SelectedIndex == 2)
			{
				AppConfig.WriteString("reader_mode", "network");
				AppConfig.WriteString("network_addr", eCcidOverTcpAddr.Text);
				AppConfig.WriteString("network_port", "3999");
			}
			
			DialogResult = DialogResult.OK;
			Close();
		}
		
		void BtnCancelClick(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();	
		}
		
		public string Mode
		{
			get
			{
				if (tabControl1.SelectedIndex == 0) return "pc/sc";
				if (tabControl1.SelectedIndex == 1) return "serial";
				if (tabControl1.SelectedIndex == 2) return "network";
				return null;
			}
		}

		public void GetPCSCParameters(out string ReaderName)
		{
			if (lvReaders.SelectedItems.Count == 1)
			{
				ReaderName = lvReaders.SelectedItems[0].Text;
			}
			else
			{
				ReaderName = "";
			}
		}
		
		public void GetSerialParameters(out string PortName, out bool UseNotifications, out bool UseLpcdPolling)
		{
			PortName = cbCcidOverSerialCommName.Text;
			UseNotifications = true;
			UseLpcdPolling = cbCcidOverSerialLPCD.Checked;
		}
		
		public void GetNetworkParameters(out string Address, out ushort Port)
		{
			Address = eCcidOverTcpAddr.Text;
			Port = 3999;
		}
		
		void LvReadersSelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateDisplay();
		}
		
		void LvReadersDoubleClick(object sender, EventArgs e)
		{
			if (lvReaders.SelectedItems.Count == 1)
			{
				BtnOKClick(sender, e);
			}
		}
		
		void TabControl1SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateDisplay();
		}
		
		void CbCcidOverSerialCommNameTextUpdate(object sender, EventArgs e)
		{
			UpdateDisplay();
		}

		void ECcidOverTcpAddrTextChanged(object sender, EventArgs e)
		{
			UpdateDisplay();	
		}
		
		void BtnRefreshLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			LoadReaderListPCSC();
		}
		
		void ImgRefreshClick(object sender, EventArgs e)
		{
			LoadReaderListPCSC();	
		}
		
		void CbCcidOverSerialCommNameSelectedIndexChanged(object sender, EventArgs e)
		{	
			UpdateDisplay();
		}
	}
}
