/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 02/09/2014
 * Time: 11:46
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml;
using System.Threading;
using System.Windows.Forms;
using SpringCard.LibCs.Windows;
using SpringCard.PCSC;
using MultiTest.Classes;
using SpringCard.LibCs.Windows.Forms;
using SpringCard.LibCs;

namespace MultiTest
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
        bool ShowSplash = true;

        TestSuites testSuites;
		TestSuite currentTestSuite;

		private volatile bool loopAllowed;
		private Thread scriptThread;
		private string scriptReader;
		private XmlNode scriptNode;
		
		public MainForm()
		{
            Logger.Trace("MultiTest starting");
			
            InitializeComponent();
			
            Text = AppUtils.ApplicationTitle(true);
		}
		
		void MainFormLoad(object sender, EventArgs e)
		{
            testSuites = new TestSuites();

            foreach (string title in testSuites.Titles)
            {
                cbTestSuite.Items.Add(title);
                if (title.Equals(AppConfig.ReadString("TestSuite")))
                    cbTestSuite.SelectedIndex = cbTestSuite.Items.Count - 1;
            }

            if ((cbTestSuite.Items.Count > 0) && (cbTestSuite.SelectedIndex < 0))
                cbTestSuite.SelectedIndex = 0;

            RefreshReaderList();
		}

        void OpenTestSuite()
        {
            treeView1.BeginUpdate();
            treeView1.Nodes.Clear();
            currentTestSuite.LoadTree(ref treeView1);
            treeView1.ExpandAll();
            treeView1.EndUpdate();
        }

        private void RefreshReaderList()
		{
			string[] readers = SCARD.Readers;
			cbReader.Items.Clear();
            if (readers != null)
            {
                foreach (string reader in readers)
                {
                    cbReader.Items.Add(reader);
                    if (reader.Equals(AppConfig.ReadString("PcscReader")))
                        cbReader.SelectedIndex = cbReader.Items.Count - 1;
                }
            }

            if ((cbReader.Items.Count > 0) && (cbReader.SelectedIndex < 0))
                cbReader.SelectedIndex = 0;
		}
		
				
		void BtnRunClick(object sender, EventArgs e)
		{
			RunScriptOnce();
		}
		
		void TreeView1DoubleClick(object sender, EventArgs e)
		{
			RunScriptOnce();
		}
		
		void BtnStopClick(object sender, EventArgs e)
		{
			btnStop.Enabled = false;
			loopAllowed = false;
		}
		
		void BtnStartClick(object sender, EventArgs e)
		{
			RunScriptLoop();
		}
		
		bool RunCommand(SCardChannel channel, string s_cmd)
		{
            try
            {
                CardBuffer cmd = new CardBuffer(s_cmd);
                CardBuffer rsp = channel.Control(cmd);

                if (rsp != null)
                {
                    Console.WriteLine(cmd.AsString() + " -> " + rsp.AsString());

                    string r = rsp.AsString();
                    if (r.Length > 2)
                    {
                        r = r.Substring(0, 2) + " " + r.Substring(2);
                    }

                    richTextBoxResponses.Text += r + "\n";
                    return true;
                }
            }
            catch
            {

            }

            return false;
        }
		
		private void ScriptRoutine()
		{
            if (scriptNode.HasChildNodes)
            {
                SCardChannel channel = new SCardChannel(scriptReader);

                try
                {
                    channel.ProtocolAsString = "DIRECT";
                    channel.Connect();
                }
                catch
                {
                }

                if (channel.Connected)
                { 
                    XmlNode xmlNode = scriptNode.FirstChild;
                    while (xmlNode != null)
                    {
                        if (xmlNode.Name == "command")
                        {
                            if (!RunCommand(channel, xmlNode.InnerText))
                                break;
                        }
                        else if (xmlNode.Name == "loop")
                        {
                            do
                            {
                                XmlNode xmlSubNode = xmlNode.FirstChild;
                                while (xmlSubNode != null)
                                {
                                    if (xmlSubNode.Name == "command")
                                    {
                                        if (!RunCommand(channel, xmlSubNode.InnerText))
                                            break;
                                    }
                                    xmlSubNode = xmlSubNode.NextSibling;
                                }
                            } while (loopAllowed);
                        }


                        xmlNode = xmlNode.NextSibling;
                    }

                    channel.Disconnect();
                }
            }
		
			OnScriptDone();
		}
		
		private void OnScriptDone()
		{
			Invoke((MethodInvoker) delegate {
				cbReader.Enabled = true;
				treeView1.Enabled = true;
				TreeView1AfterSelect(null, null);
			});						
		}
		
		void RunScriptOnce()
		{
			RunScript(false);
		}

		void RunScriptLoop()
		{
			RunScript(true);
		}
		
		void RunScript(bool doLoop)
		{					
			if (treeView1.SelectedNode == null)
				return;
			if (treeView1.SelectedNode.Tag == null)
				return;
			if (!(treeView1.SelectedNode.Tag is XmlNode))
				return;
			
			RunScript((XmlNode) treeView1.SelectedNode.Tag, doLoop);
		}
		
		void RunScript(XmlNode xmlRoot, bool doLoop)
		{
			if (cbReader.SelectedIndex < 0)
				return;

            richTextBoxResponses.Text = "";

            cbReader.Enabled = false;
			treeView1.Enabled = false;			
			
			btnRun.Enabled = false;
			btnStart.Enabled = false;
			btnStop.Enabled = doLoop;
						
			loopAllowed = doLoop;
			
			scriptNode = xmlRoot;		
			scriptReader = cbReader.Text;
			scriptThread = new Thread(ScriptRoutine);
			scriptThread.Start();
		}
		
		void TreeView1AfterSelect(object sender, TreeViewEventArgs e)
		{
			btnRun.Enabled = false;
			btnStart.Enabled = false;
			btnStop.Enabled = false;

            richTextBoxCommands.Text = "";

            if (treeView1.SelectedNode == null)
				return;
			if (treeView1.SelectedNode.Tag == null)
				return;
			if (!(treeView1.SelectedNode.Tag is XmlNode))
				return;
			
			XmlNode xmlNode = (XmlNode) treeView1.SelectedNode.Tag;
			bool hasCommands = HasCommands(xmlNode);
			bool hasLoop = HasLoop(xmlNode);

            richTextBoxCommands.Text = GetCommandsAsText(xmlNode);
		
			btnRun.Enabled = hasCommands;
			btnStart.Enabled = hasCommands && hasLoop;
		}

        private string GetCommandsAsText(XmlNode xmlRoot)
        {
            List<string> result = new List<string>();

            XmlNode xmlNode = xmlRoot.FirstChild;
            while (xmlNode != null)
            {
                if (xmlNode.Name == "command")
                {
                    result.Add(xmlNode.InnerText);
                }
                xmlNode = xmlNode.NextSibling;
            }

            return string.Join("\n", result);
        }

        private bool HasCommands(XmlNode xmlRoot)
		{
			XmlNode xmlNode = xmlRoot.FirstChild;
			while (xmlNode != null)
			{					
				if (xmlNode.Name == "command")
					return true;
				if (xmlNode.Name == "loop")
					if (HasCommands(xmlNode))
						return true;				
				xmlNode = xmlNode.NextSibling;
			}
			return false;
		}
		
		private bool HasLoop(XmlNode xmlRoot)
		{
			XmlNode xmlNode = xmlRoot.FirstChild;
			while (xmlNode != null)
			{					
				if (xmlNode.Name == "loop")
					return true;
				xmlNode = xmlNode.NextSibling;
			}
			return false;
		}

        private void lkRefresh_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            RefreshReaderList();
        }

        private void lkAbout_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            AboutForm.DoShowDialog(this);
        }

        private void cbTestSuite_SelectedIndexChanged(object sender, EventArgs e)
        {
            treeView1.Nodes.Clear();
            richTextBoxCommands.Text = "";
            richTextBoxResponses.Text = "";
            if (cbTestSuite.SelectedIndex >= 0)
            {
                currentTestSuite = testSuites[cbTestSuite.SelectedIndex];
                AppConfig.WriteString("TestSuite", currentTestSuite.Title);
                OpenTestSuite();
            }
        }

        private void cbReader_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbReader.SelectedIndex >= 0)
            {
                AppConfig.WriteString("PcscReader", cbReader.Text);
            }
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            if (ShowSplash)
            {
                Logger.Trace("Showing splash form");
                SplashForm.DoShowDialog(this);
            }
        }
    }	
}
