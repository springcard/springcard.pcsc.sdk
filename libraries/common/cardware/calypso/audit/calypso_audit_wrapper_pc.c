#include "calypso_audit.h"
#include "LowLevelApi.h"
#include <time.h>

static BOOL CardPcscOK = FALSE;

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

  strcpy(outVersion, "VOJJ 1.10 (PC)");

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
  CardPcscOK = FALSE;

  /* Prepare the context with SAM's data */
  if (audit_config.SamPcsc)
    CalypsoSamBindPcsc(cal_ctx, hSamPcsc);
  else
    CalypsoSamBindLegacy(cal_ctx, audit_config.SamLegacySlot);

  CalypsoParseSamAtr(cal_ctx, SamAtr, SamAtrLen);

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

    CalypsoBench(TRUE);
    
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

    CardPcscOK = TRUE;

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

    CalypsoBench(TRUE);

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

  set_leds_t(0x0100, 0);

  return poApplicationSelect(inAid, inAidLength, outAid, outAidLength, outSerial, outInfos, NULL);
}



void set_leds_t(WORD status, WORD duration)
{
  BYTE data[7];
  BYTE apdu[32];
  BYTE ctrl[32];

  data[0] = 0x1E;
  data[1] = (BYTE) ( status        & 0x000F);
  data[2] = (BYTE) ((status >> 4)  & 0x000F);
  data[3] = (BYTE) ((status >> 8)  & 0x000F);
  data[4] = (BYTE) ((status >> 12) & 0x000F);
  data[5] = (BYTE) ((duration >> 8) & 0x00FF);
  data[6] = (BYTE) ( duration       & 0x00FF);

  apdu[0] = 0xFF;
  apdu[1] = 0xF0;
  apdu[2] = 0x00;
  apdu[3] = 0x00;
  apdu[4] = sizeof(data);
  memcpy(&apdu[5], data, sizeof(data));

  ctrl[0] = 0x58;
  memcpy(&ctrl[1], data, sizeof(data));

  if (!audit_config.CardPcsc || !audit_config.SamPcsc)
  {
    SPROX_ControlLedsT(status, duration);
  }

  if (audit_config.SamPcsc)
  {

    CalypsoSamTransmit(cal_ctx, apdu, 5+sizeof(data), NULL, NULL);
  } else
  if (audit_config.CardPcsc)
  {
    if (CardPcscOK)
      CalypsoCardTransmit(cal_ctx, apdu, 5+sizeof(data), NULL, NULL);
    else
      PcscDirectTransmit(audit_config.CardPcscIndex, ctrl, 1+sizeof(data), NULL, NULL);
  }
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
  DWORD t = CalypsoBench(FALSE);

  if (!audit_config.Quiet)
  {
    TRACE_S("--- Release (");
    TRACE_D(t, 0);
    TRACE_S("ms)\n");
  } else
  if (audit_config.Verbose)
    TRACE_S("--- Release\n");

  for (;;)
  {
    cal_rc = CalypsoCardSendRatificationFrame(cal_ctx);
    if (cal_rc)
      break;

    Sleep(200);
  }

  CardPcscOK = FALSE;
 
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
}



/*
	Generate the current time in Date compact and Time Compact formats
 */
void getCurrentTime(unsigned char *outDateTimeCompact // The 2 first bytes of outDateTimeCompact is the DateCompact
                    // the 2 last bytes of outDateTimeCompact is the Timecomapct
  )
{
  struct tm vLocalDate;
  time_t vRefTime;
  long vUtcDiff;
  long vTimecompact;
  long vTmpTime;
  time_t vCurrentTime = time(NULL);

  // compute the utc / local time difference
  vLocalDate = *localtime(&vCurrentTime);
  vUtcDiff =
    (long) difftime(mktime(&vLocalDate), mktime(gmtime(&vCurrentTime)));

  // date January 1st, 1997;
  vLocalDate.tm_sec = 0;
  vLocalDate.tm_min = 0;
  vLocalDate.tm_hour = 0;
  vLocalDate.tm_mday = 1;
  vLocalDate.tm_mon = 1 - 1;
  vLocalDate.tm_year = 1997 - 1900;

  vRefTime = mktime(&vLocalDate) + vUtcDiff;  // 1/1/1997 in UTC time

  vTimecompact = (long) difftime(vCurrentTime, vRefTime);

  // Retrieve the numbers of days since 1/1/1997, and output it msb first
  vTmpTime = vTimecompact / (24 * 3600);
  outDateTimeCompact[0] = (vTmpTime >> 8) & 0xFF;
  outDateTimeCompact[1] = vTmpTime & 0xFF;

  // Retrieve the number of minutes since 00h00
  vTmpTime = (vTimecompact % (24 * 3600)) / 60;
  outDateTimeCompact[2] = (vTmpTime >> 8) & 0xFF;
  outDateTimeCompact[3] = vTmpTime & 0xFF;

  if (audit_config.Verbose)
  {
    TRACE_S("outDateTimeCompact ");
    TRACE_H(outDateTimeCompact, 4, FALSE);
    TRACE_S(NULL);
  }
}




void poFatal(void)
{
  if (!audit_config.Quiet)
    TRACE_S("\n--- (fatal)\n\n");

  if (audit_config.CardPcsc)
    PcscDisconnect(hCardPcsc, SCARD_LEAVE_CARD);
  CardPcscOK = FALSE;
}

