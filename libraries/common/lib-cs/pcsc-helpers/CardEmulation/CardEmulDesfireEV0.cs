/*
 * Crée par SharpDevelop.
 * Utilisateur: johann
 * Date: 05/01/2012
 * Heure: 12:02
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
using SpringCard.LibCs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpringCard.PCSC.CardEmulation
{
    /// <summary>
    /// Description of PiccEmulDesfireEV0.
    /// </summary>
    public class CardEmulDesfireEV0 : CardEmulBase
    {

        #region Consts

        public const byte OPERATION_OK = 0x00;
        public const byte INTERNAL_ERROR = 0xFF;

        public const byte NO_CHANGES = 0x0C;
        public const byte OUT_OF_EEPROM_ERROR = 0x0E;
        public const byte ILLEGAL_COMMAND_CODE = 0x1C;
        public const byte INTEGRITY_ERROR = 0x1E;
        public const byte NO_SUCH_KEY = 0x40;
        public const byte LENGTH_ERROR = 0x7E;
        public const byte PERMISSION_DENIED = 0x9D;
        public const byte PARAMETER_ERROR = 0x9E;
        public const byte APPLICATION_NOT_FOUND = 0xA0;
        public const byte APPL_INTEGRITY_ERROR = 0xA1;
        public const byte AUTHENTICATION_CORRECT = 0xAC;
        public const byte AUTHENTICATION_ERROR = 0xAE;
        public const byte ADDITIONAL_FRAME = 0xAF;
        public const byte BOUNDARY_ERROR = 0xBE;
        public const byte CARD_INTEGRITY_ERROR = 0xC1;
        public const byte COMMAND_ABORTED = 0xCA;
        public const byte CARD_DISABLED_ERROR = 0xCD;
        public const byte COUNT_ERROR = 0xCE;
        public const byte DUPLICATE_ERROR = 0xDE;
        public const byte FILE_NOT_FOUND = 0xF0;
        public const byte FILE_INTEGRITY_ERROR = 0xF1;
        public const byte EEPROM_ERROR = 0xEE;

        public const byte COMM_MODE_PLAIN = 0x00;
        public const byte COMM_MODE_MACED = 0x01;
        public const byte COMM_MODE_CIPHER = 0x03;

        public const byte ROOT_KEY_ALLOW_CHANGE_SETTINGS = 0x08;
        public const byte ROOT_KEY_FREE_CREATE_DELETE = 0x04;
        public const byte ROOT_KEY_FREE_DIRECTORY = 0x02;
        public const byte ROOT_KEY_ALLOW_CHANGE_KEY = 0x01;

        #endregion

        #region Internal types

        private class Key
        {
            public byte Version;
            public byte[] Value;

            public Key()
            {
                Version = 0;
                Value = new byte[16];
                for (int i = 0; i < 16; i++)
                    Value[i] = 0;
            }

            public Key(byte[] v)
            {
                Version = 0;
                Value = v;
            }

            public bool IsSingleDes()
            {
                for (int i = 0; i < 8; i++)
                    if (Value[i + 8] != Value[i])
                        return false;
                return true;
            }

            public void ToXML(XmlTextWriter xtw)
            {
                xtw.WriteElementString("KeyVersion", (new CardBuffer(Version)).AsString());
                xtw.WriteElementString("KeyValue", (new CardBuffer(Value)).AsString());
            }
        }

        private abstract class File
        {
            protected byte _fid;
            protected byte _type;
            protected byte _comm_settings;
            protected ushort _access_rights;

            public byte Fid
            {
                get
                {
                    return _fid;
                }
            }

            public byte Type
            {
                get
                {
                    return _type;
                }
            }

            public byte CommSettings
            {
                get
                {
                    return _comm_settings;
                }
                set
                {
                    switch (value)
                    {
                        case COMM_MODE_PLAIN:
                            break;
                        case COMM_MODE_MACED:
                            break;
                        case COMM_MODE_CIPHER:
                            break;
                        default:
                            break;
                    }
                    _comm_settings = value;
                }
            }

            public ushort AccessRights
            {
                get
                {
                    return _access_rights;
                }
                set
                {
                    _access_rights = value;
                }
            }

            public abstract bool Commit();
            public abstract bool Abort();
            public abstract void ToXML(XmlTextWriter xtw);
            public abstract bool FromXml(Dictionary<string, string> xml);

            protected void Base_ToXML(XmlTextWriter xtw, string type)
            {
                xtw.WriteElementString("ID", String.Format("{0:X02}", _fid));
                xtw.WriteElementString("Type", type);
                xtw.WriteElementString("CommSettings", String.Format("{0:X02}", _comm_settings));
                xtw.WriteElementString("AccessRights", String.Format("{0:X04}", _access_rights));
            }

            protected bool Base_FromXML(Dictionary<string, string> xml, string type)
            {
                string t;

                if (!xml.TryGetValue("Type", out t))
                    return false;
                if (!t.Equals(type))
                    return false;
                if (!xml.TryGetValue("ID", out t))
                    return false;
                _fid = byte.Parse(t, System.Globalization.NumberStyles.HexNumber);
                if (!xml.TryGetValue("CommSettings", out t))
                    return false;
                _comm_settings = byte.Parse(t, System.Globalization.NumberStyles.HexNumber);
                if (!xml.TryGetValue("AccessRights", out t))
                    return false;
                _access_rights = ushort.Parse(t, System.Globalization.NumberStyles.HexNumber);

                return true;
            }
        }

        private class ValueFile : File
        {
            long _lower_limit;
            long _upper_limit;
            long _value = 0;
            long _work_value = 0;
            bool _limited_credit;
            bool _free_get_value;

            public ValueFile()
            {
                _type = 0x02;
            }

            public ValueFile(byte Fid, byte CommSettings, ushort AccessRights, long LowerLimit, long UpperLimit, long Value, bool LimitedCredit, bool FreeGetValue)
            {
                _type = 0x02;
                _fid = Fid;
                _comm_settings = CommSettings;
                _access_rights = AccessRights;
                _lower_limit = LowerLimit;
                _upper_limit = UpperLimit;
                _value = Value;
                _limited_credit = LimitedCredit;
                _free_get_value = FreeGetValue;
            }

            public override void ToXML(XmlTextWriter xtw)
            {
                Base_ToXML(xtw, "Value");
                xtw.WriteElementString("LowerLimit", String.Format("{0:D}", _lower_limit));
                xtw.WriteElementString("UpperLimit", String.Format("{0:D}", _upper_limit));
                xtw.WriteElementString("LimitedCredit", String.Format("{0:D}", _limited_credit ? 1 : 0));
                xtw.WriteElementString("FreeGetValue", String.Format("{0:D}", _free_get_value ? 1 : 0));
                xtw.WriteElementString("CurrentValue", String.Format("{0:D}", _value));
            }

            public override bool FromXml(Dictionary<string, string> xml)
            {
                if (!Base_FromXML(xml, "Value"))
                    return false;

                string t;
                if (!xml.TryGetValue("LowerLimit", out t))
                    return false;
                _lower_limit = long.Parse(t, System.Globalization.NumberStyles.Integer);
                if (!xml.TryGetValue("UpperLimit", out t))
                    return false;
                _upper_limit = long.Parse(t, System.Globalization.NumberStyles.Integer);
                if (!xml.TryGetValue("LimitedCredit", out t))
                    return false;
                _limited_credit = long.Parse(t, System.Globalization.NumberStyles.Integer) != 0 ? true : false;
                if (!xml.TryGetValue("FreeGetValue", out t))
                    return false;
                _free_get_value = long.Parse(t, System.Globalization.NumberStyles.Integer) != 0 ? true : false;
                if (!xml.TryGetValue("CurrentValue", out t))
                    return false;
                _value = long.Parse(t, System.Globalization.NumberStyles.Integer);

                return true;
            }

            public override bool Commit()
            {
                _value = _work_value;
                return true;
            }

            public override bool Abort()
            {
                _work_value = _value;
                return true;
            }


            public long LowerLimit
            {
                get
                {
                    return _lower_limit;
                }
            }

            public long UpperLimit
            {
                get
                {
                    return _upper_limit;
                }
            }

            public long LimitedCreditValue
            {
                get
                {
                    return 0;
                }
            }

            public bool LimitedCreditEnabled
            {
                get
                {
                    return false;
                }
            }

            public bool LimitedCredit(long val)
            {
                if (val <= 0)
                    return false;

                _work_value += val;
                return true;
            }

            public bool Credit(long val)
            {
                if (val <= 0)
                    return false;

                _work_value += val;
                return true;
            }

            public bool Debit(long val)
            {
                if (val <= 0)
                    return false;

                _work_value -= val;
                return true;
            }

            public long Value
            {
                get
                {
                    return _value;
                }
            }
        }

        private abstract class StandardOrBackupFile : File
        {
            protected uint _size;
            protected byte[] _bytes;

            public uint Size
            {
                get
                {
                    return _size;
                }
            }

            public abstract void WriteData(long offset, byte[] bytes, long length);

            public void WriteData(long offset, byte[] bytes)
            {
                if (offset < bytes.Length)
                    WriteData(offset, bytes, bytes.Length - offset);
            }

            public byte[] ReadData(long offset, long length)
            {
                byte[] r = new byte[length];

                for (long i = 0; i < r.Length; i++)
                    r[i] = _bytes[offset + i];

                return r;
            }

            public bool MayWriteData(long offset, long length)
            {
                if (offset + length > _size)
                    return false;

                return true;
            }

            protected void StandardOrBackup_ToXML(XmlTextWriter xtw, string type)
            {
                Base_ToXML(xtw, type);
                xtw.WriteElementString("Size", String.Format("{0:D}", _size));
                xtw.WriteElementString("Content", (new CardBuffer(_bytes)).AsString());
            }

            protected bool StandardOrBackup_FromXML(Dictionary<string, string> xml, string type)
            {
                if (!Base_FromXML(xml, type))
                    return false;

                string t;
                if (!xml.TryGetValue("Size", out t))
                    return false;
                _size = uint.Parse(t, System.Globalization.NumberStyles.Integer);
                if (!xml.TryGetValue("Content", out t))
                    return false;
                _bytes = (new CardBuffer(t)).GetBytes();
                if ((_bytes == null) || (_bytes.Length != _size))
                    return false;

                return true;
            }
        }

        private class StandardFile : StandardOrBackupFile
        {
            public StandardFile(byte Fid, byte CommSettings, ushort AccessRights, uint Size)
            {
                _type = 0x00;
                _fid = Fid;
                _comm_settings = CommSettings;
                _access_rights = AccessRights;
                _size = Size;
                _bytes = new byte[_size];
            }

            public StandardFile()
            {
                _type = 0x00;
            }

            public override void WriteData(long offset, byte[] bytes, long length)
            {
                for (long i = 0; i < length; i++)
                    _bytes[offset + i] = bytes[i];
            }

            public override void ToXML(XmlTextWriter xtw)
            {
                StandardOrBackup_ToXML(xtw, "Standard");
            }

            public override bool FromXml(Dictionary<string, string> xml)
            {
                if (!StandardOrBackup_FromXML(xml, "Standard"))
                    return false;
                return true;
            }

            public override bool Commit()
            {
                return true;
            }

            public override bool Abort()
            {
                return true;
            }
        }

        private class BackupFile : StandardOrBackupFile
        {
            private byte[] _work_bytes;

            public BackupFile(byte Fid, byte CommSettings, ushort AccessRights, uint Size)
            {
                _type = 0x01;
                _fid = Fid;
                _comm_settings = CommSettings;
                _access_rights = AccessRights;
                _size = Size;
                _bytes = new byte[_size];
                _work_bytes = null;
            }

            public BackupFile()
            {
                _type = 0x01;
            }

            public override void ToXML(XmlTextWriter xtw)
            {
                StandardOrBackup_ToXML(xtw, "Backup");
            }

            public override bool FromXml(Dictionary<string, string> xml)
            {
                if (!StandardOrBackup_FromXML(xml, "Backup"))
                    return false;
                return true;
            }

            public override void WriteData(long offset, byte[] bytes, long length)
            {
                if (_work_bytes == null)
                {
                    _work_bytes = new byte[_size];
                    for (long i = 0; i < _size; i++)
                        _work_bytes[i] = _bytes[i];
                }

                for (long i = 0; i < length; i++)
                    _work_bytes[offset + i] = bytes[i];
            }

            public override bool Commit()
            {
                if (_work_bytes != null)
                {
                    for (long i = 0; i < _size; i++)
                        _bytes[i] = _work_bytes[i];
                    _work_bytes = null;
                }
                return true;
            }

            public override bool Abort()
            {
                _work_bytes = null;
                return true;
            }
        }

        private abstract class RecordOrCyclicFile : File
        {
            protected uint _record_size;
            protected uint _max_record_count;
            protected uint _current_record_count;

            protected byte[,] _records;
            protected byte[] _work_record;

            protected bool _clearing;

            public uint RecordSize
            {
                get
                {
                    return _record_size;
                }
            }

            public uint MaxRecordCount
            {
                get
                {
                    return _max_record_count;
                }
            }

            public uint CurrentRecordCount
            {
                get
                {
                    return _current_record_count;
                }
            }

            public byte[] GetRecord(long index)
            {
                byte[] r = null;
                if (ReadRecords(index, 1, ref r) == OPERATION_OK)
                    return r;
                return null;
            }

            public byte ReadRecords(long first, long count, ref byte[] data)
            {
                if (first == _current_record_count)
                {
                    if (_work_record != null)
                        data = _work_record;
                    else
                        data = new byte[_record_size];
                    return OPERATION_OK;
                }

                if (first > _current_record_count)
                    return BOUNDARY_ERROR;
                if (count == 0)
                    count = _current_record_count - first;
                if (first + count > _current_record_count)
                    return BOUNDARY_ERROR;

                data = new byte[_record_size * count];
                long o = 0;

                for (long i = 0; i < count; i++)
                {
                    for (long j = 0; j < _record_size; j++)
                    {
                        data[o++] = _records[first + i, j];
                    }
                }

                return OPERATION_OK;
            }

            public bool MayWriteRecord(long offset, long length)
            {
                if (_current_record_count >= _max_record_count)
                    return false;

                if (offset + length > _record_size)
                    return false;

                return true;
            }

            public void WriteRecord(long offset, byte[] data)
            {
                if (!MayWriteRecord(offset, data.Length))
                    return;

                if (_work_record == null)
                {
                    _work_record = new byte[_record_size];
                    for (long i = 0; i < _record_size; i++)
                        _work_record[i] = _records[_current_record_count, i];
                }

                for (long i = 0; i < data.Length; i++)
                    _work_record[offset + i] = data[i];
            }

            public override bool Abort()
            {
                _work_record = null;
                _clearing = false;
                return true;
            }

            public void Clear()
            {
                _clearing = true;
            }

            public bool PendingClear()
            {
                return _clearing;
            }

            protected void RecordOrCyclic_ToXML(XmlTextWriter xtw, string type)
            {
                Base_ToXML(xtw, type);
                xtw.WriteElementString("RecordSize", String.Format("{0:D}", _record_size));
                xtw.WriteElementString("RecordCount", String.Format("{0:D}", _max_record_count));
                xtw.WriteElementString("CurrentRecord", String.Format("{0:D}", _current_record_count));
                xtw.WriteStartElement("Records");
                for (int i = 0; i < _max_record_count; i++)
                    xtw.WriteElementString("Record", (new CardBuffer(GetRecord(i))).AsString());
                xtw.WriteEndElement();
            }

            protected bool RecordOrCyclic_FromXML(Dictionary<string, string> xml, string type)
            {
                if (!Base_FromXML(xml, type))
                    return false;

                string t;
                if (!xml.TryGetValue("RecordSize", out t))
                    return false;
                _record_size = uint.Parse(t, System.Globalization.NumberStyles.Integer);
                if (!xml.TryGetValue("RecordCount", out t))
                    return false;
                _max_record_count = uint.Parse(t, System.Globalization.NumberStyles.Integer);
                if (!xml.TryGetValue("CurrentRecord", out t))
                    return false;
                _current_record_count = uint.Parse(t, System.Globalization.NumberStyles.Integer);

                _records = new byte[_max_record_count, _record_size];

                for (int i = 0; i < _current_record_count; i++)
                {
                    string n = String.Format("Record.{0}", i);
                    if (!xml.TryGetValue(n, out t))
                        return false;
                    byte[] b = (new CardBuffer(t)).GetBytes();
                    if ((b == null) || (b.Length != _record_size))
                        return false;
                    for (int j = 0; j < _record_size; j++)
                        _records[i, j] = b[j];
                }

                return true;
            }
        }

        private class RecordFile : RecordOrCyclicFile
        {
            public RecordFile(byte Fid, byte CommSettings, ushort AccessRights, uint RecordSize, uint RecordCount)
            {
                _type = 0x03;
                _fid = Fid;
                _comm_settings = CommSettings;
                _access_rights = AccessRights;
                _record_size = RecordSize;
                _max_record_count = RecordCount;
                _current_record_count = 0;

                _records = new byte[_max_record_count, _record_size];
                _work_record = null;
                _clearing = false;
            }

            public RecordFile()
            {
                _type = 0x03;
            }

            public override void ToXML(XmlTextWriter xtw)
            {
                RecordOrCyclic_ToXML(xtw, "Record");
            }

            public override bool FromXml(Dictionary<string, string> xml)
            {
                if (!RecordOrCyclic_FromXML(xml, "Record"))
                    return false;
                return true;
            }

            public override bool Commit()
            {
                if (_clearing)
                {
                    _current_record_count = 0;
                    _records = new byte[_max_record_count, _record_size];
                    _clearing = false;
                    _work_record = null;
                }

                if (_work_record != null)
                {
                    if (_current_record_count >= _max_record_count)
                        return false;

                    for (uint i = 0; i < _record_size; i++)
                        _records[_current_record_count, i] = _work_record[i];

                    _current_record_count++;
                    _work_record = null;
                }

                return true;
            }
        }

        private class CyclicFile : RecordOrCyclicFile
        {
            public CyclicFile(byte Fid, byte CommSettings, ushort AccessRights, uint RecordSize, uint RecordCount)
            {
                _type = 0x04;
                _fid = Fid;
                _comm_settings = CommSettings;
                _access_rights = AccessRights;
                _record_size = RecordSize;
                _max_record_count = RecordCount;
                _current_record_count = 0;
                _records = new byte[_max_record_count, _record_size];
                _work_record = null;
            }

            public CyclicFile()
            {
                _type = 0x04;
            }

            public override void ToXML(XmlTextWriter xtw)
            {
                RecordOrCyclic_ToXML(xtw, "Cyclic");
            }

            public override bool FromXml(Dictionary<string, string> xml)
            {
                if (!RecordOrCyclic_FromXML(xml, "Cyclic"))
                    return false;
                return true;
            }

            public override bool Commit()
            {
                if (_clearing)
                {
                    _current_record_count = 0;
                    _records = new byte[_max_record_count, _record_size];
                    _clearing = false;
                    _work_record = null;
                }

                if (_work_record != null)
                {
                    if (_current_record_count >= _max_record_count)
                        return false;

                    if (_current_record_count == _max_record_count - 1)
                    {
                        /* Rotate */
                        for (uint i = 1; i < _max_record_count - 1; i++)
                            for (uint j = 0; j < _record_size; j++)
                                _records[i - 1, j] = _records[i, j];
                        for (uint j = 0; j < _record_size; j++)
                            _records[_max_record_count - 1, j] = _work_record[j];
                    }
                    else
                    {
                        for (uint j = 0; j < _record_size; j++)
                            _records[_current_record_count, j] = _work_record[j];
                        _current_record_count++;
                    }

                    _work_record = null;
                }

                return true;
            }
        }

        private class Application
        {
            public uint Aid;
            public List<Key> Keys = new List<Key>();
            public List<File> Files = new List<File>();
            public byte KeySettings;

            public File CurrentFile = null;

            public Application()
            {

            }

            public Application(uint _Aid, byte _KeySettings, byte _KeyCount)
            {
                Aid = _Aid;
                KeySettings = _KeySettings;
                for (int i = 0; i < _KeyCount; i++)
                    Keys.Add(new Key());
            }

            public void ToXML(XmlTextWriter xtw)
            {
                xtw.WriteElementString("AID", String.Format("{0:X06}", Aid));
                xtw.WriteElementString("KeySettings", String.Format("{0:X02}", KeySettings));

                if (Keys.Count > 0)
                {
                    xtw.WriteStartElement("Keys");
                    for (int i = 0; i < Keys.Count; i++)
                    {
                        xtw.WriteStartElement("Key");
                        Keys[i].ToXML(xtw);
                        xtw.WriteEndElement();
                    }
                    xtw.WriteEndElement();
                }

                if (Files.Count > 0)
                {
                    xtw.WriteStartElement("Files");
                    for (int i = 0; i < Files.Count; i++)
                    {
                        xtw.WriteStartElement("File");
                        Files[i].ToXML(xtw);
                        xtw.WriteEndElement();
                    }
                    xtw.WriteEndElement();
                }
            }

            public bool FileExists(byte _Fid)
            {
                if (Files == null)
                    return false;

                for (int i = 0; i < Files.Count; i++)
                {
                    if (Files[i].Fid == _Fid)
                        return true;
                }

                return false;
            }

            public bool SelectFile(byte _Fid)
            {
                CurrentFile = null;

                if (Files == null)
                    return false;

                for (int i = 0; i < Files.Count; i++)
                {
                    if (Files[i].Fid == _Fid)
                    {
                        CurrentFile = Files[i];
                        return true;
                    }
                }

                return false;
            }

            public bool RemoveFile(byte _Fid)
            {
                if (Files == null)
                    return false;

                for (int i = 0; i < Files.Count; i++)
                {
                    if (Files[i].Fid == _Fid)
                    {
                        if (CurrentFile == Files[i])
                            CurrentFile = null;
                        Files.RemoveAt(i);
                        return true;
                    }
                }

                return false;
            }

            public bool InsertFile(File _File)
            {
                if (FileExists(_File.Fid))
                    return false;

                Files.Add(_File);
                return SelectFile(_File.Fid);
            }

            public bool Commit()
            {
                for (int i = 0; i < Files.Count; i++)
                    if (!Files[i].Commit())
                        return false;

                return true;
            }

            public bool Abort()
            {
                for (int i = 0; i < Files.Count; i++)
                    if (!Files[i].Abort())
                        return false;

                return true;
            }
        }

        private class PendingCommand
        {
            public byte Code = 0;
            public uint Step = 0;
        }

        #endregion

        #region Variables

        private List<Application> Applications = new List<Application>();
        private Application CurrentApplication = null;

        private Key RootKey = new Key();
        private byte RootKeySettings = ROOT_KEY_ALLOW_CHANGE_SETTINGS | ROOT_KEY_FREE_CREATE_DELETE | ROOT_KEY_FREE_DIRECTORY | ROOT_KEY_ALLOW_CHANGE_KEY;


        private int AuthKeyNo = -1;
        private DesfireEV0Cipher Cipher;

        PendingCommand pending;

        #endregion

        #region Glue

        public CardEmulDesfireEV0(string ReaderName) : base(ReaderName)
        {
            OnDeselect();
        }

        protected override void OnSelect()
        {
            CurrentApplication = null;
            pending = null;
        }

        protected override void OnDeselect()
        {
            pending = null;
        }

        protected override void OnError()
        {
            
        }

        private byte OnDesfireCommand(byte Command, CardBuffer DataIn, ref CardBuffer DataOut)
        {
            byte Result;
            DataOut = null;

            if (Command == ADDITIONAL_FRAME)
            {
                if ((pending == null) || (pending.Code == 0))
                    return ILLEGAL_COMMAND_CODE;

                Command = pending.Code;
                pending.Code = 0x00;
                pending.Step++;

            }
            else if ((pending != null) && (pending.Code != 0))
            {
                pending = null;
                return COMMAND_ABORTED;
            }

            switch (Command)
            {
                case 0x5A:
                    Result = SelectApplication(DataIn, ref DataOut);
                    break;
                case 0x60:
                    Result = GetVersion(DataIn, ref DataOut);
                    break;
                case 0x64:
                    Result = GetKeyVersion(DataIn, ref DataOut);
                    break;
                case 0x0A:
                    Result = Authenticate(DataIn, ref DataOut);
                    break;
                case 0x45:
                    Result = GetKeySettings(DataIn, ref DataOut);
                    break;
                case 0x54:
                    Result = ChangeKeySettings(DataIn, ref DataOut);
                    break;
                case 0xC4:
                    Result = ChangeKey(DataIn, ref DataOut);
                    break;
                case 0xFC:
                    Result = FormatPICC(DataIn, ref DataOut);
                    break;
                case 0xCA:
                    Result = CreateApplication(DataIn, ref DataOut);
                    break;
                case 0xDA:
                    Result = DeleteApplication(DataIn, ref DataOut);
                    break;
                case 0x6A:
                    Result = GetApplicationIDs(DataIn, ref DataOut);
                    break;
                case 0xCD:
                    Result = CreateFlatFile(false, DataIn, ref DataOut);
                    break;
                case 0xCB:
                    Result = CreateFlatFile(true, DataIn, ref DataOut);
                    break;
                case 0xCC:
                    Result = CreateValueFile(DataIn, ref DataOut);
                    break;
                case 0xC0:
                    Result = CreateRecordFile(true, DataIn, ref DataOut);
                    break;
                case 0xC1:
                    Result = CreateRecordFile(false, DataIn, ref DataOut);
                    break;
                case 0x3D:
                    Result = WriteData(DataIn, ref DataOut);
                    break;
                case 0xBD:
                    Result = ReadData(DataIn, ref DataOut);
                    break;
                case 0xF5:
                    Result = GetFileSettings(DataIn, ref DataOut);
                    break;
                case 0x5F:
                    Result = ChangeFileSettings(DataIn, ref DataOut);
                    break;
                case 0x0C:
                    Result = Credit(DataIn, ref DataOut);
                    break;
                case 0xDC:
                    Result = Debit(DataIn, ref DataOut);
                    break;
                case 0x3B:
                    Result = WriteRecord(DataIn, ref DataOut);
                    break;
                case 0x6C:
                    Result = GetValue(DataIn, ref DataOut);
                    break;
                case 0xC7:
                    Result = CommitTransaction(DataIn, ref DataOut);
                    break;
                case 0xBB:
                    Result = ReadRecords(DataIn, ref DataOut);
                    break;
                case 0x1C:
                    Result = LimitedCredit(DataIn, ref DataOut);
                    break;
                case 0x6F:
                    Result = GetFileIDs(DataIn, ref DataOut);
                    break;
                case 0xDF:
                    Result = DeleteFile(DataIn, ref DataOut);
                    break;
                case 0xEB:
                    Result = ClearRecordFile(DataIn, ref DataOut);
                    break;
                case 0xA7:
                    Result = AbortTransaction(DataIn, ref DataOut);
                    break;
                default:
                    return ILLEGAL_COMMAND_CODE;
            }

            if (Result != ADDITIONAL_FRAME)
                pending = null;

            if (pending != null)
                pending.Code = Command;

            if ((Result != OPERATION_OK) && (Result != ADDITIONAL_FRAME))
                AuthKeyNo = -1;

            return Result;
        }

        protected override RAPDU OnApdu(CAPDU capdu)
        {
            if (capdu.GetByte(0) != 0x90)
            {
                /* Not a Desfire command */
                pending = null;
                return new RAPDU(0x6E, 0x00);
            }

            CardBuffer DataOut = null;
            byte Result = OnDesfireCommand(capdu.INS, capdu.data, ref DataOut);

            RAPDU rapdu;

            if (DataOut != null)
                rapdu = new RAPDU(DataOut.GetBytes(), 0x91, Result);
            else
                rapdu = new RAPDU(0x91, Result);

            return rapdu;
        }

        #endregion

        #region Card basis
        public byte FormatPICC(CardBuffer DataIn, ref CardBuffer DataOut)
        {
            CurrentApplication = null;
            Applications.Clear();
            return OPERATION_OK;
        }

        public byte GetVersion(CardBuffer DataIn, ref CardBuffer DataOut)
        {
            if (pending == null)
                pending = new PendingCommand();

            switch (pending.Step)
            {
                case 0:
                    DataOut = new CardBuffer("04010101001A05");
                    return ADDITIONAL_FRAME;
                case 1:
                    DataOut = new CardBuffer("04010101041A05");
                    return ADDITIONAL_FRAME;
                case 2:
                    DataOut = new CardBuffer("112233445566771122334455AABB");
                    return OPERATION_OK;
            }

            return INTERNAL_ERROR;
        }
        #endregion

        #region Application manipulation
        private byte SelectApplication(CardBuffer DataIn, ref CardBuffer DataOut)
        {
            if (!CheckIn(DataIn, 3))
                return LENGTH_ERROR;

            uint Aid = Get3(DataIn.GetBytes(0, 3));

            if (SelectApplication(Aid))
                return OPERATION_OK;

            return APPLICATION_NOT_FOUND;
        }

        public bool SelectApplication(uint Aid)
        {
            AuthKeyNo = -1;

            if (Aid == 0)
            {
                CurrentApplication = null;
                return true;
            }

            for (int i = 0; i < Applications.Count; i++)
            {
                if (Applications[i].Aid == Aid)
                {
                    CurrentApplication = Applications[i];
                    CurrentApplication.CurrentFile = null;
                    return true;
                }
            }

            return false;
        }

        public bool DeleteApplication(uint Aid)
        {
            if (Aid == 0)
                return false;

            for (int i = 0; i < Applications.Count; i++)
            {
                if (Applications[i].Aid == Aid)
                {
                    if (CurrentApplication == Applications[i])
                        CurrentApplication = null;
                    Applications.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        private byte GetApplicationIDs(CardBuffer DataIn, ref CardBuffer DataOut)
        {
            if (!CheckIn(DataIn, 0))
                return LENGTH_ERROR;

            if (pending == null)
                pending = new PendingCommand();

            byte[] Result;

            switch (pending.Step)
            {
                case 0:
                    if (Applications.Count == 0)
                        return OPERATION_OK;

                    if (Applications.Count > 19)
                    {
                        Result = new byte[19 * 3];
                    }
                    else
                    {
                        Result = new byte[Applications.Count * 3];
                    }

                    for (int i = 0; (i < Applications.Count) && (i < 19); i++)
                    {
                        uint Aid = Applications[i].Aid;
                        Result[3 * i + 2] = (byte)(Aid % 0x00000100);
                        Aid /= 0x00000100;
                        Result[3 * i + 1] = (byte)(Aid % 0x00000100);
                        Aid /= 0x00000100;
                        Result[3 * i + 0] = (byte)(Aid % 0x00000100);
                    }

                    DataOut = new CardBuffer(Result);

                    if (Applications.Count > 19)
                        return ADDITIONAL_FRAME;

                    return OPERATION_OK;

                case 1:
                    if (Applications.Count <= 19)
                        return INTERNAL_ERROR;

                    Result = new byte[(Applications.Count - 19) * 3];
                    for (int i = 0; i < (Applications.Count - 19); i++)
                    {
                        uint Aid = Applications[i].Aid;
                        Result[3 * i + 2] = (byte)(Aid % 0x00000100);
                        Aid /= 0x00000100;
                        Result[3 * i + 1] = (byte)(Aid % 0x00000100);
                        Aid /= 0x00000100;
                        Result[3 * i + 0] = (byte)(Aid % 0x00000100);
                    }

                    DataOut = new CardBuffer(Result);

                    return OPERATION_OK;
            }

            return INTERNAL_ERROR;
        }

        private byte DeleteApplication(CardBuffer DataIn, ref CardBuffer DataOut)
        {
            if (!CheckIn(DataIn, 3))
                return LENGTH_ERROR;

            uint Aid = Get3(DataIn.GetBytes(0, 3));

            if (CurrentApplication == null)
            {
                if ((AuthKeyNo < 0) && ((RootKeySettings & ROOT_KEY_FREE_CREATE_DELETE) == 0))
                    return AUTHENTICATION_ERROR;

            }
            else
            {
                if (CurrentApplication.Aid != Aid)
                    return PERMISSION_DENIED;

                if (AuthKeyNo < 0)
                    return AUTHENTICATION_ERROR;

                SelectApplication(0);
            }

            DeleteApplication(Aid);
            return OPERATION_OK;
        }

        private byte CreateApplication(CardBuffer DataIn, ref CardBuffer DataOut)
        {
            if (CurrentApplication != null)
                return PERMISSION_DENIED;

            if ((AuthKeyNo < 0) && ((RootKeySettings & ROOT_KEY_FREE_CREATE_DELETE) == 0))
                return AUTHENTICATION_ERROR;

            if (!CheckIn(DataIn, 5))
                return LENGTH_ERROR;

            uint Aid = Get3(DataIn.GetBytes(0, 3));
            if (Aid == 0)
                return PARAMETER_ERROR;

            for (int i = 0; i < Applications.Count; i++)
            {
                if (Applications[i].Aid == Aid)
                {
                    return DUPLICATE_ERROR;
                }
            }

            Applications.Add(new Application(Aid, DataIn.GetByte(3), (byte)(DataIn.GetByte(4) & 0x0F)));
            return OPERATION_OK;
        }
        #endregion

        #region Key manipulation
        public byte GetKeyVersion(CardBuffer DataIn, ref CardBuffer DataOut)
        {
            if (!CheckIn(DataIn, 1))
                return LENGTH_ERROR;

            byte KeyIndex = DataIn.GetByte(0);

            if (CurrentApplication != null)
            {
                if (KeyIndex >= CurrentApplication.Keys.Count)
                    return NO_SUCH_KEY;
                DataOut = new CardBuffer(CurrentApplication.Keys[KeyIndex].Version);
                return OPERATION_OK;
            }
            else
            {
                if (KeyIndex >= 1)
                    return NO_SUCH_KEY;
                DataOut = new CardBuffer(RootKey.Version);
                return OPERATION_OK;
            }
        }

        public byte GetKeySettings(CardBuffer DataIn, ref CardBuffer DataOut)
        {
            if (!CheckIn(DataIn, 0))
                return LENGTH_ERROR;

            byte[] Result = new byte[2];

            if (CurrentApplication != null)
            {
                Result[0] = CurrentApplication.KeySettings;
                Result[1] = (byte)CurrentApplication.Keys.Count;
            }
            else
            {
                Result[0] = RootKeySettings;
                Result[1] = 0;
            }

            DataOut = new CardBuffer(Result);
            return OPERATION_OK;
        }

        public byte ChangeKey(CardBuffer DataIn, ref CardBuffer DataOut)
        {
            if (AuthKeyNo < 0)
                return PERMISSION_DENIED;
            //if (!CheckIn(DataIn, 17, 24))
            //  return LENGTH_ERROR;

            byte key_no = DataIn.GetByte(0);
            Key old_key = null;

            if (CurrentApplication == null)
            {
                if (key_no != 0)
                    return NO_SUCH_KEY;
                old_key = RootKey;
            }
            else
            {
                if (key_no >= CurrentApplication.Keys.Count)
                    return NO_SUCH_KEY;
                old_key = CurrentApplication.Keys[key_no];
            }

            byte[] t = DataIn.GetBytes(1, -1);

            if (!Cipher.Recv(ref t))
                return AUTHENTICATION_ERROR;

            if (key_no != AuthKeyNo)
            {
                if (old_key == null)
                    return INTERNAL_ERROR;

                for (int i = 0; i < 16; i++)
                    t[i] ^= old_key.Value[i];
            }

            Key key = new Key((new CardBuffer(t)).GetBytes(16));

            if (CurrentApplication == null)
            {
                RootKey = key;
            }
            else
            {
                CurrentApplication.Keys[key_no] = key;
            }

            return OPERATION_OK;
        }

        public byte ChangeKeySettings(CardBuffer DataIn, ref CardBuffer DataOut)
        {
            if (AuthKeyNo < 0)
                return PERMISSION_DENIED;
            if (!CheckIn(DataIn, 8))
                return LENGTH_ERROR;

            byte[] t = DataIn.GetBytes();

            if (!Cipher.Recv(ref t))
                return INTEGRITY_ERROR;

            if (CurrentApplication == null)
            {
                RootKeySettings = t[0];
            }
            else
            {
                CurrentApplication.KeySettings = t[0];
            }

            return OPERATION_OK;
        }
        #endregion

        #region File manipulation
        private byte GetFileIDs(CardBuffer DataIn, ref CardBuffer DataOut)
        {
            if (CurrentApplication == null)
                return PERMISSION_DENIED;

            if (!CheckIn(DataIn, 0))
                return LENGTH_ERROR;

            int c = CurrentApplication.Files.Count;
            if (c != 0)
            {
                byte[] r = new byte[c];

                for (int i = 0; i < c; i++)
                    r[i] = CurrentApplication.Files[i].Fid;

                DataOut = new CardBuffer(r);
            }

            return OPERATION_OK;
        }

        private byte DeleteFile(CardBuffer DataIn, ref CardBuffer DataOut)
        {
            if (CurrentApplication == null)
                return PERMISSION_DENIED;
            if (!CheckIn(DataIn, 1))
                return LENGTH_ERROR;

            byte Fid = DataIn.GetByte(0);

            if (!CurrentApplication.FileExists(Fid))
                return FILE_NOT_FOUND;

            if (CurrentApplication.RemoveFile(Fid))
                return OPERATION_OK;

            return APPL_INTEGRITY_ERROR;
        }

        private byte CreateFlatFile(bool Backup, CardBuffer DataIn, ref CardBuffer DataOut)
        {
            if (CurrentApplication == null)
                return PERMISSION_DENIED;
            if (!CheckIn(DataIn, 7))
                return LENGTH_ERROR;

            byte Fid = DataIn.GetByte(0);
            byte CommSettings = DataIn.GetByte(1);
            ushort AccessRights = Get2(DataIn.GetBytes(2, 2));

            uint FileSize = Get3(DataIn.GetBytes(4, 3));

            if (CurrentApplication.FileExists(Fid))
                return DUPLICATE_ERROR;

            File file;

            if (Backup)
                file = new BackupFile(Fid, CommSettings, AccessRights, FileSize);
            else
                file = new StandardFile(Fid, CommSettings, AccessRights, FileSize);

            if (CurrentApplication.InsertFile(file))
                return OPERATION_OK;
            return APPL_INTEGRITY_ERROR;
        }

        private byte CreateRecordFile(bool Cyclic, CardBuffer DataIn, ref CardBuffer DataOut)
        {
            if (CurrentApplication == null)
                return PERMISSION_DENIED;
            if (!CheckIn(DataIn, 10))
                return LENGTH_ERROR;

            byte Fid = DataIn.GetByte(0);
            byte CommSettings = DataIn.GetByte(1);
            ushort AccessRights = Get2(DataIn.GetBytes(2, 2));

            uint RecordSize = Get3(DataIn.GetBytes(4, 3));
            uint RecordCount = Get3(DataIn.GetBytes(7, 3));

            if (CurrentApplication.FileExists(Fid))
                return DUPLICATE_ERROR;

            File file;

            if (Cyclic)
                file = new CyclicFile(Fid, CommSettings, AccessRights, RecordSize, RecordCount);
            else
                file = new RecordFile(Fid, CommSettings, AccessRights, RecordSize, RecordCount);

            if (CurrentApplication.InsertFile(file))
                return OPERATION_OK;
            return APPL_INTEGRITY_ERROR;
        }

        private byte CreateValueFile(CardBuffer DataIn, ref CardBuffer DataOut)
        {
            if (CurrentApplication == null)
                return PERMISSION_DENIED;
            if (!CheckIn(DataIn, 17))
                return LENGTH_ERROR;

            byte Fid = DataIn.GetByte(0);
            byte CommSettings = DataIn.GetByte(1);
            ushort AccessRights = Get2(DataIn.GetBytes(2, 2));

            long LowerLimit = Get4(DataIn.GetBytes(4, 4));
            long UpperLimit = Get4(DataIn.GetBytes(8, 4));
            long Value = Get4(DataIn.GetBytes(12, 4));
            byte MoreSettings = DataIn.GetByte(16);

            if (CurrentApplication.FileExists(Fid))
                return DUPLICATE_ERROR;

            File file = new ValueFile(Fid, CommSettings, AccessRights, LowerLimit, UpperLimit, Value, ((MoreSettings & 0x01) != 0), ((MoreSettings & 0x01) != 0));

            if (CurrentApplication.InsertFile(file))
                return OPERATION_OK;
            return APPL_INTEGRITY_ERROR;
        }

        private byte GetFileSettings(CardBuffer DataIn, ref CardBuffer DataOut)
        {
            if (!CheckIn(DataIn, 1))
                return LENGTH_ERROR;

            byte file_id = DataIn.GetByte(0);
            byte[] r;

            if (CurrentApplication == null)
                return FILE_NOT_FOUND;

            if (!CurrentApplication.SelectFile(file_id))
                return FILE_NOT_FOUND;

            if (CurrentApplication.CurrentFile is StandardOrBackupFile)
            {
                StandardOrBackupFile file = (StandardOrBackupFile)CurrentApplication.CurrentFile;

                r = new byte[7];

                r[0] = file.Type;
                r[1] = file.CommSettings;
                Set2(ref r, 2, file.AccessRights);
                Set3(ref r, 4, file.Size);

            }
            else if (CurrentApplication.CurrentFile is RecordOrCyclicFile)
            {
                RecordOrCyclicFile file = (RecordOrCyclicFile)CurrentApplication.CurrentFile;

                r = new byte[13];

                r[0] = file.Type;
                r[1] = file.CommSettings;
                Set2(ref r, 2, file.AccessRights);
                Set3(ref r, 4, file.RecordSize);
                Set3(ref r, 7, file.MaxRecordCount);
                Set3(ref r, 10, file.CurrentRecordCount);

            }
            else if (CurrentApplication.CurrentFile is ValueFile)
            {
                ValueFile file = (ValueFile)CurrentApplication.CurrentFile;

                r = new byte[17];

                r[0] = file.Type;
                r[1] = file.CommSettings;
                Set4(ref r, 2, file.LowerLimit);
                Set4(ref r, 6, file.UpperLimit);
                Set4(ref r, 10, file.LimitedCreditValue);
                if (file.LimitedCreditEnabled)
                    r[14] = 0x01;
                else
                    r[14] = 0x00;

            }
            else
                return INTERNAL_ERROR;

            DataOut = new CardBuffer(r);
            return OPERATION_OK;
        }

        private byte FileSize(CardBuffer DataIn, ref CardBuffer DataOut)
        {
            return INTERNAL_ERROR;
        }

        private byte ChangeFileSettings(CardBuffer DataIn, ref CardBuffer DataOut)
        {
            if (CurrentApplication == null)
                return PERMISSION_DENIED;
            if (!CheckIn(DataIn, 4, 9))
                return LENGTH_ERROR;

            byte[] t;

            if (DataIn.Length == 4)
            {
                t = DataIn.GetBytes(1, 3);
            }
            else if (DataIn.Length == 9)
            {
                t = DataIn.GetBytes(1, 8);
                if (!Cipher.Recv(ref t))
                    return INTEGRITY_ERROR;
            }
            else
                return LENGTH_ERROR;

            byte Fid = DataIn.GetByte(0);
            byte CommSettings = (new CardBuffer(t)).GetByte(0);
            ushort AccessRights = Get2((new CardBuffer(t)).GetBytes(1, 2));

            if (!CurrentApplication.SelectFile(Fid))
                return FILE_NOT_FOUND;

            CurrentApplication.CurrentFile.CommSettings = CommSettings;
            CurrentApplication.CurrentFile.AccessRights = AccessRights;

            return OPERATION_OK;
        }
        #endregion

        #region Flat files

        private class PendingReadData : PendingCommand
        {
            public byte[] buffer = null;
            public long offset = 0;
        }

        public byte ReadData(CardBuffer DataIn, ref CardBuffer DataOut)
        {
            if (pending == null)
                pending = new PendingReadData();
            if (!(pending is PendingReadData))
                return INTERNAL_ERROR;

            PendingReadData _pending = (PendingReadData)pending;

            if (_pending.Step == 0)
            {
                if (CurrentApplication == null)
                    return PERMISSION_DENIED;
                if (!CheckIn(DataIn, 7))
                    return LENGTH_ERROR;

                byte file_id = DataIn.GetByte(0);
                uint offset = Get3(DataIn.GetBytes(1, 3));
                uint length = Get3(DataIn.GetBytes(4, 3));
                bool padd80 = false;

                if (!CurrentApplication.SelectFile(file_id))
                    return FILE_NOT_FOUND;

                if (!(CurrentApplication.CurrentFile is StandardOrBackupFile))
                    return PERMISSION_DENIED;

                StandardOrBackupFile file = (StandardOrBackupFile)CurrentApplication.CurrentFile;

                if (length == 0)
                {
                    padd80 = true;
                    if (offset >= file.Size)
                        return BOUNDARY_ERROR;
                    length = file.Size - offset;
                }

                if (offset + length > file.Size)
                    return BOUNDARY_ERROR;

                _pending.buffer = file.ReadData(offset, length);

                switch (file.CommSettings)
                {
                    case COMM_MODE_PLAIN:
                        break;
                    case COMM_MODE_MACED:
                        _pending.buffer = Cipher.AppendMAC(_pending.buffer);
                        break;
                    case COMM_MODE_CIPHER:
                        if (!Cipher.AppendCRC(ref _pending.buffer))
                            return INTERNAL_ERROR;
                        if (!Cipher.Send(ref _pending.buffer, padd80))
                            return INTERNAL_ERROR;
                        break;
                }
            }

            long l = _pending.buffer.Length - _pending.offset;
            if (l > 59)
                l = 59;

            DataOut = new CardBuffer(_pending.buffer, _pending.offset, l);
            _pending.offset += l;

            if (_pending.offset >= _pending.buffer.Length)
                return OPERATION_OK;
            else
                return ADDITIONAL_FRAME;
        }

        private class PendingWriteData : PendingCommand
        {
            public byte[] buffer = null;
            public long offset = 0;

            public StandardOrBackupFile to_file;
            public long to_offset = 0;
            public long to_length = 0;
        }

        public byte WriteData(CardBuffer DataIn, ref CardBuffer DataOut)
        {
            if (pending == null)
                pending = new PendingWriteData();
            if (!(pending is PendingWriteData))
                return INTERNAL_ERROR;

            PendingWriteData _pending = (PendingWriteData)pending;

            if (_pending.Step == 0)
            {
                if (CurrentApplication == null)
                    return PERMISSION_DENIED;
                if (!CheckIn(DataIn, 7, 64))
                    return LENGTH_ERROR;

                byte file_id = DataIn.GetByte(0);
                _pending.to_offset = Get3(DataIn.GetBytes(1, 3));
                _pending.to_length = Get3(DataIn.GetBytes(4, 3));

                if (!CurrentApplication.SelectFile(file_id))
                    return FILE_NOT_FOUND;

                if (!(CurrentApplication.CurrentFile is StandardOrBackupFile))
                    return PERMISSION_DENIED;

                _pending.to_file = (StandardOrBackupFile)CurrentApplication.CurrentFile;

                if (!_pending.to_file.MayWriteData(_pending.to_offset, _pending.to_length))
                    return BOUNDARY_ERROR;

                long l = _pending.to_length;
                switch (_pending.to_file.CommSettings)
                {
                    case COMM_MODE_PLAIN:
                        break;
                    case COMM_MODE_MACED:
                        l += 4;
                        break;
                    case COMM_MODE_CIPHER:
                        l += 3;
                        while ((l % 8) != 0)
                            l++;
                        break;
                    default:
                        break;
                }

                _pending.buffer = new byte[l];

                for (long i = 7; i < DataIn.Length; i++)
                {
                    if (_pending.offset >= _pending.buffer.Length)
                        return LENGTH_ERROR;
                    _pending.buffer[_pending.offset++] = DataIn.GetByte(i);
                }

            }
            else
            {
                if (!CheckIn(DataIn, 0, 64))
                    return LENGTH_ERROR;

                for (long i = 0; i < DataIn.Length; i++)
                {
                    if (_pending.offset >= _pending.buffer.Length)
                        return LENGTH_ERROR;
                    _pending.buffer[_pending.offset++] = DataIn.GetByte(i);
                }
            }

            if (_pending.buffer.Length > _pending.offset)
                return ADDITIONAL_FRAME;

            switch (_pending.to_file.CommSettings)
            {
                case COMM_MODE_PLAIN:
                    break;
                case COMM_MODE_MACED:
                    break;
                case COMM_MODE_CIPHER:
                    if (!Cipher.Recv(ref _pending.buffer))
                        return INTEGRITY_ERROR;
                    break;
            }

            _pending.to_file.WriteData(_pending.to_offset, _pending.buffer, _pending.to_length);
            return OPERATION_OK;
        }
        #endregion

        #region Record files
        private class PendingReadRecords : PendingCommand
        {
            public byte[] buffer = null;
            public long offset = 0;
        }

        public byte ReadRecords(CardBuffer DataIn, ref CardBuffer DataOut)
        {
            if (pending == null)
                pending = new PendingReadRecords();
            if (!(pending is PendingReadRecords))
                return INTERNAL_ERROR;

            PendingReadRecords _pending = (PendingReadRecords)pending;

            if (_pending.Step == 0)
            {
                if (CurrentApplication == null)
                    return PERMISSION_DENIED;
                if (!CheckIn(DataIn, 7))
                    return LENGTH_ERROR;

                byte file_id = DataIn.GetByte(0);
                uint first = Get3(DataIn.GetBytes(1, 3));
                if (first == 0x00FFFFFF)
                    first = 0;
                uint count = Get3(DataIn.GetBytes(4, 3));
                bool padd80 = false;

                if (!CurrentApplication.SelectFile(file_id))
                    return FILE_NOT_FOUND;

                if (!(CurrentApplication.CurrentFile is RecordOrCyclicFile))
                    return PERMISSION_DENIED;

                RecordOrCyclicFile file = (RecordOrCyclicFile)CurrentApplication.CurrentFile;

                if (count == 0)
                    padd80 = true;
                byte r = file.ReadRecords(first, count, ref _pending.buffer);
                if (r != OPERATION_OK)
                    return r;

                switch (file.CommSettings)
                {
                    case COMM_MODE_PLAIN:
                        break;
                    case COMM_MODE_MACED:
                        _pending.buffer = Cipher.AppendMAC(_pending.buffer);
                        break;
                    case COMM_MODE_CIPHER:
                        if (!Cipher.AppendCRC(ref _pending.buffer))
                            return INTERNAL_ERROR;
                        if (!Cipher.Send(ref _pending.buffer, padd80))
                            return INTERNAL_ERROR;
                        break;
                }
            }

            if (_pending.buffer == null)
            {
                return OPERATION_OK;
            }
            else
            {
                long l = _pending.buffer.Length - _pending.offset;
                if (l > 59)
                    l = 59;

                DataOut = new CardBuffer(_pending.buffer, _pending.offset, l);
                _pending.offset += l;

                if (_pending.offset >= _pending.buffer.Length)
                    return OPERATION_OK;
                else
                    return ADDITIONAL_FRAME;
            }
        }

        private class PendingWriteRecord : PendingCommand
        {
            public byte[] buffer = null;
            public long offset = 0;

            public RecordOrCyclicFile to_file;
            public long to_offset = 0;
            public long to_length = 0;
        }

        public byte WriteRecord(CardBuffer DataIn, ref CardBuffer DataOut)
        {
            if (pending == null)
                pending = new PendingWriteRecord();
            if (!(pending is PendingWriteRecord))
                return INTERNAL_ERROR;

            PendingWriteRecord _pending = (PendingWriteRecord)pending;

            if (_pending.Step == 0)
            {
                if (CurrentApplication == null)
                    return PERMISSION_DENIED;
                if (!CheckIn(DataIn, 7, 64))
                    return LENGTH_ERROR;

                byte file_id = DataIn.GetByte(0);
                _pending.to_offset = Get3(DataIn.GetBytes(1, 3));
                _pending.to_length = Get3(DataIn.GetBytes(4, 3));

                if (!CurrentApplication.SelectFile(file_id))
                    return FILE_NOT_FOUND;

                if (!(CurrentApplication.CurrentFile is RecordOrCyclicFile))
                    return PERMISSION_DENIED;

                _pending.to_file = (RecordOrCyclicFile)CurrentApplication.CurrentFile;

                if (_pending.to_file.PendingClear())
                    return PERMISSION_DENIED;

                if (!_pending.to_file.MayWriteRecord(_pending.to_offset, _pending.to_length))
                    return BOUNDARY_ERROR;

                long l = _pending.to_length;
                switch (_pending.to_file.CommSettings)
                {
                    case COMM_MODE_PLAIN:
                        break;
                    case COMM_MODE_MACED:
                        l += 4;
                        break;
                    case COMM_MODE_CIPHER:
                        l += 3;
                        while ((l % 8) != 0)
                            l++;
                        break;
                    default:
                        break;
                }

                _pending.buffer = new byte[l];

                for (long i = 7; i < DataIn.Length; i++)
                {
                    if (_pending.offset >= _pending.buffer.Length)
                        return LENGTH_ERROR;
                    _pending.buffer[_pending.offset++] = DataIn.GetByte(i);
                }

            }
            else
            {
                if (!CheckIn(DataIn, 0, 64))
                    return LENGTH_ERROR;

                for (long i = 0; i < DataIn.Length; i++)
                {
                    if (_pending.offset >= _pending.buffer.Length)
                        return LENGTH_ERROR;
                    _pending.buffer[_pending.offset++] = DataIn.GetByte(i);
                }
            }

            if (_pending.buffer.Length > _pending.offset)
                return ADDITIONAL_FRAME;

            _pending.to_file.WriteRecord(_pending.to_offset, _pending.buffer);
            return OPERATION_OK;
        }

        public byte ClearRecordFile(CardBuffer DataIn, ref CardBuffer DataOut)
        {
            if (CurrentApplication == null)
                return PERMISSION_DENIED;
            if (!CheckIn(DataIn, 1))
                return LENGTH_ERROR;

            byte file_id = DataIn.GetByte(0);

            if (!CurrentApplication.SelectFile(file_id))
                return FILE_NOT_FOUND;

            if (!(CurrentApplication.CurrentFile is RecordOrCyclicFile))
                return PERMISSION_DENIED;

            RecordOrCyclicFile file = (RecordOrCyclicFile)CurrentApplication.CurrentFile;

            file.Clear();
            return OPERATION_OK;
        }
        #endregion

        #region Value files
        public byte GetValue(CardBuffer DataIn, ref CardBuffer DataOut)
        {
            ValueFile file;

            if (CurrentApplication == null)
                return PERMISSION_DENIED;

            byte file_id = DataIn.GetByte(0);

            if (!CurrentApplication.SelectFile(file_id))
                return FILE_NOT_FOUND;

            if (!(CurrentApplication.CurrentFile is ValueFile))
                return PERMISSION_DENIED;

            file = (ValueFile)CurrentApplication.CurrentFile;

            byte[] r = new byte[4];
            Set4(ref r, 0, file.Value);

            switch (file.CommSettings)
            {
                case COMM_MODE_PLAIN:
                    break;
                case COMM_MODE_MACED:
                    r = Cipher.AppendMAC(r);
                    break;
                case COMM_MODE_CIPHER:
                    if (!Cipher.AppendCRC(ref r))
                        return INTERNAL_ERROR;
                    if (!Cipher.Send(ref r))
                        return INTERNAL_ERROR;
                    break;
            }

            DataOut = new CardBuffer(r);

            return OPERATION_OK;
        }

        public byte Debit(CardBuffer DataIn, ref CardBuffer DataOut)
        {
            ValueFile file;

            if (CurrentApplication == null)
                return PERMISSION_DENIED;

            byte file_id = DataIn.GetByte(0);

            if (!CurrentApplication.SelectFile(file_id))
                return FILE_NOT_FOUND;

            if (!(CurrentApplication.CurrentFile is ValueFile))
                return PERMISSION_DENIED;

            file = (ValueFile)CurrentApplication.CurrentFile;

            byte[] t = DataIn.GetBytes(1, -1);

            switch (file.CommSettings)
            {
                case COMM_MODE_PLAIN:
                    break;
                case COMM_MODE_MACED:
                    if (!Cipher.CheckMAC(t))
                        return INTEGRITY_ERROR;
                    break;
                case COMM_MODE_CIPHER:
                    if (!Cipher.Recv(ref t))
                        return INTEGRITY_ERROR;
                    break;
            }

            long amount = Get4(t);

            if (amount < 0)
                return PARAMETER_ERROR;

            if (!file.Debit(amount))
                return BOUNDARY_ERROR;

            return OPERATION_OK;
        }

        public byte Credit(CardBuffer DataIn, ref CardBuffer DataOut)
        {
            ValueFile file;

            if (CurrentApplication == null)
                return PERMISSION_DENIED;

            byte file_id = DataIn.GetByte(0);

            if (!CurrentApplication.SelectFile(file_id))
                return FILE_NOT_FOUND;

            if (!(CurrentApplication.CurrentFile is ValueFile))
                return PERMISSION_DENIED;

            file = (ValueFile)CurrentApplication.CurrentFile;

            byte[] t = DataIn.GetBytes(1, -1);

            switch (file.CommSettings)
            {
                case COMM_MODE_PLAIN:
                    break;
                case COMM_MODE_MACED:
                    if (!Cipher.CheckMAC(t))
                        return INTEGRITY_ERROR;
                    break;
                case COMM_MODE_CIPHER:
                    if (!Cipher.Recv(ref t))
                        return INTEGRITY_ERROR;
                    break;
            }

            long amount = Get4(t);

            if (amount < 0)
                return PARAMETER_ERROR;

            if (!file.Credit(amount))
                return BOUNDARY_ERROR;

            return OPERATION_OK;
        }

        public byte LimitedCredit(CardBuffer DataIn, ref CardBuffer DataOut)
        {
            ValueFile file;

            if (CurrentApplication == null)
                return PERMISSION_DENIED;

            byte file_id = DataIn.GetByte(0);

            if (!CurrentApplication.SelectFile(file_id))
                return FILE_NOT_FOUND;

            if (!(CurrentApplication.CurrentFile is ValueFile))
                return PERMISSION_DENIED;

            file = (ValueFile)CurrentApplication.CurrentFile;

            byte[] t = DataIn.GetBytes(1, -1);

            switch (file.CommSettings)
            {
                case COMM_MODE_PLAIN:
                    break;
                case COMM_MODE_MACED:
                    if (!Cipher.CheckMAC(t))
                        return INTEGRITY_ERROR;
                    break;
                case COMM_MODE_CIPHER:
                    if (!Cipher.Recv(ref t))
                        return INTEGRITY_ERROR;
                    break;
            }

            long amount = Get4(t);

            if (amount < 0)
                return PARAMETER_ERROR;

            if (!file.LimitedCredit(amount))
                return BOUNDARY_ERROR;

            return OPERATION_OK;
        }
        #endregion

        #region Transactions
        public byte CommitTransaction(CardBuffer DataIn, ref CardBuffer DataOut)
        {
            if (CurrentApplication == null)
                return PERMISSION_DENIED;

            if (CurrentApplication.Commit())
                return OPERATION_OK;

            return EEPROM_ERROR;
        }

        public byte AbortTransaction(CardBuffer DataIn, ref CardBuffer DataOut)
        {
            if (CurrentApplication == null)
                return PERMISSION_DENIED;

            if (CurrentApplication.Abort())
                return OPERATION_OK;

            return EEPROM_ERROR;
        }
        #endregion

        #region Authentication and ciphering
        private class PendingAuthenticate : PendingCommand
        {
            public byte KeyNo;
            public Key Key;
            public byte[] RndA;
            public byte[] RndB;
        }

        public byte Authenticate(CardBuffer DataIn, ref CardBuffer DataOut)
        {
            Key key;
            int i;
            byte[] t;

            if (pending == null)
                pending = new PendingAuthenticate();
            if (!(pending is PendingAuthenticate))
                return INTERNAL_ERROR;

            PendingAuthenticate _pending = (PendingAuthenticate)pending;

            switch (_pending.Step)
            {
                case 0:

                    AuthKeyNo = -1;

                    if (!CheckIn(DataIn, 1))
                        return LENGTH_ERROR;

                    _pending.KeyNo = DataIn.GetByte(0);

                    if (CurrentApplication == null)
                    {
                        if (_pending.KeyNo != 0)
                            return NO_SUCH_KEY;
                        _pending.Key = RootKey;
                    }
                    else
                    {
                        if (_pending.KeyNo >= CurrentApplication.Keys.Count)
                            return NO_SUCH_KEY;
                        _pending.Key = CurrentApplication.Keys[_pending.KeyNo];
                    }

                    _pending.RndB = new byte[8];
                    for (i = 0; i < 8; i++)
                        _pending.RndB[i] = (byte)(1 + i);

                    Cipher = new DesfireEV0Cipher(_pending.Key.Value);

                    t = new byte[8];

                    for (i = 0; i < 8; i++)
                        t[i] = _pending.RndB[i];
                    if (!Cipher.Send(ref t))
                        return INTERNAL_ERROR;

                    DataOut = new CardBuffer(t);
                    return ADDITIONAL_FRAME;

                case 1:

                    if (!CheckIn(DataIn, 16))
                        return LENGTH_ERROR;

                    t = DataIn.GetBytes();
                    if (!Cipher.Recv(ref t))
                        return INTEGRITY_ERROR;

                    _pending.RndA = new byte[8];
                    for (i = 0; i < 8; i++)
                        _pending.RndA[i] = t[i];

                    t = new byte[8];

                    for (i = 0; i < 7; i++)
                        t[i] = _pending.RndA[i + 1];
                    t[7] = _pending.RndA[0];

                    if (!Cipher.Send(ref t))
                        return INTERNAL_ERROR;

                    DataOut = new CardBuffer(t);

                    t = new byte[16];

                    if (_pending.Key.IsSingleDes())
                    {
                        for (i = 0; i < 4; i++)
                            t[0 + i] = _pending.RndA[0 + i];
                        for (i = 0; i < 4; i++)
                            t[4 + i] = _pending.RndB[0 + i];
                        for (i = 0; i < 4; i++)
                            t[8 + i] = _pending.RndA[0 + i];
                        for (i = 0; i < 4; i++)
                            t[12 + i] = _pending.RndB[0 + i];
                    }
                    else
                    {
                        for (i = 0; i < 4; i++)
                            t[0 + i] = _pending.RndA[0 + i];
                        for (i = 0; i < 4; i++)
                            t[4 + i] = _pending.RndB[0 + i];
                        for (i = 0; i < 4; i++)
                            t[8 + i] = _pending.RndA[4 + i];
                        for (i = 0; i < 4; i++)
                            t[12 + i] = _pending.RndB[4 + i];
                    }

                    key = new Key(t);

                    Cipher = new DesfireEV0Cipher(key.Value);
                    AuthKeyNo = _pending.KeyNo;

                    return OPERATION_OK;
            }

            return INTERNAL_ERROR;
        }



        #endregion

        #region Misc

        public bool CheckIn(CardBuffer DataIn, uint minLen, uint maxLen)
        {
            if (DataIn == null)
            {
                if (minLen > 0)
                    return false;
            }
            else if (DataIn.Length < minLen)
            {
                return false;
            }
            else if (DataIn.Length > maxLen)
            {
                return false;
            }
            return true;
        }

        public bool CheckIn(CardBuffer DataIn, uint Len)
        {
            return CheckIn(DataIn, Len, Len);
        }

        public long Get4(byte[] buf, uint offset)
        {
            uint t;
            int r;

            t = buf[offset + 3];
            t *= 0x00000100;
            t += buf[offset + 2];
            t *= 0x00000100;
            t += buf[offset + 1];
            t *= 0x00000100;
            t += buf[offset + 0];

            if (t < 0x80000000)
                r = (int)t;
            else
                r = (int)(t - 0x100000000);

            return r;
        }

        public long Get4(byte[] buf)
        {
            return Get4(buf, 0);
        }

        public uint Get3(byte[] buf, uint offset)
        {
            uint r;

            r = buf[offset + 2];
            r *= 0x00000100;
            r += buf[offset + 1];
            r *= 0x00000100;
            r += buf[offset + 0];

            return r;
        }

        public uint Get3(byte[] buf)
        {
            return Get3(buf, 0);
        }

        public ushort Get2(byte[] buf, uint offset)
        {
            ushort r;

            r = buf[offset + 1];
            r *= 0x0100;
            r += buf[offset + 0];

            return r;
        }

        public ushort Get2(byte[] buf)
        {
            return Get2(buf, 0);
        }

        public void Set4(ref byte[] buf, uint offset, long val)
        {
            uint t;

            if (val >= 0)
                t = (uint)val;
            else
                t = (uint)(val + 0x100000000);

            buf[offset + 0] = (byte)(t % 0x00000100);
            t /= 0x00000100;
            buf[offset + 1] = (byte)(t % 0x00000100);
            t /= 0x00000100;
            buf[offset + 2] = (byte)(t % 0x00000100);
            t /= 0x00000100;
            buf[offset + 3] = (byte)(t % 0x00000100);
        }


        public void Set3(ref byte[] buf, uint offset, uint val)
        {
            buf[offset + 0] = (byte)(val % 0x00000100);
            val /= 0x00000100;
            buf[offset + 1] = (byte)(val % 0x00000100);
            val /= 0x00000100;
            buf[offset + 2] = (byte)(val % 0x00000100);
        }

        public void Set2(ref byte[] buf, uint offset, ushort val)
        {
            buf[offset + 0] = (byte)(val % 0x0100);
            val /= 0x0100;
            buf[offset + 1] = (byte)(val % 0x0100);
        }
        #endregion

        #region XML import/export
        public bool SaveToXML(string FileName)
        {
            XmlTextWriter xtw = new XmlTextWriter(FileName, Encoding.UTF8);
            xtw.Formatting = Formatting.Indented;

            xtw.WriteStartDocument(true);
            xtw.WriteStartElement("Desfire");
            xtw.WriteElementString("Version", "EV0");
            xtw.WriteElementString("RootKeySettings", String.Format("{0:X02}", RootKeySettings));
            xtw.WriteStartElement("RootKey");
            RootKey.ToXML(xtw);
            xtw.WriteEndElement();
            if (Applications.Count > 0)
            {
                xtw.WriteStartElement("Applications");
                for (int i = 0; i < Applications.Count; i++)
                {
                    xtw.WriteStartElement("Application");
                    Applications[i].ToXML(xtw);
                    xtw.WriteEndElement();
                }
                xtw.WriteEndElement();
            }
            xtw.WriteEndElement();
            xtw.WriteEndDocument();
            xtw.Flush();
            xtw.Close();

            return true;
        }

        private bool LoadFromXML_Key(XmlTextReader XTR, ref Key key)
        {
            key = new Key();

            while (XTR.Read())
            {
                if (XTR.IsStartElement())
                {
                    string n = XTR.Name;

                    if (n.Equals("KeyVersion"))
                    {
                        string v = XTR.ReadString();
                        key.Version = byte.Parse(v, System.Globalization.NumberStyles.HexNumber);
                    }
                    else if (n.Equals("KeyValue"))
                    {
                        string v = XTR.ReadString();
                        key.Value = (new CardBuffer(v)).GetBytes();
                        if (key == null)
                            return false;
                    }
                    else
                    {
                        Logger.Trace("Desfire/Application/Key/" + n);
                        return false;
                    }
                }
                else
                    break;
            }

            return true;
        }

        private bool LoadFromXML_Application_File(XmlTextReader XTR, ref File file)
        {
            Dictionary<string, string> xml = new Dictionary<string, string>();
            file = null;

            while (XTR.Read())
            {
                if (XTR.IsStartElement())
                {
                    string n = XTR.Name;

                    if (n.Equals("Records"))
                    {
                        int i = 0;

                        while (XTR.Read())
                        {
                            if (XTR.IsStartElement())
                            {
                                if (XTR.Name.Equals("Record"))
                                {
                                    n = String.Format("Record.{0}", i++);
                                    string v = XTR.ReadString();
                                    Logger.Trace(n + "=" + v);
                                    xml[n] = v;
                                }
                                else
                                    return false;
                            }
                            else
                                break;
                        }

                    }
                    else
                    {
                        string v = XTR.ReadString();
                        Logger.Trace(n + "=" + v);
                        xml[n] = v;

                        if (n.Equals("Type"))
                        {
                            if (v.Equals("Standard"))
                            {
                                file = new StandardFile();
                            }
                            else if (v.Equals("Backup"))
                            {
                                file = new BackupFile();
                            }
                            else if (v.Equals("Record"))
                            {
                                file = new RecordFile();
                            }
                            else if (v.Equals("Cyclic"))
                            {
                                file = new CyclicFile();
                            }
                            else if (v.Equals("Value"))
                            {
                                file = new ValueFile();
                            }
                            else
                            {
                                file = null;
                            }
                        }
                    }
                }
                else
                    break;
            }

            if (file == null)
            {
                Logger.Trace("file KO");
                return false;
            }

            if (!file.FromXml(xml))
            {
                Logger.Trace("FromXml KO");
                return false;
            }

            Logger.Trace("OK pour ce fichier");

            return true;
        }

        private bool LoadFromXML_Application(XmlTextReader XTR, ref Application application)
        {
            application = new Application();

            while (XTR.Read())
            {
                if (XTR.IsStartElement())
                {
                    string n = XTR.Name;

                    if (n.Equals("AID"))
                    {
                        string v = XTR.ReadString();
                        application.Aid = uint.Parse(v, System.Globalization.NumberStyles.HexNumber);
                    }
                    else if (n.Equals("KeySettings"))
                    {
                        string v = XTR.ReadString();
                        application.KeySettings = byte.Parse(v, System.Globalization.NumberStyles.HexNumber);
                    }
                    else if (n.Equals("Keys"))
                    {
                        while (XTR.Read())
                        {
                            if (XTR.IsStartElement())
                            {
                                if (XTR.Name.Equals("Key"))
                                {
                                    Key key = null;
                                    if (!LoadFromXML_Key(XTR, ref key))
                                        return false;
                                    application.Keys.Add(key);
                                }
                                else
                                    return false;
                            }
                            else
                                break;
                        }
                    }
                    else if (n.Equals("Files"))
                    {
                        while (XTR.Read())
                        {
                            if (XTR.IsStartElement())
                            {
                                if (XTR.Name.Equals("File"))
                                {
                                    File file = null;
                                    if (!LoadFromXML_Application_File(XTR, ref file))
                                        return false;
                                    if (file == null)
                                        return false;
                                    application.Files.Add(file);
                                }
                                else
                                    return false;
                            }
                            else
                                break;
                        }
                    }
                    else
                    {
                        Logger.Trace("Desfire/Application/" + n);
                        return false;
                    }
                }
                else
                    break;
            }

            return true;
        }

        public bool LoadFromXML(string FileName)
        {
            XmlTextReader xtr = new XmlTextReader(FileName);

            bool inDesfire = false;

            while (xtr.Read())
            {
                if (xtr.IsStartElement())
                {
                    string n = xtr.Name;

                    if (inDesfire)
                    {
                        if (n.Equals("Version"))
                        {

                        }
                        else if (n.Equals("RootKeySettings"))
                        {
                            string v = xtr.ReadString();
                            RootKeySettings = byte.Parse(v, System.Globalization.NumberStyles.HexNumber);
                        }
                        else if (n.Equals("RootKey"))
                        {
                            if (!LoadFromXML_Key(xtr, ref RootKey))
                                goto failed;
                            if (RootKey == null)
                                goto failed;
                        }
                        else if (n.Equals("Applications"))
                        {
                            while (xtr.Read())
                            {
                                if (xtr.IsStartElement())
                                {
                                    if (xtr.Name.Equals("Application"))
                                    {
                                        Application application = null;
                                        if (!LoadFromXML_Application(xtr, ref application))
                                            goto failed;
                                        if (application == null)
                                            goto failed;
                                        Applications.Add(application);
                                    }
                                    else
                                        goto failed;
                                }
                                else
                                    break;
                            }
                        }
                        else
                            goto failed;
                    }
                    else
                    {
                        if (n.Equals("Desfire"))
                        {
                            inDesfire = true;
                        }
                    }
                }
            }

            xtr.Close();
            return true;
        failed:
            xtr.Close();
            return false;
        }
        #endregion
    }
}
