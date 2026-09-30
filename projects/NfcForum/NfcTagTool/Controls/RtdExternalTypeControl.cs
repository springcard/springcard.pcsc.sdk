using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SpringCard.NfcForum.Ndef;

namespace SpringCardApplication.Controls
{
    public partial class RtdExternalTypeControl : RtdControl
    {
        public RtdExternalTypeControl()
        {
            InitializeComponent();
        }

        public override void SetEditable(bool yes)
        {
            eType.ReadOnly = !yes;
            eData.ReadOnly = !yes;
        }

        public override void ClearContent()
        {
            eType.Text = "";
            eData.Text = "";
        }

        public override bool ValidateUserContent()
        {
            if (eType.Text.Equals(""))
            {
                MessageBox.Show("You must provide a text");
                return false;
            }
            return true;
        }

        public void SetContent(RtdExternalType externalType)
        {
            ClearContent();
            eType.Text = externalType.TYPE;
            eData.Text = externalType.PAYLOAD_Str;
        }

        public RtdExternalType GetContentEx()
        {
            RtdExternalType t = new RtdExternalType(eType.Text, eData.Text);
            return t;
        }

        public override void SetContent(NdefObject ndef)
        {
            if (ndef is RtdExternalType)
                SetContent((RtdExternalType)ndef);
        }

        public override NdefObject GetContent()
        {
            return GetContentEx();
        }
    }
}
