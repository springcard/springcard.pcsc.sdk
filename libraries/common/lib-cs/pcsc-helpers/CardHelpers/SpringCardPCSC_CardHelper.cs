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

namespace SpringCard.PCSC.CardHelpers
{
	public abstract class CardHelper
	{	
		protected ICardApduTransmitter Channel;

		public CardHelper(ICardApduTransmitter Channel)
		{
			this.Channel = Channel;
		}
		
		public abstract bool SelfTest();
	}	
}
