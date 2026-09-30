using SpringCard.AppleVas;
using SpringCard.GoogleVas;
using SpringCard.LibCs;
using SpringCard.PCSC;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpringpassRdrLinux
{
    public partial class Form1 : Form
    {
        private List<string> readerList;
        SCardReader activeReader;
        GoogleVasConfig googleConfig;
        AppleVasTerminalConfig appleConfig;

        public Form1()
        {
            InitializeComponent();
            Cursor.Hide();

            readerList = new List<string>();
            googleConfig = GoogleVasConfig.SpringCardDemo();

            const string TestMerchantName = "pass.com.springcard.springblue.generic";
            const string TestKeyPrivate = "MHcCAQEEICp+PT7K8FQSOi2HED1Ar5RqxxN2EkiKJMCSfaL4htYNoAoGCCqGSM49AwEHoUQDQgAE9RCZaHxXUIjQFQnwKmq6+cVqFBNO6ZKQmekosMQRZmutPs8szUsiLokILdaiT/7F5qUl8qSfEvlocYy6z98jIw==";
            string json = $@"{{
                ""P2"" : ""FullVAS"",
                ""Capabilities"": ""SingleMode"",
	            ""Merchants"" : [
		            {{
			            ""Name"" : ""{TestMerchantName}"",
			            ""PrivateKey"" : ""{TestKeyPrivate}"",
			            ""Url"": ""https://springpass.springcard.com""
                    }}
	            ],
	            ""Description"" : ""FullVAS, DualMode, 2 merchant IDs with 2nd matching, merchants have an URL""
            }}";
            appleConfig = AppleVasTerminalConfig.LoadFromJson(json);

            LoadReaders();

            if (readerList.Count > 0)
            {
                string reader = readerList[0];
                Logger.Trace("Starting...");

                resetUiEvent();

                activeReader = new SCardReader(reader);
                activeReader.StartWaitCard(new SCardReader.CardConnectedCallback(CardConnectedCallback), new SCardReader.CardRemovedCallback(CardRemovedCallback));
            }
            else
            {
                ShowResult("No reader found");
            }
        }

        delegate void ShowResultInvoker(string DataMsg);
        void ShowResult(string DataMsg)
        {
            if (InvokeRequired)
            {
                this.BeginInvoke(new ShowResultInvoker(ShowResult), DataMsg);
                return;
            }

            if (DataMsg != null)
            {
                label1.Text = DataMsg;
            }
            else
            {
                label1.Text = "";
            }
        }

        void CardRemovedCallback()
        {
            //ShowResult("Waiting for NFC mobile...", null, false);
        }

        void CardConnectedCallback(SCardChannel cardChannel)
        {
            /* The CardConnected function is called as a delegate (callback) by the SCardReader object */
            /* within its backgroung thread. Therefore it is not allowed to use the UI objects.        */


            /* Apple exhange */

            AppleVasTerminal appleTerminal = new AppleVasTerminal();
            foreach (AppleVasConfig merchConfig in appleConfig.Merchants)
            {
                appleTerminal.AddConfig(merchConfig);
            }

            if (appleTerminal.DoTransaction(cardChannel, out AppleVasData data2, out AppleVasError error2, out RAPDU selectOseresponseApple))
            {
                ShowResult("Apple Wallet\n" + data2.Text);
                Task.Delay(5000).ContinueWith(t => resetUiEvent());
                return;
            }

            Logger.Trace("Card connected - Trying to run the transaction");

            /* Google exhange */

            GoogleVasTerminal googleTerminal = new GoogleVasTerminal(googleConfig);

            /* Provide selectOseResponseApple to speed up the transaction */
            if (googleTerminal.DoTransaction(cardChannel, out GoogleVasData data1, out GoogleVasError error1, selectOseresponseApple))
            {
                /* Convert JSON to SpringPass data with regex */
                Regex rx = new Regex(@"[0-9a-zA-Z]+\|[a-z0-9.-_]+@[a-z.]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

                MatchCollection matches = rx.Matches(data1.GetJsonString());

                if (matches.Count >= 1)
                {
                    ShowResult("Google Smart Tap\n" + matches[0].Value);
                    Task.Delay(5000).ContinueWith(t => resetUiEvent());
                }
                else
                {
                    ShowResult("Read succeded but no SpringPass data found");
                }
                return;
            }
        }

        void LoadReaders()
        {
            string[] readers = SCARD.Readers;
            readerList.Clear();
            if (readers != null)
            {
                foreach (string reader in readers)
                {
                    readerList.Add(reader);
                }
            }
        }

        private void resetUiEvent()
        {
            ShowResult("Waiting for NFC mobile...");
        }
    }
}
