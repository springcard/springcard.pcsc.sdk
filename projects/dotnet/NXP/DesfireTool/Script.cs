using SpringCard.PCSC.CardHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesfireTool
{
    partial class Program
    {
        Result RunScript(Desfire desfire, string fileName)
        {
            string[] lines = File.ReadAllLines(fileName);
            return RunScript(desfire, lines);
        }

        Result RunScript(Desfire desfire, string[] lines)
        {
            Result rc = Result.EmptyScript;
            foreach (string line in lines)
            {
                rc = RunLine(desfire, line);
                if (rc != 0)
                    break;
            }
            return rc;
        }

        Result RunLine(Desfire desfire, string line)
        {
            string[] t = line.Split(' ');

            if (!ParseArgs(t, out Action action, out Parameters parameters))
                return Result.InvalidArgument;

            return RunEx(desfire, action, parameters);
        }
    }
}
