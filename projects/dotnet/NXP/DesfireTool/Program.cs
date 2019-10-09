using System;
using System.IO;
using Sprincard.DesfireTool;
using SpringCard.LibCs;
using SpringCard.PCSC;
using SpringCard.PCSC.CardHelpers;
using static SpringCard.PCSC.CardHelpers.Desfire;

namespace DesfireTool
{
    partial class Program : CLIProgram
    {
        const string Progname = "DesfireTool";

        /*
        const int MAX_ACTION_COUNTER = 10;
        static string[] Actions = new string[MAX_ACTION_COUNTER];
        static string wildcard = null;
        static int ReaderIndex = 0;
        static string ReaderName = null;
        static bool ListReaders = false;
        static bool Quiet = false;
        static bool Keep = false;
        static bool Yes = false;
        */                

        public enum Action
        {
            Unknown,
            ListReaders,
            Script,
            Create,
            Authenticate,
            AuthenticateAES,
            AuthenticateEV2First,
            AuthenticateEV2Next,
            AuthenticateISO,
            ChangeFileSettings,
            ChangeKey,
            ChangeKeyAES,
            ChangeKeyEV2,
            ClearRecordFile,
            CommitReaderID,
            CommitTransaction,
            CreateApplication,
            CreateBackupDataFile,
            CreateCyclicRecordFile,
            CreateDelegatedApplication,
            CreateLinearRecordFile,
            CreateStdDataFile,
            CreateTransactionMacFile,
            CreateValueFile,
            Credit,
            Debit,
            DeleteApplication,
            DeleteFile,
            FinalizeKeySet,
            Format,
            FreeMem,
            GetApplicationIDs,
            GetCardUID,
            GetDFNames,
            GetDelegatedInfo,
            GetFileIDs,
            GetFileSettings,
            GetISOFileIDs,
            GetKeySettings,
            GetKeyVersion,
            GetValue,
            GetVersion,
            InitializeKeySet,
            LimitedCredit,
            ReadData,
            ReadRecords,
            RollKeySet,
            SelectApplication,
            SetConfiguration,
            UpdateRecord,
            WriteData,
            WriteRecord
        }


        static int Main(string[] args)
        {
            Program p = new Program();
            return (int)p.Run(args);
        }
    }
}
