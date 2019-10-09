/**h* DesfireAPI/ISO
 *
 * NAME
 *   DesfireAPI :: ISO 7816-4 functions
 *
 * COPYRIGHT
 *   (c) 2009 SpringCard - www.springcard.com
 *
 * DESCRIPTION
 *   Implementation of DESFIRE ISO 7816-4 compliant functions.
 *
 **/
#include "sprox_desfire_i.h"

SPROX_API_FUNC(Desfire_IsoApdu) (SPROX_PARAM  BYTE INS, BYTE P1, BYTE P2, BYTE Lc, const BYTE data_in[], BYTE Le, BYTE data_out[], WORD *data_out_len, WORD *SW)
{
  SPROX_RC status;
  BYTE send_buffer[280];
  WORD send_length = 0;
  BYTE recv_buffer[280]; 
#ifndef _USE_PCSC
  WORD recv_length = sizeof(recv_buffer);
#else
  DWORD recv_length = sizeof(recv_buffer);
#endif
	WORD i;

  WORD _SW;

	SPROX_DESFIRE_GET_CTX();

	if ( ((Lc == 0) && (data_in != NULL)) || ((Lc != 0) && (data_in == NULL)) )
	  return DFCARD_LIB_CALL_ERROR;

  send_buffer[send_length++] = 0;
	send_buffer[send_length++] = INS;
	send_buffer[send_length++] = P1;
	send_buffer[send_length++] = P2;

	if (data_in != NULL)
	{
	  send_buffer[send_length++] = Lc;
		for (i=0; i<Lc; i++)
		  send_buffer[send_length++] = data_in[i];
	}
	if (data_out != NULL)
  {
    send_buffer[send_length++] = Le;
	}

#ifndef _USE_PCSC
	status = SPROX_API_CALL(TclA_Exchange) (SPROX_PARAM_P  5, ctx->tcl_cid, 0xFF, send_buffer, send_length, recv_buffer, &recv_length);
	if (status != MI_OK)
	  return status;
#else
  status = SCardTransmit(hCard,
                         SCARD_PCI_T1,
                         send_buffer,
                         send_length,
                         NULL,
                         recv_buffer,
                         &recv_length);
	if (status != SCARD_S_SUCCESS)
	  return status;
#endif

	if (recv_length < 2)
	  return DFCARD_PCSC_BAD_RESP_LEN;

  recv_length -= 2;
  if (data_out_len != NULL)
	  *data_out_len = (WORD) recv_length;

	if (data_out != NULL)
	{
	  if ((Le == 0) && (recv_length > 256))
		{
		  recv_length = 256;
			status = DFCARD_OVERFLOW;
		} else
		if ((Le != 0) && (recv_length > Le))
		{
		  recv_length = Le;
			status = DFCARD_OVERFLOW;
		}
		
	  memcpy(data_out, recv_buffer, recv_length);
	}

	_SW   = recv_buffer[recv_length+0];
	_SW <<= 8;
	_SW  |= recv_buffer[recv_length+1];

	if (SW != NULL)
	  *SW = _SW;

	if (_SW != 0x9000)
		status = DFCARD_PCSC_BAD_RESP_SW;

	return status;
}

#define SW_AUTHENT    0x01
#define SW_SELECT_DF  0x02
#define SW_WRITE      0x04

static SPROX_RC TranslateSW(WORD SW, BYTE param)
{
  SPROX_RC status;

	switch (SW)
	{
	  case 0x9000 : status = DF_OPERATION_OK;
		              break;

		case 0x6282 : /* End of file reached before reading Le bytes */
		              status = DFCARD_ERROR-DF_BOUNDARY_ERROR;
									break;

		case 0x6581 : /* Memory failure */
		              status = DFCARD_ERROR-DF_EEPROM_ERROR;
									break;

		case 0x6700 : /* Wrong length */
		              status = DFCARD_ERROR-DF_LENGTH_ERROR;
									break;

		case 0x6982 : /* File access not allowed */
		              status = DFCARD_ERROR-DF_AUTHENTICATION_ERROR;
									break;

		case 0x6985 : 
		              if (param & SW_WRITE)
									{
									  /* Access condition not satisfied */
		                status = DFCARD_ERROR-DF_PERMISSION_DENIED;
									} else
									{
									  /* File empty */
		                status = DFCARD_ERROR-DF_BOUNDARY_ERROR;
									}
									break;

		case 0x6A82 : 
		              if (param & SW_SELECT_DF)
									{
									  /* DF not found */
		                status = DFCARD_ERROR-DF_APPLICATION_NOT_FOUND;
									} else
									{
									  /* EF not found */
									  status = DFCARD_ERROR-DF_FILE_NOT_FOUND;
									}
									break;

		case 0x6A86 : /* Wrong parameter P1 and/or P2 */
		              status = DFCARD_ERROR-DF_PARAMETER_ERROR;
									break;

		case 0x6A87 : /* Lc inconsistent with P1/P2 */
		              status = DFCARD_ERROR-DF_LENGTH_ERROR;
									break;

		case 0x6B00 : /* Wrong parameter P1 and/or P2 */
		              status = DFCARD_ERROR-DF_PARAMETER_ERROR;
									break;

    case 0x6C00 : if (param & SW_AUTHENT)
		              {
									  /* Wrong Le */
		                status = DFCARD_ERROR-DF_PARAMETER_ERROR;
									} else
									{
									  /* File not found */
									  status = DFCARD_ERROR-DF_FILE_NOT_FOUND;
									}
									break;

    case 0x6D00 : /* Instruction not supported */
		              status = DFCARD_ERROR-DF_ILLEGAL_COMMAND_CODE;
									break;

    case 0x6E00 : /* Wrong CLA */
		              status = DFCARD_ERROR-DF_ILLEGAL_COMMAND_CODE;
									break;


	  default     : status = DFCARD_PCSC_BAD_RESP_SW;
	}

	return status;
}

/**f* DesfireAPI/IsoSelectApplet
 *
 * NAME
 *   IsoSelectApplet
 *
 * DESCRIPTION
 *   Send the ISO 7816-4 SELECT FILE command with the DESFIRE applet name as parameter
 *   (P2 = 0x04, DataIn = 0xD2, 0x76, 0x00, 0x00, 0x85, 0x01, 0x00 )
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_IsoSelectApplet(WORD *SW);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_IsoSelectApplet(SPROX_INSTANCE rInst, 
 *                                        WORD *SW);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_IsoSelectApplet(SCARDHANDLE hCard,
 *                                      WORD *SW);
 *
 * INPUTS
 *   WORD *SW            : optional pointer to retrieve the Status Word in
 *                         case an error occurs
 *
 * RETURNS
 *   DF_OPERATION_OK   : success
 *   Other code if internal or communication error has occured. 
 *
 * SIDE EFFECT
 *   Wrapping mode is implicitly defined to DF_ISO_WRAPPING_CARD
 *
 **/
SPROX_API_FUNC(Desfire_IsoSelectApplet) (SPROX_PARAM  WORD *SW)
{
  static const BYTE DesfireAID[] = { 0xD2, 0x76, 0x00, 0x00, 0x85, 0x01, 0x00 };
  SPROX_DESFIRE_GET_CTX();

	Desfire_CleanupAuthentication(SPROX_PARAM_PV);
	ctx->iso_wrapping = DF_ISO_WRAPPING_CARD;

  return SPROX_API_CALL(Desfire_IsoApdu) (SPROX_PARAM_P  0xA4, 0x04, 0x00, sizeof(DesfireAID), DesfireAID, 0, NULL, NULL, SW);
}

/**f* DesfireAPI/IsoSelectDF
 *
 * NAME
 *   IsoSelectDF
 *
 * DESCRIPTION
 *   Implementation of ISO 7816-4 SELECT FILE command using a Directory File ID (P2=0x02)
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_IsoSelectDF(WORD wFileID,
 *                                   BYTE abFci[],
 *                                   WORD wMaxFciLength,
 *                                   WORD *wGotFciLength,
 *                                   WORD *SW);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_IsoSelectDF(SPROX_INSTANCE rInst, 
 *                                    WORD wFileID,
 *                                    BYTE abFci[],
 *                                    WORD wMaxFciLength,
 *                                    WORD *wGotFciLength,
 *                                    WORD *SW);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_IsoSelectDF(SCARDHANDLE hCard,
 *                                  WORD wFileID,
 *                                  BYTE abFci[],
 *                                  WORD wMaxFciLength,
 *                                  WORD *wGotFciLength,
 *                                  WORD *SW);
 *
 * INPUTS
 *   WORD wFileID        : the identifier of the DF
 *   BYTE abFci[]        : buffer to receive the FCI of the DF (if some)
 *   WORD wMaxFciLength  : maximum length of FCI
 *   WORD *wGotFciLength : actual length of FCI
 *   WORD *SW            : optional pointer to retrieve the Status Word in
 *                         case an error occurs
 *
 * RETURNS
 *   DF_OPERATION_OK   : success
 *   Other code if internal or communication error has occured. 
 *
 * NOTES
 *   The abFci and wGotFciLength parameter could be set to NULL if no FCI is expected
 *   or if the caller doesn't care of the FCI.
 *   This function is also relevant for the root application (Master File -> wFileID = 0x3F00)
 *
 * SEE ALSO
 *   IsoSelectDFName
 *   IsoSelectEF
 *
 **/
SPROX_API_FUNC(Desfire_IsoSelectDF) (SPROX_PARAM  WORD wFileID, BYTE abFci[], WORD wFciMaxLength, WORD *wFciGotLength, WORD *SW)
{
	SPROX_RC status;
  BYTE data[2];
	WORD _SW;
	SPROX_DESFIRE_GET_CTX();

	if (SW == NULL)
	  SW = &_SW;

  /* Change application = forget authentication state */
	Desfire_CleanupAuthentication(SPROX_PARAM_PV);

	data[0] = (BYTE) (wFileID >> 8);
	data[1] = (BYTE)  wFileID;

  /* Do the select */
  if (abFci == NULL)
	{
	  status = SPROX_API_CALL(Desfire_IsoApdu) (SPROX_PARAM_P  0xA4, 0x00, 0x0C, 2, data, 0, NULL, NULL, SW);
	} else
	{
	  BYTE Le;

		Le  = (wFciMaxLength > 255) ? 0 : (BYTE) wFciMaxLength;

	  status = SPROX_API_CALL(Desfire_IsoApdu) (SPROX_PARAM_P  0xA4, 0x00, 0x00, 2, data, Le, abFci, wFciGotLength, SW);
	}

	/* Translate the Status Word into a Desfire error code */
	if (status == DFCARD_PCSC_BAD_RESP_SW)
	  status = TranslateSW(*SW, SW_SELECT_DF);

  return status;
}

/**f* DesfireAPI/IsoSelectDFName
 *
 * NAME
 *   IsoSelectDFName
 *
 * DESCRIPTION
 *   Implementation of ISO 7816-4 SELECT FILE command using a Directory Name (P2=0x04)
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_IsoSelectDFName(const BYTE abDFName[],
 *                                       BYTE bDFNameLength,
 *                                       BYTE abFci[],
 *                                       WORD wMaxFciLength,
 *                                       WORD *wGotFciLength,
 *                                       WORD *SW);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_IsoSelectDFName(SPROX_INSTANCE rInst, 
 *                                       const BYTE abDFName[],
 *                                       BYTE bDFNameLength,
 *                                       BYTE abFci[],
 *                                       WORD wMaxFciLength,
 *                                       WORD *wGotFciLength,
 *                                       WORD *SW);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_IsoSelectDFName(SCARDHANDLE hCard,
 *                                      const BYTE abDFName[],
 *                                      BYTE bDFNameLength,
 *                                      BYTE abFci[],
 *                                      WORD wMaxFciLength,
 *                                      WORD *wGotFciLength,
 *                                      WORD *SW);
 *
 * INPUTS
 *   const BYTE abDFName : the name of the DF
 *   BYTE bDFNameLength  : the size of the name of the DF
 *   BYTE abFci[]        : buffer to receive the FCI of the DF (if some)
 *   WORD wMaxFciLength  : maximum length of FCI
 *   WORD *wGotFciLength : actual length of FCI
 *   WORD *SW            : optional pointer to retrieve the Status Word in
 *                         case an error occurs
 *
 * RETURNS
 *   DF_OPERATION_OK   : success
 *   Other code if internal or communication error has occured. 
 *
 * NOTES
 *   The abFci and wGotFciLength parameter could be set to NULL if no FCI is expected
 *   or if the caller doesn't care of the FCI.
 *
 * SEE ALSO
 *   IsoSelectDF
 *   IsoSelectEF
 *
 **/
SPROX_API_FUNC(Desfire_IsoSelectDFName) (SPROX_PARAM  const BYTE abDFName[], BYTE abDFNameLength, BYTE abFci[], WORD wFciMaxLength, WORD *wFciGotLength, WORD *SW)
{
  SPROX_RC status;
  WORD _SW;
  SPROX_DESFIRE_GET_CTX();

	if (SW == NULL)
	  SW = &_SW;

  /* Change application = forget authentication state */
	Desfire_CleanupAuthentication(SPROX_PARAM_PV);

  /* Do the select */
  if (abFci == NULL)
	{
	  status = SPROX_API_CALL(Desfire_IsoApdu) (SPROX_PARAM_P  0xA4, 0x04, 0x0C, abDFNameLength, abDFName, 0, NULL, NULL, SW);
	} else
	{
	  BYTE Le;

		Le  = (wFciMaxLength > 255) ? 0 : (BYTE) wFciMaxLength;

	  status = SPROX_API_CALL(Desfire_IsoApdu) (SPROX_PARAM_P  0xA4, 0x04, 0x00, abDFNameLength, abDFName, Le, abFci, wFciGotLength, SW);
	}

	/* Translate the Status Word into a Desfire error code */
	if (status == DFCARD_PCSC_BAD_RESP_SW)
	  status = TranslateSW(*SW, SW_SELECT_DF);

  return status;
}

/**f* DesfireAPI/IsoSelectEF
 *
 * NAME
 *   IsoSelectEF
 *
 * DESCRIPTION
 *   Implementation of ISO 7816-4 SELECT FILE command using a Elementary File ID (P2=0x02)
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_IsoSelectEF(WORD wFileID,
 *                                   WORD *SW);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_IsoSelectEF(SPROX_INSTANCE rInst, 
 *                                    WORD wFileID,
 *                                    WORD *SW);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_IsoSelectEF(SCARDHANDLE hCard,
 *                                  WORD wFileID,
 *                                  WORD *SW);
 *
 * INPUTS
 *   WORD wFileID      : the identifier of the EF
 *   WORD *SW          : optional pointer to retrieve the Status Word in
 *                       case an error occurs
 *
 * RETURNS
 *   DF_OPERATION_OK   : success
 *   Other code if internal or communication error has occured. 
 *
 * SEE ALSO
 *   IsoSelectDF
 *
 **/
SPROX_API_FUNC(Desfire_IsoSelectEF) (SPROX_PARAM  WORD wFileID, WORD *SW)
{
  SPROX_RC status;
  BYTE data[2];
	WORD _SW;
	SPROX_DESFIRE_GET_CTX();

	if (SW == NULL)
	  SW = &_SW;

	data[0] = (BYTE) (wFileID >> 8);
	data[1] = (BYTE)  wFileID;

	status = SPROX_API_CALL(Desfire_IsoApdu) (SPROX_PARAM_P  0xA4, 0x02, 0x0C, 2, data, 0, NULL, NULL, SW);

	/* Translate the Status Word into a Desfire error code */
	if (status == DFCARD_PCSC_BAD_RESP_SW)
	  status = TranslateSW(*SW, 0);

  return status;
}

/**f* DesfireAPI/IsoReadBinary
 *
 * NAME
 *   IsoReadBinary
 *
 * DESCRIPTION
 *   Implementation of ISO 7816-4 READ BINARY command in Desfire EV1 flavour
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_IsoReadBinary(WORD wOffset,
 *                                     BYTE abData[],
 *                                     BYTE bWantLength,
 *                                     WORD *wGotLength,
 *                                     WORD *SW);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_IsoReadBinary(SPROX_INSTANCE rInst,
 *                                      WORD wOffset,
 *                                      BYTE abData[],
 *                                      BYTE bWantLength,
 *                                      WORD *wGotLength,
 *                                      WORD *SW);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_IsoReadBinary(SCARDHANDLE hCard,
 *                                    WORD wOffset,
 *                                    BYTE abData[],
 *                                    BYTE bWantLength,
 *                                    WORD *wGotLength,
 *                                    WORD *SW);
 *
 * INPUTS
 *   WORD wOffset      : starting position for the read operation
 *   BYTE abData[]     : buffer to receive the data
 *   BYTE bWantLength  : maximum data length to read. Set to 0 to read 256 bytes.
 *   WORD *wGotLength  : actual data length read
 *   WORD *SW          : optional pointer to retrieve the Status Word in
 *                       case an error occurs
 *
 * RETURNS
 *   DF_OPERATION_OK   : success
 *   Other code if internal or communication error has occured. 
 *
 * NOTES
 *   After a successfull authentication, a CMAC is added to card's response.
 *   The value of bWantLength must be choosen in consequence.
 *   This command checks the value of the CMAC in card's response and removes
 *   it from the data buffer.
 *
 * SEE ALSO
 *   IsoUpdateBinary
 *   IsoReadRecord
 *
 **/
SPROX_API_FUNC(Desfire_IsoReadBinary) (SPROX_PARAM  WORD wOffset, BYTE abData[], BYTE bWantLength, WORD *wGotLength, WORD *SW)
{
  BYTE P1, P2;
  SPROX_RC status;
  BYTE _abData[256+8];
	WORD _wGotLength;
	WORD _SW;
	SPROX_DESFIRE_GET_CTX();

	if (SW == NULL)
	  SW = &_SW;
	if (wGotLength == NULL)
	  wGotLength = &_wGotLength;
	if (abData == NULL)
	  abData = _abData;

  if (wOffset > 32767)
	  return DFCARD_LIB_CALL_ERROR;

	P1 = (BYTE) (wOffset >> 8);
	P2 = (BYTE)  wOffset;

  status = SPROX_API_CALL(Desfire_IsoApdu) (SPROX_PARAM_P  0xB0, P1, P2, 0, NULL, bWantLength, abData, wGotLength, SW);

	if ((status == DF_OPERATION_OK) && (ctx->session_type))
	{
    /* There's a CMAC at the end of the data */
    BYTE l;
		BYTE buffer[280];
		BYTE cmac[8];

		if (*wGotLength < 8)
		{
      /* CMAC not included ? */
			return DFCARD_WRONG_LENGTH;
		}

    l = *wGotLength-8;
		memcpy(&buffer[0], abData, l);
		buffer[l++] = (BYTE) *SW;

    Desfire_ComputeCmac(SPROX_PARAM_P  buffer, l, FALSE, cmac);

		*wGotLength -= 8;

		if (memcmp(cmac, &abData[*wGotLength], 8))
		{
		  /* Wrong CMAC */
      return DFCARD_WRONG_MAC;
		}
	}

	/* Translate the Status Word into a Desfire error code */
	if (status == DFCARD_PCSC_BAD_RESP_SW)
	  status = TranslateSW(*SW, 0);

	return status;
}

/**f* DesfireAPI/IsoUpdateBinary
 *
 * NAME
 *   IsoUpdateBinary
 *
 * DESCRIPTION
 *   Implementation of ISO 7816-4 UPDATE BINARY command in Desfire EV1 flavour
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_IsoUpdateBinary(WORD wOffset,
 *                                       const BYTE abData[],
 *                                       BYTE bLength,
 *                                       WORD *SW);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_IsoUpdateBinary(SPROX_INSTANCE rInst,
 *                                        WORD wOffset,
 *                                        const BYTE abData[],
 *                                        BYTE bLength,
 *                                        WORD *SW);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_IsoUpdateBinary(SCARDHANDLE hCard,
 *                                      WORD wOffset;
 *                                      const BYTE abData[],
 *                                      BYTE bLength,
 *                                      WORD *SW);
 *
 * INPUTS
 *   WORD wOffset        : starting position for the write operation in bytes
 *   const BYTE abData[] : buffer containing the data to write
 *   BYTE bLength        : size of data to be written in bytes
 *   WORD *SW            : optional pointer to retrieve the Status Word in
 *                         case an error occurs
 *
 * RETURNS
 *   DF_OPERATION_OK     : success
 *   Other code if internal or communication error has occured. 
 *
 * SEE ALSO
 *   IsoReadBinary
 *
 **/
SPROX_API_FUNC(Desfire_IsoUpdateBinary) (SPROX_PARAM  WORD wOffset, const BYTE abData[], BYTE bLength, WORD *SW)
{
  BYTE P1, P2;
  SPROX_RC status;
  WORD _SW;
	SPROX_DESFIRE_GET_CTX();

	if (SW == NULL)
	  SW = &_SW;

  if (wOffset > 32767)
	  return DFCARD_LIB_CALL_ERROR;

	P1 = (BYTE) (wOffset >> 8);
	P2 = (BYTE)  wOffset;

  status = SPROX_API_CALL(Desfire_IsoApdu) (SPROX_PARAM_P  0xD6, P1, P2, bLength, abData, 0, NULL, NULL, SW);

	/* Translate the Status Word into a Desfire error code */
	if (status == DFCARD_PCSC_BAD_RESP_SW)
	  status = TranslateSW(*SW, SW_WRITE);

  return status;
}

/**f* DesfireAPI/IsoReadRecord
 *
 * NAME
 *   IsoReadRecord
 *
 * DESCRIPTION
 *   Implementation of ISO 7816-4 READ RECORD command in Desfire EV1 flavour
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_IsoReadRecord(BYTE bRecNum,
 *                                     BOOL fReadAll;
 *                                     BOOL abData[],
 *                                     WORD wMaxLength,
 *                                     WORD *wGotLength,
 *                                     WORD *SW);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_IsoReadRecord(SPROX_INSTANCE rInst,
 *                                      BYTE bRecNum,
 *                                      BOOL fReadAll;
 *                                      BOOL abData[],
 *                                      WORD wMaxLength,
 *                                      WORD *wGotLength,
 *                                      WORD *SW);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_IsoReadRecord(SCARDHANDLE hCard,
 *                                    BYTE bRecNum,
 *                                    BOOL fReadAll;
 *                                    BOOL abData[],
 *                                    WORD wMaxLength,
 *                                    WORD *wGotLength,
 *                                    WORD *SW);
 *
 * INPUTS
 *   BYTE bRecNum      : first (or only) record to read
 *   BOOL fReadAll     : TRUE  : read all records (starting from bRecNum)
 *                       FALSE : read only record # bRecNum
 *   BYTE abData[]     : buffer to receive the data
 *   WORD wMaxLength   : size of the buffer
 *   WORD *wGotLength  : actual data length read
 *   WORD *SW          : optional pointer to retrieve the Status Word in
 *                       case an error occurs
 *
 * RETURNS
 *   DF_OPERATION_OK   : success
 *   Other code if internal or communication error has occured. 
 *
 * NOTES
 *   After a successfull authentication, a CMAC is added to card's response.
 *   This command checks the value of the CMAC in card's response and removes
 *   it from the data buffer.
 *
 * SEE ALSO
 *   IsoAppendRecord
 *   IsoReadBinary
 *
 **/
SPROX_API_FUNC(Desfire_IsoReadRecord) (SPROX_PARAM  BYTE bRecNum, BOOL fReadAll, BYTE abData[], WORD wMaxLength, WORD *wGotLength, WORD *SW)
{
  BYTE P2, Le;
  SPROX_RC status;
  BYTE _abData[256+8];
	WORD _wGotLength;
	WORD _SW;
	SPROX_DESFIRE_GET_CTX();

	if (SW == NULL)
	  SW = &_SW;
	if (wGotLength == NULL)
	  wGotLength = &_wGotLength;
	if (abData == NULL)
	  abData = _abData;

  P2 = (fReadAll) ? 0x05 : 0x04;
	Le = (wMaxLength > 255) ? 0 : (BYTE) wMaxLength;

  status = SPROX_API_CALL(Desfire_IsoApdu) (SPROX_PARAM_P  0xB2, bRecNum, P2, 0, NULL, Le, abData, wGotLength, SW);

	if ((status == DF_OPERATION_OK) && (ctx->session_type))
	{
    /* There's a CMAC at the end of the data */
    BYTE l;
		BYTE buffer[280];
		BYTE cmac[8];

		if (*wGotLength < 8)
		{
      /* CMAC not included ? */
			return DFCARD_WRONG_LENGTH;
		}

    l = *wGotLength-8;
		memcpy(&buffer[0], abData, l);
		buffer[l++] = (BYTE) *SW;

    Desfire_ComputeCmac(SPROX_PARAM_P  buffer, l, FALSE, cmac);

		*wGotLength -= 8;

		if (memcmp(cmac, &abData[*wGotLength], 8))
		{
		  /* Wrong CMAC */
      return DFCARD_WRONG_MAC;
		}
	}

	/* Translate the Status Word into a Desfire error code */
	if (status == DFCARD_PCSC_BAD_RESP_SW)
	  status = TranslateSW(*SW, 0);

  return status;
}

/**f* DesfireAPI/IsoAppendRecord
 *
 * NAME
 *   IsoAppendRecord
 *
 * DESCRIPTION
 *   Implementation of ISO 7816-4 APPEND RECORD command in Desfire EV1 flavour
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_IsoAppendRecord(const BYTE abData[],
 *                                       BYTE bLength,
 *                                       WORD *SW);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_IsoAppendRecord(SPROX_INSTANCE rInst,
 *                                        const BYTE abData[],
 *                                        BYTE bLength,
 *                                        WORD *SW);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_IsoAppendRecord(SCARDHANDLE hCard,
 *                                      const BYTE abData[],
 *                                      BYTE bLength,
 *                                      WORD *SW);
 *
 * INPUTS
 *   const BYTE abData[] : buffer containing the data to write
 *   BYTE bLength        : size of data to be written in bytes
 *   WORD *SW            : optional pointer to retrieve the Status Word in
 *                         case an error occurs
 *
 * RETURNS
 *   DF_OPERATION_OK     : success
 *   Other code if internal or communication error has occured. 
 *
 * SEE ALSO
 *   IsoReadRecord
 *
 **/
SPROX_API_FUNC(Desfire_IsoAppendRecord) (SPROX_PARAM  const BYTE abData[], BYTE bLength, WORD *SW)
{
  SPROX_RC status;
  WORD _SW;
  SPROX_DESFIRE_GET_CTX();

	if (SW == NULL)
	  SW = &_SW;

  status = SPROX_API_CALL(Desfire_IsoApdu) (SPROX_PARAM_P  0xE2, 0x00, 0x00, bLength, abData, 0, NULL, NULL, SW);

	/* Translate the Status Word into a Desfire error code */
	if (status == DFCARD_PCSC_BAD_RESP_SW)
	  status = TranslateSW(*SW, SW_WRITE);

  return status;
}

/**f* DesfireAPI/IsoGetChallenge
 *
 * NAME
 *   IsoGetChallenge
 *
 * DESCRIPTION
 *   Implementation of ISO 7816-4 GET CHALLENGE command in Desfire EV1 flavour
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_IsoGetChallenge(BYTE bKeyAlgorithm,
 *                                       BYTE bRndSize,
 *                                       BYTE abRndCard1[],
 *                                       WORD *SW);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_IsoGetChallenge(SPROX_INSTANCE rInst,
 *                                        BYTE bRndSize,
 *                                        BYTE abRndCard1[],
 *                                        WORD *SW);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_IsoGetChallenge(SCARDHANDLE hCard,
 *                                      BYTE bRndSize,
 *                                      BYTE abRndCard1[],
 *                                      WORD *SW);
 *
 * INPUTS
 *   BYTE bRndSize              : size of the challenge
 *                                (8 bytes for DES/3DES2K, 16 bytes for 3DES3K or AES)
 *   BYTE abRndCard1[]          : card's first challenge (not involved in session key)
 *   WORD *SW                   : optional pointer to retrieve the Status Word in
 *                                case an error occurs
 *
 * RETURNS
 *   DF_OPERATION_OK    : success
 *   Other code if internal or communication error has occured. 
 *
 * SEE ALSO
 *   IsoMutualAuthenticate
 *   IsoExternalAuthenticate
 *   IsoInternalAuthenticate
 *
 **/
SPROX_API_FUNC(Desfire_IsoGetChallenge) (SPROX_PARAM  BYTE bRndSize, BYTE abRndCard1[], WORD *SW)
{
  SPROX_RC status;
  WORD _SW;
	SPROX_DESFIRE_GET_CTX();

	if (SW == NULL)
	  SW = &_SW;
  
	status = SPROX_API_CALL(Desfire_IsoApdu) (SPROX_PARAM_P  0x84, 0x00, 0x00, 0, NULL, bRndSize, abRndCard1, NULL, SW);

	/* Translate the Status Word into a Desfire error code */
	if (status == DFCARD_PCSC_BAD_RESP_SW)
	  status = TranslateSW(*SW, SW_AUTHENT);

  return status;
}

static SPROX_RC Desfire_IsoLoadKey(SPROX_PARAM  BYTE bKeyAlgorithm, const BYTE abKeyValue[])
{
  SPROX_DESFIRE_GET_CTX();

  if (abKeyValue == NULL)
	  return DFCARD_LIB_CALL_ERROR;

	if (bKeyAlgorithm == DF_ISO_CIPHER_2KDES)
	{
	  if (!memcmp(&abKeyValue[0], &abKeyValue[8], 8))
		{
			ctx->session_type = KEY_ISO_DES;
			Desfire_InitCrypto3Des(SPROX_PARAM_P  &abKeyValue[0], &abKeyValue[8], &abKeyValue[0]);
		} else
		{
			ctx->session_type = KEY_ISO_DES;
			Desfire_InitCrypto3Des(SPROX_PARAM_P  &abKeyValue[0], &abKeyValue[0], &abKeyValue[0]);
		}
	} else
	if (bKeyAlgorithm == DF_ISO_CIPHER_3KDES)
	{
		ctx->session_type = KEY_ISO_3DES3K;
		Desfire_InitCrypto3Des(SPROX_PARAM_P  &abKeyValue[0], &abKeyValue[8], &abKeyValue[16]);
	} else
	if (bKeyAlgorithm == DF_ISO_CIPHER_AES)
	{
		ctx->session_type = KEY_ISO_AES;		
		Desfire_InitCryptoAes(SPROX_PARAM_P  abKeyValue);
	} else
		return DFCARD_LIB_CALL_ERROR;

	Desfire_CleanupInitVector(SPROX_PARAM_PV);
	return DF_OPERATION_OK;
}

/**f* DesfireAPI/IsoExternalAuthenticate
 *
 * NAME
 *   IsoExternalAuthenticate
 *
 * DESCRIPTION
 *   Implementation of ISO 7816-4 EXTERNAL AUTHENTICATE command in Desfire EV1 flavour
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_IsoExternalAuthenticate(BYTE bKeyAlgorithm,
 *                                               BYTE bKeyReference,
 *                                               BYTE bRndSize,
 *                                               const BYTE abRndCard1[],
 *                                               const BYTE abRndHost1[],
 *                                               const BYTE abKeyValue[],
 *                                               WORD *SW);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_IsoExternalAuthenticate(SPROX_INSTANCE rInst,
 *                                                BYTE bKeyAlgorithm,
 *                                                BYTE bKeyReference,
 *                                                BYTE bRndSize,
 *                                                const BYTE abRndCard1[],
 *                                                const BYTE abRndHost1[],
 *                                                const BYTE abKeyValue[],
 *                                                WORD *SW);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_IsoExternalAuthenticate(SCARDHANDLE hCard,
 *                                              BYTE bKeyAlgorithm,
 *                                              BYTE bKeyReference,
 *                                              BYTE bRndSize,
 *                                              const BYTE abRndCard1[],
 *                                              const BYTE abRndHost1[],
 *                                              const BYTE abKeyValue[],
 *                                              WORD *SW);
 *
 * INPUTS
 *   BYTE bKeyAlgorithm         : algorithm to be used:
 *                                - 0x02 : DES or 3DES2K (16-byte key)
 *                                - 0x04 : 3DES3K (24-byte key)
 *                                - 0x09 : AES (16-byte key)
 *   BYTE bKeyReference         : reference to the key in the card
 *                                - 0x00 : card's master key (valid only on root application)
 *                                - 0x8n : application's key #n 
 *   BYTE bRndSize              : size of the challenge
 *                                (8 bytes for DES/3DES2K, 16 bytes for 3DES3K or AES)
 *   const BYTE abRndCard1[]    : card's first challenge (as returned by IsoGetChallenge - not involved in session key)
 *   const BYTE abRndHost1[]    : host's first challenge (choosen by the caller - involved in session key)
 *   const BYTE abKeyValue [16] : the key itself
 *   WORD *SW                   : optional pointer to retrieve the Status Word in
 *                                case an error occurs
 *
 * RETURNS
 *   DF_OPERATION_OK    : success
 *   Other code if internal or communication error has occured. 
 *
 * SEE ALSO
 *   IsoMutualAuthenticate
 *   IsoGetChallenge
 *   IsoInternalAuthenticate
 *
 **/
SPROX_API_FUNC(Desfire_IsoExternalAuthenticate) (SPROX_PARAM  BYTE bKeyAlgorithm, BYTE bKeyReference, BYTE bRndSize, const BYTE abRndCard1[], const BYTE abRndHost1[], const BYTE abKeyValue[], WORD *SW)
{
	SPROX_RC status;
	DWORD t;
	WORD _SW;
  BYTE buffer[32];
  SPROX_DESFIRE_GET_CTX();

	if (SW == NULL)
	  SW = &_SW;

	memcpy(&buffer[0], abRndHost1, bRndSize);
	memcpy(&buffer[bRndSize], abRndCard1, bRndSize);

  if (abKeyValue != NULL)
	{
	  status = Desfire_IsoLoadKey(SPROX_PARAM_P  bKeyAlgorithm, abKeyValue);
		if (status != DF_OPERATION_OK)
			return status;
	}

	t = 2 * bRndSize;
	Desfire_CipherSend(SPROX_PARAM_P  buffer, &t, sizeof(buffer));

  status = SPROX_API_CALL(Desfire_IsoApdu) (SPROX_PARAM_P  0x82, bKeyAlgorithm, bKeyReference, (BYTE) (2 * bRndSize), buffer, 0, NULL, NULL, SW);

	if (status == DFCARD_PCSC_BAD_RESP_SW)
	  status = TranslateSW(*SW, SW_AUTHENT);

	return status;
}

/**f* DesfireAPI/IsoInternalAuthenticate
 *
 * NAME
 *   IsoInternalAuthenticate
 *
 * DESCRIPTION
 *   Implementation of ISO 7816-4 INTERNAL AUTHENTICATE command in Desfire EV1 flavour
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_IsoInternalAuthenticate(BYTE bKeyAlgorithm,
 *                                               BYTE bKeyReference,
 *                                               BYTE bRndSize,
 *                                               const BYTE abRndHost2[],
 *                                               BYTE abRndCard2[],
 *                                               const BYTE abKeyValue[],
 *                                               WORD *SW);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_IsoInternalAuthenticate(SPROX_INSTANCE rInst,
 *                                                BYTE bKeyAlgorithm,
 *                                                BYTE bKeyReference,
 *                                                BYTE bRndSize,
 *                                                const BYTE abRndHost2[],
 *                                                BYTE abRndCard2[],
 *                                                const BYTE abKeyValue[],
 *                                                WORD *SW);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_IsoInternalAuthenticate(SCARDHANDLE hCard,
 *                                              BYTE bKeyAlgorithm,
 *                                              BYTE bKeyReference,
 *                                              BYTE bRndSize,
 *                                              const BYTE abRndHost2[],
 *                                              BYTE abRndCard2[],
 *                                              const BYTE abKeyValue[],
 *                                              WORD *SW);
 *
 * INPUTS
 *   BYTE bKeyAlgorithm         : algorithm to be used:
 *                                - 0x02 : DES or 3DES2K (16-byte key)
 *                                - 0x04 : 3DES3K (24-byte key)
 *                                - 0x09 : AES (16-byte key)
 *   BYTE bKeyReference         : reference to the key in the card
 *                                - 0x00 : card's master key (valid only on root application)
 *                                - 0x8n : application's key #n 
 *   BYTE bRndSize              : size of the challenge
 *                                (8 bytes for DES/3DES2K, 16 bytes for 3DES3K or AES)
 *   const BYTE abRndHost2[]    : host's second challenge (choosen by the caller - not involved in session key)
 *   BYTE abRndCard2[]          : card's second challenge (choosen by the card - involved in session key)
 *   const BYTE abKeyValue [16] : the key itself
 *   WORD *SW                   : optional pointer to retrieve the Status Word in
 *                                case an error occurs
 *
 * RETURNS
 *   DF_OPERATION_OK    : success
 *   Other code if internal or communication error has occured. 
 *
 * SEE ALSO
 *   IsoMutualAuthenticate
 *   IsoGetChallenge
 *   IsoExternalAuthenticate
 *
 **/
SPROX_API_FUNC(Desfire_IsoInternalAuthenticate) (SPROX_PARAM  BYTE bKeyAlgorithm, BYTE bKeyReference, BYTE bRndSize, const BYTE abRndHost2[], BYTE abRndCard2[], const BYTE abKeyValue[], WORD *SW)
{
  SPROX_RC status;
  WORD length;
  BYTE buffer[32];
	DWORD t;
	WORD _SW;
  SPROX_DESFIRE_GET_CTX();

	if (SW == NULL)
	  SW = &_SW;

	if (abRndHost2 == NULL)
	  return DFCARD_LIB_CALL_ERROR;

	status = SPROX_API_CALL(Desfire_IsoApdu) (SPROX_PARAM_P  0x88, bKeyAlgorithm, bKeyReference, bRndSize, abRndHost2, (BYTE) (2 * bRndSize), buffer, &length, SW);
	if (status != DF_OPERATION_OK)
	{
		if (status == DFCARD_PCSC_BAD_RESP_SW)
			status = TranslateSW(*SW, SW_AUTHENT);

	  return status;
	}

  if (abKeyValue != NULL)
	{
	  status = Desfire_IsoLoadKey(SPROX_PARAM_P  bKeyAlgorithm, abKeyValue);
		if (status != DF_OPERATION_OK)
			return status;
	}

	t = 2 * bRndSize;
	Desfire_CipherRecv(SPROX_PARAM_P  buffer, &t);

  if (memcmp(abRndHost2, &buffer[bRndSize], bRndSize))
	  status = DFCARD_WRONG_KEY;

	if (abRndCard2 != NULL)
	  memcpy(abRndCard2, &buffer[0], bRndSize);

  return status;
}

/**f* DesfireAPI/IsoMutualAuthenticate
 *
 * NAME
 *   IsoMutualAuthenticate
 *
 * DESCRIPTION
 *   Perform a mutual-authentication using the Desfire ISO 7816-4 commands (IsoGetChallenge,
 *   IsoExternalAuthenticate, IsoInternalAuthenticate) using the specified key value.
 *   Depending on bKeyAlgorithm, the key is either DES/3DES2K (16 bytes), AES (16 bytes)
 *   or 3DES3K (24 bytes).
 *   The generated session key is afterwards used for ISO CMACing.
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_IsoMutualAuthenticate(BYTE bKeyAlgorithm,
 *                                             BYTE bKeyReference,
 *                                             const BYTE abKeyValue[],
 *                                             WORD *SW);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_IsoMutualAuthenticate(SPROX_INSTANCE rInst,
 *                                              BYTE bKeyAlgorithm,
 *                                              BYTE bKeyReference,
 *                                              const BYTE abKeyValue[],
 *                                              WORD *SW);
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_IsoMutualAuthenticate(SCARDHANDLE hCard,
 *                                            BYTE bKeyAlgorithm,
 *                                            BYTE bKeyReference,
 *                                            const BYTE abKeyValue[],
 *                                            WORD *SW);
 *
 * INPUTS
 *   BYTE bKeyAlgorithm         : algorithm to be used:
 *                                - 0x02 : DES or 3DES2K (16-byte key)
 *                                - 0x04 : 3DES3K (24-byte key)
 *                                - 0x09 : AES (16-byte key)
 *   BYTE bKeyReference         : reference to the key in the card
 *                                - 0x00 : card's master key (valid only on root application)
 *                                - 0x8n : application's key #n 
 *   const BYTE abKeyValue [16] : the key itself
 *   WORD *SW                   : optional pointer to retrieve the Status Word in
 *                                case an error occurs
 *
 * RETURNS
 *   DF_OPERATION_OK    : authentication succeed
 *   Other code if internal or communication error has occured. 
 *
 * SEE ALSO
 *   Authenticate
 *   AuthenticateAes
 *   AuthenticateIso24
 *   AuthenticateIso
 *   IsoGetChallenge
 *   IsoExternalAuthenticate
 *   IsoInternalAuthenticate
 *
 **/
SPROX_API_FUNC(Desfire_IsoMutualAuthenticate) (SPROX_PARAM  BYTE bKeyAlgorithm, BYTE bKeyReference, const BYTE abKeyValue[], WORD *SW)
{
  SPROX_RC status;
  WORD _SW;

	BYTE bRndSize;
	BYTE abRndCard1[16], abRndCard2[16];
	BYTE abRndHost1[16], abRndHost2[16];

	SPROX_DESFIRE_GET_CTX();

	if (SW == NULL) SW = &_SW;

	switch (bKeyAlgorithm)
	{
	  case DF_ISO_CIPHER_2KDES : bRndSize = 8; break;
	  case DF_ISO_CIPHER_3KDES : bRndSize = 16; break;
	  case DF_ISO_CIPHER_AES   : bRndSize = 16; break;
		default                  : return DFCARD_LIB_CALL_ERROR;
  }

	if (abKeyValue == NULL)
	  return DFCARD_LIB_CALL_ERROR;

  ctx->session_key_id = bKeyReference & 0x7F;

  /* Get random bytes to populate host's challenges */
	GetRandomBytes(SPROX_PARAM_P  abRndHost1, bRndSize);
	GetRandomBytes(SPROX_PARAM_P  abRndHost2, bRndSize);

  /* Get first challenge from card */
	status = SPROX_API_CALL(Desfire_IsoGetChallenge) (SPROX_PARAM_P  bRndSize, abRndCard1, SW);
  if (status != DF_OPERATION_OK)
	  return status;

  /* Activate the cipher engine */
  Desfire_CleanupAuthentication(SPROX_PARAM_PV);
	status = Desfire_IsoLoadKey(SPROX_PARAM_P  bKeyAlgorithm, abKeyValue);
	if (status != DF_OPERATION_OK)
		return status;

  /* External authenticate feeds the card with first challenge from the host */
	status = SPROX_API_CALL(Desfire_IsoExternalAuthenticate) (SPROX_PARAM_P  bKeyAlgorithm, bKeyReference, bRndSize, abRndCard1, abRndHost1, NULL, SW);
  if (status != DF_OPERATION_OK)
	  return status;

  /* Internal authenticate returns second challenge from the card */
	status = SPROX_API_CALL(Desfire_IsoInternalAuthenticate) (SPROX_PARAM_P  bKeyAlgorithm, bKeyReference, bRndSize, abRndHost2, abRndCard2, NULL, SW);
  if (status != DF_OPERATION_OK)
	  return status;

  /* Compute the session key over host's first challenge and card's second challenge */
  if (bKeyAlgorithm == DF_ISO_CIPHER_2KDES)
	{
	  if (!memcmp(&abKeyValue[0], &abKeyValue[8], 8))
		{
      /* Single DES key */
			memcpy(&ctx->session_key[0],  &abRndHost1[0], 4);
			memcpy(&ctx->session_key[4],  &abRndCard2[0], 4);
			memcpy(&ctx->session_key[8],  &ctx->session_key[0], 8);
			memcpy(&ctx->session_key[16], &ctx->session_key[0], 8);

			Desfire_InitCrypto3Des(SPROX_PARAM_P  &ctx->session_key[0], NULL, NULL);

		} else
		{
		  /* Triple DES with 2 keys */
			memcpy(&ctx->session_key[0],  &abRndHost1[0], 4);
			memcpy(&ctx->session_key[4],  &abRndCard2[0], 4);
			memcpy(&ctx->session_key[8],  &abRndHost1[4], 4);
			memcpy(&ctx->session_key[12], &abRndCard2[4], 4);
			memcpy(&ctx->session_key[16], &ctx->session_key[0], 8);

			Desfire_InitCrypto3Des(SPROX_PARAM_P  &ctx->session_key[0], &ctx->session_key[8], NULL);

		}
	} else
	if (bKeyAlgorithm == DF_ISO_CIPHER_3KDES)
	{
    /* Triple DES with 3 keys */
		memcpy(&ctx->session_key[0],  &abRndHost1[0],  4);
		memcpy(&ctx->session_key[4],  &abRndCard2[0],  4);
		memcpy(&ctx->session_key[8],  &abRndHost1[6],  4);
		memcpy(&ctx->session_key[12], &abRndCard2[6],  4);
		memcpy(&ctx->session_key[16], &abRndHost1[12], 4);
		memcpy(&ctx->session_key[20], &abRndCard2[12], 4);

		Desfire_InitCrypto3Des(SPROX_PARAM_P  &ctx->session_key[0], &ctx->session_key[8], &ctx->session_key[16]);

	} else
	if (bKeyAlgorithm == DF_ISO_CIPHER_AES)
	{
    /* AES */
		memcpy(&ctx->session_key[0],  &abRndHost1[0],  4);
		memcpy(&ctx->session_key[4],  &abRndCard2[0],  4);
		memcpy(&ctx->session_key[8],  &abRndHost1[12], 4);
		memcpy(&ctx->session_key[12], &abRndCard2[12], 4);
		    
		Desfire_InitCryptoAes(SPROX_PARAM_P  ctx->session_key);
  }

  Desfire_CleanupInitVector(SPROX_PARAM_PV);

  /* Initialize the CMAC calculator */
  Desfire_InitCmac(SPROX_PARAM_PV);

  return DF_OPERATION_OK;
}
