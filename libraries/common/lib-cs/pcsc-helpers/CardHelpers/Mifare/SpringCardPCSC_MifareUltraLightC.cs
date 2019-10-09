/**
 *
 * \defgroup MifareUltraLightC
 *
 * \brief Mifare UltraLight C library (.NET only, no native depedency)
 *
 * \author
 *   Johann.D et al. / SpringCard
 */
/*
 * Read LICENSE.txt for license details and restrictions.
 */
using System;
using System.Runtime.InteropServices;
using SpringCard.PCSC;

namespace SpringCard.PCSC.CardHelpers.Mifare
{
	public class MifareUltraLightC : MifareUltraLight
	{
		/**
		 * \brief Instanciate a Mifare UltraLight C card object over a card channel. The channel must already be connected.
		 */
		public MifareUltraLightC(ICardApduTransmitter Channel) : base(Channel)
		{
		}
		

		
	}
}
