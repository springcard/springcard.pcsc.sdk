/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 28/03/2017
 * Time: 11:01
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using SpringCard.LibCs;
using SpringCard.PCSC;
using MoonSharp.Interpreter;

namespace PcscHce
{
	/// <summary>
	/// Description of LuaCApdu.
	/// </summary>
	public class LuaRApdu
	{
		public RAPDU rapdu;
		
		private LuaRApdu(RAPDU rapdu)
		{
			this.rapdu = rapdu;
		}
		
		public static LuaRApdu create(byte sw1, byte sw2)
		{
			return new LuaRApdu(new RAPDU(sw1, sw2));
		}

		public static LuaRApdu create(string data, byte sw1, byte sw2)
		{
			return new LuaRApdu(new RAPDU(data, sw1, sw2));
		}

		public static LuaRApdu create(LuaBytes data, byte sw1, byte sw2)
		{
			return new LuaRApdu(new RAPDU(data.tostring(), sw1, sw2));
		}
		
		public static void InjectIntoScript(ref Script script)
		{
			UserData.RegisterType<LuaRApdu>();
			DynValue obj = UserData.Create(new LuaRApdu(null));
			script.Globals.Set("rapdu", obj);
			script.DoString("log.print(log.DEBUG,\"RAPDU library loaded\")");
		}		
	}
}
