#include "calypso_audit.h"
#include "LowLevelApi.h"




/* ----------------------------------------------------------------------------
	uiSignalState
		Signal the algorithm state to the user interface.
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
int   poSignalState
(
	unsigned char	inState		// state to signal to the user:
								// 0: NOK => Red led is activated 2 s
								// 1: OK  => Green led is activated 2 s
								// 2: OK_SHORT => Green led is activated 100 ms
								// 3: OK_BLINK => Green led blinks 4 seconds
)
{
  switch (inState)
  {
    case kSignal_NOK       : if (!audit_config.Quiet)
                               TRACE_S(">>> NOK\n");
                             set_leds_t(0x0001, 2000); 
                             break;

    case kSignal_OK        : if (!audit_config.Quiet)
                               TRACE_S(">>> OK\n");
                             set_leds_t(0x0010, 2000);
                             break;

    case kSignal_OK_SHORT  : if (!audit_config.Quiet)
                               TRACE_S(">>> OK short\n"); 
                             set_leds_t(0x0010, 100);                            
                             break;

    case kSignal_OK_BLINK  : if (!audit_config.Quiet)
                               TRACE_S(">>> OK blink\n");
                             set_leds_t(0x0040, 4000);
                             break;

    case kSignal_NOK_BLINK : if (!audit_config.Quiet)
                               TRACE_S(">>> NOK blink\n");
                             set_leds_t(0x0004, 2000);
                             break;

    default                : TRACE_S(">>> ???\n");
                             set_leds_t(0x0044, 2000);
  }

  return kSUCCESS;
}




/* ----------------------------------------------------------------------------
	poApplicationSelect
		Selects an application in the portable object by its AID.
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
int   poApplicationSelect
(
	unsigned char	inAid[16],			// beginning of AID to select
	unsigned char	inAidLength,	// length of AID
	unsigned char	outAid[16],		// AID of the application selected
	unsigned char * outAidLength,	// 0 if no app. selected, else length of outAid
	unsigned char 	outSerial[8],	// Calypso Serial number of the selected application
	unsigned char 	outInfos[7],	// Startup Infos as returned by Select App.
	unsigned char	outSW[ 2 ]		// Status Word from the PO
)
{
  SIZE_T l;

  if (audit_config.Verbose)
    TRACE_S("--- ApplicationSelect\n");

  *outAidLength = 0;

  /* Check this card is actually CALYPSO (ATR parsing or SELECT APPLICATION + FCI parsing) */
  cal_rc = CalypsoCardActivate(cal_ctx, inAid, inAidLength);
  if (cal_rc)
  {
    if (!audit_config.Quiet)
    {
      TRACE_S("CardActivate(");
      TRACE_H(inAid, inAidLength, FALSE);
      TRACE_RC(")");
    }
    // TODO ???

  } else
  {
    /* SELECT APPLICATION succeeded, let's retrieve actual AID */
    l = 16;
    cal_rc = CalypsoCardDFName(cal_ctx, outAid, &l);
    if (cal_rc)
    {
      if (!audit_config.Quiet)
        TRACE_RC("--- ApplicationSelect:GetDFName");
      /* Return OK anyway ? */
    } else
    {
      *outAidLength = (unsigned char) l;
    }

    if (audit_config.Verbose)
    {
      if (l)
        TRACE_S("    ApplicationSelect (OK)\n");
      else
        TRACE_S("    ApplicationSelect (not found)\n");
    }

    /* Retrieve serial number */
    cal_rc = CalypsoCardSerialNumber(cal_ctx, outSerial);
    if (cal_rc)
    {
      if (!audit_config.Quiet)
        TRACE_RC("--- ApplicationSelect:GetSerialNumber");
      /* Return OK anyway ? */
    }

    /* Retrieve Startup Infos */
    cal_rc = CalypsoCardStartupInfo(cal_ctx, outInfos);
    if (cal_rc)
    {
      if (!audit_config.Quiet)
        TRACE_RC("--- ApplicationSelect:GetStartupInfo");
      /* Return OK anyway ? */
    }
  }

  /* Retrieve status word */
  CalypsoCardGetSW(cal_ctx, outSW);
  return kSUCCESS;
}




/* ----------------------------------------------------------------------------
	poSessionOpen
		Opens a secure session with optional reading of a record.
		If the open session does not return the KIF and KVC of the key,
		the default values to use are:
		21h for key#1, 27h for key#2 and 30h for key#3
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
int   poSessionOpen
(
	unsigned char	inKey,			// key index (1, 2 or 3)
	unsigned char	inRecNum,		// record to read (or 0 not to read)
	unsigned char	inSFI,			// SFI of file to select (or 0 not to select)
	unsigned char *	outRatified,	// 0 if prev. session not ratified, 1 if ratified
	unsigned char 	outRecord[256],	// in: buffer for the data to read (or NULL if inRecNum is 0)
	unsigned char *	outRecordLength,// length of the record data read (or NULL if inRecNum is 0)
	unsigned char	outSW[ 2 ]		// Status Word from the PO
)
{
  SIZE_T l_outRecordLength;
  BOOL l_outRatified;
  BYTE l_Kif;

  if (audit_config.Verbose)
    TRACE_S("--- SessionOpen\n");

  switch (inKey)
  {
    case 1  : l_Kif = 0x21; break;
    case 2  : l_Kif = 0x27; break;
    case 3  : l_Kif = 0x30; break;
    default : return kErrNotImplemented;
  }

  l_outRecordLength = 256;
  cal_rc = CalypsoStartTransactionEx(cal_ctx,
                                     &l_outRatified,
                                     inKey,
                                     l_Kif,
                                     inSFI,
                                     inRecNum,
                                     outRecord,
                                     &l_outRecordLength);

  if (cal_rc && (cal_rc != CALYPSO_ERR_STATUS_WORD))
  {
    if (!audit_config.Quiet)
      TRACE_RC("--- SessionOpen");

    if (cal_rc & CALYPSO_ERR_FATAL_)
    {
      poFatal();
      return kErrNoCard;
    }
  }
  
  if (outRatified != NULL)
    *outRatified = l_outRatified ? 1 : 0;

  if (outRecordLength != NULL)
    *outRecordLength = (unsigned char) l_outRecordLength;

  /* Retrieve status word */
  CalypsoCardGetSW(cal_ctx, outSW);
  return kSUCCESS;
}

/* ----------------------------------------------------------------------------
	poSessionClose
		Closes the secure session currently opened. If a ratification outside the
		session is requested, this command will be sent only if the session is
		successfully closed. The function return code is the Close Session one.
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
int   poSessionClose
(
	unsigned char	inMode,		// 0: Close without ratification, 
								// 1: Close with immediate ratification (ratification 
								//    in Close Secured Session)
								// 2: Close with ratification after Close Session,
								//    using an explicit ratification command 
								//    (GetChallenge with Le incorrect)
								// 3: Close cancelling the modifications
	unsigned char	outSW[ 2 ]	// Status Word from the PO
)
{

  switch (inMode)
  {
    case kCLOSE_WITHOUT_RATIFICATION   : if (audit_config.Verbose)
                                           TRACE_S("--- SessionClose without ratification\n");

                                         cal_rc = CalypsoCommitTransaction(cal_ctx, FALSE);

                                         if (cal_rc && (cal_rc != CALYPSO_ERR_STATUS_WORD))
                                         {
                                           if (!audit_config.Quiet)
                                             TRACE_RC("--- CommitTransaction(FALSE)");
                                            
                                           if (cal_rc & CALYPSO_ERR_FATAL_)
                                           {
                                             poFatal();
                                             return kErrNoCard;
                                           }
                                         }

                                         /* Retrieve status word */
                                         CalypsoCardGetSW(cal_ctx, outSW);
                                         break;

    case kCLOSE_IMMEDIATE_RATIFICATION : if (audit_config.Verbose)
                                           TRACE_S("--- SessionClose, immediate ratification\n");

                                         cal_rc = CalypsoCommitTransaction(cal_ctx, TRUE);

                                         if (cal_rc && (cal_rc != CALYPSO_ERR_STATUS_WORD))
                                         {
                                           if (!audit_config.Quiet)
                                             TRACE_RC("--- CommitTransaction(TRUE)");

                                           if (cal_rc & CALYPSO_ERR_FATAL_)
                                           {
                                             poFatal();
                                             return kErrNoCard;
                                           }
                                         }

                                         /* Retrieve status word */
                                         CalypsoCardGetSW(cal_ctx, outSW);
                                         break;

    case kCLOSE_EXPLICIT_RATIFICATION  : if (audit_config.Verbose)
                                           TRACE_S("--- SessionClose, delayed ratification\n");

                                         cal_rc = CalypsoCommitTransaction(cal_ctx, FALSE);

                                         if (cal_rc && (cal_rc != CALYPSO_ERR_STATUS_WORD))
                                         {
                                           if (!audit_config.Quiet)
                                             TRACE_RC("--- CommitTransaction(FALSE)");

                                           if (cal_rc & CALYPSO_ERR_FATAL_)
                                           {
                                             poFatal();
                                             return kErrNoCard;
                                           }
                                         }

                                         /* Retrieve status word */
                                         CalypsoCardGetSW(cal_ctx, outSW);

                                         cal_rc = CalypsoCardSendRatificationFrame(cal_ctx);

                                         if (cal_rc)
                                         {
                                           if (!audit_config.Quiet)
                                             TRACE_RC("--- SendRatificationFrame");

                                           if (cal_rc & CALYPSO_ERR_FATAL_)
                                           {
                                             poFatal();
                                             return kErrNoCard;
                                           }
                                         }
                                         break;

    case kCLOSE_CANCEL                 : if (audit_config.Verbose)
                                           TRACE_S("--- SessionClose, rollback\n");

                                         cal_rc = CalypsoCancelTransaction(cal_ctx);

                                         if (cal_rc && (cal_rc != CALYPSO_ERR_STATUS_WORD))
                                         {
                                           if (!audit_config.Quiet)
                                             TRACE_RC("--- CancelTransaction");

                                           if (cal_rc & CALYPSO_ERR_FATAL_)
                                           {
                                             poFatal();
                                             return kErrNoCard;
                                           }
                                         }

                                         /* Retrieve status word */
                                         CalypsoCardGetSW(cal_ctx, outSW);
                                         break;

    default                            : return kErrNotImplemented;
  }

  return kSUCCESS;
}

//////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////

/* ----------------------------------------------------------------------------
	poBinaryUpdate
		Updates data to a binary file of the currently selected application.
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
int   poBinaryUpdate
(
	unsigned char	inOffset,		// Offset of data in the file
	unsigned char	inSFI,			// SFI of file to select (or 0 not to select)
	unsigned char	inData[128],	// data to update
	unsigned char	inDataLength,	// length of inData
	unsigned char	outSW[ 2 ]		// Status Word from the PO
)
{
  if (audit_config.Verbose)
    TRACE_S("--- UpdateBinary\n");

  cal_rc = CalypsoCardUpdateBinary(cal_ctx, inSFI, 0, inData, inDataLength);

  if (cal_rc && (cal_rc != CALYPSO_ERR_STATUS_WORD))
  {
    if (!audit_config.Quiet)
    {
      TRACE_S("--- UpdateBinary(");
      TRACE_HB(inSFI);
      TRACE_B(',');
      TRACE_D(inDataLength,0);
      TRACE_RC(")");
    }

    if (cal_rc & CALYPSO_ERR_FATAL_)
    {
      poFatal();
      return kErrNoCard;
    }
  }

  /* Retrieve status word */
  CalypsoCardGetSW(cal_ctx, outSW);
  return kSUCCESS;
}



/* ----------------------------------------------------------------------------
	poRecordUpdate
		Updates a record of a file of the currently selected application.
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
int   poRecordUpdate
(
	unsigned char	inRecNum,		// record number
	unsigned char	inSFI,			// SFI of file to select (or 0 not to select)
	unsigned char	inRecord[128],	// data of the record to update
	unsigned char	inRecordLength,	// length of inRecord
	unsigned char	outSW[ 2 ]		// Status Word from the PO
)
{
  if (audit_config.Verbose)
    TRACE_S("--- UpdateRecord\n");

  cal_rc = CalypsoCardUpdateRecord(cal_ctx, inSFI, inRecNum, inRecord, inRecordLength);

  if (cal_rc && (cal_rc != CALYPSO_ERR_STATUS_WORD))
  {
    if (!audit_config.Quiet)
    {
      TRACE_S("--- UpdateRecord(");
      TRACE_HB(inSFI);
      TRACE_B(',');
      TRACE_D(inRecNum,0);
      TRACE_B(',');
      TRACE_D(inRecordLength,0);
      TRACE_RC(")");
    }

    if (cal_rc & CALYPSO_ERR_FATAL_)
    {
      poFatal();
      return kErrNoCard;
    }
  }

  /* Retrieve status word */
  CalypsoCardGetSW(cal_ctx, outSW);
  return kSUCCESS;
}

/* ----------------------------------------------------------------------------
	poRecordAppend
		Appends a record to a cyclic file of the currently selected
		application.
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
int   poRecordAppend
(
	unsigned char	inSFI,			// SFI of file to select (or 0 not to select)
	unsigned char 	inRecord[128],	// data of the record to append
	unsigned char	inRecordLength,	// length of inRecord
	unsigned char	outSW[ 2 ]		// Status Word from the PO
)
{
  if (audit_config.Verbose)
    TRACE_S("--- AppendRecord\n");

  cal_rc = CalypsoCardAppendRecord(cal_ctx, inSFI, inRecord, inRecordLength);

  if (cal_rc && (cal_rc != CALYPSO_ERR_STATUS_WORD))
  {
    if (!audit_config.Quiet)
    {
      TRACE_S("--- AppendRecord(");
      TRACE_HB(inSFI);
      TRACE_B(',');
      TRACE_D(inRecordLength,0);
      TRACE_RC(")");
    }

    if (cal_rc & CALYPSO_ERR_FATAL_)
    {
      poFatal();
      return kErrNoCard;
    }
  }

  /* Retrieve status word */
  CalypsoCardGetSW(cal_ctx, outSW);
  return kSUCCESS;
}

/* ----------------------------------------------------------------------------
	poRecordWrite
		Writes into a record of a file of the currently selected application.
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
int   poRecordWrite
(
	unsigned char	inRecNum,		// record number
	unsigned char	inSFI,			// SFI of file to select (or 0 not to select)
	unsigned char	inRecord[128],	// data of the record to write
	unsigned char	inRecordLength,	// length of inRecord
	unsigned char	outSW[ 2 ]		// Status Word from the PO
)
{
  if (audit_config.Verbose)
    TRACE_S("--- WriteRecord\n");

  cal_rc = CalypsoCardWriteRecord(cal_ctx, inSFI, inRecNum, inRecord, inRecordLength);

  if (cal_rc && (cal_rc != CALYPSO_ERR_STATUS_WORD))
  {
    if (!audit_config.Quiet)
    {
      TRACE_S("--- WriteRecord(");
      TRACE_HB(inSFI);
      TRACE_B(',');
      TRACE_D(inRecNum,0);
      TRACE_B(',');
      TRACE_D(inRecordLength,0);
      TRACE_RC(")");
    }

    if (cal_rc & CALYPSO_ERR_FATAL_)
    {
      poFatal();
      return kErrNoCard;
    }
  }

  /* Retrieve status word */
  CalypsoCardGetSW(cal_ctx, outSW);
  return kSUCCESS;
}

/* ----------------------------------------------------------------------------
	poBinaryRead
		Reads data from a binary file of the currently selected application.
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
int   poBinaryRead
(
	unsigned char	inOffset,		// Offset of data in the file
	unsigned char	inSFI,			// SFI of file to select (or 0 not to select)
	unsigned char *	outData,		// in: buffer for the data to read
	unsigned char *	ioLength,		// input:  length of the data to be read
									// output: length of the data read
	unsigned char	outSW[ 2 ]		// Status Word from the PO
)
{
  BYTE l_iLength = *ioLength;
  SIZE_T l_oLength = *ioLength;

  if (audit_config.Verbose)
    TRACE_S("--- ReadBinary\n");

  cal_rc = CalypsoCardReadBinary(cal_ctx, inSFI, 0, l_iLength, outData, &l_oLength);

  *ioLength = (unsigned char) l_oLength;

  if (cal_rc && (cal_rc != CALYPSO_ERR_STATUS_WORD))
  {
    if (!audit_config.Quiet)
    {
      TRACE_S("--- ReadBinary(");
      TRACE_HB(inSFI);
      TRACE_B(',');
      TRACE_D(l_iLength,0);
      TRACE_RC(")");
    }

    if (cal_rc & CALYPSO_ERR_FATAL_)
    {
      poFatal();
      return kErrNoCard;
    }
  }

  /* Retrieve status word */
  CalypsoCardGetSW(cal_ctx, outSW);
  return kSUCCESS;
}


/* ----------------------------------------------------------------------------
	poRecordRead
		Reads a record from a file of the currently selected application.
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
int   poRecordRead
(
	unsigned char	inRecNum,		// record number
	unsigned char	inSFI,			// SFI of file to select (or 0 not to select)
	unsigned char	outRecord[128],	// in: buffer for the record data to read
	unsigned char * ioRecordLength,	// input:  length of the data to be read
									// output: length of the data read
	unsigned char	outSW[ 2 ]		// Status Word from the PO
)
{
  BYTE l_iLength = *ioRecordLength;
  SIZE_T l_oLength = *ioRecordLength;

  if (audit_config.Verbose)
    TRACE_S("--- ReadRecord\n");

  cal_rc = CalypsoCardReadRecord(cal_ctx, inSFI, inRecNum, l_iLength, outRecord, &l_oLength);

  *ioRecordLength = (unsigned char) l_oLength;

  if (cal_rc && (cal_rc != CALYPSO_ERR_STATUS_WORD))
  {
    if (!audit_config.Quiet)
    {
      TRACE_S("--- ReadRecord(");
      TRACE_HB(inSFI);
      TRACE_B(',');
      TRACE_D(inRecNum,0);
      TRACE_B(',');
      TRACE_D(l_iLength,0);
      TRACE_RC(")");
    }

    if (cal_rc & CALYPSO_ERR_FATAL_)
    {
      poFatal();
      return kErrNoCard;
    }
  }

  /* Retrieve status word */
  CalypsoCardGetSW(cal_ctx, outSW);
  return kSUCCESS;
}

/* ----------------------------------------------------------------------------
	poInvalidate
		Invalidate the current DF
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
int   poInvalidate
(
	unsigned char	outSW[ 2 ]	// Status Word from the PO
)
{
  if (audit_config.Verbose)
    TRACE_S("--- Invalidate\n");

  cal_rc = CalypsoCardInvalidate(cal_ctx);

  if (cal_rc && (cal_rc != CALYPSO_ERR_STATUS_WORD))
  {
    if (!audit_config.Quiet)
      TRACE_RC("--- Invalidate");

    if (cal_rc & CALYPSO_ERR_FATAL_)
    {
      poFatal();
      return kErrNoCard;
    }
  }

  /* Retrieve status word */
  CalypsoCardGetSW(cal_ctx, outSW);
  return kSUCCESS;
}


/* ----------------------------------------------------------------------------
	poRehabilitate
		Rehabilitate the current DF
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
int   poRehabilitate
(
	unsigned char	outSW[ 2 ]	// Status Word from the PO
)
{
  if (audit_config.Verbose)
    TRACE_S("--- Rehabilitate\n");

  cal_rc = CalypsoCardRehabilitate(cal_ctx);

  if (cal_rc && (cal_rc != CALYPSO_ERR_STATUS_WORD))
  {
    if (!audit_config.Quiet)
      TRACE_RC("--- Rehabilitate");

    if (cal_rc & CALYPSO_ERR_FATAL_)
    {
      poFatal();
      return kErrNoCard;
    }
  }

  /* Retrieve status word */
  CalypsoCardGetSW(cal_ctx, outSW);
  return kSUCCESS;
}


/* ----------------------------------------------------------------------------
	poIncrease
		Increases one to seven counters of the currently selected application.
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
int   poIncrease
(
	unsigned char	inCounter,	// counter number
	unsigned char	inSFI,		// SFI of file to select (or 0 not to select)
	unsigned long	inValue,	// value to add to the counter (3 bytes)
	unsigned char	outSW[ 2 ]	// Status Word from the PO
)
{
  if (audit_config.Verbose)
    TRACE_S("--- Increase\n");

  cal_rc = CalypsoCardIncrease(cal_ctx, inSFI, inCounter, inValue, NULL);

  if (cal_rc && (cal_rc != CALYPSO_ERR_STATUS_WORD))
  {
    if (!audit_config.Quiet)
    {
      TRACE_S("--- Increase(");
      TRACE_HB(inSFI);
      TRACE_B(',');
      TRACE_D(inCounter, 0);
      TRACE_B(',');
      TRACE_D(inValue, 0);
      TRACE_RC(")");
    }

    if (cal_rc & CALYPSO_ERR_FATAL_)
    {
      poFatal();
      return kErrNoCard;
    }
  }

  /* Retrieve status word */
  CalypsoCardGetSW(cal_ctx, outSW);
  return kSUCCESS;
}


/* ----------------------------------------------------------------------------
	poDecrease
		Decreases one to seven counters of the currently selected application.
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
int   poDecrease
(
	unsigned char	inCounter,	// counter number
	unsigned char	inSFI,		// SFI of file to select (or 0 not to select)
	unsigned long	inValue,	// value to substract to the counter (3 bytes)
	unsigned char	outSW[ 2 ]	// Status Word from the PO
)
{
  if (audit_config.Verbose)
    TRACE_S("--- Decrease\n");

  cal_rc = CalypsoCardDecrease(cal_ctx, inSFI, inCounter, inValue, NULL);

  if (cal_rc && (cal_rc != CALYPSO_ERR_STATUS_WORD))
  {
    if (!audit_config.Quiet)
    {
      TRACE_S("--- Decrease(");
      TRACE_HB(inSFI);
      TRACE_B(',');
      TRACE_D(inCounter, 0);
      TRACE_B(',');
      TRACE_D(inValue, 0);
      TRACE_RC(")");
    }

    if (cal_rc & CALYPSO_ERR_FATAL_)
    {
      poFatal();
      return kErrNoCard;
    }
  }

  /* Retrieve status word */
  CalypsoCardGetSW(cal_ctx, outSW);
  return kSUCCESS;
}


/* ----------------------------------------------------------------------------
	poSVGet
		Get the Stored Value balance of the portable object
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
int   poSVGet
(
	unsigned char	inTransType,	// Transaction type. 0: reload, 1: Debit, 
									//                   2: DebitUndo
	long	*		outValue,		// Value of the Stored Value
	unsigned char	outSW[ 2 ]		// Status Word from the PO
)
{
  if (audit_config.Verbose)
    TRACE_S("--- StoredValueGet\n");

  switch (inTransType)
  {
    case 0  : cal_rc = CalypsoCardStoredValueGetForReload(cal_ctx, NULL, NULL); break;

    case 1  : 
    case 2  : cal_rc = CalypsoCardStoredValueGetForDebit(cal_ctx, NULL, NULL); break;


    default : return kErrNotImplemented;

  }

  if (cal_rc)
  {
    if (!audit_config.Quiet)
      TRACE_RC("--- StoredValueGet");

    if (cal_rc & CALYPSO_ERR_FATAL_)
    {
      poFatal();
      return kErrNoCard;
    }
  }

  cal_rc = CalypsoCardStoredValueGetDecode(cal_ctx, NULL, 0, outValue, NULL);

  if (cal_rc && (cal_rc != CALYPSO_ERR_STATUS_WORD))
  {
    if (!audit_config.Quiet)
      TRACE_RC("--- StoredValueGetDecode");

    return kErrOther;
  }

  /* Retrieve status word */
  CalypsoCardGetSW(cal_ctx, outSW);

  if ((outSW[0] == 0x90) && (outSW[1] == 0x00) && (audit_config.Verbose))
  {
    TRACE_S("SV balance=");
    TRACE_D(*outValue, 0);
    TRACE_S(NULL);
  }  

  return kSUCCESS;
}


/* ----------------------------------------------------------------------------
	poSVLoad
		Load the given value into the Stored Value of the portable object
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
int   poSVLoad
(
	long			inValue,	// Value to load into the Stored Value (signed value on 3 bytes)
	unsigned char	outSW[ 2 ]	// Status Word from the PO
)
{
  BYTE t[4];
  WORD pdate, ptime;
  BYTE pfree[2] = { 0x00, 0x00 };

  getCurrentTime(t);

  // The 2 first bytes of outDateTimeCompact is the DateCompact 
  // the 2 last bytes of outDateTimeCompact is the Timecomapct
  pdate = (t[0] << 8) | t[1];
  ptime = (t[2] << 8) | t[3];

  if (audit_config.Verbose)
    TRACE_S("--- StoredValueReload\n");

  cal_rc = CalypsoCardStoredValueReload(cal_ctx, NULL, 0, inValue, pdate, ptime, pfree, NULL, NULL);

  if (cal_rc && (cal_rc != CALYPSO_ERR_STATUS_WORD))
  {
    if (!audit_config.Quiet)
      TRACE_RC("--- StoredValueReload");

    if (cal_rc & CALYPSO_ERR_FATAL_)
    {
      poFatal();
      return kErrNoCard;
    }
  }

  /* Retrieve status word */
  CalypsoCardGetSW(cal_ctx, outSW);
  return kSUCCESS;
}

/* ----------------------------------------------------------------------------
	poSVDebit
		Debit the given value from the Stored Value of the portable object 
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
int   poSVDebit
(
	short			inValue,	// Value to debit from the Stored Value
	unsigned char	outSW[ 2 ]	// Status Word from the PO
)
{
  BYTE t[4];
  WORD pdate, ptime;

  getCurrentTime(t);

  // The 2 first bytes of outDateTimeCompact is the DateCompact 
  // the 2 last bytes of outDateTimeCompact is the Timecomapct
  pdate = (t[0] << 8) | t[1];
  ptime = (t[2] << 8) | t[3];

  if (audit_config.Verbose)
    TRACE_S("--- StoredValueDebit\n");

  cal_rc = CalypsoCardStoredValueDebit(cal_ctx, NULL, 0, inValue, pdate, ptime, NULL, NULL);

  if (cal_rc && (cal_rc != CALYPSO_ERR_STATUS_WORD))
  {
    if (!audit_config.Quiet)
      TRACE_RC("--- StoredValueDebit");

    if (cal_rc & CALYPSO_ERR_FATAL_)
    {
      poFatal();
      return kErrNoCard;
    }
  }

  /* Retrieve status word */
  CalypsoCardGetSW(cal_ctx, outSW);
  return kSUCCESS;
}





/* ----------------------------------------------------------------------------
	poSamSendReceive
		Send an APDU to the reader SAM and receive the answer.
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
int   poSamSendReceive
(
	unsigned char *	inSendData,		// APDU to send to the reader SAM
	unsigned char	inSendLength,	// Length of the APDU to send to the reader SAM
	unsigned char *	outRecvData,	 // Answer received from the reader SAM
	unsigned char *	outRecvLength	// Length of the answer received from the reader SAM
)
{
  SIZE_T l_outRecvLength = 256;

  if (audit_config.Verbose)
    TRACE_S("--- SamTransmit\n");

  cal_rc = CalypsoSamTransmit(cal_ctx, inSendData, inSendLength, outRecvData, &l_outRecvLength);

  if (cal_rc)
  {
    if (!audit_config.Quiet)
      TRACE_RC("--- SamTransmit");

    if (cal_rc & CALYPSO_ERR_FATAL_)
    {
      poFatal();
      return kErrNoCard;
    }
  }

  /* Get response */
  if ((l_outRecvLength == 2) && (outRecvData[0] == 0x90) && (outRecvData[1] == 0x00))
  {
    BYTE apdu[5];    
    BYTE data_len = 0;

again:
    apdu[0] = 0x94;
    apdu[1] = 0xC0;
    apdu[2] = 0x00;
    apdu[3] = 0x00;
    apdu[4] = data_len;

    l_outRecvLength = 256;
    cal_rc = CalypsoSamTransmit(cal_ctx, apdu, 5, outRecvData, &l_outRecvLength);

    if ((l_outRecvLength == 2) && (outRecvData[0] == 0x6C))
    {
      data_len = outRecvData[1];
      goto again;
    }
  }

  *outRecvLength = (unsigned char) l_outRecvLength;
  return kSUCCESS;
}





/* ----------------------------------------------------------------------------
	poVerifyPIN
		Verify the PIN
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
int   poVerifyPin
(
	unsigned char	inMode,		// 0: Plain data, 1: Encrypted data
	unsigned char	inPIN[4],	// Plain PIN to verify
	unsigned char	outSW[ 2 ]	// Status Word from the PO
)
{
  switch (inMode)
  {
    case 0  : if (audit_config.Verbose)
                TRACE_S("--- VerifyPin (plain)\n");
              
              cal_rc = CalypsoCardVerifyPinPlain(cal_ctx, inPIN);

              if (cal_rc && (cal_rc != CALYPSO_ERR_STATUS_WORD))
              {
                if (!audit_config.Quiet)
                  TRACE_RC("--- VerifyPin (plain)");

                if (cal_rc & CALYPSO_ERR_FATAL_)
                {
                  poFatal();
                  return kErrNoCard;
                }
              }

              break;

    case 1  : if (audit_config.Verbose)
                TRACE_S("--- VerifyPin (encrypted)\n");

              cal_rc = CalypsoCardVerifyPinCipher(cal_ctx, inPIN);

              if (cal_rc && (cal_rc != CALYPSO_ERR_STATUS_WORD))
              {
                if (!audit_config.Quiet)
                  TRACE_RC("--- VerifyPin (encrypted)");

                if (cal_rc & CALYPSO_ERR_FATAL_)
                {
                  poFatal();
                  return kErrNoCard;
                }
              }

              break;

    default : return kErrNotImplemented;
  }

  /* Retrieve status word */
  CalypsoCardGetSW(cal_ctx, outSW);
  return kSUCCESS;
}

/* ----------------------------------------------------------------------------
	poChangePIN
		Change the PIN
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
int   poChangePin
(
	unsigned char	inMode,		// 0: Plain data, 1: Encrypted data
	unsigned char	inPIN[4],	// plain PIN value to set
	unsigned char	outSW[ 2 ]	// Status Word from the PO
)
{
  switch (inMode)
  {
    case 0  : if (audit_config.Verbose)
                TRACE_S("--- ChangePin (plain)\n");
              
              cal_rc = CalypsoCardChangePinPlain(cal_ctx, inPIN);

              if (cal_rc && (cal_rc != CALYPSO_ERR_STATUS_WORD))
              {
                if (!audit_config.Quiet)
                  TRACE_RC("--- ChangePin (plain)");

                if (cal_rc & CALYPSO_ERR_FATAL_)
                {
                  poFatal();
                  return kErrNoCard;
                }
              }

              break;

    case 1  : if (audit_config.Verbose)
                TRACE_S("--- ChangePin (encrypted)\n");

              cal_rc = CalypsoCardChangePinCipher(cal_ctx, inPIN);

              if (cal_rc && (cal_rc != CALYPSO_ERR_STATUS_WORD))
              {
                if (!audit_config.Quiet)
                  TRACE_RC("--- ChangePin (encrypted)");

                if (cal_rc & CALYPSO_ERR_FATAL_)
                {
                  poFatal();
                  return kErrNoCard;
                }
              }

              break;


    default : return kErrNotImplemented;
  }

  /* Retrieve status word */
  CalypsoCardGetSW(cal_ctx, outSW);
  return kSUCCESS;
}




void TRACE_RC(const char *s)
{
  TRACE_S(s);
  TRACE_S(" failed, rc=");
  TRACE_HW(cal_rc);
  TRACE_S(NULL);
}
