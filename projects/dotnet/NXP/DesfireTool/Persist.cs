using System;
using System.IO;
using SpringCard.LibCs;
using SpringCard.PCSC.CardHelpers;

namespace DesfireTool
{
    partial class Program
    {
        void SaveCardContext(Desfire desfire)
        {
            byte isoWrapping = 0;

            UInt32 current_aid = 0;

            byte session_type = 0;
            byte[] session_key = null;
            int session_key_id = 0;

            byte[] init_vector = null;
            byte[] cmac_subkey_1 = null;
            byte[] cmac_subkey_2 = null;

            if (desfire.ExportContext(ref isoWrapping, ref current_aid, ref session_type, out session_key, ref session_key_id, out init_vector, out cmac_subkey_1, out cmac_subkey_2))
            {
                String filename = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location).Replace(@"\", @"\\") + "\\" + Progname + ".sss";

                CfgFile cfgFile = new CfgFile(filename, true);
                if (cfgFile != null)
                {

                    cfgFile.WriteString("isoWrapping", BinConvert.ToHex(isoWrapping));
                    cfgFile.WriteString("current_aid", BinConvert.ToHex(current_aid));
                    cfgFile.WriteString("session_type", BinConvert.ToHex(session_type));
                    cfgFile.WriteString("session_key", BinConvert.ToHex(session_key));
                    cfgFile.WriteInteger("session_key_id", session_key_id);
                    cfgFile.WriteString("init_vector", BinConvert.ToHex(init_vector));
                    cfgFile.WriteString("cmac_subkey_1", BinConvert.ToHex(cmac_subkey_1));
                    cfgFile.WriteString("cmac_subkey_2", BinConvert.ToHex(cmac_subkey_2));
                    if (!cfgFile.Save())
                    {
                        Logger.Error("Unable to save context file: " + filename);
                    }
                    Logger.Trace("Context saved.");
                }
            }
            else
            {
                Logger.Error("Unable to save context.");
            }
        }

        void LoadCardContext(Desfire desfire)
        {
            byte isoWrapping = 0;

            UInt32 current_aid = 0;

            byte session_type = 0;
            byte[] session_key = null;
            int session_key_id = 0;

            byte[] init_vector = null;
            byte[] cmac_subkey_1 = null;
            byte[] cmac_subkey_2 = null;

            String filename = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location).Replace(@"\", @"\\") + "\\" + Progname + ".sss";
            if (File.Exists(filename))
            {
                CfgFile cfgFile = new CfgFile(filename, true);
                cfgFile.Load();

                if (cfgFile != null)
                {
                    isoWrapping = BinConvert.HexToByte(cfgFile.ReadString("isoWrapping"));
                    current_aid = BinConvert.ParseHexUInt32(cfgFile.ReadString("current_aid"));
                    session_type = BinConvert.HexToByte(cfgFile.ReadString("session_type"));
                    session_key = BinConvert.HexToBytes(cfgFile.ReadString("session_key"));
                    session_key_id = cfgFile.ReadInteger("session_key_id");
                    init_vector = BinConvert.HexToBytes(cfgFile.ReadString("init_vector"));
                    cmac_subkey_1 = BinConvert.HexToBytes(cfgFile.ReadString("cmac_subkey_1"));
                    cmac_subkey_2 = BinConvert.HexToBytes(cfgFile.ReadString("cmac_subkey_2"));

                    if (desfire.ImportContext(isoWrapping, current_aid, session_type, session_key, session_key_id, init_vector, cmac_subkey_1, cmac_subkey_2))
                    {
                        Logger.Trace("Context loaded.");
                    }
                }
            }
        }

    }
}
