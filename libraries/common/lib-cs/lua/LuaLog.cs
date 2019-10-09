/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 28/03/2017
 * Time: 09:50
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using SpringCard.LibCs;
using MoonSharp.Interpreter;

namespace PcscHce
{
	/// <summary>
	/// Description of LuaLog.
	/// </summary>
	public class LuaLog
	{
		public static int DEBUG = (int) Logger.Level.Debug;
		public static int TRACE = (int) Logger.Level.Trace;
		public static int INFO = (int) Logger.Level.Info;
		public static int WARNING = (int) Logger.Level.Warning;
		public static int ERROR = (int) Logger.Level.Error;
				
		public void print(int level, string message)
		{
			Logger.Log((Logger.Level) level, message);
		}
		
		public static void InjectIntoScript(ref Script script)
		{
			UserData.RegisterType<LuaLog>();
			DynValue obj = UserData.Create(new LuaLog());
			script.Globals.Set("log", obj);
			script.DoString("log.print(log.DEBUG,\"Log library loaded\")");
		}
	}
}
