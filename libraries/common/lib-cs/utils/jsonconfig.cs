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
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace SpringCard.LibCs
{
    /**
	 * \brief JSON config file object
	 */
    public class JsonCfgFile : IConfigReader
    {
        public string Prefix = "";
        private JSONObject json;

        /**
		 * \brief Read a string value
		 */
        public string ReadString(string Name, string Default = "")
        {
            try
            {
                return json.GetString(Prefix + Name);
            }
            catch
            {
                return Default;
            }
        }

        /**
		 * \brief Read an integer value
		 */
        public int ReadInteger(string Name, int Default = 0)
        {
            try
            {
                return json.GetInteger(Prefix + Name);
            }
            catch
            {
                return Default;
            }
        }

        /**
		 * \brief Read an unsigned integer value
		 */
        public uint ReadUnsigned(string Name, uint Default = 0)
        {
            try
            {
                return json.GetUnsigned(Prefix + Name);
            }
            catch
            {
                return Default;
            }
        }

        /**
		 * \brief Read a boolean value
		 */
        public bool ReadBoolean(string Name, bool Default = false)
        {
            try
            {
                return json.GetBool(Prefix + Name);
            }
            catch
            {
                return Default;
            }
        }

        /**
		 * \brief Load a JSON file
		 */
        public bool LoadFromFile(string FileName)
        {
            try
            {
                json = JSONDecoder.DecodeFromFile(FileName);
                if (json == null)
                    return false;
            }
            catch
            {
                return false;
            }
            return true;
        }

        /**
		 * \brief Create an instance of the JsonCfgFile object for the given JSON file. The instance is read only
		 */
        public static JsonCfgFile OpenReadOnly(string FileName, string Prefix = "")
        {
            JsonCfgFile f = new JsonCfgFile();
            f.Prefix = Prefix;
            if (!f.LoadFromFile(FileName))
                return null;
            return f;
        }

    }
}