using SpringCard.LibCs;
using System;

namespace Sprincard.DesfireTool
{
    public class DesfireFile
    {
        public byte FileID;
        public String Type;        
        public byte bCommMode;
        public ushort bFileAccess;
        public Int32 Size;
        public Int32 RecordSize;
        public Int32 RecordCount;
        public Int32 ValueMin;
        public Int32 ValueMax;
        public Int32 Value;
        public bool LimitedCreditEnabled;
        public bool FreeGetValueEnabled;
        public Int32 Offset;
        public byte[] Data = null;
        public byte WriteKeyIdx = 0x0E;
        public byte ReadWriteKeyIdx = 0x0E;

        private String CommMode;
        private byte ReadKeyIdx = 0x0E;        
        private byte AdminKeyIdx = 0x0E;
        

        private byte GetKeyValue(JSONObject json, String s)
        {
            byte rc = 0x0E;
            if (json.GetString( s ) == "")
            {
                rc = 0x0E;
            }
            else
            {
                rc = BinConvert.ParseHexB(json.GetString( s));
                if (rc > 0x0E)
                {
                    rc = 0x0E;
                }
            }
            return rc;
        }

        private bool Parse(JSONObject json)
        {            
            Type = json.GetString("Type");

            if (Type == "" || ( Type != "standard" && Type != "backup" && Type != "linear" && Type != "cyclic" && Type != "value") )
                Type = "standard";
            
            CommMode = json.GetString("CommMode");
            if (CommMode == "" || ( CommMode != "plain" && CommMode != "maced" && CommMode != "secure") )
                CommMode = "plain";

            Size = json.GetInteger("Size");
            RecordSize = json.GetInteger("RecordSize");
            RecordCount = json.GetInteger("RecordCount");
            ValueMin = json.GetInteger("ValueMin");
            ValueMax = json.GetInteger("ValueMax");
            Value = json.GetInteger("Value");
            LimitedCreditEnabled = json.GetBool("LimitedCreditEnabled");
            FreeGetValueEnabled = json.GetBool("FreeGetValueEnabled");
            Offset = json.GetInteger("Offset");
            CommMode = json.GetString("CommMode");
            if (CommMode == "" || ( CommMode != "plain" && CommMode != "maced" && CommMode != "secure") )
                CommMode = "plain";

            switch (CommMode)
            {
                case "plain":
                    bCommMode = 0;
                    break;
                case "maced":
                    bCommMode = 1;
                    break;
                case "secure":
                    bCommMode = 3;
                    break;
            }

            ReadKeyIdx = GetKeyValue(json, "ReadKeyIdx");            
            WriteKeyIdx = GetKeyValue(json, "WriteKeyIdx");
            ReadWriteKeyIdx = GetKeyValue(json, "ReadWriteKeyIdx");
            AdminKeyIdx = GetKeyValue(json, "AdminKeyIdx");

            bFileAccess = ReadKeyIdx;
            bFileAccess <<= (byte)4;
            bFileAccess |= WriteKeyIdx;
            bFileAccess <<= (byte)4;
            bFileAccess |= ReadWriteKeyIdx;
            bFileAccess <<= (byte)4;
            bFileAccess |= AdminKeyIdx;

            if (json.GetString("Data") != "")           
                Data = BinConvert.ParseHex(json.GetString("Data"));

            /* check integrity */
            switch (Type)
            {
                case "standard":
                case "backup":
                    /* check Size */
                    if (Size == 0)
                    {
                        Logger.Error("Size must be specified!");
                        return false;
                    }
                    break;
                
                case "cyclic":
                case "linear":
                    /* check RecordSize and RecordCount */
                    if ( RecordSize == 0 )
                    {
                        Logger.Error("RecordSize must be specified!");
                        return false;
                    }
                    if (RecordCount == 0)
                    {
                        Logger.Error("RecordCount must be specified!");
                        return false;
                    }
                    break;

                case "value":
                    /* check ValueMin, ValueMax and Value */
                    if ( (ValueMin==0) && ( ValueMax == 0) && (Value == 0) )
                    {
                        Logger.Error("ValueMin, ValueMax and Value must be specified!");
                        return false;
                    }
                    if (ValueMin >= ValueMax)
                    {
                        Logger.Error("Wrong ValueMin and ValueMax values!");
                        return false;
                    }
                    if (( Value <ValueMin ) || (Value > ValueMax) )
                    {
                        Logger.Error("Wrong initial Value!");
                        return false;
                    }

                    break;
            }

            return true;
        }

        public static DesfireFile CreateFromJSON(JSONObject json, String fileid)
        {
            DesfireFile result = new DesfireFile();

            if (result.Parse(json))
            {
                result.FileID =(byte) BinConvert.ParseInt(fileid);
                return result;
            }
            else return null;
        }

    }
}
