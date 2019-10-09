/*
 * Created by SharpDevelop.
 * User: Jerome Izaac
 * Date: 29/02/2016
 * Time: 10:18
 *
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using SpringCard.LibCs;

#error CETTE LIBRAIRIE EST ABANDONNEE - PAS FONCTIONNELLE SOUS LE FRAMEWORK 4

namespace SpringCard.LibCs.Web
{
	public class DynamicRestClient : RestClient
	{
		public dynamic GET_Dynamic(string Url)
		{
			Url = ActualUrl(Url);

			if (ThrowExceptions)
			{
				HttpWebResponse response = GET(Url, Credentials);
				return ResponseToDynamic(response);
			}
			else
			{
				try
				{
					HttpWebResponse response = GET(Url, Credentials);
					return ResponseToDynamic(response);
				}
				catch (Exception e)
				{
					Logger.Error("GET " + Url + " failed: " + e.Message);
				}
			}

			return null;
		}

		public dynamic POST_Dynamic(string Url, JSON Data)
		{
			string DataString = "";

			if (ThrowExceptions)
			{
				DataString = Data.AsString();
			}
			else
			{
				try
				{
					DataString = Data.AsString();
				}
				catch (Exception e)
				{
					Logger.Error("Failed to encode JSON data: " + e.Message);
					return null;
				}
			}

			return POST_Dynamic(Url, DataString, "application/json");
		}

		public dynamic POST_Dynamic(string Url, JSONObject Data)
		{
			string DataString = "";

			if (ThrowExceptions)
			{
				DataString = JSONEncoder.Encode(Data);
			}
			else
			{
				try
				{
					DataString = JSONEncoder.Encode(Data);
				}
				catch (Exception e)
				{
					Logger.Error("Failed to encode JSON data: " + e.Message);
					return null;
				}
			}

			return POST_Dynamic(Url, DataString, "application/json");
		}

		public dynamic POST_Dynamic(string Url, string ContentData, string ContentType = "application/json")
		{
			Url = ActualUrl(Url);

			if (ThrowExceptions)
			{
				HttpWebResponse response = POST(Url, ContentData, ContentType, Credentials);
				return ResponseToDynamic(response);
			}
			else
			{
				try
				{
					HttpWebResponse response = POST(Url, ContentData, ContentType, Credentials);
					return ResponseToDynamic(response);
				}
				catch (Exception e)
				{
					Logger.Error("POST " + Url + " failed: " + e.Message);
				}
			}

			return null;
		}

		public DynamicRestClient(bool IgnoreHttpsCertificateError = false) : base(IgnoreHttpsCertificateError)
		{
		}

		public DynamicRestClient(string BaseUrl, bool IgnoreHttpsCertificateError = false) : base(BaseUrl, IgnoreHttpsCertificateError)
		{
		}

		public DynamicRestClient(string BaseUrl, string UserName, string UserPassword, bool IgnoreHttpsCertificateError = false) : base(BaseUrl, UserName, UserPassword, IgnoreHttpsCertificateError)
		{
		}

		public static dynamic ResponseToDynamic(HttpWebResponse Response)
		{
			using (var streamReader = new StreamReader(Response.GetResponseStream()))
			{
				string response_string = streamReader.ReadToEnd();

				Logger.Debug("Server's Response: " + response_string);

		 		var serializer = new JavaScriptSerializer();
				serializer.RegisterConverters(new[] { new DynamicJsonConverter() });
				return serializer.Deserialize(response_string, typeof(object));
			}
		}
	}
}
