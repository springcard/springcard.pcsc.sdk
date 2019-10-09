/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 28/03/2017
 * Time: 11:01
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using SpringCard.PCSC;
using MoonSharp.Interpreter;

namespace PcscHce
{
	/// <summary>
	/// Description of LuaCApdu.
	/// </summary>
	public class LuaCApdu
	{
		private CAPDU capdu;
		
		public LuaCApdu(CAPDU capdu)
		{
			this.capdu = capdu;
		}
		
		public byte CLA { get { return capdu.CLA; } }
		public byte INS { get { return capdu.INS; } }
		public byte P1 { get { return capdu.P1; } }
		public byte P2 { get { return capdu.P2; } }
		public uint Lc { get { return capdu.Lc; } }
		public string Data { get { return DataString; } }
		public string DataString { get { if (capdu.data == null) return null; return capdu.data.AsString(); } }
		public byte[] DataBytes { get { if (capdu.data == null) return null; return capdu.data.GetBytes(); } }
		public uint Le { get { return capdu.Le; } }
		
		public static void InjectIntoScript(ref Script script)
		{
			UserData.RegisterType<LuaCApdu>();
			DynValue obj = UserData.Create(new LuaCApdu(null));
			script.Globals.Set("capdu", obj);
			script.DoString("log.print(log.DEBUG,\"CAPDU library loaded\")");
		}		
	}
}
