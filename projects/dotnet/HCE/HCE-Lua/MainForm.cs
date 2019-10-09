/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 22/03/2017
 * Time: 16:28
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SpringCard.LibCs;
using MoonSharp.Interpreter;

namespace PcscHce
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
		CardEmulLua card;
		RestClient restClient;
		string cloud_token = null;
		string cloud_user_id = null;
		bool show_log = false;
		
		public MainForm()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
			
			Logger.LogCallback = new Logger.LogCallbackDelegate(LogCallback);
			restClient = new RestClient("http://rdr.springcard.com/cloud/api/v1");
		}
				
		public delegate void AppendLogDelegate(string message, Color color, bool newline);
		public void AppendLog(string message, Color color, bool newline)
		{
			if (richTextBox1.InvokeRequired)
			{
				object[] args = { message, color, newline };
				richTextBox1.Invoke(new AppendLogDelegate(AppendLog), args);
				return;
			}
			
			if (!show_log) return;
			
			try
			{
				richTextBox1.SelectionColor = color;				
				richTextBox1.AppendText(message);
				if (newline) richTextBox1.AppendText(Environment.NewLine);
				richTextBox1.SelectionStart = richTextBox1.TextLength;
				richTextBox1.ScrollToCaret();
				richTextBox1.Update();
			}
			catch
			{

			}
		}

		DateTime lastLogWhen = DateTime.MinValue;
		
		void LogCallback(Logger.Level level, string message)
		{
			string datetime_text;
			
			if (lastLogWhen == DateTime.MinValue)
			{
				datetime_text = "  '  \"    ";
			} else
			{
				TimeSpan ts = DateTime.Now - lastLogWhen;
				datetime_text = string.Format("{0:00}'{1:00}\"{2:000} ", ts.Minutes, ts.Seconds, ts.Milliseconds);
			}
			lastLogWhen = DateTime.Now;
			
			Color color;
			switch (level)
			{
				case Logger.Level.Debug :
					color = Color.DarkGray;
					break;
				case Logger.Level.Trace :
					color = Color.DarkBlue;
					break;
				case Logger.Level.Info :
					color = Color.Black;
					break;
				case Logger.Level.Warning :
					color = Color.Orange;
					break;
				case Logger.Level.Error :
				case Logger.Level.Fatal :					
					color = Color.Red;
					break;
				default :
					color = Color.Black;
					break;
			}
			
			AppendLog(datetime_text + message, color, true);
		}
		
//		private static IEnumerable<int> GetNumbers()
//		{
//			for (int i = 1; i <= 10; i++)
//				yield return i;
//		}
//		
//		private class LuaScriptLoader : MoonSharp.Interpreter.Loaders.ScriptLoaderBase
//		{
//			public LuaScriptLoader()
//			{
//				this.ModulePaths = new string[] {
//					@"../binaries/pcsc-hce-scripts/?",
//					@"../binaries/pcsc-hce-scripts/?.lua"
//				};
//			}
//
//			public override object LoadFile(string filename, Table globalContext)
//			{
//				Logger.Debug(String.Format("Lua:LoadFile({0})", filename));
//				
//				string actualFileName = filename;
//				
//				if (!File.Exists(filename))
//				{
//					actualFileName = ResolveModuleName(filename, globalContext);
//					if (actualFileName == null)
//					{
//						Logger.Error(String.Format("Lua:Unable to load file {0}", filename));
//						throw new FileNotFoundException();
//					}
//				}
//				
//				return new FileStream(actualFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
//			}
//			
//			public override bool ScriptFileExists(string filename)
//			{				
//				Logger.Debug(String.Format("Lua:ScriptFileExists({0}) ?", filename));
//				return File.Exists(filename);
//			}
//		}
//		
//		private static double EnumerableTest()
//		{
//			Script script = new Script();
//			
//			script.Options.ScriptLoader = new LuaScriptLoader();
//			
//			script.Globals["getNumbers"] = (Func<IEnumerable<int>>)GetNumbers;
//			
//			DynValue res = script.DoFile("test");
//
//			return res.Number;
//		}

		void Button1Click(object sender, EventArgs e)
		{
			card = new CardEmulLua();
		}
		
		void Button2Click(object sender, EventArgs e)		
		{
			if (card != null)
				card.TestSelect();
		}
		
		void Button3Click(object sender, EventArgs e)
		{
			if (card != null)
				card.TestDeselect();
		}
		
		void Button4Click(object sender, EventArgs e)
		{
			if (card != null)
				card.TestApdu();	
		}
		
		void Button5Click(object sender, EventArgs e)
		{
			show_log = true;
			
			card = new CardEmulLua();
      //card.Start("SpringCard H512 NFC 0");
      card.Start("SpringCard RX65N Contactless 0");
		}
		
		void Button6Click(object sender, EventArgs e)
		{
			if (card != null)
			{
				card.Stop();
				card = null;
			}
			
			show_log = false;
		}
		
		void Button7Click(object sender, EventArgs e)
		{
			JSON json = new JSON();
			json.Add("user", "test-user");
			json.Add("password", "test-password");
			json.Add("version", 1);
			
			JSONObject response = restClient.POST_Json("login", json);
			
			cloud_token = response["token"].StringValue;
			cloud_user_id = response["user_id"].StringValue;
			if (cloud_user_id.Length > 16)
				cloud_user_id = cloud_user_id.Substring(0, 16);
			
			Logger.Info(string.Format("token={0}, user_id={1}", cloud_token, cloud_user_id));
		}
	}
}
