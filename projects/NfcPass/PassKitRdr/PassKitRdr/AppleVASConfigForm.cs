using SpringCard.LibCs;
using SpringCard.AppleVas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SpringCard.LibCs.Windows;
using System.IO;
using Org.BouncyCastle.Crypto.Parameters;

namespace PassKitRdr
{
    public partial class AppleVASConfigForm : Form
    {
        /* Test data */
        const string TestMerchantName = "pass.com.springcard.springblue.generic";
        const string TestKeyPrivate = "MHcCAQEEICp+PT7K8FQSOi2HED1Ar5RqxxN2EkiKJMCSfaL4htYNoAoGCCqGSM49AwEHoUQDQgAE9RCZaHxXUIjQFQnwKmq6+cVqFBNO6ZKQmekosMQRZmutPs8szUsiLokILdaiT/7F5qUl8qSfEvlocYy6z98jIw==";
        // const string TestKeyPublic = "MDkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDIgAD9RCZaHxXUIjQFQnwKmq6+cVqFBNO6ZKQmekosMQRZms=";

        ECPrivateKeyParameters merchant1PrivateKey = null;
        int merchant1PrivateKeyIndex = -1;
        ECPrivateKeyParameters merchant2PrivateKey = null;
        int merchant2PrivateKeyIndex = -1;

        bool updating = false;

        public AppleVASConfigForm()
        {
            InitializeComponent();
        }

        private void AppleVASConfigForm_Load(object sender, EventArgs e)
        {
            rbBase64.Checked = true;
            cbAutoClear.Checked = AppConfig.ReadBoolean("AutoClear", true);
            udClearAfter.Value = AppConfig.ReadInteger("ClearAfter", 10);

            updating = true;
            eMerchant1Id.Text = AppConfig.ReadSettingString("MerchantId");
            eMerchant2Id.Text = AppConfig.ReadSettingString("MerchantId2");
            updating = false;

            eMerchant1Name.Text = AppConfig.ReadSettingString("MerchantName");
            if (eMerchant1Name.Text == "")
            {
                eMerchant1Id.ReadOnly = false;
                eMerchant1Id.BackColor = eMerchant1Name.BackColor;
            }

            eMerchant2Name.Text = AppConfig.ReadSettingString("MerchantName2");
            if (eMerchant2Name.Text == "")
            {
                eMerchant2Id.ReadOnly = false;
                eMerchant2Id.BackColor = eMerchant2Name.BackColor;
            }

            ePrivateKey1.Text = AppConfig.ReadSettingString("PrivateKey");
            if ((eMerchant1Name.Text == "") && (ePrivateKey1.Text == ""))
                lkSetDefault_LinkClicked(sender, null);

            ePrivateKey2.Text = AppConfig.ReadSettingString("PrivateKey2");
        }

        public AppleVasTerminalConfig GetConfig()
        {
            Logger.Debug("Loading terminal configuration");

            string merchantName1 = AppConfig.ReadSettingString("MerchantName", TestMerchantName);
            string merchantId1 = AppConfig.ReadSettingString("MerchantId");
            string privateKey1 = AppConfig.ReadSettingString("PrivateKey", TestKeyPrivate);

            string merchantName2 = AppConfig.ReadSettingString("MerchantName2");
            string merchantId2 = AppConfig.ReadSettingString("MerchantId2");
            string privateKey2 = AppConfig.ReadSettingString("PrivateKey2");

            string json = $@"{{
            ""P2"" : ""FullVAS"",
            ""Capabilities"": ""SingleMode"",
	        ""Merchants"" : [";

            if (string.IsNullOrEmpty(merchantName1))
            {
                Logger.Debug("MerchantId#1: {0}", merchantId1);
                json += $@"
		            {{
			            ""Id"" : ""{merchantId1}"",
			            ""PrivateKey"" : ""{privateKey1}""
                    }}";
            }
            else
            {
                Logger.Debug("MerchantName#1: {0}", merchantName1);
                json += $@"
		            {{
			            ""Name"" : ""{merchantName1}"",
			            ""PrivateKey"" : ""{privateKey1}""
                    }}";
            }
            Logger.Debug("PrivateKey#1: {0}", privateKey1);

            if (!string.IsNullOrEmpty(merchantName2) || !string.IsNullOrEmpty(merchantId2))
            {
                if (string.IsNullOrEmpty(merchantName2))
                {
                    Logger.Debug("MerchantId#2: {0}", merchantId2);
                    json += $@",
		            {{
			            ""Id"" : ""{merchantId2}"",
			            ""PrivateKey"" : ""{privateKey2}""
                    }}";
                }
                else
                {
                    Logger.Debug("MerchantName#2: {0}", merchantName2);
                    json += $@",
		            {{
			            ""Name"" : ""{merchantName2}"",
			            ""PrivateKey"" : ""{privateKey2}""
                    }}";
                }
                Logger.Debug("PrivateKey#2: {0}", privateKey2);
            }

            json += $@"
                ],
                ""Description"" : ""FullVAS, DualMode""
            }}";

            return AppleVasTerminalConfig.LoadFromJson(json);
        }

        private void lkSetDefault_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Logger.Debug("Set default merchant name=" + TestMerchantName);
            eMerchant1Name.Text = TestMerchantName;
            rbBase64.Checked = true;
            string str = cut(TestKeyPrivate);
            Logger.Debug("Set default private key=\n" + str);
            ePrivateKey1.Text = str;
            eMerchant2Name.Text = "";
            ePrivateKey2.Text = "";
        }

        private void eMerchantName1_TextChanged(object sender, EventArgs e)
        {
            if (eMerchant1Name.Text == "")
            {
                AppConfig.WriteSettingString("MerchantName", "");
                eMerchant1Id.ReadOnly = false;
                eMerchant1Id.BackColor = eMerchant1Name.BackColor;
                return;
            }

            eMerchant1Id.ReadOnly = true;
            eMerchant1Id.BackColor = ePublicKey1.BackColor;
            byte[] t = AppleVasConfig.ComputeId(eMerchant1Name.Text);
            Logger.Trace("MerchantId#1={0}", BinConvert.ToHex(t));
            eMerchant1Id.Text = BinConvert.ToHex_nice(t, ":", "", 0);
            AppConfig.WriteSettingString("MerchantName", eMerchant1Name.Text);
            AppConfig.WriteSettingString("MerchantId", eMerchant1Id.Text);
        }

        private void eMerchant1Id_TextChanged(object sender, EventArgs e)
        {
            if (!updating && (eMerchant1Name.Text == ""))
            {
                try
                {
                    byte[] t = BinConvert.HexToBytes(eMerchant1Id.Text);
                    if ((t != null) && (t.Length == 32))
                    {
                        Logger.Trace("MerchantId#1={0}", BinConvert.ToHex(t));
                        AppConfig.WriteSettingString("MerchantId", BinConvert.ToHex(t));
                        updating = true;
                        eMerchant1Id.Text = BinConvert.ToHex(t);
                        updating = false;
                    }
                }
                catch { }
            }
        }

        private void eMerchant2Name_TextChanged(object sender, EventArgs e)
        {
            if (eMerchant2Name.Text == "")
            {
                AppConfig.WriteSettingString("MerchantName2", "");
                eMerchant2Id.ReadOnly = false;
                eMerchant2Id.BackColor = eMerchant2Name.BackColor;
                return;
            }

            eMerchant2Id.ReadOnly = true;
            eMerchant2Id.BackColor = ePublicKey2.BackColor;
            byte[] t = AppleVasConfig.ComputeId(eMerchant2Name.Text);
            Logger.Trace("MerchantId#2={0}", BinConvert.ToHex(t));
            eMerchant2Id.Text = BinConvert.ToHex(t);
            AppConfig.WriteSettingString("MerchantName2", eMerchant2Name.Text);
            AppConfig.WriteSettingString("MerchantId2", eMerchant2Id.Text);
        }

        private void eMerchant2Id_TextChanged(object sender, EventArgs e)
        {
            if (!updating && (eMerchant2Name.Text == ""))
            {
                try
                {
                    byte[] t = BinConvert.HexToBytes(eMerchant2Id.Text);
                    if ((t != null) && (t.Length == 32))
                    {
                        Logger.Trace("MerchantId#2={0}", BinConvert.ToHex(t));
                        AppConfig.WriteSettingString("MerchantId2", BinConvert.ToHex(t));
                        updating = true;
                        eMerchant2Id.Text = BinConvert.ToHex(t);
                        updating = false;
                    }
                }
                catch { }
            }
        }

        private void ePrivateKey1_TextChanged(object sender, EventArgs e)
        {
            merchant1PrivateKey = null;
            ePublicKey1.Text = "";

            if (ePrivateKey1.Text == "")
            {
                AppConfig.WriteSettingString("PrivateKey", "");
                return;
            }

            string str = ePrivateKey1.Text;
            str = str.Replace(Environment.NewLine, "");
            str = str.Trim();

            if (str.Length == 0)
                return;


            if (str.Length == 1 || str.Length == 2)
            {
                if (str.Length == 1)
                    str = "0" + str;
                Logger.Trace("PublicKey#1 Index in Secure Element={0}", str);
                AppConfig.WriteSettingString("PrivateKey", str);
                eKeyId1.Text = "";
                ePublicKey1.Text = "";
                try
                {
                    merchant1PrivateKeyIndex = BinConvert.HexToByte(str);
                    merchant1PrivateKey = null;
                }
                catch
                {
                    return;
                }
            }
            else if (rbBase64.Checked)
            {
                try
                {
                    merchant1PrivateKey = AppleVasCrypto.ECC.ImportPrivateKey(StrUtils.Base64Decode(str));
                    merchant1PrivateKeyIndex = -1;
                }
                catch
                {
                    return;
                }
            }
            else if (rbHex.Checked)
            {
                try
                {
                    merchant1PrivateKey = AppleVasCrypto.ECC.ImportPrivateKey(BinConvert.HexToBytes(str));
                    merchant1PrivateKeyIndex = -1;
                }
                catch
                {
                    return;
                }
            }
            else
            {
                return;
            }

            if (AppleVasConfig.GetPrivateKeyData(merchant1PrivateKey, out uint keyId, out byte[] publicKey))
            {
                Logger.Trace("PrivateKey#1={0}", BinConvert.ToHex(AppleVasCrypto.ECC.EncodePrivate(merchant1PrivateKey)));
                Logger.Trace("KeyId#1={0}", BinConvert.ToHex(keyId));
                eKeyId1.Text = BinConvert.ToHex(keyId);
                Logger.Trace("PublicKey#1(raw)={0}", BinConvert.ToHex(publicKey));
                if (rbBase64.Checked)
                {
                    byte[] publicKeyPem = AppleVasConfig.EncodePublicPem(publicKey);
                    Logger.Trace("PublicKey#1(pem)={0}", BinConvert.ToHex(publicKeyPem));
                    ePublicKey1.Text = StrUtils.Base64Encode(publicKeyPem);
                }
                else if (rbHex.Checked)
                {
                    ePublicKey1.Text = BinConvert.ToHex(publicKey);
                }
                AppConfig.WriteSettingString("PrivateKey", AppleVasCrypto.ECC.EncodePrivatePem(merchant1PrivateKey));
            }
            else
            {
                merchant1PrivateKey = null;
            }
        }

        private void ePrivateKey2_TextChanged(object sender, EventArgs e)
        {
            merchant2PrivateKey = null;
            ePublicKey2.Text = "";

            if (ePrivateKey2.Text == "")
            {
                AppConfig.WriteSettingString("PrivateKey2", "");
                return;
            }

            string str = ePrivateKey2.Text;
            str = str.Replace(Environment.NewLine, "");
            str = str.Trim();

            if (str.Length == 0)
                return;

            if (str.Length == 1 || str.Length == 2)
            {
                if (str.Length == 1)
                    str = "0" + str;
                Logger.Trace("PublicKey#2 Index in Secure Element={0}", str);
                AppConfig.WriteSettingString("PrivateKey2", str);
                eKeyId2.Text = "";
                ePublicKey2.Text = "";
                try
                {
                    merchant2PrivateKeyIndex = BinConvert.HexToByte(str);
                    merchant2PrivateKey = null;
                }
                catch
                {
                    return;
                }
            }
            else if (rbBase64.Checked)
            {
                try
                {
                    merchant2PrivateKey = AppleVasCrypto.ECC.ImportPrivateKey(StrUtils.Base64Decode(str));
                    merchant2PrivateKeyIndex = -1;
                }
                catch
                {
                    return;
                }
            }
            else if (rbHex.Checked)
            {
                try
                {
                    merchant2PrivateKey = AppleVasCrypto.ECC.ImportPrivateKey(BinConvert.HexToBytes(str));
                    merchant2PrivateKeyIndex = -1;
                }
                catch
                {
                    return;
                }
            }
            else
            {
                return;
            }

            if (AppleVasConfig.GetPrivateKeyData(merchant2PrivateKey, out uint keyId, out byte[] publicKey))
            {
                Logger.Trace("PrivateKey#2={0}", BinConvert.ToHex(AppleVasCrypto.ECC.EncodePrivate(merchant2PrivateKey)));
                Logger.Trace("KeyId#2={0}", BinConvert.ToHex(keyId));
                eKeyId2.Text = BinConvert.ToHex(keyId);
                Logger.Trace("PublicKey#2(raw)={0}", BinConvert.ToHex(publicKey));
                if (rbBase64.Checked)
                {
                    byte[] publicKeyPem = AppleVasConfig.EncodePublicPem(publicKey);
                    Logger.Trace("PublicKey#2(pem)={0}", BinConvert.ToHex(publicKeyPem));
                    ePublicKey2.Text = StrUtils.Base64Encode(publicKeyPem);
                }
                else if (rbHex.Checked)
                {
                    ePublicKey2.Text = BinConvert.ToHex(publicKey);
                }
                AppConfig.WriteSettingString("PrivateKey2", AppleVasCrypto.ECC.EncodePrivatePem(merchant2PrivateKey));
            }
            else
            {
                merchant2PrivateKey = null;
            }
        }

        private void rbBase_CheckedChanged(object sender, EventArgs e)
        {
            if (merchant1PrivateKeyIndex != -1)
            {
                ePrivateKey1.Text = BinConvert.ToHex((byte)merchant1PrivateKeyIndex);
            }
            else if (merchant1PrivateKey != null)
            {
                if (rbHex.Checked)
                {
                    ePrivateKey1.Text = BinConvert.ToHex(AppleVasCrypto.ECC.EncodePrivate(merchant1PrivateKey));
                }
                else if (rbBase64.Checked)
                {
                    ePrivateKey1.Text = AppleVasCrypto.ECC.EncodePrivatePem(merchant1PrivateKey);
                }
            }

            if (merchant2PrivateKeyIndex != -1)
            {
                ePrivateKey2.Text = BinConvert.ToHex((byte)merchant2PrivateKeyIndex);
            }
            else if (merchant2PrivateKey != null)
            {
                if (rbHex.Checked)
                {
                    ePrivateKey2.Text = BinConvert.ToHex(AppleVasCrypto.ECC.EncodePrivate(merchant2PrivateKey));
                }
                else if (rbBase64.Checked)
                {
                    ePrivateKey2.Text = AppleVasCrypto.ECC.EncodePrivatePem(merchant2PrivateKey);
                }
            }
        }

        string cut(string str, int lineLength = 64)
        {
            string result = "";

            while (str.Length > lineLength)
            {
                result += str.Substring(0, lineLength);
                str = str.Substring(lineLength);
                if (str.Length > 0)
                    result += Environment.NewLine;
            }
            if (str.Length > 0)
                result += str;

            return result;
        }

        private void lkClose_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            AppConfig.WriteBoolean("AutoClear", cbAutoClear.Checked);
            AppConfig.WriteInteger("ClearAfter", (int)udClearAfter.Value);
            Close();
        }

        private void cbAutoClear_CheckedChanged(object sender, EventArgs e)
        {
            udClearAfter.Enabled = cbAutoClear.Checked;
        }

    }
}
