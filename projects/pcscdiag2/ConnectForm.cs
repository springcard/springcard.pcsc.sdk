/*
 * Crée par SharpDevelop.
 * Utilisateur: johann
 * Date: 26/03/2012
 * Heure: 09:22
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using SpringCard.PCSC;

namespace PcscDiag2
{
  /// <summary>
  /// Description of ConnectForm.
  /// </summary>
  public partial class ConnectForm : Form
  {
    public uint ShareMode;
    public uint Protocol;
    
    public ConnectForm()
    {
      InitializeComponent();
    }
    
    void ConnectFormLoad(object sender, EventArgs e)
    {
      ShareMode = Settings.DefaultConnectShare;
      Protocol = Settings.DefaultConnectProtocol;     
      
      switch (ShareMode)
      {
        case SCARD.SHARE_SHARED :
          rbShared.Checked = true;
          break;
        case SCARD.SHARE_EXCLUSIVE :
          rbExclusive.Checked = true;
          break;
        case SCARD.SHARE_DIRECT :
          rbDirect.Checked = true;
          break;
        default :
          rbExclusive.Checked = true;
          break;
      }
      
      switch (Protocol)
      {
        case SCARD.PROTOCOL_NONE :
          rbNone.Checked = true;
          break;
        case SCARD.PROTOCOL_T0 :
          rbT0.Checked = true;
          break;
        case SCARD.PROTOCOL_T1 :
          rbT1.Checked = true;
          break;
        case SCARD.PROTOCOL_RAW :
          rbRaw.Checked = true;
          break;
        case (SCARD.PROTOCOL_T0|SCARD.PROTOCOL_T1) :
        default :
          rbT0orT1.Checked = true;
          break;
      }
    }
    
    void BtnOKClick(object sender, EventArgs e)
    {
      if (rbShared.Checked) ShareMode = SCARD.SHARE_SHARED;
      if (rbExclusive.Checked) ShareMode = SCARD.SHARE_EXCLUSIVE;
      if (rbDirect.Checked) ShareMode = SCARD.SHARE_DIRECT;

      if (rbNone.Checked) Protocol = SCARD.PROTOCOL_NONE;
      if (rbT0.Checked) Protocol = SCARD.PROTOCOL_T0;
      if (rbT1.Checked) Protocol = SCARD.PROTOCOL_T1;
      if (rbT0orT1.Checked) Protocol = (SCARD.PROTOCOL_T0|SCARD.PROTOCOL_T1);
      if (rbRaw.Checked) Protocol = SCARD.PROTOCOL_RAW;
      
      Settings.DefaultConnectShare = ShareMode;
      Settings.DefaultConnectProtocol = Protocol;
      
      DialogResult = DialogResult.OK;
      Close();
    }
    
    
    void BtnCancelClick(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }
    
    void RbDirectCheckedChanged(object sender, EventArgs e)
    {
      if (rbDirect.Checked)
        rbNone.Checked = true;
    }
    
    void RbNoneCheckedChanged(object sender, EventArgs e)
    {
      if (rbNone.Checked)
        rbDirect.Checked = true;
    }
  }
}
