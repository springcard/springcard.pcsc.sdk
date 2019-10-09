/**
 *
 * \ingroup LibCs
 *
 * \copyright
 *   Copyright (c) 2008-2018 SpringCard - www.springcard.com
 *   All right reserved
 *
 * \author
 *   Johann.D et al. / SpringCard
 *
 */
/*
 * Read LICENSE.txt for license details and restrictions.
 */
using System;
using System.Net;
using System.IO;

namespace SpringCard.LibCs
{
	/**
	 * \brief A simple REST client, to access a remote REST API through HTTP or HTTPS
	 */
	public class RestClient
	{
        private static bool GlobalIgnoreHttpsCertificateError = false;

        string BaseUrl = null;
		protected NetworkCredential Credentials = null;
        public bool ThrowExceptions = false;
        public uint RequestTimeout = 0;

        /**
		 * \brief GET a text from the REST server
		 */
        public string GET(string Url)
        {
            Url = ActualUrl(Url);

            if (ThrowExceptions)
            {
                HttpWebResponse response = _GET(Url);
                return ResponseToString(response);
            }
            else
            {
                try
                {
                    HttpWebResponse response = _GET(Url);
                    return ResponseToString(response);
                }
                catch (Exception e)
                {
                    Logger.Debug("GET " + Url + " failed: " + e.Message);
                }
            }

            return null;
        }

        /**
		 * \brief GET a JSON object from the REST server
		 */
        public JSONObject GET_Json(string Url)
		{
			Url = ActualUrl(Url);

			if (ThrowExceptions)
			{
				HttpWebResponse response = _GET(Url);
				return ResponseToJSON(response);
			}
			else
			{
				try
				{
					HttpWebResponse response = _GET(Url);
					return ResponseToJSON(response);
				}
				catch (Exception e)
				{
                    Logger.Debug("GET " + Url + " failed: " + e.Message);
				}
			}

			return null;
		}

		/**
		 * \brief POST a JSON object to the REST server, and receive a JSON object in response
		 */
		public JSONObject POST_Json(string Url, JSON Data)
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
                    Logger.Debug("Failed to encode JSON data: " + e.Message);
					return null;
				}
			}

			return POST_Json(Url, DataString, "application/json");
		}

        /**
		 * \brief POST a JSON object to the REST server, and receive a JSON object in response
		 */
        public JSONObject POST_Json(string Url, JSONObject Data)
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
                    Logger.Debug("Failed to encode JSON data: " + e.Message);
					return null;
				}
			}

			return POST_Json(Url, DataString, "application/json");
		}

		/**
		 * \brief POST a JSON object to the REST server, and receive a JSON object in response
		 */
		public JSONObject POST_Json(string Url, string ContentData, string ContentType = "application/json")
		{
			Url = ActualUrl(Url);

			if (ThrowExceptions)
			{
				HttpWebResponse response = _POST(Url, ContentData, ContentType);
				return ResponseToJSON(response);
			}
			else
			{
				try
				{
					HttpWebResponse response = _POST(Url, ContentData, ContentType);
					return ResponseToJSON(response);
				}
				catch (Exception e)
				{
                    Logger.Debug("POST " + Url + " failed: " + e.Message);
				}
			}

			return null;
		}

        /**
		 * \brief POST a JSON object to the REST server, and receive a JSON object in response
		 */
        public bool POST_Json(string Url, JSONObject RequestData, out JSONObject ResponseData, string ContentType = "application/json")
        {
            return POST_Json(Url, RequestData, out int StatusCode, out string StatusDescription, out ResponseData, ContentType);
        }

        /**
		 * \brief POST a JSON object to the REST server, and receive a JSON object in response
		 */
        public bool POST_Json(string Url, JSONObject RequestData, out int StatusCode, out JSONObject ResponseData, string ContentType = "application/json")
        {
            return POST_Json(Url, RequestData, out StatusCode, out string StatusDescription, out ResponseData, ContentType);
        }

        /**
		 * \brief POST a JSON object to the REST server, and receive a JSON object in response
		 */
        public bool POST_Json(string Url, JSONObject RequestData, out int StatusCode, out string StatusDescription, out JSONObject ResponseData, string ContentType = "application/json")
        {
            StatusCode = -1;
            StatusDescription = null;
            ResponseData = null;

            Url = ActualUrl(Url);

            string RequestString;
            if (ThrowExceptions)
            {
                RequestString = JSONEncoder.Encode(RequestData);
            }
            else
            {
                try
                {
                    RequestString = JSONEncoder.Encode(RequestData);
                }
                catch (Exception e)
                {
                    Logger.Debug("Failed to encode JSON data: " + e.Message);
                    return false;
                }
            }

            HttpWebResponse response = null;
            if (ThrowExceptions)
            {
                response = _POST(Url, RequestString, ContentType, out StatusCode, out StatusDescription);
            }
            else
            {
                try
                {
                    response = _POST(Url, RequestString, ContentType, out StatusCode, out StatusDescription);
                }
                catch (Exception e)
                {
                    Logger.Debug("POST " + Url + " failed: " + e.Message);
                    return false;
                }
            }

            if (response == null)
            {
                return false;
            }

            string ResponseString = ResponseToString(response);

            if (ThrowExceptions)
            {
                ResponseData = JSONDecoder.Decode(ResponseString);
            }
            else
            {
                try
                {
                    ResponseData = JSONDecoder.Decode(ResponseString);
                }
                catch (Exception e)
                {
                    Logger.Debug("Failed to decode JSON data: " + e.Message);
                    return false;
                }
            }

            return true;
        }

        /**
		 * \brief GET a content from the server, and save it (as text) into a local file
		 */
        public bool DownloadTextFile(string Url, string LocalFileName)
		{
			Url = ActualUrl(Url);

			if (ThrowExceptions)
			{
				HttpWebResponse response = _GET(Url);
				ResponseToTextFile(response, LocalFileName);
				return true;
			}
			else
			{
				try
				{
					HttpWebResponse response = _GET(Url);
					ResponseToTextFile(response, LocalFileName);
					return true;
				}
				catch (Exception e)
				{
                    Logger.Debug("GET " + Url + " failed: " + e.Message);
				}
			}

			return false;
		}

        /**
		 * \brief GET a content from the server, and save it into a local (raw) file
		 */
        public bool DownloadBinaryFile(string Url, string LocalFileName)
        {
            Url = ActualUrl(Url);

            if (ThrowExceptions)
            {
                HttpWebResponse response = _GET(Url);
                ResponseToBinaryFile(response, LocalFileName);
                return true;
            }
            else
            {
                try
                {
                    HttpWebResponse response = _GET(Url);
                    ResponseToBinaryFile(response, LocalFileName);
                    return true;
                }
                catch (Exception e)
                {
                    Logger.Debug("GET " + Url + " failed: " + e.Message);
                }
            }

            return false;
        }

        protected string ActualUrl(string RelativeUrl)
		{
			if (BaseUrl == null)
				return RelativeUrl;
			return BaseUrl + "/" + RelativeUrl;
		}

		/**
		 * \brief Instanciate a new REST client
		 */
		public RestClient()
		{
			this.BaseUrl = null;
		}

		/**
		 * \brief Instanciate a new REST client, using the specified URL as the base for all requests
		 */
		public RestClient(string BaseUrl)
		{
			this.BaseUrl = BaseUrl;
		}

		/**
		 * \brief Instanciate a new REST client, using the specified URL as the base for all requests, with HTTP authentication
		 */
		public RestClient(string BaseUrl, string UserName, string UserPassword)
		{
			this.BaseUrl = BaseUrl;
			Credentials = new NetworkCredential(UserName, UserPassword);
		}

		public void SetIgnoreHttpsCertificateError()
		{
			if (GlobalIgnoreHttpsCertificateError) return;
			ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
			GlobalIgnoreHttpsCertificateError = true;
		}

		protected HttpWebResponse _GET(string Url)
		{
			var request = WebRequest.Create(Url);

			if (Credentials != null)
				request.Credentials = Credentials;

			return (HttpWebResponse) request.GetResponse();
		}

        protected HttpWebResponse _POST(string Url, string ContentData, string ContentType)
        {
            return _POST(Url, ContentData, ContentType, out int StatusCode, out string StatusDescription);
        }

        protected HttpWebResponse _POST(string Url, string ContentData, string ContentType, out int StatusCode)
        {
            return _POST(Url, ContentData, ContentType, out StatusCode, out string StatusDescription);
        }

        protected HttpWebResponse _POST(string Url, string ContentData, string ContentType, out int StatusCode, out string StatusDescription)
		{
            WebRequest request = WebRequest.Create(Url);

            if (RequestTimeout > 0)
                request.Timeout = (int)RequestTimeout;

			request.ContentType = ContentType;
			request.Method = "POST";
			if (Credentials != null)
				request.Credentials = Credentials;

			using (StreamWriter streamWriter = new StreamWriter(request.GetRequestStream()))
			{
				streamWriter.Write(ContentData);
			}

            HttpWebResponse result;
            try
            {
                WebResponse response = request.GetResponse();
                result = (HttpWebResponse)response;
                StatusCode = (int)result.StatusCode;
                StatusDescription = result.StatusDescription;
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    result = (HttpWebResponse)response;
                    StatusCode = (int)result.StatusCode;
                    StatusDescription = result.StatusDescription;
                    return null;
                }
            }

            return result;
		}

        public string ResponseToString(HttpWebResponse Response)
        {
            using (var streamReader = new StreamReader(Response.GetResponseStream()))
            {
                string response_string = streamReader.ReadToEnd();
                return response_string;
            }
        }

        public JSONObject ResponseToJSON(HttpWebResponse Response)
		{
            string response_string = ResponseToString(Response);
			JSONObject result = JSONDecoder.Decode(response_string);
			return result;
		}

        public void ResponseToTextFile(HttpWebResponse Response, string FileName)
		{
            string response_string = ResponseToString(Response);
			File.WriteAllText(FileName, response_string);
		}

        public byte[] ResponseToBytes(HttpWebResponse Response)
        {
            using (var streamReader = Response.GetResponseStream())
            {
                BinaryReader binaryReader = new BinaryReader(streamReader);
                byte[] buffer = binaryReader.ReadBytes((int) Response.ContentLength);
                return buffer;
            }
        }

        public void ResponseToBinaryFile(HttpWebResponse Response, string FileName)
        {
            byte[] response_bytes = ResponseToBytes(Response);
            File.WriteAllBytes(FileName, response_bytes);
        }
    }
}
