using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Reflection;
using SpringCard.PCSC;
using SpringCard.LibCs;

namespace SpringCard.PCSC.CardHelpers
{
    public partial class SamAV
    {
        private bool isByte(char c)
        {
            bool r = false;

            if ((c >= '0') && (c <= '9'))
            {
                r = true;
            }
            else
            if ((c >= 'A') && (c <= 'F'))
            {
                r = true;
            }
            else
            if ((c >= 'a') && (c <= 'f'))
            {
                r = true;
            }

            return r;
        }

        void Debug(string s)
        {
            Logger.Debug(s);
        }
    }
}
