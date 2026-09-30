using Org.BouncyCastle.Crypto.Parameters;
using SpringCard.GoogleVas;
using SpringCard.LibCs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartTapRdr
{
    public partial class GoogleVASPublicKeyForm : Form
    {
        public GoogleVASPublicKeyForm(string privateKeyString)
        {
            InitializeComponent();
            ePrivateKey.Text = privateKeyString;
        }

        private void lkClose_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Close();
        }

        private void GoogleVASPublicKeyForm_Shown(object sender, EventArgs e)
        {
            try
            {
                byte[] privateKeyRaw = BinConvert.HexToBytes(ePrivateKey.Text);
                ECPrivateKeyParameters privateKey = GoogleVasCrypto.ECC.ImportPrivateKey(privateKeyRaw);
                ECPublicKeyParameters publicKey = GoogleVasCrypto.ECC.ExtractPublicKey(privateKey);
                ePublicKey.Text = BinConvert.ToHex(GoogleVasCrypto.ECC.EncodePublic(publicKey, true));
                ePublicKeyPem.Text = GoogleVasCrypto.ECC.EncodePublicPem(publicKey, 64, true);
            }
            catch { }
        }
    }
}
