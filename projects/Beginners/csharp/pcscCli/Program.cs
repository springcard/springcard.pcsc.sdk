using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            bool testExtendedApdu = false;
            bool testDelay = false;
            int lot_nb = -1;

            Console.WriteLine("pcscCli -- demo for PC/SC with no dependency to Windows");
            Console.WriteLine("-------------------------------------------------------");


            SCARD.UseLogger = true;
            Logger.ReadArgs(args);

            if (args.Contains("all"))
            {
                testExtendedApdu = true;
                testDelay = true;
            }
            else
            {
                if (args.Contains("eapdus"))
                {
                    testExtendedApdu = true;

                    if (int.TryParse(args[args.Length - 1], out lot_nb))
                    {
                        Console.WriteLine($"Only doing lot nb {lot_nb}");
                    }
                }

                if (args.Contains("delay"))
                {
                    testDelay = true; 
                }
            }


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
                        SCardChannel channel = new SCardChannel(reader);
                       if (!channel.Connect())
                        {
                            Console.WriteLine("Error, can't connect to the card");
                            return;
                        }
                        Console.WriteLine("Connect OK");
                    }
                }

                if (testExtendedApdu)
                {
                    if (TestExtendedApdus(reader))
                    {
                        Console.WriteLine("TEST OK");
                    }
                    else
                    {
                        Console.WriteLine("ERROR");
                    }
                }

                if (testDelay)
                {
                    if (TestDelay(reader))
                    {
                        Console.WriteLine("TEST OK");
                    }
                    else
                    {
                        Console.WriteLine("ERROR");
                    }
                }
            }

            bool TestDelay(SCardReader reader)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                /* j = Le */
                for (byte delay = 0; delay < 64; delay++)
                {
                    Console.WriteLine($"Delay = {delay}s");

                    byte[] cmd = new byte[] { 0xFF, 0xFD, 0x00, (byte)(0x80 | (delay & (byte)0x3F)), 0x00 };


                    Console.WriteLine($"< {BinConvert.ToHex(cmd)}");

                    CAPDU capdu = new CAPDU(cmd);    // Command sent to the reader
                    RAPDU rapdu = reader.Control(capdu);

                    Console.WriteLine($"> {BinConvert.ToHex(rapdu.Bytes)}");

                    if (rapdu.SW != 0x9000)
                    {
                        Console.WriteLine("Get UID APDU failed!");
                        reader.Dispose();
                        return false;
                    }
                }

                sw.Stop();
                Console.WriteLine("Elapsed={0}", sw.Elapsed);
                return true;
            }

            bool TestExtendedApdus(SCardReader reader)
            {
                List<Tuple<IEnumerable<int>, IEnumerable<int>>> lots = new List<Tuple<IEnumerable<int>, IEnumerable<int>>>();


                /* Lot 1
                 * Lc = 0 --> Lc = 320
                 * Le = 0 --> Le = 320 
                */
                lots.Add(new Tuple<IEnumerable<int>, IEnumerable<int>>(Enumerable.Range(0, 320 + 1), Enumerable.Range(0, 320 + 1)));

                /* Lot 2
                 * Lc = 565 --> Lc = 577
                 * Le = 565 --> Le = 577
                 */
                lots.Add(new Tuple<IEnumerable<int>, IEnumerable<int>>(Enumerable.Range(565, 577 - 565 + 1), Enumerable.Range(565, 577 - 565 + 1)));

                /* Lot 3
                 * Lc = 960 --> Lc = 1088
                 * Le = 960 --> Le = 1088
                 */
                lots.Add(new Tuple<IEnumerable<int>, IEnumerable<int>>(Enumerable.Range(960, 1088 - 960 + 1), Enumerable.Range(960, 1088 - 960 + 1)));

                /* Lot 4
                 * Lc = 2048 --> Lc = 64512 par pas de 512
                 * Le = 2048 --> Le = 64512 par pas de 512
                 */
                lots.Add(new Tuple<IEnumerable<int>, IEnumerable<int>>(Enumerable.Range(0, (64512-2048)/512 + 1).Select(x => (x*512) +2048), Enumerable.Range(0, (64512 - 2048) / 512 + 1).Select(x => (x * 512) + 2048)));

                /* Lot 5
                 *  Lc = 65530 --> Lc = 65535
                 *  Le = 0
                 */
                lots.Add(new Tuple<IEnumerable<int>, IEnumerable<int>>(Enumerable.Range(65530, 65535 - 65530 + 1), Enumerable.Range(0, 1)));

                /* Lot 6
                 * Lc = 0
                 * Le = 65530 -- > Le = 65535
                 */
                lots.Add(new Tuple<IEnumerable<int>, IEnumerable<int>>(Enumerable.Range(0, 1), Enumerable.Range(65530, 65535 - 65530 + 1)));

                /* Lot 7
                 * Lc = 65530 --> Lc = 65535
                 * Le = 65530 --> Le = 65535
                 */
                lots.Add(new Tuple<IEnumerable<int>, IEnumerable<int>>(Enumerable.Range(65530, 65535 - 65530 + 1), Enumerable.Range(65530, 65535 - 65530 + 1)));

                Stopwatch sw = new Stopwatch();
                sw.Start();
                int nb = 0;
                foreach (var lot in lots)
                {
          
                    Console.WriteLine($"Lot {++nb}");

                    if (lot_nb >= 1 && lot_nb != nb)
                    {
                        Console.WriteLine($"Skipped");
                        continue;
                    }

                    Stopwatch sw_lot = new Stopwatch();
                    sw_lot.Start();

                    var lc_array = lot.Item1.ToList();
                    var le_array = lot.Item2.ToList();
                    /* i = Lc */
                    for (ushort i = 0; i < lc_array.Count; i++)
                    {
                        /* j = Le */
                        for (ushort j = 0; j < le_array.Count; j++)
                        {
                            Console.WriteLine($"Lc = {lc_array[i]}, Le = {le_array[j]}");

                            byte[] cmd = new byte[] { 0xFF, 0xFD, 0x00, 0x80, 0x00, (byte)(lc_array[i] / 256), (byte)(lc_array[i] % 256) };
                            byte[] random = PRNG.Generate(lc_array[i]);
                            cmd = cmd.Concat(random).ToArray();
                            if (le_array[j] != 0)
                            {
                                cmd = cmd.Concat(new byte[] { (byte)(le_array[j] / 256), (byte)(le_array[j] % 256) }).ToArray();
                            }

                            //Console.WriteLine($"< {BinConvert.ToHex(cmd)}");

                            CAPDU capdu = new CAPDU(cmd);    // Command sent to the reader

                            RAPDU rapdu = reader.Control(capdu);

                            //Console.WriteLine($"> {BinConvert.ToHex(rapdu.Bytes)}");

                            if(rapdu == null)
                            {
                                Console.WriteLine("RAPDU is null");
                                return false;
                            }

                            if (rapdu.SW != 0x9000)
                            {
                                Console.WriteLine("Bad SW");
                                reader.Dispose();
                                return false;
                            }

                            byte[] rapduB = rapdu.data.GetBytes();

                            if (rapduB.Length != le_array[j])
                            {
                                Console.WriteLine("Length received is invalid");
                                reader.Dispose();
                                return false;
                            }


                            int lengthToCompare = Math.Min(rapduB.Length, random.Length);


                            if (!rapduB.Take(lengthToCompare).SequenceEqual(random.Take(lengthToCompare)))
                            {
                                Console.WriteLine("Response is not the same as command");
                                reader.Dispose();
                                return false;
                            }

                            if (rapduB.Length > random.Length)
                            {
                                for (int k = random.Length; k < rapduB.Length; k++)
                                {
                                    if (rapduB[k] != k % 256)
                                    {
                                        Console.WriteLine("Response is not incremental after random.length");
                                        reader.Dispose();
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                    sw_lot.Stop();
                    Console.WriteLine($"Lot took {sw_lot.Elapsed}");

                }

                sw.Stop();
                Console.WriteLine("Total time {0}", sw.Elapsed);
                return true;
            }

        }
    }
}
