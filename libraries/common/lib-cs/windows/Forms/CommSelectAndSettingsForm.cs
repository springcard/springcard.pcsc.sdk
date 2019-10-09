/*
 * Crée par SharpDevelop.
 * Utilisateur: johann
 * Date: 07/08/2013
 * Heure: 09:19
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO.Ports;

namespace SpringCard.LibCs
{
	/// <summary>
	/// Description of PortSelectForm.
	/// </summary>
	public partial class CommSelectAndSettingsForm : Form
	{
		Project _project;
		
		public CommSelectAndSettingsForm(Project project)
		{
			InitializeComponent();
			_project = project;
			if (_project == null)
				_project = new Project();
		}
		
		public Project GetProject()
		{
			return _project;
		}
		
		void CommSelectAndSettingsFormShown(object sender, EventArgs e)
		{
			BtnRefreshClick(sender, e);
			
			try
			{
				cbCommPort.Text = _project.GetCommPort();
			}
			catch (Exception)
			{
				if (cbCommPort.Items.Count > 0)
					cbCommPort.SelectedIndex = 0;
			}
			
			CommSettings c = _project.GetCommSettings();
			
			if (c.AsString() == "38400:8:N:1:")
			{
				rbDefault.Checked = true;
			} else
			{
				rbCustom.Checked = true;
				
				try
				{
					cbBitRate.Text = String.Format("{0}", c.BitRate);
					cbDataBits.Text = String.Format("{0}", c.DataBits);
					switch (c.Parity)
					{
						case Parity.Even:
							cbParity.Text = "Even";
							break;
						case Parity.Odd:
							cbParity.Text = "Odd";
							break;
						case Parity.None:
						default:
							cbParity.Text = "None";
							break;							
					}
					switch (c.StopBits)
					{
						case StopBits.Two:
							cbStopBits.Text = "2";
							break;
						case StopBits.OnePointFive:
							cbStopBits.Text = "1.5";
							break;
						case StopBits.One:
						default:
							cbStopBits.Text = "1";
							break;							
					}
					switch (c.Handshake)
					{
						case Handshake.RequestToSend:
							cbHandshake.Text = "Hardware";
							break;
						case Handshake.XOnXOff:
							cbHandshake.Text = "Xon/Xoff";
							break;
						case Handshake.None:
						default:
							cbHandshake.Text = "None";
							break;							
					}
				}
				catch (Exception)
				{
					
				}				
			}

			rbParamsChanged(sender, e);			
		}		
		
		void rbParamsChanged(object sender, EventArgs e)
		{
			if (rbCustom.Checked)
			{
				cbBitRate.Enabled = true;
				cbDataBits.Enabled = true;
				cbParity.Enabled = true;
				cbStopBits.Enabled = true;
				cbHandshake.Enabled = true;
			} else
			if (rbDefault.Checked)
			{
				cbBitRate.Enabled = false;
				cbBitRate.Text = "38400";
				cbDataBits.Enabled = false;
				cbDataBits.Text = "8";
				cbParity.Enabled = false;
				cbParity.Text = "None";
				cbStopBits.Enabled = false;
				cbStopBits.Text = "1";
				cbHandshake.Enabled = false;
				cbHandshake.Text = "None";
			}
		}
			
		void BtnRefreshClick(object sender, EventArgs e)
		{
			string oldComm = cbCommPort.Text;
			cbCommPort.Items.Clear();
			foreach (String portName in System.IO.Ports.SerialPort.GetPortNames())
			{
		         cbCommPort.Items.Add(portName);
			}
			try
			{
				cbCommPort.Text = oldComm;
			}
			catch (Exception)
			{
				
			}
			
			btnOK.Enabled = (cbCommPort.Text != "");
		}
		
		void CbCommSelectedIndexChanged(object sender, EventArgs e)
		{
			btnOK.Enabled = (cbCommPort.Text != "");
		}
		
		void BtnCancelClick(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}
		
		void BtnOKClick(object sender, EventArgs e)
		{
			_project.SetCommPort(cbCommPort.Text);
			
			CommSettings c = _project.GetCommSettings();
			
			int t;
			int.TryParse(cbBitRate.Text, out t);
			c.BitRate = t;
			int.TryParse(cbDataBits.Text, out t);
			c.DataBits = t;
			switch (cbParity.Text)
			{
				case "Odd":
					c.Parity = Parity.Odd;
					break;
				case "Even":
					c.Parity = Parity.Even;
					break;
				case "None":
				default :
					c.Parity = Parity.None;
					break;					
			}
			switch (cbStopBits.Text)
			{
				case "2" :
					c.StopBits = StopBits.Two;
					break;
				case "1.5" :
					c.StopBits = StopBits.OnePointFive;
					break;
				case "1" :
				default :
					c.StopBits = StopBits.One;
					break;
			}
			switch (cbHandshake.Text)
			{
				case "Hardware" :
					c.Handshake = Handshake.RequestToSend;
					break;
				case "Xon/Xoff" :
					c.Handshake = Handshake.XOnXOff;
					break;
				case "None":
				default :
					c.Handshake = Handshake.None;
					break;
			}
			
			_project.SetCommSettings(c);
			
			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
