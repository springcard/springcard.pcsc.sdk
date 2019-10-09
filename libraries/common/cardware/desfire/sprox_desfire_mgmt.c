/**h* DesfireAPI/Management
 *
 * NAME
 *   DesfireAPI :: Card management functions
 *
 * COPYRIGHT
 *   (c) 2009 SpringCard - www.springcard.com
 *
 * DESCRIPTION
 *   Implementation of management functions to personalize or format the
 *   DESFIRE card.
 *
 **/
#include "sprox_desfire_i.h"

/**f* DesfireAPI/GetFreeMemory
 *
 * NAME
 *   GetFreeMemory
 *
 * DESCRIPTION
 *   Reads out the number of available bytes on the PICC
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_GetFreeMemory(DWORD *pdwFreeBytes);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_GetFreeMemory(SPROX_INSTANCE rInst,
 *                                      DWORD *pdwFreeBytes);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_GetFreeMemory(SCARDHANDLE hCard,
 *                                    DWORD *pdwFreeBytes);
 *
 * INPUTS
 *   DWORD *pdwFreeBytes : number of free bytes on the PICC
 *
 * RETURNS
 *   DF_OPERATION_OK : operation succeeded
 *   Other code if internal or communication error has occured.
 *
 * NOTES
 *   This command can be issued without valid authentication.
 *
 **/
SPROX_API_FUNC(Desfire_GetFreeMemory) (SPROX_PARAM  DWORD *pdwFreeBytes)
{
  SPROX_RC   status;
  SPROX_DESFIRE_GET_CTX();

  /* Create the info block containing the command code and the key number argument. */
  ctx->xfer_buffer[INF + 0] = DF_GET_FREE_MEMORY;
  ctx->xfer_length = 1;

  /* Communicate the info block to the card and check the operation's return status. */
  /* The returned info block must contain 4 bytes, the status code and the requested */
  /* information.                                                                    */
  status = SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  4, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
  if (status != DF_OPERATION_OK)
    return status;

  /* Get the actual value. */
  if (pdwFreeBytes != NULL)
  {
    *pdwFreeBytes  = 0;
    *pdwFreeBytes += ctx->xfer_buffer[INF + 3]; *pdwFreeBytes <<= 8;
    *pdwFreeBytes += ctx->xfer_buffer[INF + 2]; *pdwFreeBytes <<= 8;
    *pdwFreeBytes += ctx->xfer_buffer[INF + 1];
  }

  /* Success. */
  return DF_OPERATION_OK;
}

/**f* DesfireAPI/GetCardUID
 *
 * NAME
 *   GetCardUID
 *
 * DESCRIPTION
 *   Reads out the 7-byte serial number of the PICC
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_GetCardUID(BYTE uid[7]);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_GetCardUID(SPROX_INSTANCE rInst,
 *                                   BYTE uid[7]);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_GetCardUID(SCARDHANDLE hCard,
 *                                 BYTE uid[7]);
 *
 *
 * RETURNS
 *   DF_OPERATION_OK : operation succeeded
 *   Other code if internal or communication error has occured.
 *
 * NOTES
 *   This command must be preceded by an authentication (with any key).
 *
 **/
SPROX_API_FUNC(Desfire_GetCardUID) (SPROX_PARAM  BYTE uid[7])
{
  SPROX_RC   status;
  SPROX_DESFIRE_GET_CTX();

  /* Create the info block containing the command code and the key number argument. */
  ctx->xfer_buffer[INF + 0] = DF_GET_CARD_UID;
  ctx->xfer_length = 1;

  /* Communicate the info block to the card and check the operation's return status. */
  /* The returned info block must contain 4 bytes, the status code and the requested */
  /* information.                                                                    */
  status = SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  17, COMPUTE_COMMAND_CMAC | WANTS_OPERATION_OK);
  if (status != DF_OPERATION_OK)
    return status;

#ifdef SPROX_DESFIRE_WITH_SAM
  if (ctx->sam_session_active)
  {
    BYTE pbOut[30];
    DWORD dwOutLength = sizeof(pbOut);
    BOOL bSkipStatus = FALSE;
    if ((ctx->session_type == KEY_LEGACY_3DES) || (ctx->session_type == KEY_LEGACY_DES))
     bSkipStatus = TRUE;

    status = SAM_DecipherData(ctx->sam_context.hSam, ctx->xfer_buffer[0], 7, &ctx->xfer_buffer[1], ctx->xfer_length-1, bSkipStatus, pbOut, &dwOutLength);
    if (status != DF_OPERATION_OK)
      return status;

     /* Get the actual value. */
    if (uid != NULL)
      memcpy(uid, pbOut, 7);

  } else
#endif
  {
    /* at first we have to decipher the recv_buffer */
    ctx->xfer_length -= 1;

    Desfire_CipherRecv(SPROX_PARAM_P  &ctx->xfer_buffer[1], &ctx->xfer_length);

   /* Get the actual value. */
    if (uid != NULL)
      memcpy(uid, &ctx->xfer_buffer[1], 7);

  }




  /* Success. */
  return DF_OPERATION_OK;

}



/**f* DesfireAPI/FormatPICC
 *
 * NAME
 *   FormatPICC
 *
 * DESCRIPTION
 *   Releases the DesFire card user memory
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_FormatPICC(void);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_FormatPICC(SPROX_INSTANCE rInst);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_FormatPICC(SCARDHANDLE hCard);
 *
 * RETURNS
 *   DF_OPERATION_OK    : format succeeded
 *   Other code if internal or communication error has occured.
 *
 * NOTES
 *   All applications are deleted and all files within those applications  are deleted.
 *   This command always requires a preceding authentication with the DesFire card master key.
 *
 **/
SPROX_API_FUNC(Desfire_FormatPICC) (SPROX_PARAM_V)
{
  SPROX_DESFIRE_GET_CTX();

  /* Create the info block containing the command code. */
  ctx->xfer_buffer[INF + 0] = DF_FORMAT_PICC;
  ctx->xfer_length = 1;

  /* Communicate the info block to the card and check the operation's return status. */
  return SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  0, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
}

/**f* DesfireAPI/CreateApplication
 *
 * NAME
 *   CreateApplication
 *
 * DESCRIPTION
 *   Create a new application on the DesFire card
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_CreateApplication(DWORD aid,
 *                                         BYTE key_setting_1,
 *                                         BYTE key_setting_2);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_CreateApplication(SPROX_INSTANCE rInst,
 *                                          DWORD aid,
 *                                          BYTE key_setting_1,
 *                                          BYTE key_setting_2);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_CreateApplication(SCARDHANDLE hCard,
 *                                        DWORD aid,
 *                                        BYTE key_setting_1,
 *                                        BYTE key_setting_2);
 *
 * INPUTS
 *   DWORD aid          : Application IDentifier
 *   BYTE key_setting_1 : Settings of the Application master key (see chapter 4.3.2 of datasheet
 *                        of mifare DesFire MF3ICD40 for more information)
 *   BYTE key_setting_2 : Number of keys that can be stored within the application for
 *                        cryptographic purposes, plus flags to specify cryptographic method and
 *                        to enable giving ISO names to the EF.
 *
 * RETURNS
 *   DF_OPERATION_OK : application created successfully
 *   Other code if internal or communication error has occured.
 *
 * SEE ALSO
 *   CreateApplicationIso
 *   DeleteApplication
 *   GetApplicationIDs
 *   SelectApplication
 *
 **/
SPROX_API_FUNC(Desfire_CreateApplication) (SPROX_PARAM  DWORD aid, BYTE key_setting_1, BYTE key_setting_2)
{
  SPROX_DESFIRE_GET_CTX();

  /* Create the info block containing the command code and the given parameters. */
  ctx->xfer_length = 0;
  ctx->xfer_buffer[ctx->xfer_length++] = DF_CREATE_APPLICATION;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (aid & 0x000000FF);
  aid >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (aid & 0x000000FF);
  aid >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (aid & 0x000000FF);
  ctx->xfer_buffer[ctx->xfer_length++] = key_setting_1;
  ctx->xfer_buffer[ctx->xfer_length++] = key_setting_2;

  /* Communicate the info block to the card and check the operation's return status. */
  return SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  0, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
}

/**f* DesfireAPI/CreateIsoApplication
 *
 * NAME
 *   CreateIsoApplication
 *
 * DESCRIPTION
 *   Create a new application on the DesFire card, and defines the ISO identifier
 *   and name of the application
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_CreateIsoApplication(DWORD aid,
 *                                            BYTE key_setting_1,
 *                                            BYTE key_setting_2,
 *                                            WORD iso_df_id,
 *                                            const BYTE iso_df_name[],
 *                                            BYTE iso_df_namelen);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_CreateIsoApplication(SPROX_INSTANCE rInst,
 *                                             DWORD aid,
 *                                             BYTE key_setting_1,
 *                                             BYTE key_setting_2,
 *                                             WORD iso_df_id,
 *                                             const BYTE iso_df_name[],
 *                                             BYTE iso_df_namelen);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_CreateIsoApplication(SCARDHANDLE hCard,
 *                                           DWORD aid,
 *                                           BYTE key_setting_1,
 *                                           BYTE key_setting_2,
 *                                           WORD iso_df_id,
 *                                           const BYTE iso_df_name[],
 *                                           BYTE iso_df_namelen);
 *
 * INPUTS
 *   DWORD aid                : Application IDentifier
 *   BYTE key_setting_1       : Settings of the Application master key (see chapter 4.3.2 of datasheet
 *                              of mifare DesFire MF3ICD40 for more information)
 *   BYTE key_setting_2       : Number of keys that can be stored within the application for
 *                              cryptographic purposes, plus flags to specify cryptographic method and
 *                              to enable giving ISO names to the EF.
 *   BYTE iso_df_id           : ID of the ISO DF
 *   const BYTE iso_df_name[] : name of the ISO DF
 *   BYTE iso_df_namelen      : length of iso_df_name
 *
 * RETURNS
 *   DF_OPERATION_OK : application created successfully
 *   Other code if internal or communication error has occured.
 *
 * SEE ALSO
 *   CreateApplication
 *   DeleteApplication
 *   GetApplicationIDs
 *   SelectApplication
 *
 **/
SPROX_API_FUNC(Desfire_CreateIsoApplication) (SPROX_PARAM  DWORD aid, BYTE key_setting_1, BYTE key_setting_2, WORD iso_df_id, const BYTE iso_df_name[16], BYTE iso_df_namelen)
{
  SPROX_DESFIRE_GET_CTX();

  ctx->xfer_length = 0;

  /* Create the info block containing the command code and the given parameters. */
  ctx->xfer_buffer[ctx->xfer_length++] = DF_CREATE_APPLICATION;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (aid & 0x000000FF);
  aid >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (aid & 0x000000FF);
  aid >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (aid & 0x000000FF);
  ctx->xfer_buffer[ctx->xfer_length++] = key_setting_1;
  ctx->xfer_buffer[ctx->xfer_length++] = key_setting_2;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (iso_df_id); // JDA
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (iso_df_id >> 8); // JDA

  if (iso_df_name != NULL)
  {
    if (iso_df_namelen == 0)
      iso_df_namelen = strlen((char*)iso_df_name);
    if (iso_df_namelen > 16)
      return DFCARD_LIB_CALL_ERROR;

    memcpy(&ctx->xfer_buffer[ctx->xfer_length], iso_df_name, iso_df_namelen);
    ctx->xfer_length += iso_df_namelen;
  }

  /* Communicate the info block to the card and check the operation's return status. */
  return SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  0, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
}

/**f* DesfireAPI/DeleteApplication
 *
 * NAME
 *   DeleteApplication
 *
 * DESCRIPTION
 *   Permanently deactivates an application on the DesFire card
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_DeleteApplication(DWORD aid);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_DeleteApplication(SPROX_INSTANCE rInst,
 *                                     DWORD aid);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_DeleteApplication(SCARDHANDLE hCard,
 *                                     DWORD aid);
 *
 * INPUTS
 *   DWORD aid                   : Application IDentifier
 *
 * RETURNS
 *   DF_OPERATION_OK    : application deleted successfully
 *   Other code if internal or communication error has occured.
 *
 * SEE ALSO
 *   CreateApplication
 *   GetApplicationIDs
 *   SelectApplication
 *
 **/
SPROX_API_FUNC(Desfire_DeleteApplication) (SPROX_PARAM  DWORD aid)
{
  SPROX_RC status;
  SPROX_DESFIRE_GET_CTX();

  /* Create the info block containing the command code and the given parameters. */
  ctx->xfer_length = 0;
  ctx->xfer_buffer[ctx->xfer_length++] = DF_DELETE_APPLICATION;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (aid & 0x000000FF); aid >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (aid & 0x000000FF); aid >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (aid & 0x000000FF);

  /* Communicate the info block to the card and check the operation's return status. */
  status = SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  0, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | LOOSE_RESPONSE_CMAC | WANTS_OPERATION_OK);

  /* If the current application is deleted, the root application is implicitly selected */
  if ((status == DF_OPERATION_OK) && (ctx->current_aid == aid))
  {
    ctx->current_aid = 0;
    Desfire_CleanupAuthentication(SPROX_PARAM_PV);
  }

  return status;
}

/**f* DesfireAPI/GetApplicationIDs
 *
 * NAME
 *   GetApplicationIDs
 *
 * DESCRIPTION
 *   Returns the Application IDentifiers of all active applications on a DesFire card
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_GetApplicationIDs(BYTE aid_max_count,
 *                                     DWORD aid_list[],
 *                                     BYTE *aid_count);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_GetApplicationIDs(SPROX_INSTANCE rInst,
 *                                     BYTE aid_max_count,
 *                                     DWORD aid_list[],
 *                                     BYTE *aid_count);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_GetApplicationIDs(SCARDHANDLE hCard,
 *                                     BYTE aid_max_count,
 *                                     DWORD aid_list[],
 *                                     BYTE *aid_count);
 *
 * INPUTS
 *   BYTE aid_max_count          : maximum number of Application IDentifiers
 *   DWORD aid_list[]            : Application IDentifier list
 *   BYTE *aid_count             : number of Application IDentifiers on DesFire card
 *
 * RETURNS
 *   DF_OPERATION_OK    : operation succeeded
 *   Other code if internal or communication error has occured.
 *
 * SEE ALSO
 *   CreateApplication
 *   DeleteApplication
 *   SelectApplication
 *
 **/
SPROX_API_FUNC(Desfire_GetApplicationIDs) (SPROX_PARAM  BYTE aid_max_count, DWORD aid_list[], BYTE *aid_count)
{
  BYTE       i;
  DWORD      recv_length = 1;
  BYTE       recv_buffer[256];
  SPROX_RC   status;
  SPROX_DESFIRE_GET_CTX();

  /* Set the number of applications to zero */
  if (aid_count != NULL)
    *aid_count = 0;

  /* create the info block containing the command code */
  ctx->xfer_length = 0;
  ctx->xfer_buffer[ctx->xfer_length++] = DF_GET_APPLICATION_IDS;

  for (;;)
  {
    status = SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  0, COMPUTE_COMMAND_CMAC | WANTS_ADDITIONAL_FRAME | WANTS_OPERATION_OK);

    if (status != DF_OPERATION_OK)
      goto done;

    memcpy(&recv_buffer[recv_length], &ctx->xfer_buffer[INF + 1], ctx->xfer_length - 1);
    recv_length += (ctx->xfer_length - 1);

    if (ctx->xfer_buffer[INF + 0] != DF_ADDITIONAL_FRAME)
      break;

    ctx->xfer_length = 1;
  }

  recv_buffer[0] = DF_OPERATION_OK;

  /* Check the CMAC */
  status = Desfire_VerifyCmacRecv(SPROX_PARAM_P  recv_buffer, &recv_length);
  if (status != DF_OPERATION_OK)
    goto done;


  recv_length -= 1;     /* substract 1 byte for the received status */

  /* ByteCount must be in multiples of APPLICATION_ID_SIZE bytes
     we check this to proof the format integrity
     if zero bytes have been received this is ok -> means no
     applications existing */
  if (recv_length % APPLICATION_ID_SIZE)
  {
    status = DFCARD_WRONG_LENGTH;
    goto done;
  }

  for (i = 0; i < (recv_length / APPLICATION_ID_SIZE); i++)
  {
    /* Extract AID */
    if ((aid_list != NULL) && (i < aid_max_count))
		{
		  DWORD aid;

      aid = recv_buffer[INF + 3 + APPLICATION_ID_SIZE * i];
      aid <<= 8;
      aid += recv_buffer[INF + 2 + APPLICATION_ID_SIZE * i];
      aid <<= 8;
      aid += recv_buffer[INF + 1 + APPLICATION_ID_SIZE * i];

      aid_list[i] = aid;
		}
  }

  if (aid_count != NULL)
    *aid_count = i;

	if ((aid_list != NULL) && (i >= aid_max_count))
		status = DFCARD_OVERFLOW;

done:
  return status;
}

/**f* DesfireAPI/Desfire_GetIsoApplications
 *
 * NAME
 *   Desfire_GetIsoApplications
 *
 * DESCRIPTION
 *   Returns the Application IDentifiers, ISO DF IDs and ISO DF Names of all active
 *   applications on a DesFire card having an ISO DF ID / DF Name
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_GetIsoApplications(BYTE app_max_count,
 *                                          DF_ISO_APPLICATION_ST app_list[],
 *                                          BYTE *app_count);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_GetIsoApplications(SPROX_INSTANCE rInst,
 *                                           BYTE app_max_count,
 *                                           DF_ISO_APPLICATION_ST app_list[],
 *                                           BYTE *app_count);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_GetIsoApplications(SCARDHANDLE hCard,
 *                                         BYTE app_max_count,
 *                                         DF_ISO_APPLICATION_ST app_list[],
 *                                         BYTE *app_count);
 *
 * INPUTS
 *   BYTE app_max_count               : maximum number of Applications
 *   DF_ISO_APPLICATION_ST app_list[] : list of Applications
 *   BYTE *app_count                  : number of Applications on DesFire card
 *
 * RETURNS
 *   DF_OPERATION_OK    : operation succeeded
 *   Other code if internal or communication error has occured.
 *
 * SEE ALSO
 *   CreateIsoApplication
 *   GetApplicationIDs
 *
 **/
SPROX_API_FUNC(Desfire_GetIsoApplications) (SPROX_PARAM  BYTE app_max_count, DF_ISO_APPLICATION_ST app_list[], BYTE *app_count)
{
  BYTE       i;
  DWORD      recv_length = 1;
  BYTE       recv_buffer[1024];
  SPROX_RC   status;
  SPROX_DESFIRE_GET_CTX();

  /* Set the number of applications to zero */
  if (app_count != NULL)
    *app_count = 0;

  /* create the info block containing the command code */
  ctx->xfer_length = 0;
  ctx->xfer_buffer[ctx->xfer_length++] = DF_GET_DF_NAMES;

	i = 0;
  for (;;)
  {
    status = SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  0, COMPUTE_COMMAND_CMAC | WANTS_ADDITIONAL_FRAME | WANTS_OPERATION_OK);

    if (status != DF_OPERATION_OK)
      goto done;

		if ( (ctx->xfer_length !=1) && (ctx->xfer_length<6))
		{
			status = DFCARD_WRONG_LENGTH;
			goto done;
		}

		/* Extract application data */
		if ((app_list != NULL) && (i < app_max_count))
		{
			DF_ISO_APPLICATION_ST app;

			app.dwAid = ctx->xfer_buffer[INF + 3];
			app.dwAid <<= 8;
			app.dwAid += ctx->xfer_buffer[INF + 2];
			app.dwAid <<= 8;
			app.dwAid += ctx->xfer_buffer[INF + 1];

			app.wIsoId = ctx->xfer_buffer[INF + 5];
			app.wIsoId <<= 8;
			app.wIsoId += ctx->xfer_buffer[INF + 4];

			for (app.bIsoNameLen = 0; app.bIsoNameLen < sizeof(app.abIsoName); app.bIsoNameLen++)
			{
				if ((WORD) (INF + 6 + app.bIsoNameLen) >= ctx->xfer_length)
					break;
				app.abIsoName[app.bIsoNameLen] = ctx->xfer_buffer[INF + 6 + app.bIsoNameLen];
			}

			memcpy(&app_list[i], &app, sizeof(DF_ISO_APPLICATION_ST));
		}
		i++;

		/* Remember the frame for later CMAC processing */
    memcpy(&recv_buffer[recv_length], &ctx->xfer_buffer[INF + 1], ctx->xfer_length - 1);
    recv_length += (ctx->xfer_length - 1);

    if (ctx->xfer_buffer[INF + 0] != DF_ADDITIONAL_FRAME)
      break;

    ctx->xfer_length = 1;
  }

  recv_buffer[0] = DF_OPERATION_OK;

  /* Check the CMAC */
  status = Desfire_VerifyCmacRecv(SPROX_PARAM_P  recv_buffer, &recv_length);
  if (status != DF_OPERATION_OK)
    goto done;

  recv_length -= 1;     /* substract 1 byte for the received status */

  if (app_count != NULL)
    *app_count = i;

	if ((app_list != NULL) && (i >= app_max_count))
		status = DFCARD_OVERFLOW;

done:
  return status;
}

/**f* DesfireAPI/SelectApplication
 *
 * NAME
 *   SelectApplication
 *
 * DESCRIPTION
 *   Selects one specific application for further access
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_SelectApplication(DWORD aid);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_SelectApplication(SPROX_INSTANCE rInst,
 *                                     DWORD aid);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_SelectApplication(SCARDHANDLE hCard,
 *                                     DWORD aid);
 *
 * INPUTS
 *   DWORD aid                   : Application IDentifier
 *
 * RETURNS
 *   DF_OPERATION_OK    : application selected
 *   Other code if internal or communication error has occured.
 *
 * SEE ALSO
 *   CreateApplication
 *   DeleteApplication
 *   GetApplicationIDs
 *
 **/
SPROX_API_FUNC(Desfire_SelectApplication) (SPROX_PARAM  DWORD aid)
{
  SPROX_RC status;
  SPROX_DESFIRE_GET_CTX();

  /* Each SelectApplication causes a currently valid authentication state to be lost */
  Desfire_CleanupAuthentication(SPROX_PARAM_PV);

  /* Create the info block containing the command code and the given parameters. */
  ctx->xfer_buffer[INF + 0] = DF_SELECT_APPLICATION;
  ctx->xfer_buffer[INF + 1] = (BYTE) (aid & 0x000000FF);
  aid >>= 8;
  ctx->xfer_buffer[INF + 2] = (BYTE) (aid & 0x000000FF);
  aid >>= 8;
  ctx->xfer_buffer[INF + 3] = (BYTE) (aid & 0x000000FF);
  ctx->xfer_length = 4;

  /* Communicate the info block to the card and check the operation's return status. */
  status = SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  0, WANTS_OPERATION_OK);

  if (status == DF_OPERATION_OK)
    ctx->current_aid = aid;

  return status;
}

/**t* DesfireAPI/DF_VERSION_INFO
 *
 * NAME
 *   DF_VERSION_INFO
 *
 * DESCRIPTION
 *   Structure for returning the information supplied by the GetVersion command.
 *
 * SOURCE
 *   typedef struct
 *   {
 *     // hardware related information
 *     BYTE    bHwVendorID;     // vendor ID (0x04 for NXP)
 *     BYTE    bHwType;         // type (0x01)
 *     BYTE    bHwSubType;      // subtype (0x01)
 *     BYTE    bHwMajorVersion; // major version number
 *     BYTE    bHwMinorVersion; // minor version number
 *     BYTE    bHwStorageSize;  // storage size (0x18 = 4096 bytes)
 *     BYTE    bHwProtocol;     // communication protocol type (0x05 meaning ISO 14443-2 and -3)

 *     // software related information
 *     BYTE    bSwVendorID;     // vendor ID (0x04 for NXP)
 *     BYTE    bSwType;         // type (0x01)
 *     BYTE    bSwSubType;      // subtype (0x01)
 *     BYTE    bSwMajorVersion; // major version number
 *     BYTE    bSwMinorVersion; // minor version number
 *     BYTE    bSwStorageSize;  // storage size (0x18 = 4096 bytes)
 *     BYTE    bSwProtocol;     // communication protocol type (0x05 meaning ISO 14443-3 and -4)
 *
 *     BYTE    abUid[7];        // unique serial number
 *     BYTE    abBatchNo[5];    // production batch number
 *     BYTE    bProductionCW;   // calendar week of production
 *     BYTE    bProductionYear; // year of production
 *   } DF_VERSION_INFO;
 *
 **/

/**f* DesfireAPI/GetVersion
 *
 * NAME
 *   GetVersion
 *
 * DESCRIPTION
 *   Returns manufacturing related data of the DesFire card
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_GetVersion(DF_VERSION_INFO *pVersionInfo);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_GetVersion(SPROX_INSTANCE rInst,
 *                                     DF_VERSION_INFO *pVersionInfo);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_GetVersion(SCARDHANDLE hCard,
 *                                     DF_VERSION_INFO *pVersionInfo);
 *
 * INPUTS
 *   DF_VERSION_INFO *pVersionInfo : card's version information
 *
 * RETURNS
 *   DF_OPERATION_OK    : operation succeeded
 *   Other code if internal or communication error has occured.
 *
 **/
SPROX_API_FUNC(Desfire_GetVersion) (SPROX_PARAM  DF_VERSION_INFO *pVersionInfo)
{
  DWORD      recv_length = 1;
  BYTE       recv_buffer[256];
  SPROX_RC   status;
  SPROX_DESFIRE_GET_CTX();

  if (pVersionInfo != NULL)
    memset(pVersionInfo, 0, sizeof(DF_VERSION_INFO));

  /* create the info block containing the command code */
  ctx->xfer_length = 0;
  ctx->xfer_buffer[ctx->xfer_length++] = DF_GET_VERSION;

  for (;;)
  {
    status = SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  0, COMPUTE_COMMAND_CMAC | WANTS_ADDITIONAL_FRAME | WANTS_OPERATION_OK);
    if (status != DF_OPERATION_OK)
      goto done;

    memcpy(&recv_buffer[recv_length], &ctx->xfer_buffer[INF + 1], ctx->xfer_length - 1);

    recv_length += (ctx->xfer_length - 1);

    if (ctx->xfer_buffer[INF + 0] != DF_ADDITIONAL_FRAME)
      break;

    ctx->xfer_length = 1;
  }

  recv_buffer[0] = DF_OPERATION_OK;

  /* Check the CMAC */
  status = Desfire_VerifyCmacRecv(SPROX_PARAM_P  recv_buffer, &recv_length);

  if (status != DF_OPERATION_OK)
    goto done;

  recv_length -= 1;     /* substract 1 byte for the received status */

  if (recv_length != sizeof(DF_VERSION_INFO))
  {
    status = DFCARD_WRONG_LENGTH;
    goto done;
  }

  if (pVersionInfo != NULL)
    memcpy(pVersionInfo, &recv_buffer[1], sizeof(DF_VERSION_INFO));

done:
  return status;

}

/**f* DesfireAPI/SetConfiguration
 *
 * NAME
 *   SetConfiguration
 *
 * DESCRIPTION
 *   Sends the SetConfiguration command to the DESFIRE card.
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_SetConfiguration(BYTE option,
 *                                        const BYTE data[],
 *                                        BYTE length);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_SetConfiguration(SPROX_INSTANCE rInst,
 *                                         BYTE option,
 *                                         const BYTE data[],
 *                                         BYTE length);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_SetConfiguration(SCARDHANDLE hCard,
 *                                       BYTE option,
 *                                       const BYTE data[],
 *                                       BYTE length);
 *
 * INPUTS
 *
 * RETURNS
 *   DF_OPERATION_OK : operation succeeded
 *   Other code if internal or communication error has occured.
 *
 * NOTES
 *   Read DESFIRE EV1 manual, chapter 9.4.9 for details.
 *   DO NOT USE THIS COMMAND unless you're really sure you know
 *   what you're doing!!!
 *
 **/
SPROX_API_FUNC(Desfire_SetConfiguration) (SPROX_PARAM  BYTE option, const BYTE data[], BYTE length)
{
  SPROX_DESFIRE_GET_CTX();

  ctx->xfer_length = 0;

  /* Create the info block containing the command code and the key number argument. */
  ctx->xfer_buffer[ctx->xfer_length++] = DF_SET_CONFIGURATION;
  ctx->xfer_buffer[ctx->xfer_length++] = option;

  if (data != NULL)
  {
    memcpy(&ctx->xfer_buffer[ctx->xfer_length], data, length);
    ctx->xfer_length += length;
  }

  /* Add the CRC */
  Desfire_XferAppendCrc(SPROX_PARAM_P  2);

  /* Cipher the option, the data and the CRC */
  Desfire_XferCipherSend(SPROX_PARAM_P  2);

  return SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  1, CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
}
