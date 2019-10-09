/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 29/03/2017
 * Time: 15:55
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using SpringCard.LibCs;
using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace PcscHce
{
	/// <summary>
	/// Description of LuaRestClient.
	/// </summary>
	public class LuaHttpClient
	{
		public LuaHttpClient()
		{
		}
		
		public DynValue post_json(string url, DynValue request_data)
		{
			Logger.Info("post_json:" + url);
			
			JSON json = new JSON();
			
			Table table = request_data.Table;
			foreach (TablePair pair in table.Pairs)
			{
				if (pair.Value.Type == DataType.String)
				{
					json.Add(pair.Key.String, pair.Value.String);
				} else
				if (pair.Value.Type == DataType.Number)
				{
					json.Add(pair.Key.String, pair.Value.Number);				
				} else
				if (pair.Value.Type == DataType.Boolean)
				{
					json.Add(pair.Key.String, pair.Value.Boolean);				
				}
			}			

			RestClient restClient = new RestClient();
			JSONObject response = restClient.POST_Json(url, json);
			
			DynValue result = DynValue.NewPrimeTable();
			
			foreach (KeyValuePair<string, JSONObject> item in response.ObjectValue)
			{
				if (item.Value.Kind == JSONObjectType.String)
				{
					if (item.Key == "user_id")
					{
						string s = item.Value.StringValue;
						s = s.Substring(0, 16);
						result.Table.Set(item.Key, DynValue.NewString(s));
						continue;
					}
					
					result.Table.Set(item.Key, DynValue.NewString(item.Value.StringValue));
				} else
				if (item.Value.Kind == JSONObjectType.Number)
				{				
					result.Table.Set(item.Key, DynValue.NewNumber(item.Value.DoubleValue));
				} else
				if (item.Value.Kind == JSONObjectType.Boolean)
				{				
					result.Table.Set(item.Key, DynValue.NewBoolean(item.Value.BooleanValue));
				}
			}
						
			return result;
		}
		
		public static void InjectIntoScript(ref Script script)
		{
			UserData.RegisterType<LuaHttpClient>();
			DynValue obj = UserData.Create(new LuaHttpClient());
			script.Globals.Set("httpclient", obj);
			script.DoString("log.print(log.DEBUG,\"REST Client library loaded\")");
		}
	}
}
