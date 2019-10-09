/**
 *
 * \ingroup PCSC 
 *  
 * \copyright
 *   Copyright (c) 2010-2018 SpringCard - www.springcard.com
 *   All right reserved
 *
 * \author
 *   Johann.D / SpringCard 
 *
 */
 /*
  *	This software is part of the SPRINGCARD SDK FOR PC/SC
  *
  *   Redistribution and use in source (source code) and binary
  *   (object code) forms, with or without modification, are
  *   permitted provided that the following conditions are met :
  *
  *   1. Redistributed source code or object code shall be used
  *   only in conjunction with products (hardware devices) either
  *   manufactured, distributed or developed by SPRINGCARD,
  *
  *   2. Redistributed source code, either modified or
  *   un-modified, must retain the above copyright notice,
  *   this list of conditions and the disclaimer below,
  *
  *   3. Redistribution of any modified code must be clearly
  *   identified "Code derived from original SPRINGCARD 
  *   copyrighted source code", with a description of the
  *   modification and the name of its author,
  *
  *   4. Redistributed object code must reproduce the above
  *   copyright notice, this list of conditions and the
  *   disclaimer below in the documentation and/or other
  *   materials provided with the distribution,
  *
  *   5. The name of SPRINGCARD may not be used to endorse
  *   or promote products derived from this software or in any
  *   other form without specific prior written permission from
  *   SPRINGCARD.
  *
  *   THIS SOFTWARE IS PROVIDED BY SPRINGCARD "AS IS".
  *   SPRINGCARD SHALL NOT BE LIABLE FOR INFRINGEMENTS OF THIRD
  *   PARTIES RIGHTS BASED ON THIS SOFTWARE.
  *
  *   ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
  *   LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
  *   FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
  *
  *   SPRINGCARD DOES NOT WARRANT THAT THE FUNCTIONS CONTAINED IN
  *   THIS SOFTWARE WILL MEET THE USER'S REQUIREMENTS OR THAT THE
  *   OPERATION OF IT WILL BE UNINTERRUPTED OR ERROR-FREE.
  *
  *   IN NO EVENT SHALL SPRINGCARD BE LIABLE FOR ANY DIRECT,
  *   INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
  *   DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
  *   SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
  *   OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
  *   LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
  *   (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF
  *   THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY
  *   OF SUCH DAMAGE. 
  *
  **/
using System;
using System.Windows.Forms;
using System.IO.Ports;
using SpringCard.PCSC.ZeroDriver;
using SpringCard.LibCs;
using SpringCard.LibCs.Windows;
using SpringCard.Bluetooth;
using System.Drawing;

namespace SpringCard.PCSC.Forms
{
	/**
	 * \brief A simple form to let the user select one reader, either PC/SC or using a direct PC/SC-like method
	 */
	public partial class ReaderSelectAnyForm : Form
	{
		const int pageIndexPcsc = 0;
		const int pageIndexSerial = 1;
		const int pageIndexNetwork = 2;
		const int pageIndexBle = 3;
		string bleDeviceAddress;

		BLE.Adapter bleAdapter;

		/**
		 * \brief Create the form
		 */
		public ReaderSelectAnyForm()
		{
			InitializeComponent();

			LoadSerialPortNames();

			cbRemember.Checked = AppConfig.ReadBoolean("reader_reconnect");
			if (AppConfig.ReadString("reader_mode") == "serial")
			{
				tabControl1.SelectedIndex = pageIndexSerial;
			}
			else if (AppConfig.ReadString("reader_mode") == "network")
			{
				tabControl1.SelectedIndex = pageIndexNetwork;
			}
			else if (AppConfig.ReadString("reader_mode") == "ble")
			{
				tabControl1.SelectedIndex = pageIndexBle;
			}
			else
			{
				tabControl1.SelectedIndex = pageIndexPcsc;
			}

			try
			{
				cbCcidOverSerialCommName.Text = AppConfig.ReadString("serial_port");
			}
			catch
			{

			}
            cbCcidOverSerialNotifications.Enabled = false; // JDA TODO
            cbCcidOverSerialNotifications.Checked = true; // AppConfig.ReadBoolean("serial_notifications");
            cbCcidOverSerialLPCD.Checked = AppConfig.ReadBoolean("serial_use_lpcd");

            try
			{
				eCcidOverTcpAddr.Text = AppConfig.ReadString("network_addr");
			}
			catch
			{

			}

			bleDeviceAddress = AppConfig.ReadString("ble_device_address");
			UpdateDisplay();
		}

		/**
		 * \brief Create the form, specifying the color of the header (default is SpringCard-red)
		 */
		public ReaderSelectAnyForm(Color headerColor) : this()
		{
			panel1.BackColor = headerColor;
		}


		void ReaderSelectAnyFormShown(object sender, EventArgs e)
		{
			if (btnRefresh.Visible)
				BtnRefreshLinkClicked(sender, null);
		}

		void LoadReaderListPCSC()
		{
			string[] readers = SCARD.Readers;

			lvReaders.BeginUpdate();
			lvReaders.Items.Clear();
			if (readers != null)
				for (int i = 0; i < readers.Length; i++)
					lvReaders.Items.Add(readers[i]);
			lvReaders.EndUpdate();
		}

		void LoadReaderListBLE()
		{
			Cursor.Current = Cursors.WaitCursor;
			Application.DoEvents();
			Enabled = false;

			bleAdapter = BLE.WindowsAdapter.Instance;

			if (!bleAdapter.Open())
			{
				MessageBox.Show("No access to the BLE interface!");
				bleAdapter.Close();
				bleAdapter = null;
				return;
			}

			bleAdapter.StartScan(3000, new SpringCard.Bluetooth.BLE.Adapter.ScanTerminatedCallback(OnBleScanTerminated));
		}

		void OnBleScanResponse(SpringCard.Bluetooth.BLE.DeviceInfo deviceInfo)
		{
			try
			{
				Logger.Trace("Found device " + deviceInfo.Address.ToString());
				string strAddress = deviceInfo.Address.ToString();
				if (deviceInfo.PrimaryServiceUuid != null)
				{
					Logger.Trace("\tPrimary service UUID=" + deviceInfo.PrimaryServiceUuid.ToString());
				}
			}
			catch { }
		}

		delegate void OnBleScanTerminatedInvoker(SpringCard.Bluetooth.BLE.DeviceInfo[] deviceInfoList);
		void OnBleScanTerminated(SpringCard.Bluetooth.BLE.DeviceInfo[] deviceInfoList)
		{
			if (InvokeRequired)
			{
				object[] parameters = new object[1];
				parameters[0] = deviceInfoList;
				Invoke(new OnBleScanTerminatedInvoker(OnBleScanTerminated), parameters);
				return;
			}

			Enabled = true;
			Cursor.Current = Cursors.Default;
			lvBleDevices.Items.Clear();
			Application.DoEvents();
			if (deviceInfoList == null)
			{
				MessageBox.Show(this,
								"Failed to scan for BLE readers. Please verify the BLE interface and try again.",
								AppUtils.ApplicationTitle(),
								MessageBoxButtons.OK,
								MessageBoxIcon.Exclamation);
			}
			else
			{
				if (deviceInfoList.Length != 0)
				{
					for (int i = 0; i < deviceInfoList.Length; i++)
					{
						SpringCard.Bluetooth.BLE.DeviceInfo deviceInfo = deviceInfoList[i];

						if (!SCardReaderList_CcidOverBle.ServiceUuidSupported(deviceInfo.PrimaryServiceUuid))
							continue;

						ListViewItem entry = new ListViewItem(deviceInfo.Address.ToString());
						entry.SubItems.Add(deviceInfo.GetNameString());
						entry.SubItems.Add(deviceInfo.GetPrimaryServiceUuidString());
						entry.SubItems.Add(deviceInfo.GetRssiString());
						entry.SubItems.Add(deviceInfo.GetStatusString());
						lvBleDevices.Items.Add(entry);

						if (deviceInfoList[i].Address.Equals(bleDeviceAddress))
						{
							lvBleDevices.Items[i].Selected = true;
							MessageBox.Show("Selecting " + i);
						}
					}
				}
				else
				{
					MessageBox.Show(this,
									"No compliant BLE reader found. Please verify that your device is turned ON and not already connected to another host.",
									AppUtils.ApplicationTitle(),
									MessageBoxButtons.OK,
									MessageBoxIcon.Exclamation);
				}
			}
		}

		//		void OnBleOpenInterface(bool result)
		//		{
		//			if (InvokeRequired)
		//			{
		//				Invoke(new SCardReaderList_CcidOverBle_BgApi.BooleanPromiseCallback(OnBleOpenInterface), result);
		//				return;
		//			}
		//			
		//			if (result)
		//			{
		//				MessageBox.Show(this,
		//					                "Connected.",
		//					                AppUtils.ApplicationTitle(),
		//					                MessageBoxButtons.OK,
		//					                MessageBoxIcon.Exclamation);					
		//				
		//				SCardReaderList_CcidOverBle_BgApi.BackgroundScan(new SCardReaderList_CcidOverBle_BgApi.BooleanPromiseCallback(OnBleScanTerminated), 3000);
		//			} else
		//			{
		//				Enabled = true;	
		//				Cursor.Current = Cursors.Default;
		//				Application.DoEvents();
		//
		//				MessageBox.Show(this,
		//				                "Failed to connect to the BLE interface. Please verify the BLE dongle and the serial port to use, and make sure that no other application is already using the dongle.",
		//				                AppUtils.ApplicationTitle(),
		//				                MessageBoxButtons.OK,
		//				                MessageBoxIcon.Exclamation);
		//			}
		//		}

		void LoadSerialPortNames()
		{
			string[] PortNames = SerialPort.GetPortNames();

			cbCcidOverSerialCommName.Items.Clear();
			if (PortNames != null)
			{
				for (int i = 0; i < PortNames.Length; i++)
					cbCcidOverSerialCommName.Items.Add(PortNames[i]);
			}
		}

		void UpdateDisplay()
		{
			if (tabControl1.SelectedIndex == pageIndexPcsc)
			{
				btnOK.Enabled = (lvReaders.SelectedItems.Count == 1);
				imgRefresh.Visible = true;
				btnRefresh.Visible = true;
			}
			else
			if (tabControl1.SelectedIndex == pageIndexSerial)
			{
				btnOK.Enabled = (cbCcidOverSerialCommName.Text != "");
				imgRefresh.Visible = false;
				btnRefresh.Visible = false;
			}
			else
			if (tabControl1.SelectedIndex == pageIndexNetwork)
			{
				btnOK.Enabled = (eCcidOverTcpAddr.Text != "");
				imgRefresh.Visible = false;
				btnRefresh.Visible = false;
			}
			else
			if (tabControl1.SelectedIndex == pageIndexBle)
			{
				btnOK.Enabled = (lvBleDevices.SelectedItems.Count == 1);
				imgRefresh.Visible = true;
				btnRefresh.Visible = true;
			}
			else
			{
				btnOK.Enabled = false;
			}
		}

		void BtnOKClick(object sender, EventArgs e)
		{
			AppConfig.WriteBoolean("reader_reconnect", cbRemember.Checked);

			if (tabControl1.SelectedIndex == pageIndexPcsc)
			{
				AppConfig.WriteString("reader_mode", "pc/sc");
				AppConfig.WriteString("reader_name", lvReaders.SelectedItems[0].Text);
			}
			else
			if (tabControl1.SelectedIndex == pageIndexSerial)
			{
				AppConfig.WriteString("reader_mode", "serial");
				AppConfig.WriteString("serial_port", cbCcidOverSerialCommName.Text);
                AppConfig.WriteBoolean("serial_notifications", cbCcidOverSerialNotifications.Checked);
                AppConfig.WriteBoolean("serial_use_lpcd", cbCcidOverSerialLPCD.Checked);
			}
			else
			if (tabControl1.SelectedIndex == pageIndexNetwork)
			{
				AppConfig.WriteString("reader_mode", "network");
				AppConfig.WriteString("network_addr", eCcidOverTcpAddr.Text);
				AppConfig.WriteString("network_port", "3999");
			}
			else
			if (tabControl1.SelectedIndex == pageIndexBle)
			{
				AppConfig.WriteString("reader_mode", "ble");
				bleDeviceAddress = lvBleDevices.SelectedItems[0].Text;
				AppConfig.WriteString("ble_device_address", bleDeviceAddress);
				AppConfig.WriteString("ble_device_primary_service", lvBleDevices.SelectedItems[0].SubItems[2].Text);
			}

			DialogResult = DialogResult.OK;
			Close();
		}

		void BtnCancelClick(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		/**
		 * \brief Get the selected mode: "pc/sc" for standard PC/SC, or either "serial", "network" or "ble" for the PC/SC-Like modes
		 */
		public string Mode
		{
			get
			{
				if (tabControl1.SelectedIndex == pageIndexPcsc) return "pc/sc";
				if (tabControl1.SelectedIndex == pageIndexSerial) return "serial";
				if (tabControl1.SelectedIndex == pageIndexNetwork) return "network";
				if (tabControl1.SelectedIndex == pageIndexBle) return "ble";
				return null;
			}
		}

		/**
		 * \brief If selected Mode is "pc/sc", get the name of the reader
		 */
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

		/**
		 * \brief If selected Mode is "serial", get the communication settings
		 */
		public void GetSerialParameters(out string PortName, out bool UseNotifications, out bool UseLpcdPolling)
		{
			PortName = cbCcidOverSerialCommName.Text;
            UseNotifications = cbCcidOverSerialNotifications.Checked;
			UseLpcdPolling = cbCcidOverSerialLPCD.Checked;
		}

		/**
		 * \brief If selected Mode is "network", get the reader's address (or DNS name) and communication port
		 */
		public void GetNetworkParameters(out string Address, out ushort Port)
		{
			Address = eCcidOverTcpAddr.Text;
			Port = 3999;
		}

		/**
		 * \brief If selected Mode is "ble", get the BLE adapter and the reader's BT_ADDR
		 */
		public void GetBleParameters(out BLE.Adapter Adapter, out BluetoothAddress DeviceAddress, out BluetoothUuid DevicePrimaryService)
		{
			if (lvBleDevices.SelectedItems.Count == 1)
			{
				DeviceAddress = new BluetoothAddress(lvBleDevices.SelectedItems[0].Text);
				DevicePrimaryService = new BluetoothUuid(lvBleDevices.SelectedItems[0].SubItems[2].Text);
			}
			else
			{
				DeviceAddress = null;
				DevicePrimaryService = null;
			}
			Adapter = bleAdapter;
		}

		void LvReadersSelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateDisplay();
		}

		void LvBleDevicesSelectedIndexChanged(object sender, EventArgs e)
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

		void LvBleDevicesDoubleClick(object sender, MouseEventArgs e)
		{
			if (lvBleDevices.SelectedItems.Count == 1)
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
			if (tabControl1.SelectedIndex == pageIndexPcsc)
			{
				LoadReaderListPCSC();
			}
			else
			if (tabControl1.SelectedIndex == pageIndexBle)
			{
				LoadReaderListBLE();
			}
		}

		void ImgRefreshClick(object sender, EventArgs e)
		{
			if (tabControl1.SelectedIndex == pageIndexPcsc)
			{
				LoadReaderListPCSC();
			}
			else
			if (tabControl1.SelectedIndex == pageIndexBle)
			{
				LoadReaderListBLE();
			}
		}

		void CbCcidOverSerialCommNameSelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateDisplay();
		}
		void CbCcidOverBleCommNameSelectedIndexChanged(object sender, EventArgs e)
		{

		}
	}
}
