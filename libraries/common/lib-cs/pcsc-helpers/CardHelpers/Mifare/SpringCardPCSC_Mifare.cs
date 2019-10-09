/**
 *
 * \author
 *   Johann.D et al. / SpringCard
 */
/*
 * Read LICENSE.txt for license details and restrictions.
 */
using System;
using System.Runtime.InteropServices;
using SpringCard.LibCs;
using SpringCard.PCSC;
using SpringCard.PCSC.CardAnalysis;

namespace SpringCard.PCSC.CardHelpers.Mifare
{
	public abstract class CardHelperMifare : CardHelper
	{
		public const ushort NN_MifareClassic1K = 0x0001;
		public const ushort NN_MifareClassic4K = 0x0002;
		public const ushort NN_MifareUltraLight = 0x0003;
		public const ushort NN_MifareUltraLightC = 0x003A;
		public const ushort NN_MifareUltraLightEV1 = 0x003D;
		
		public CardHelperMifare(ICardApduTransmitter Channel) : base(Channel)
		{

		}
		
		public static CardHelperMifare Instantiate(SCardChannel Channel)
		{
			ushort NN = CardPixList.NN(Channel.CardAtr);
			
			switch (NN)
			{
				case NN_MifareUltraLight :
					return new MifareUltraLight(Channel);
				case NN_MifareUltraLightC :
					return new MifareUltraLightC(Channel);					
				case NN_MifareUltraLightEV1 :
					return new MifareUltraLightEV1(Channel);
			}

            return null;
		}		
	}	
}
