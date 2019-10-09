/**h* CalypsoAPI/Calypso_PC_Explorer.c
 *
 * NAME
 *   SpringCard Calypso API :: Card exploration
 *
 * COPYRIGHT
 *   (c) 2008 SpringCard SAS - See LICENCE.txt for licence information
 *
 * AUTHOR
 *   Johann Dantant
 *
 * HISTORY
 *   JDA 21/10/2008 : first public release
 *
 **/
#include "../calypso_api_i.h"

static BOOL is_0(BYTE data[], BYTE size)
{
  BYTE i; 
  for (i=0; i<size; i++)
    if (data[i] != 0) return FALSE;  
  return TRUE;
}

static BYTE div_by_100(BYTE data[], BYTE size)
{
  BYTE i;
  DWORD rest = 0;
  
  for (i=0; i<size; i++)
  {
    rest   = rest * 256 + (data[i] & 0xFF);
    data[i]= (BYTE) (rest / 100);
    rest   = (BYTE) (rest % 100); 
  }
  
  return (BYTE) rest;
}

static BYTE div_by_10(BYTE data[], BYTE size)
{
  BYTE i;
  DWORD rest = 0;
  
  for (i=0; i<size; i++)
  {
    rest   = rest * 256 + (data[i] & 0xFF);
    data[i]= (BYTE) (rest / 10);
    rest   = (BYTE) (rest % 10);
  }
  
  return (BYTE) rest;
}

/**f* CSB6_Calypso/CalypsoExploreAndParse
 *
 * NAME
 *   CalypsoExploreAndParse
 *
 * DESCRIPTION
 *   Explore a Calypso card, parse its data, and export the result in an INI or XML
 *
 * INPUTS
 *   P_CSB6_CALYPSO_CTX  p_ctx          : library context
 *
 * RETURNS
 *   DWORD                             : S_SUCCESS or an error code
 *
 * SEE ALSO
 *   CalypsoParserSetXmlOutput
 *   CalypsoParserSetIniOutput
 *   CalypsoParserDispose
 *
 **/
CALYPSO_PROC CalypsoExploreAndParse(CALYPSO_CTX_ST *ctx)
{
  BYTE card_uid[8];
  DWORD rc;
  SIZE_T RecDataSize;
  BYTE RecData[252];
  BYTE RecNo;
  FILE_INFO_ST file_info;

  if (ctx == NULL)
  {
    rc = CALYPSO_ERR_INVALID_CONTEXT;
    goto failed;
  }

  ParserOut_SectionBegin(ctx, strCard);
  
  /* Card and application level data */
  /* ------------------------------- */
  if (ctx->Card.AtrLen)
  {
    /* Explain the ATR */
    ParserOut_SectionBegin(ctx, strAtr);
    rc = CalypsoOutputCardAtr(ctx, NULL, 0);
    if (rc) goto failed;
    ParserOut_SectionEnd(ctx, strAtr);
  }

  /* Explain the FCI returned by the Ticketing application (if some) */
  ParserOut_SectionBegin(ctx, strCardInfo);
  rc = CalypsoOutputCardInfo(ctx);
  ParserOut_SectionEnd(ctx, strCardInfo);
  if (rc) goto failed;
    
  /* Now we must have the complete serial number */
  rc = CalypsoCardSerialNumber(ctx, card_uid);
  if (rc) goto failed;

  ParserOut_Hex(ctx, strUid, card_uid, sizeof(card_uid));

  /* Convert this number into BCD */
  {
    BYTE card_uid_copy[8];
    char card_uid_bcd[40+1];
    BYTE i, bcd_size = 0;

    memset(card_uid_bcd, '\0', sizeof(card_uid_bcd));

    /* Guess BCD output length */
    memcpy(card_uid_copy, card_uid, 8);
    while (!is_0(card_uid_copy, 8))
    {
      div_by_100(card_uid_copy, 8);
      bcd_size++;
    }

    /* Translate the data */
    memcpy(card_uid_copy, card_uid, 8);
    for (i=1; i<=bcd_size; i++)
    {
      card_uid_bcd[2 * (bcd_size-i) + 1] = (char) ('0' + div_by_10(card_uid_copy, 8));
      card_uid_bcd[2 * (bcd_size-i) + 0] = (char) ('0' + div_by_10(card_uid_copy, 8));
    }

    /* Find first non-zero char */
    for (i=0; i<2*bcd_size; i++)
      if (card_uid_bcd[i] != '0') break;

    /* OK */
    ParserOut_Str(ctx, "UID_d", &card_uid_bcd[i]);
  }

  /* Ready to go deep in the Ticketing application !!! */
  /* Select the DF */
  rc = CalypsoCardSelectDF(ctx, CALYPSO_DF_TICKETING, NULL, NULL);
  if (rc == CALYPSO_ERR_STATUS_WORD)
  {
    /* Classical selection failed -> dummy read record to make sure... */
    RecDataSize = sizeof(RecData);
    rc = CalypsoCardReadRecord(ctx, 7, 1, CALYPSO_MIN_RECORD_SIZE, RecData, &RecDataSize);
  }
  if (rc & CALYPSO_ERR_FATAL_) goto failed;

  /* Environment file */
  /* ---------------- */

  /* Select the file */
  rc = CalypsoCardSelectEF(ctx, 0x2001, NULL, NULL);
  if (rc & CALYPSO_ERR_FATAL_) goto failed;

  ParserOut_SectionBegin(ctx, strEnvHolder);

  /* Explain the FCI */
  rc = CalypsoParseSelectResp(ctx, NULL, 0, &file_info);
  if (rc) goto failed;
  ParserOut_SectionBegin(ctx, strFileInfo);
  rc = CalypsoOutputFileInfo(ctx, &file_info);
  ParserOut_SectionEnd(ctx, strFileInfo);
  if (rc) goto failed;

  /* Read all records */
  for (RecNo=1; RecNo <= file_info.NumRec; RecNo++)
  {
    RecDataSize = sizeof(RecData);

    rc = CalypsoCardReadRecord(ctx, 0, RecNo, file_info.RecSize, RecData, &RecDataSize);
    if (rc & CALYPSO_ERR_FATAL_) goto failed;

    ParserOut_SectionBeginId(ctx, strRecord, RecNo);
    CalypsoOutputEnvAndHolderRecordEx(ctx, RecData, RecDataSize, FALSE);
    ParserOut_SectionEnd(ctx, strRecord);
  }
  ParserOut_SectionEnd(ctx, strEnvHolder);

  /* Contracts file */
  /* -------------- */

  /* Select the file */
  rc = CalypsoCardSelectEF(ctx, 0x2020, NULL, NULL);
  if (rc & CALYPSO_ERR_FATAL_) goto failed;

  ParserOut_SectionBegin(ctx, strContracts);

  /* Explain the FCI */
  rc = CalypsoParseSelectResp(ctx, NULL, 0, &file_info);
  if (rc) goto failed;
  ParserOut_SectionBegin(ctx, strFileInfo);
  rc = CalypsoOutputFileInfo(ctx, &file_info);
  ParserOut_SectionEnd(ctx, strFileInfo);
  if (rc) goto failed;

  /* Read all records */
  for (RecNo=1; RecNo <= file_info.NumRec; RecNo++)
  {
    RecDataSize = sizeof(RecData);
    rc = CalypsoCardReadRecord(ctx, 0, RecNo, file_info.RecSize, RecData, &RecDataSize);
    if (rc & CALYPSO_ERR_FATAL_) goto failed;

    ParserOut_SectionBeginId(ctx, strRecord, RecNo);
    CalypsoOutputContractRecordEx(ctx, RecData, RecDataSize);
    ParserOut_SectionEnd(ctx, strRecord);
  }
  ParserOut_SectionEnd(ctx, strContracts);

  /* Transport log file */
  /* ------------------ */

  /* Select the file */
  rc = CalypsoCardSelectEF(ctx, 0x2010, NULL, NULL);
  if (rc & CALYPSO_ERR_FATAL_) goto failed;

  ParserOut_SectionBegin(ctx, strTransportLog);

  /* Explain the FCI */
  rc = CalypsoParseSelectResp(ctx, NULL, 0, &file_info);
  if (rc) goto failed;
  ParserOut_SectionBegin(ctx, strFileInfo);
  rc = CalypsoOutputFileInfo(ctx, &file_info);
  ParserOut_SectionEnd(ctx, strFileInfo);
  if (rc) goto failed;

  /* Read all records */
  for (RecNo=1; RecNo <= file_info.NumRec; RecNo++)
  {
    RecDataSize = sizeof(RecData);
    rc = CalypsoCardReadRecord(ctx, 0, RecNo, file_info.RecSize, RecData, &RecDataSize);
    if (rc & CALYPSO_ERR_FATAL_) goto failed;

    ParserOut_SectionBeginId(ctx, strRecord, RecNo);
    CalypsoOutputEventRecordEx(ctx, RecData, RecDataSize);
    ParserOut_SectionEnd(ctx, strRecord);
  }
  ParserOut_SectionEnd(ctx, strTransportLog);


  ParserOut_SectionEnd(ctx, strCard);
  return 0;

failed:
  CalypsoTraceRC(TR_CARD|TR_TRACE, "CalypsoExploreAndParse", rc);
  return rc;
}

