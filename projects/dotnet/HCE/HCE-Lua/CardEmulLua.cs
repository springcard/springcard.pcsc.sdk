/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 22/03/2017
 * Time: 16:34
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using SpringCard.PCSC;
using SpringCard.PCSC.CardEmulation;
using SpringCard.LibCs;
using MoonSharp.Interpreter;

namespace PcscHce
{
	/// <summary>
	/// Description of CardEmulLua.
	/// </summary>
	public class CardEmulLua : CardEmulBase
	{
		Script script;
		
		public CardEmulLua()
		{
			string[] ModulePaths = new string[] {
				@"./conf/hce-scripts/?",
				@"./conf/hce-scripts/?.lua",
				@"../conf/hce-scripts/?",
				@"../conf/hce-scripts/?.lua"
			};			
			
			script = new Script();			
			script.Options.ScriptLoader = new LuaLoader(ModulePaths);
			script.Options.DebugPrint = LuaToLogger;
			LuaLog.InjectIntoScript(ref script);
			LuaLoader.InjectIntoScript(ref script, ModulePaths);
			LuaBytes.InjectIntoScript(ref script);
			LuaCApdu.InjectIntoScript(ref script);
			LuaRApdu.InjectIntoScript(ref script);
			LuaHttpClient.InjectIntoScript(ref script);
			LoadHceScript();
			InitHceScript();
		}
		
		private void LoadHceScript()
		{
			try
			{
				script.DoFile("hce");
			}
			catch (ScriptRuntimeException e)
			{
				Logger.Error(e.DecoratedMessage);
			}
			catch (Exception e)
			{
				Logger.Error(e.Message);
			}
		}

		private void InitHceScript()
		{
			try
			{
				script.DoString("hce_init()");
			}
			catch (ScriptRuntimeException e)
			{
				Logger.Error(e.DecoratedMessage);
			}			
			catch (Exception e)
			{
				Logger.Error(e.Message);
			}
		}
		
		public void TestSelect()
		{
			OnCardSelect();
		}

		public void TestDeselect()
		{
			OnCardDeselect();
		}
		
		public void TestApdu()
		{
			CAPDU capdu;
			RAPDU rapdu;

			capdu = new CAPDU("00A4040010A000000614537072696E67426C756531");
			rapdu = OnApdu(capdu);
			
			capdu = new CAPDU("00DA010017C00101C1080123456789ABCDEFC208CCCCCCCCCCCCCCCC");
			rapdu = OnApdu(capdu);
			
			capdu = new CAPDU("00DA010012C310DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
			rapdu = OnApdu(capdu);

			capdu = new CAPDU("00CA010000");
			rapdu = OnApdu(capdu);

			capdu = new CAPDU("00CA010000");
			rapdu = OnApdu(capdu);
		}
		
		void LuaToLogger(string p)
		{
			Logger.Info(p);
		}		
	
		protected override RAPDU OnApdu(CAPDU capdu)
		{
			RAPDU rapdu = new RAPDU(0x6F, 00);
			
			Logger.Debug("Lua:C-Apdu:" + capdu.AsString());
			
			DynValue lua_c = DynValue.FromObject(script, new LuaCApdu(capdu));

			try
			{			
				DynValue lua_r = script.Call(script.Globals["hce_process"], lua_c);
				if ((lua_r != null) && !lua_r.IsNil())
				{	
					LuaRApdu r = lua_r.ToObject<LuaRApdu>();				
					rapdu = r.rapdu;				
				}
			}
			catch (ScriptRuntimeException e)
			{
				Logger.Error(e.DecoratedMessage);
			}			
			catch (Exception e)
			{
				Logger.Error(e.Message);
			}
				
			Logger.Debug("Lua:R-Apdu:" + rapdu.AsString());
			return rapdu;
		}
		
		protected override void OnCardSelect()
		{
			Logger.Debug("Lua:OnCardSelect");
			
			try
			{
				script.DoString("hce_select()");
			}
			catch (ScriptRuntimeException e)
			{
				Logger.Error(e.DecoratedMessage);
			}			
			catch (Exception e)
			{
				Logger.Error(e.Message);
			}
		}
		
		protected override void OnCardDeselect()
		{
			Logger.Debug("Lua:OnCardDeselect");
			
			try
			{
				script.DoString("hce_deselect()");
			}
			catch (ScriptRuntimeException e)
			{
				Logger.Error(e.DecoratedMessage);
			}			
			catch (Exception e)
			{
				Logger.Error(e.Message);
			}
		}
		
		
		
	
	}
}
