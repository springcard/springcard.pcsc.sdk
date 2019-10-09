/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 28/03/2017
 * Time: 14:51
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using SpringCard.LibCs;
using MoonSharp.Interpreter;

namespace PcscHce
{
	/// <summary>
	/// Description of LuaLoader.
	/// </summary>
	public class LuaLoader : MoonSharp.Interpreter.Loaders.ScriptLoaderBase
	{
		public LuaLoader(string[] ModulePaths)
		{
			this.ModulePaths = ModulePaths;
		}

		public override object LoadFile(string filename, Table globalContext)
		{			
			string actualFileName = filename;
			
			if (!File.Exists(filename))
			{
				actualFileName = ResolveModuleName(filename, globalContext);
				if (actualFileName == null)
				{
					Logger.Error(String.Format("Lua:Unable to load file {0}", filename));
					throw new FileNotFoundException();
				}
			}

			Logger.Debug(String.Format("Lua:LoadFile({0})", actualFileName));			
			return new FileStream(actualFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		}
		
		public override bool ScriptFileExists(string filename)
		{
			return File.Exists(filename);
		}
		
		public bool module_exists(string filename)
		{
			if (!File.Exists(filename))
			{
				string actualFileName = ResolveModuleName(filename, ModulePaths);
				if (actualFileName == null)
					return false;
			}
			
			return true;
		}
		
		public static void InjectIntoScript(ref Script script, string[] ModulePaths)
		{
			UserData.RegisterType<LuaLoader>();
			DynValue obj = UserData.Create(new LuaLoader(ModulePaths));
			script.Globals.Set("loader", obj);
			script.DoString("log.print(log.DEBUG,\"Loader library loaded\")");
		}				
	}
}
