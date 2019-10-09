using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SpringCard.LibCs;
using SpringCard.PCSC;

namespace pcscCli
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("pcscCli -- demo for PC/SC with no dependency to Windows");
            Console.WriteLine("-------------------------------------------------------");

            Logger.ConsoleLevel = Logger.Level.All;
            SCARD.UseLogger = true;

            IntPtr dummy = IntPtr.Zero;
            Logger.Debug("Sizeof(IntPtr)={0}", Marshal.SizeOf(dummy));

            string[] readerNames = SCARD.GetReaderList();

            if ((readerNames == null) || (readerNames.Length == 0))
            {
                Console.WriteLine("No PC/SC reader");
                return;
            }

            Console.WriteLine("{0} PC/SC reader(s):", readerNames.Length);
            foreach (string readerName in readerNames)
            {
                Console.WriteLine("\t{0}", readerName);
            }
            Console.WriteLine();

            foreach (string readerName in readerNames)
            {
                SCardReader reader = new SCardReader(readerName);

                if (reader.CardPresent)
                {
                    Console.WriteLine("There is a card in reader {0}", readerName);
                    Console.WriteLine("Card ATR={0}", BinConvert.ToHex(reader.CardAtr.Bytes));
                    if (reader.CardAvailable)
                    {
                        Console.WriteLine("The card is available, let's try to connect it");
                    }
                }
            }
        }
    }
}
