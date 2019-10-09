#include "calypso_audit.h"
#include "LowLevelApi.h"

CALYPSO_AUDIT_CFG_ST audit_config;
P_CALYPSO_CTX cal_ctx = NULL;
CALYPSO_RC    cal_rc;

const char version[] = "VOJJ 1.12 (CW)";


/* ----------------------------------------------------------------------------
	poGetVersion
		Get the version of the coupler (firmware and/or hardware).
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
int   poGetVersion
(
	unsigned char	outVersion[16]	// Version of the coupler, encoded in Ascii format, the
									// ending bytes are set to 00h if unused
)
{
  if (audit_config.Verbose)
    TRACE_S("--- GetVersion\n");

  if (outVersion == NULL)
    return kErrWrongParameter;

  memcpy(outVersion, version, sizeof(version));

  return kSUCCESS;
}

/* ----------------------------------------------------------------------------
	poDetect
		Searches a portable object and connects to it, specifying the AID.
           * Searches at least for ISO14443 Type B or
           * Searches at least for ISO14443 Type A and B
             if optional Type A audit is requested
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
int   poDetect
(
	unsigned char	  inAid[16],		// beginning of AID to select
	unsigned char	  inAidLength,		// length of AID
	unsigned char	  outAid[16],		// AID of the application selected
	unsigned char	* outAidLength,		// 0 if no app. selected, else length of outAid
	unsigned char	  outSerial[8],		// Calypso Serial number of the selected application
	unsigned char	  outInfos[7]		// Startup Infos as returned by Select App.
)
{
#if 0
  /* Prepare the context with SAM's data */
  if (audit_config.SamPcsc)
    CalypsoSamBindPcsc(cal_ctx, hSamPcsc);
  else
    CalypsoSamBindLegacy(cal_ctx, audit_config.SamLegacySlot);

  CalypsoParseSamAtr(cal_ctx, SamAtr, SamAtrLen);



  set_leds_t(0x01EE, 0xFFFF);

  if (audit_config.CardPcsc)
  {
    BYTE atr[32]; DWORD atrlen = sizeof(atr);

    if (!PcscWaitCard(audit_config.CardPcscIndex, atr, &atrlen))
    {
      if (!audit_config.Quiet)
        TRACE_S("--- Detect : wait failed\n");

      TRACE_S("PcscWaitCard error\n");
      exit(EXIT_FAILURE);
    }
    
    if (audit_config.Verbose)
    {
      TRACE_S("--- Detect\nATR=");
      TRACE_H(atr, atrlen, FALSE);
      TRACE_S(NULL);
    }

    if (!PcscConnect(audit_config.CardPcscIndex, &hCardPcsc))
    {
      if (!audit_config.Quiet)
        TRACE_S("--- Detect : card found, but connection failed\n");

      return kErrNoCard;
    }

    cal_rc = CalypsoCardBindPcsc(cal_ctx, hCardPcsc);
    if (cal_rc)
    {
      if (!audit_config.Quiet)
        TRACE_RC("--- Detect:BindPcsc");
      return kErrNoCard;
    }

  } else
  {
    WORD proto;
    BYTE uid[12];    BYTE uidlen;
    BYTE repgen[64]; BYTE repgenlen;

    for (;;)
    {
      uidlen    = sizeof(uid);
      repgenlen = sizeof(repgen);

      rdr_rc = SPROX_FindEx(audit_config.CardLegacyProtos, &proto, uid, &uidlen, repgen, &repgenlen);
      if (rdr_rc == MI_OK)
        break;

      SPROX_ControlRF(20);
    }

    if (audit_config.Verbose)
    {
      TRACE_S("--- Detect\n");
      TRACE_S("Proto=");
      TRACE_HW(proto);
      TRACE_S(" ID=");
      TRACE_H(uid, uidlen, FALSE);
      TRACE_S(NULL);
    }

    cal_rc = CalypsoCardBindLegacyEx(cal_ctx, proto, repgen, repgenlen);
    if (cal_rc)
    {
      if (!audit_config.Quiet)
        TRACE_RC("--- Detect:BindLegacy");
      return kErrNoCard;
    }
  }

  set_leds_t(0x1000, 100);

#endif
  return poApplicationSelect(inAid, inAidLength, outAid, outAidLength, outSerial, outInfos, NULL);
}


/* ----------------------------------------------------------------------------
	poRelease
		Send ratification commands (GetChallenge with Le to 1 : 0084000001h)
        waiting 200 ms between each command  until no response is received.
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
int   poRelease
(
  void
)
{
#if 0
  if (audit_config.Verbose)
    TRACE_S("--- Release\n");

  for (;;)
  {
    cal_rc = CalypsoCardSendRatificationFrame(cal_ctx);
    if (cal_rc)
      break;


    Sleep(200);
  }
 
  if (!audit_config.Quiet)
    TRACE_S(NULL);

  if (audit_config.CardPcsc)
  {
    PcscDisconnect(hCardPcsc, SCARD_LEAVE_CARD);
    while (PcscCardPresent(audit_config.CardPcscIndex))
    {
      Sleep(250);
    }
  } else
  {
    SPROX_ControlRF(10);
  }

  CalypsoCleanupContext(cal_ctx);
  return kSUCCESS;
#endif
}




void poFatal(void)
{
  if (!audit_config.Quiet)
    TRACE_S("\n--- (fatal)\n\n");
}

