using SpringCard.LibCs;
using SpringCard.PCSC;
using SpringCard.PCSC.CardEmulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCE_Demo
{
    class Cardlet : CardEmulBase
    {
        uint ApduCounter;

        public Cardlet(string ReaderName) : base(ReaderName)
        {
            Console.WriteLine("Creating cardlet");
        }

        protected override void OnSelect()
        {
            Console.WriteLine("Cardlet is selected");
        }

        protected override void OnDeselect()
        {
            Console.WriteLine("Cardlet is deselected");
        }

        protected override void OnError()
        {
            Console.WriteLine("Cardlet execution error");
        }

        protected override RAPDU OnApdu(CAPDU capdu)
        {
            Console.WriteLine("Cardlet>{0}", capdu.AsString());
            RAPDU rapdu = new RAPDU(BinUtils.FromDword(ApduCounter++), 0x90, 0x00);
            Console.WriteLine("Cardlet<{0}", rapdu.AsString());
            return rapdu;
        }


    }
}
