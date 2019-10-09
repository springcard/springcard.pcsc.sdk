/**
 *
 * \defgroup MifareUltraLightCWrapper
 *
 * \brief .NET wrapper for the pcsc_mifulc.dll native library
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

namespace SpringCard.PCSC.CardLibraries.MifareUltraLightC
{
  public abstract partial class SCARD
  {
    [DllImport("pcsc_mifulc.dll", EntryPoint = "SCardMifUlC_AttachLibrary")]
      public static extern uint MifUlC_AttachLibrary(IntPtr hCard);

    [DllImport("pcsc_mifulc.dll", EntryPoint = "SCardMifUlC_DetachLibrary")]
      public static extern uint MifUlC_DetachLibrary(IntPtr hCard);

    [DllImport("pcsc_mifulc.dll", EntryPoint = "SCardMifUlC_Authenticate")]
      public static extern uint MifUlC_Authenticate(IntPtr hCard,
                                                    byte[]pbKey);

    [DllImport("pcsc_mifulc.dll", EntryPoint = "SCardMifUlC_ChangeKey")]
      public static extern uint MifUlC_ChangeKey(IntPtr hCard, byte[]pbKey);

    [DllImport("pcsc_mifulc.dll", EntryPoint = "SCardMifUlC_Read")]
      public static extern uint MifUlC_Read(IntPtr hCard, byte bAddress,
                                            byte[]pbData);

    [DllImport("pcsc_mifulc.dll", EntryPoint = "SCardMifUlC_Write")]
      public static extern uint MifUlC_Write(IntPtr hCard, byte bAddress,
                                             byte[]pbData);

    [DllImport("pcsc_mifulc.dll", EntryPoint = "SCardMifUlC_Write4")]
      public static extern uint MifUlC_Write4(IntPtr hCard, byte bAddress,
                                              byte[]pbData);

    public const string MifUlC_BlankKey_Str =
      "00000000000000000000000000000000";
    public const string MifUlC_DefaultKey_Str =
      "49454D4B41455242214E4143554F5946";
  }

  public class SCardMifareUltraLightC
  {
    private IntPtr hCard;
    private uint _last_error;


    public SCardMifareUltraLightC(SCardChannel card_channel)
    {
      hCard = card_channel.hCard;
      _last_error = SCARD.MifUlC_AttachLibrary(hCard);
    }

     ~SCardMifareUltraLightC()
    {
      SCARD.MifUlC_DetachLibrary(hCard);
    }

    public bool Authenticate(CardBuffer key)
    {
      _last_error = SCARD.MifUlC_Authenticate(hCard, key.GetBytes());
      if (_last_error != 0)
        return false;
      return true;
    }

    public bool ChangeKey(CardBuffer key)
    {
      _last_error = SCARD.MifUlC_ChangeKey(hCard, key.GetBytes());
      if (_last_error != 0)
        return false;
      return true;
    }

    public CardBuffer Read(byte address)
    {
      byte[]buffer = new byte[16];
      _last_error = SCARD.MifUlC_Read(hCard, address, buffer);
      if (_last_error != 0)
        return null;
      return new CardBuffer(buffer);
    }

    public bool Write(byte address, CardBuffer buffer)
    {
      _last_error = SCARD.MifUlC_Write4(hCard, address, buffer.GetBytes());
      if (_last_error != 0)
        return false;
      return true;
    }

  }
}
