using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Sprincard.DesfireTool;
using SpringCard.LibCs;
using SpringCard.PCSC;
using SpringCard.PCSC.CardHelpers;

namespace DesfireTool
{
    partial class Program
    {
        enum Options : int
        {
            OPTION_CARD_MASTER_KEY = 1,
            OPTION_APPLICATION_ID,
            OPTION_KEY,
            OPTION_NEW_KEY,
            OPTION_KEY_SETTINGS,
            OPTION_DATA,
            OPTION_KEY_NUMBER,
            OPTION_DATA_FILE_NUMBER,
            OPTION_DATA_COMMUNICATION_TYPE,
            OPTION_DATA_ACCESS_RIGHTS,
            OPTION_DATA_SIZE,
            OPTION_DATA_OFFSET,
            OPTION_KEY_VERSION,
            OPTION_KEY_MAX_RECORDS,
            OPTION_LOWER_LIMIT,
            OPTION_UPPER_LIMIT,
            OPTION_INITIAL_VALUE,
            OPTION_LIMITED_CREDIT_ENABLED,
            OPTION_INPUT_FILE
        }

        enum Errors : long
        {
            ERROR_NO_ERROR = 0, 
            ERROR_PARSE_ERROR,
            ERROR_READER_ERROR,
            ERROR_CARD_ERROR
        }

        enum Result : int
        {
            Success = 0,
            Failure,            
            UnknownError,
            Cancelled,
            EmptyScript,
            ActionNotImplemented,
            OptionNotImplemented,
            MissingArgument,
            InvalidArgument,
            InvalidDataInFile,
            CardOperationFailed,
            InternalError,
            ReadFromFileFailed,
            WriteToFileFailed,
            InvalidReader
        }


        static bool AskForUserConfirmation(string s)
        {
            String answer;

            do
            {
                Console.WriteLine(s);
                answer = Console.ReadLine();

                if (answer.Equals("Y") || answer.Equals("y"))
                {
                    return true;
                }
                else
                if (answer.Equals("N") || answer.Equals("n"))
                {
                    return false;
                }
                else
                {
                    Console.WriteLine("We only understand 'Y' or 'N' answers...");
                }
            } while (true);

        }

        static Result LoadContent(string fileName, out DesfireContent content)
        {
            Logger.Trace("Reading input file {0}", fileName);

            string strInput;

            try
            {
                strInput = File.ReadAllText(fileName);
            }
            catch
            {
                Console.WriteLine("Failed to read {0}", fileName);
                content = null;
                return Result.ReadFromFileFailed;
            }

            JSONObject jsonInput;

            try
            {
                jsonInput = JSONDecoder.Decode(strInput);
            }
            catch
            {
                Console.WriteLine("Content of file {0} is malformed", fileName);
                content = null;
                return Result.InvalidDataInFile;
            }

            if (!DesfireContent.TryCreateFromJSON(jsonInput, out content))
            {
                Console.WriteLine("Content in file {0} is invalid", fileName);
                return Result.InvalidDataInFile;
            }

            return Result.Success;
        }

        // ? - any character(one and only one)
        // * - any characters(zero or more)
        // Boolean result = Regex.IsMatch(test, WildCardToRegular( filter ));
        private static String WildCardToRegular(String value)
        {
            return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
        }

        private static Boolean WildCardMatch( String Wildcard, String lookin)
        {            
            return Regex.IsMatch(lookin.ToLower(), WildCardToRegular(Wildcard.ToLower()));
        }

    }
}
