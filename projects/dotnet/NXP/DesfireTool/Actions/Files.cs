using SpringCard.LibCs;
using SpringCard.PCSC;
using SpringCard.PCSC.CardHelpers;
using System;

namespace DesfireTool
{
    partial class Program
    {
        Result CreateStdDataFile(Desfire desfire, Parameters parameters)
        {
            if (parameters.FileId < 0)
            {
                ConsoleError("CreateStdDataFile: File ID must be specified");
                ConsoleWriteLine("Use parameter --file-id|-f");
                return Result.MissingArgument;
            }
            if (parameters.CommMode < 0)
            {
                ConsoleError("CreateStdDataFile: Communication mode must be specified");
                ConsoleWriteLine("Use parameter --comm-mode|-m");
                return Result.MissingArgument;
            }
            if (parameters.AccessRights < 0)
            {
                ConsoleError("CreateStdDataFile: Access rights must be specified");
                ConsoleWriteLine("Use parameter --access-rights");
                return Result.MissingArgument;
            }
            if (parameters.Size < 0)
            {
                ConsoleError("CreateStdDataFile: Size must be specified");
                ConsoleWriteLine("Use parameter --size|-z");
                return Result.MissingArgument;
            }

            long rc = desfire.CreateStdDataFile((byte)parameters.FileId, (byte)parameters.CommMode, (ushort)parameters.AccessRights, (uint)parameters.Size);
            if (rc != SCARD.S_SUCCESS)
                return OnError("CreateStdDataFile", rc);

            return Result.Success;
        }

        Result CreateBackupDataFile(Desfire desfire, Parameters parameters)
        {
            if (parameters.FileId < 0)
            {
                ConsoleError("CreateBackupDataFile: File ID must be specified");
                ConsoleWriteLine("Use parameter --file-id|-f");
                return Result.MissingArgument;
            }
            if (parameters.CommMode < 0)
            {
                ConsoleError("CreateBackupDataFile: Communication mode must be specified");
                ConsoleWriteLine("Use parameter --comm-mode|-m");
                return Result.MissingArgument;
            }
            if (parameters.AccessRights < 0)
            {
                ConsoleError("CreateBackupDataFile: Access rights must be specified");
                ConsoleWriteLine("Use parameter --access-rights");
                return Result.MissingArgument;
            }
            if (parameters.Size < 0)
            {
                ConsoleError("CreateBackupDataFile: Size must be specified");
                ConsoleWriteLine("Use parameter --size|-z");
                return Result.MissingArgument;
            }

            long rc = desfire.CreateBackupDataFile((byte)parameters.FileId, (byte)parameters.CommMode, (ushort)parameters.AccessRights, (uint)parameters.Size);
            if (rc != SCARD.S_SUCCESS)
                return OnError("CreateStdDataFile", rc);

            return Result.Success;
        }

        Result CreateValueFile(Desfire desfire, Parameters parameters)
        {
            if (parameters.FileId < 0)
            {
                ConsoleError("CreateValueFile: File ID must be specified");
                ConsoleWriteLine("Use parameter --file-id|-f");
                return Result.MissingArgument;
            }
            if (parameters.CommMode < 0)
            {
                ConsoleError("CreateValueFile: Communication mode must be specified");
                ConsoleWriteLine("Use parameter --comm-mode|-m");
                return Result.MissingArgument;
            }
            if (parameters.AccessRights < 0)
            {
                ConsoleError("CreateValueFile: Access rights must be specified");
                ConsoleWriteLine("Use parameter --access-rights");
                return Result.MissingArgument;
            }
            if (parameters.LowerLimit == NO_VALUE_64)
            {
                ConsoleError("CreateValueFile: Lower limit must be specified");
                ConsoleWriteLine("Use parameter --lower-limit");
                return Result.MissingArgument;
            }
            if (parameters.UpperLimit == NO_VALUE_64)
            {
                ConsoleError("CreateValueFile: Upper limit must be specified");
                ConsoleWriteLine("Use parameter --upper-limit");
                return Result.MissingArgument;
            }
            if (parameters.InitialValue == NO_VALUE_64)
            {
                ConsoleError("CreateValueFile: Initial Value must be specified");
                ConsoleWriteLine("Use parameter --initial-value");
                return Result.MissingArgument;
            }
            if (parameters.ValueFlags < 0)
            {
                ConsoleError("CreateValueFile: Value flags must be specified");
                ConsoleWriteLine("Use parameter --value-flags");
                return Result.MissingArgument;
            }

            long rc = desfire.CreateValueFile((byte)parameters.FileId, (byte)parameters.CommMode, (ushort)parameters.AccessRights, parameters.LowerLimit, parameters.UpperLimit, parameters.InitialValue, (byte)parameters.ValueFlags);
            if (rc != SCARD.S_SUCCESS)
                return OnError("CreateValueFile", rc);

            return Result.Success;
        }

        Result CreateCyclicRecordFile(Desfire desfire, Parameters parameters)
        {
            if (parameters.FileId < 0)
            {
                ConsoleError("CreateCyclicRecordFile: File ID must be specified");
                ConsoleWriteLine("Use parameter --file-id|-f");
                return Result.MissingArgument;
            }
            if (parameters.CommMode < 0)
            {
                ConsoleError("CreateCyclicRecordFile: Communication mode must be specified");
                ConsoleWriteLine("Use parameter --comm-mode|-m");
                return Result.MissingArgument;
            }
            if (parameters.AccessRights < 0)
            {
                ConsoleError("CreateCyclicRecordFile: Access rights must be specified");
                ConsoleWriteLine("Use parameter --access-rights");
                return Result.MissingArgument;
            }
            if (parameters.Size < 0)
            {
                ConsoleError("CreateCyclicRecordFile: Size of records must be specified");
                ConsoleWriteLine("Use parameter --size|-z");
                return Result.MissingArgument;
            }
            if (parameters.Count < 0)
            {
                ConsoleError("CreateCyclicRecordFile: Count of records must be specified");
                ConsoleWriteLine("Use parameter --count|-c");
                return Result.MissingArgument;
            }

            long rc = desfire.CreateCyclicRecordFile((byte)parameters.FileId, (byte)parameters.CommMode, (ushort)parameters.AccessRights, (uint)parameters.Size, (uint)parameters.Count);
            if (rc != SCARD.S_SUCCESS)
                return OnError("CreateCyclicRecordFile", rc);

            return Result.Success;
        }

        Result CreateLinearRecordFile(Desfire desfire, Parameters parameters)
        {
            if (parameters.FileId < 0)
            {
                ConsoleError("CreateLinearRecordFile: File ID must be specified");
                ConsoleWriteLine("Use parameter --file-id|-f");
                return Result.MissingArgument;
            }
            if (parameters.CommMode < 0)
            {
                ConsoleError("CreateLinearRecordFile: Communication mode must be specified");
                ConsoleWriteLine("Use parameter --comm-mode|-m");
                return Result.MissingArgument;
            }
            if (parameters.AccessRights < 0)
            {
                ConsoleError("CreateLinearRecordFile: Access rights must be specified");
                ConsoleWriteLine("Use parameter --access-rights");
                return Result.MissingArgument;
            }
            if (parameters.Size < 0)
            {
                ConsoleError("CreateLinearRecordFile: Size of records must be specified");
                ConsoleWriteLine("Use parameter --size|-z");
                return Result.MissingArgument;
            }
            if (parameters.Count < 0)
            {
                ConsoleError("CreateLinearRecordFile: Count of records must be specified");
                ConsoleWriteLine("Use parameter --count|-c");
                return Result.MissingArgument;
            }

            long rc = desfire.CreateLinearRecordFile((byte)parameters.FileId, (byte)parameters.CommMode, (ushort)parameters.AccessRights, (uint)parameters.Size, (uint)parameters.Count);
            if (rc != SCARD.S_SUCCESS)
                return OnError("CreateLinearRecordFile", rc);

            return Result.Success;
        }

        Result WriteData(Desfire desfire, Parameters parameters)
        {
            if (parameters.FileId < 0)
            {
                ConsoleError("WriteData: File ID must be specified");
                ConsoleWriteLine("Use parameter --file-id|-f");
                return Result.MissingArgument;
            }
            if (parameters.Offset < 0)
            {
                ConsoleError("WriteData: Offset must be specified");
                ConsoleWriteLine("Use parameter --offset|-o");
                return Result.MissingArgument;
            }
            if (parameters.Data == null)
            {
                ConsoleError("WriteData: Data input must be specified");
                ConsoleWriteLine("Use parameter --data|-d");
                return Result.MissingArgument;
            }

            if (parameters.CommMode < 0)
            {
                long rc = desfire.WriteData2((byte)parameters.FileId, (uint)parameters.Offset, (uint)parameters.Data.Length, parameters.Data);
                if (rc != SCARD.S_SUCCESS)
                    return OnError("WriteData", rc);
            }
            else
            {
                long rc = desfire.WriteData((byte)parameters.FileId, (byte)parameters.CommMode, (uint)parameters.Offset, (uint)parameters.Data.Length, parameters.Data);
                if (rc != SCARD.S_SUCCESS)
                    return OnError("WriteData", rc);
            }

            return Result.Success;
        }


        Result CommitTransaction(Desfire desfire, Parameters parameters)
        {
            long rc = desfire.CommitTransaction();
            if (rc != SCARD.S_SUCCESS)
                OnError("CommitTransaction", rc);

            return Result.Success;
        }

        Result ReadData(Desfire desfire, Parameters parameters)
        {
            if (parameters.FileId < 0)
            {
                ConsoleError("ReadData: File ID must be specified");
                ConsoleWriteLine("Use parameter --file-id|-f");
                return Result.MissingArgument;
            }
            if (parameters.Offset < 0)
            {
                ConsoleError("ReadData: Offset must be specified");
                ConsoleWriteLine("Use parameter --offset|-o");
                return Result.MissingArgument;
            }
            if (parameters.Size < 0)
            {
                ConsoleError("ReadData: Size must be specified");
                ConsoleWriteLine("Use parameter --size|-z");
                return Result.MissingArgument;
            }

            byte[] output = new byte[59];
            uint read_length = 0;

            if (parameters.CommMode < 0)
            {
                long rc = desfire.ReadData2((byte)parameters.FileId, (uint)parameters.Offset, (uint)parameters.Size, ref output, ref read_length);
                if (rc != SCARD.S_SUCCESS)
                    return OnError("ReadData", rc);
            }
            else
            {
                long rc = desfire.ReadData((byte)parameters.FileId, (byte)parameters.CommMode, (uint)parameters.Offset, (uint)parameters.Size, ref output, ref read_length);
                if (rc != SCARD.S_SUCCESS)
                    return OnError("ReadData", rc);
            }

            if (read_length > 0)
            {
                Console.WriteLine("Desfire 'ReadData' command done: " + BinConvert.ToHex(output, read_length));
            }
            else
            {
                Console.WriteLine("Desfire 'ReadData' command done: no data.");
            }

            return Result.Success;
        }


        Result ReadRecords(Desfire desfire, Parameters parameters)
        {
            if (parameters.FileId < 0)
            {
                ConsoleError("ReadRecords: File ID must be specified");
                ConsoleWriteLine("Use parameter --file-id|-f");
                return Result.MissingArgument;
            }
            if (parameters.Number < 0)
            {
                ConsoleError("ReadRecords: Record Number must be specified");
                ConsoleWriteLine("Use parameter --number");
                return Result.MissingArgument;
            }
            if (parameters.Count < 0)
            {
                ConsoleError("ReadRecords: Record Count must be specified");
                ConsoleWriteLine("Use parameter --count");
                return Result.MissingArgument;
            }
            if (parameters.Size < 0)
            {
                ConsoleError("ReadRecords: Record Size must be specified");
                ConsoleWriteLine("Use parameter --size");
                return Result.MissingArgument;
            }

            byte[] output = new byte[59];
            uint record_count = 0;

            /* retreive some records */
            if (parameters.CommMode < 0)
            {
                long rc = desfire.ReadRecords2((byte)parameters.FileId, (uint)parameters.Number, (uint)parameters.Count, (uint)parameters.Size, ref output, ref record_count);
                if (rc != SCARD.S_SUCCESS)
                    return OnError("ReadRecords", rc);
            }
            else
            {
                long rc = desfire.ReadRecords((byte)parameters.FileId, (byte)parameters.CommMode, (uint)parameters.Number, (uint)parameters.Count, (uint)parameters.Size, ref output, ref record_count);
                if (rc != SCARD.S_SUCCESS)
                    return OnError("ReadRecords", rc);
            }

            if (record_count > 0)
            {
                Console.WriteLine("Desfire 'ReadRecords' command done: " + BinConvert.ToHex(output, (int)(record_count * parameters.Size)));
            }
            else
            {
                Console.WriteLine("Desfire 'ReadRecords' command done: no data.");
            }

            return Result.Success;
        }

        Result WriteRecord(Desfire desfire, Parameters parameters)
        {
            if (parameters.FileId < 0)
            {
                ConsoleError("WriteRecord: File ID must be specified");
                ConsoleWriteLine("Use parameter --file-id|-f");
                return Result.MissingArgument;
            }
            if (parameters.Number < 0)
            {
                ConsoleError("WriteRecord: Record Number must be specified");
                ConsoleWriteLine("Use parameter --number");
                return Result.MissingArgument;
            }
            if (parameters.Data == null)
            {
                ConsoleError("WriteRecord: Data input must be specified");
                ConsoleWriteLine("Use parameter --data|-d");
                return Result.MissingArgument;
            }

            if (parameters.CommMode < 0)
            {
                long rc = desfire.WriteRecord2((byte)parameters.FileId, (uint)parameters.Number, (uint)parameters.Data.Length, parameters.Data);
                if (rc != SCARD.S_SUCCESS)
                    return OnError("WriteRecord", rc);
            }
            else
            {
                long rc = desfire.WriteRecord((byte)parameters.FileId, (byte)parameters.CommMode, (uint)parameters.Number, (uint)parameters.Data.Length, parameters.Data);
                if (rc != SCARD.S_SUCCESS)
                    return OnError("WriteRecord", rc);
            }

            return Result.Success;
        }

    }
}
