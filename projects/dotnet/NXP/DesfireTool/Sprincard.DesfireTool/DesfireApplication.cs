using SpringCard.LibCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprincard.DesfireTool
{
    public class DesfireApplication
    {
        public Dictionary<Byte, DesfireFile> DesfireFileEntries = new Dictionary<Byte, DesfireFile>();
        public Dictionary<Byte, byte[]> Keys = new Dictionary<Byte, byte[]>();
        public enum KeyTypeEnum
        {
            KEY_TYPE_AES,
            KEY_TYPE_3DES,
            KEY_TYPE_3DES3K,
        };

        public uint aid = 0;
        public byte keySettings1;
        public byte keySettings2;
        public KeyTypeEnum KeyType = KeyTypeEnum.KEY_TYPE_AES;
        byte keyCount = 0;
        
        bool FreeDirectory = true;
        bool FreeCreateDelete = true;
        bool LockConfiguration = false;
        bool LockMasterKey = false;
        byte ChangeKeyIdx = 0;

        private bool Parse(JSONObject json)
        {

            keyCount = (byte)json.GetInteger("KeyCount");
            String s = json.GetString("KeyType");
            switch (s)
            {
                case "3des":
                    KeyType = KeyTypeEnum.KEY_TYPE_3DES;
                    break;
                case "3des3k":
                    KeyType = KeyTypeEnum.KEY_TYPE_3DES3K;
                    break;
            }
            FreeDirectory = json.GetBool("FreeDirectory");
            FreeCreateDelete = json.GetBool("FreeCreateDelete");
            LockConfiguration = json.GetBool("LockConfiguration");
            LockMasterKey = json.GetBool("LockMasterKey");
            ChangeKeyIdx = (byte)json.GetInteger("ChangeKeyIdx");
            
            /* are there files? */
            JSONObject some_files = json.Get("Files");
            byte cpt = 0;
            if (some_files != null)
            {
                foreach (var one in some_files.ObjectValue)
                {
                    JSONObject job = one.Value;
                    DesfireFile df = DesfireFile.CreateFromJSON(job, one.Key);
                    if (df == null)
                        return false;

                    DesfireFileEntries.Add(cpt, df);
                    cpt++;
                }
            }

            /* are there application keys? */
            JSONObject some_keys = json.Get("Keys");
            if (some_keys != null)
            {
                foreach (var one in some_keys.ObjectValue)
                {
                    Keys.Add(BinConvert.ParseHexB(one.Key), BinConvert.ParseHex(one.Value.StringValue));
                }
            }

            return true;
        }        

        public static DesfireApplication CreateFromJSON(JSONObject json, String said)
        {
            DesfireApplication result = new DesfireApplication();

            if (result.Parse(json))
            {
                /* now create aid */
                if (!BinConvert.isValidHexString(said))
                {
                    Logger.Error("Your AID contains non-hexadecimal characters!");
                    Logger.Error("Application_id is a 3 bytes Hexadecimal array.");
                    return null;
                }

                if (said.Length != 6)
                {
                    Logger.Error("Your AID is {0} characters long!", said.Length);
                    Logger.Error("Application_id is a 3 bytes Hexadecimal array.");
                    return null;
                }

                byte r1 = BinConvert.ParseHexB(said.Substring(0, 2));
                byte r2 = BinConvert.ParseHexB(said.Substring(2, 2));
                byte r3 = BinConvert.ParseHexB(said.Substring(4, 2));

                result.aid = ((uint)r1 << 16) | (uint)r2 << 8 | (uint)r3;

                /* create keysettings (we need to know aid first! */
                result.keySettings1 = result.ChangeKeyIdx;
                result.keySettings1 <<= (byte)4;

                if (!result.LockConfiguration)
                    result.keySettings1 |= 0x08;

                if (!result.FreeCreateDelete)
                    result.keySettings1 |= 0x04;

                if (!result.FreeDirectory)
                    result.keySettings1 |= 0x02;

                if (!result.LockMasterKey)
                    result.keySettings1 |= 0x01;

                result.keySettings2 = result.keyCount;
                switch(result.KeyType)
                {
                    case KeyTypeEnum.KEY_TYPE_3DES:
                        break;

                    case KeyTypeEnum.KEY_TYPE_AES:
                        result.keySettings2 |= 0x80;
                        break;

                    case KeyTypeEnum.KEY_TYPE_3DES3K:
                        result.keySettings2 |= 0x40;
                        break;
                }

                return result;
            }
            else return null;
        }
    }
}
