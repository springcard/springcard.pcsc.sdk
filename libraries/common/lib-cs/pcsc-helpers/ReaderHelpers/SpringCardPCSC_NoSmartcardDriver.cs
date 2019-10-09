/**h* SpringCard/PCSC_Utils
 *
 * NAME
 *   PCSC : PCSC_Utils
 * 
 * DESCRIPTION
 *   SpringCard's misc utilities for the PC/SC API
 *
 * COPYRIGHT
 *   Copyright (c) 2010-2015 SpringCard - www.springcard.com
 *
 * AUTHOR
 *   Johann.D / SpringCard
 *
 **/
using System;
using SpringCard.PCSC;
using SpringCard.LibCs;

namespace SpringCard.PCSC.ReaderHelper
{
	public static class NoSmartcardDriver
	{
		public static bool DisableDriverForThisAtr(CardBuffer Atr, out bool Added)
		{
			Added = false;
			
			IntPtr hContext = IntPtr.Zero;
			uint rc = SCARD.EstablishContext(SCARD.SCOPE_SYSTEM, IntPtr.Zero, IntPtr.Zero, ref hContext);
			if (rc != SCARD.S_SUCCESS)
			{
				Logger.Trace("SCardEstablishContext failed with error " + SCARD.ErrorToMessage(rc));
				return false;
			}
			
			byte[] atr = Atr.GetBytes();
			byte[] dummy = new byte[99];
			int cchCards = -1; /*	SCARD_AUTOALLOCATE */
			rc = SCARD.ListCards(hContext, atr, null, 0, null, ref cchCards);			
			if (rc != SCARD.S_SUCCESS)
			{
				Logger.Trace("SCardListCards failed with error " + SCARD.ErrorToMessage(rc));
				SCARD.ReleaseContext(hContext);
				return false;
			}
			
			if (cchCards <= 1)
			{
				/* Card not found. We need to add it. */
				string name = "NoMinidriver-"+ Atr.AsString();
				rc = SCARD.IntroduceCardType(hContext, name, null, null, 0, atr, null, (uint) atr.Length);				
				if (rc != SCARD.S_SUCCESS)
				{
					Logger.Trace("SCardIntroduceCardType failed with error " + SCARD.ErrorToMessage(rc));
					SCARD.ReleaseContext(hContext);
					return false;
				}
				
				rc = SCARD.SetCardTypeProviderName(hContext, name, 2 /*	SCARD_PROVIDER_CSP	*/, "$DisableSCPnP$");				
				if (rc != SCARD.S_SUCCESS)
				{
					Logger.Trace("SCardSetCardTypeProviderName failed with error " + SCARD.ErrorToMessage(rc));
					SCARD.ReleaseContext(hContext);
					return false;
				}
				
				Added = true;
			}
			
			return true;
		}
	}
}
