/**h* DesfireAPI/Values
 *
 * NAME
 *   DesfireAPI :: Value file related functions
 *
 * COPYRIGHT
 *   (c) 2009 SpringCard - www.springcard.com
 *
 * DESCRIPTION
 *   Implementation of DESFIRE functions to handle value files (counters).
 *
 **/
#include "sprox_desfire_i.h"

/* DesfireAPI/ModifyValue
 *
 * NAME
 *   ModifyValue
 *
 * DESCRIPTION
 *   Allows to modify a value stored in a Value File.
 *
 * INPUTS
 *   BYTE modify_command : command to send,  DF_LIMITED_CREDIT, DF_CREDIT or DF_DEBIT
 *   BYTE file_id        : ID of the file
 *   BYTE comm_mode      : communication mode
 *   LONG amount         : amount to increase/decrease to the current value stored in the file. Only positive values allowed.
 *
 * RETURNS
 *   DF_OPERATION_OK     : success, data has been written
 *   Other code if internal or communication error has occured.
 *
 * SEE ALSO
 *   LimitedCredit
 *   LimitedCredit2
 *   Credit
 *   Credit2
 *   Debit
 *   Debit2
 *
 **/
SPROX_API_FUNC(Desfire_ModifyValue) (SPROX_PARAM  BYTE modify_command, BYTE file_id, BYTE comm_mode, LONG amount)
{
  BYTE comm_flags = CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK;
  SPROX_DESFIRE_GET_CTX();

  ctx->xfer_length = 0;

  /* Create the info block containing the command code, the file ID argument and the */
  /* modification amount argument.                                                   */
  ctx->xfer_buffer[ctx->xfer_length++] = modify_command;
  ctx->xfer_buffer[ctx->xfer_length++] = file_id;

  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (amount & 0x000000FF); amount >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (amount & 0x000000FF); amount >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (amount & 0x000000FF); amount >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (amount & 0x000000FF);

  /* Process the amount argument according to the selected communication mode. */
  if (comm_mode == DF_COMM_MODE_ENCIPHERED)
  {
#ifdef SPROX_DESFIRE_WITH_SAM
    if (ctx->sam_session_active)
    {
      DWORD i;
      BYTE pbOut[30];
      DWORD dwOutLength = sizeof(pbOut);

      if (ctx->session_type & KEY_ISO_MODE)
      {
        SAM_EncipherData(ctx->sam_context.hSam, ctx->xfer_buffer, ctx->xfer_length, 2, pbOut, &dwOutLength);
      } else
      {
        /* Non-ISO authenticated: don't send command */
        SAM_EncipherData(ctx->sam_context.hSam, &ctx->xfer_buffer[2], ctx->xfer_length-2, 0, pbOut, &dwOutLength);
      }

      ctx->xfer_length = 0;
      ctx->xfer_buffer[ctx->xfer_length++] = modify_command;
      ctx->xfer_buffer[ctx->xfer_length++] = file_id;
      for (i=0; i<dwOutLength; i++)
        ctx->xfer_buffer[ctx->xfer_length++] = pbOut[i];

    } else
#endif
    {
      /* Append a CRC to the amount argument. */
      Desfire_XferAppendCrc(SPROX_PARAM_P  2);
      /* 'Encrypt' the amount argument bytes and the CRC bytes */
      Desfire_XferCipherSend(SPROX_PARAM_P  2);
    }

  } else
  if (comm_mode == DF_COMM_MODE_MACED)
  {
    if (ctx->session_type & KEY_ISO_MODE)
    {
      /* Compute the CMAC, both ways */
      comm_flags |= COMPUTE_COMMAND_CMAC;
      comm_flags |= APPEND_COMMAND_CMAC;
    } else
    {
      /* Append a MAC to the amount argument */
#ifdef SPROX_DESFIRE_WITH_SAM
      if (ctx->sam_session_active)
      {
        BYTE pbMac[4];
        DWORD dwMacLength = sizeof(pbMac), k;

        LONG status = SAM_GenerateMAC(ctx->sam_context.hSam,&ctx->xfer_buffer[INF + 2], 4, pbMac, &dwMacLength);
        if (status == DF_OPERATION_OK)
        {
          for (k=0; k<dwMacLength; k++)
            ctx->xfer_buffer[INF + 6 + k] = pbMac[k];
          ctx->xfer_length += 4;
        } else
        {
          return status;
        }

      } else
#endif
      {
        Desfire_ComputeMac(SPROX_PARAM_P  &ctx->xfer_buffer[INF + 2], 4, &ctx->xfer_buffer[INF + 6]);
        ctx->xfer_length += 4;
      }
    }
  } else
  {
    if (ctx->session_type & KEY_ISO_MODE)
    {
      /* Check the CMAC in response */
      comm_flags |= COMPUTE_COMMAND_CMAC;
    }
  }

  /* Communicate the info block to the card and check the operation's return status. */
  return SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  0, comm_flags);
}

/**f* DesfireAPI/GetValue
 *
 * NAME
 *   GetValue
 *
 * DESCRIPTION
 *   Allows to read current stored value from Value Files
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_GetValue(BYTE file_id,
 *                                     BYTE comm_mode,
 *                                     LONG *value);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_GetValue(SPROX_INSTANCE rInst,
 *                                     BYTE file_id,
 *                                     BYTE comm_mode,
 *                                     LONG *value);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_GetValue(SCARDHANDLE hCard,
 *                                     BYTE file_id,
 *                                     BYTE comm_mode,
 *                                     LONG *value);
 *
 * INPUTS
 *   BYTE file_id      : File IDentifier
 *   BYTE comm_mode    : file's communication settings (DF_COMM_MODE_PLAIN, DF_COMM_MODE_MACED,
 *                       DF_COMM_MODE_PLAIN2 or DF_COMM_MODE_ENCIPHERED)(see chapter 3.2 of
 *                       datasheet of mifare DesFire MF3ICD40 for more information)
 *   LONG *value       : pointer to receive current value
 *
 * RETURNS
 *   DF_OPERATION_OK   : success, value has been read
 *   Other code if internal or communication error has occured.
 *
 * SEE ALSO
 *   GetValue2
 *
 **/
SPROX_API_FUNC(Desfire_GetValue) (SPROX_PARAM  BYTE file_id, BYTE comm_mode, LONG *value)
{
  DWORD t;
  LONG  temp;
  SPROX_RC   status;
  SPROX_DESFIRE_GET_CTX();

  /* Create the info block containing the command code and the file ID argument. */
  ctx->xfer_buffer[INF + 0] = DF_GET_VALUE;
  ctx->xfer_buffer[INF + 1] = file_id;
  ctx->xfer_length = 2;

  if (comm_mode == DF_COMM_MODE_ENCIPHERED)
  {
    /* The communication mode is DF_COMM_MODE_ENCIPHERED.                 */
    /* GetValue returns 9 bytes in DES/3DES mode and 17 bytes in AES mode */

    if (ctx->session_type == KEY_ISO_AES)
    {
      status = SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  17, COMPUTE_COMMAND_CMAC | WANTS_OPERATION_OK);
      t = 16;
    } else
    {
      status = SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  9, COMPUTE_COMMAND_CMAC | WANTS_OPERATION_OK);
      t = 8;
    }

    if (status != DF_OPERATION_OK)
      return status;

#ifdef SPROX_DESFIRE_WITH_SAM
    if (ctx->sam_session_active)
    {
      BYTE pbOut[MAX_DATA_SIZE];
      DWORD dwOutLength = sizeof(pbOut);
      BOOL fSkipStatus = TRUE;

      if (pbOut == NULL)
        return DF_OUT_OF_EEPROM_ERROR;

      if (ctx->session_type & KEY_ISO_MODE)
        fSkipStatus = FALSE;

      status = SAM_DecipherData(ctx->sam_context.hSam, ctx->xfer_buffer[0], 4, &ctx->xfer_buffer[1], ctx->xfer_length-1, fSkipStatus, pbOut, &dwOutLength);


      if (status == DF_OPERATION_OK)
      {
        memcpy(&ctx->xfer_buffer[1], pbOut, dwOutLength);
        ctx->xfer_length = dwOutLength+1;
      } else
      {
        return status;
      }

    } else
#endif
    {
      /* Decipher the received block. */
      Desfire_CipherRecv(SPROX_PARAM_P  &ctx->xfer_buffer[INF + 1], &t);

      if (ctx->session_type & KEY_ISO_MODE)
      {
        /* Verify the CRC */
        if (Desfire_VerifyCrc32(SPROX_PARAM_P  &ctx->xfer_buffer[INF], 5, TRUE, NULL) != DF_OPERATION_OK)
        {
          /* abortion due to integrity error -> wrong CRC */
          return DFCARD_WRONG_CRC;
        }
      } else
      {
        /* Verify the CRC */
        if (Desfire_VerifyCrc16(SPROX_PARAM_P  &ctx->xfer_buffer[INF + 1], 4, NULL) != DF_OPERATION_OK)
        {
          /* abortion due to integrity error -> wrong CRC */
          return DFCARD_WRONG_CRC;
        }
        /* Check also the padding bytes for enhanced security. */
        if (ctx->xfer_buffer[INF + 7] || ctx->xfer_buffer[INF + 8])
        {
          /* Error: cryptogram contains incorrect padding bytes. */
          return DFCARD_WRONG_PADDING;
        }
      }
    }

  } else
  if ((comm_mode == DF_COMM_MODE_MACED) && !(ctx->session_type & KEY_ISO_MODE))
  {
    BYTE    mac32[4];

    /* The communication mode is DF_COMM_MODE_MACED.                      */
    /* GetValue returns 9 bytes, the status byte, the four byte value and */
    /* the four byte MAC.                                                 */
    status = SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  9, WANTS_OPERATION_OK);
    if (status != DF_OPERATION_OK)
      return status;

    /* Check the received MAC. */
#ifdef SPROX_DESFIRE_WITH_SAM
    if (ctx->sam_session_active)
    {
      DWORD dwMac32Length = sizeof(mac32);
      status = SAM_GenerateMAC(ctx->sam_context.hSam, &ctx->xfer_buffer[INF + 1], 4, mac32, &dwMac32Length);
      if ((status != DF_OPERATION_OK) || (dwMac32Length != sizeof(mac32)))
        return status;

    } else
#endif
    {
      Desfire_ComputeMac(SPROX_PARAM_P  &ctx->xfer_buffer[INF + 1], 4, mac32);
    }

    if (memcmp(mac32, &ctx->xfer_buffer[INF + 5], 4))
    {
      /* abortion due to integrity error -> wrong MAC */
      return DFCARD_WRONG_MAC;
    }


  } else
  {
    /* GetValue returns 5 bytes, the status byte and the four byte value. */
    status = SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  5, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
    if (status != DF_OPERATION_OK)
      return status;
  }

  /* Return the requested value bytes. */
  temp = ctx->xfer_buffer[INF + 4];  temp <<= 8;
  temp += ctx->xfer_buffer[INF + 3]; temp <<= 8;
  temp += ctx->xfer_buffer[INF + 2]; temp <<= 8;
  temp += ctx->xfer_buffer[INF + 1];

  if (value != NULL)
    *value = temp;

  /* Success. */
  return DF_OPERATION_OK;
}

/**f* DesfireAPI/GetValue2
 *
 * NAME
 *   GetValue2
 *
 * DESCRIPTION
 *   Allows to read current stored value from Value Files
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_GetValue2(BYTE file_id,
 *                                     LONG *value);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_GetValue2(SPROX_INSTANCE rInst,
 *                                     BYTE file_id,
 *                                     LONG *value);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_GetValue2(SCARDHANDLE hCard,
 *                                     BYTE file_id,
 *                                     LONG *value);
 *
 * INPUTS
 *   BYTE file_id      : File IDentifier
 *   LONG *value       : pointer to receive current value
 *
 * RETURNS
 *   DF_OPERATION_OK   : success, value has been read
 *   Other code if internal or communication error has occured.
 *
 * SEE ALSO
 *   GetValue
 *
 **/
SPROX_API_FUNC(Desfire_GetValue2) (SPROX_PARAM  BYTE file_id, LONG *value)
{
  BYTE    comm_mode;
  WORD    access_rights;
  BYTE    read_only_access;
  BYTE    write_only_access;
  BYTE    read_write_access;
  SPROX_RC   status;
  SPROX_DESFIRE_GET_CTX();

  /* we have to receive the communications mode first */
  status = SPROX_API_CALL(Desfire_GetFileSettings) (SPROX_PARAM_P  file_id, NULL, &comm_mode, &access_rights, NULL);
  if (status != DF_OPERATION_OK)
    return status;

  /* GetValue is controlled by the fields r, w, r/w within the access_rights.               */
  /* Depending on the access_rights field (settings r, w and r/w) we have to decide whether */
  /* we are able to communicate in the mode indicated by comm_mode.                         */
  /* If ctx->auth_key does neither match r, w nor r/w and one of this settings              */
  /* contains the value "ever" (0xE) communication has to be done in plain mode regardless  */
  /* of the mode indicated by comm_mode.                                                    */
  read_only_access  = (BYTE) ((access_rights & DF_READ_ONLY_ACCESS_MASK)  >> DF_READ_ONLY_ACCESS_SHIFT);
  write_only_access = (BYTE) ((access_rights & DF_WRITE_ONLY_ACCESS_MASK) >> DF_WRITE_ONLY_ACCESS_SHIFT);
  read_write_access = (BYTE) ((access_rights & DF_READ_WRITE_ACCESS_MASK) >> DF_READ_WRITE_ACCESS_SHIFT);

  if ((read_only_access  != ctx->session_key_id)
   && (write_only_access != ctx->session_key_id)
   && (read_write_access != ctx->session_key_id)
   && ((read_only_access == 0x0E) || (write_only_access == 0x0E) || (read_write_access == 0x0E)))
  {
    comm_mode = DF_COMM_MODE_PLAIN;
  }

  /* Now execute the command */
  return SPROX_API_CALL(Desfire_GetValue) (SPROX_PARAM_P  file_id, comm_mode, value);
}

/**f* DesfireAPI/LimitedCredit
 *
 * NAME
 *   LimitedCredit
 *
 * DESCRIPTION
 *   Allows a limited increase of a value stored in a Value File without having full Read&Write permissions to the file.
 *   This feature can be enabled or disabled during value file creation.
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_LimitedCredit(BYTE file_id,
 *                                     BYTE comm_mode,
 *                                     LONG amount);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_LimitedCredit(SPROX_INSTANCE rInst,
 *                                     BYTE file_id,
 *                                     BYTE comm_mode,
 *                                     LONG amount);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_LimitedCredit(SCARDHANDLE hCard,
 *                                     BYTE file_id,
 *                                     BYTE comm_mode,
 *                                     LONG amount);
 *
 * INPUTS
 *   BYTE file_id      : File IDentifier
 *   BYTE comm_mode    : file's communication settings (DF_COMM_MODE_PLAIN, DF_COMM_MODE_MACED,
 *                       DF_COMM_MODE_PLAIN2 or DF_COMM_MODE_ENCIPHERED)(see chapter 3.2 of
 *                       datasheet of mifare DesFire MF3ICD40 for more information)
 *   LONG amount       : amount to increase to the current value stored in the file. Only positive values allowed.
 *
 * RETURNS
 *   DF_OPERATION_OK   : success, data has been written
 *   Other code if internal or communication error has occured.
 *
 * SEE ALSO
 *   LimitedCredit2
 *
 **/
SPROX_API_FUNC(Desfire_LimitedCredit) (SPROX_PARAM  BYTE file_id, BYTE comm_mode, LONG amount)
{
  return SPROX_API_CALL(Desfire_ModifyValue) (SPROX_PARAM_P  DF_LIMITED_CREDIT, file_id, comm_mode, amount);
}

/**f* DesfireAPI/LimitedCredit2
 *
 * NAME
 *   LimitedCredit2
 *
 * DESCRIPTION
 *   Allows a limited increase of a value stored in a Value File without having full Read&Write permissions to the file.
 *   This feature can be enabled or disabled during value file creation.
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_LimitedCredit2(BYTE file_id,
 *                                     LONG amount);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_LimitedCredit2(SPROX_INSTANCE rInst,
 *                                     BYTE file_id,
 *                                     LONG amount);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_LimitedCredit2(SCARDHANDLE hCard,
 *                                     BYTE file_id,
 *                                     LONG amount);
 *
 * INPUTS
 *   BYTE file_id      : File IDentifier
 *   LONG amount       : amount to increase to the current value stored in the file. Only positive values allowed.
 *
 * RETURNS
 *   DF_OPERATION_OK   : success, data has been written
 *   Other code if internal or communication error has occured.
 *
 * SEE ALSO
 *   LimitedCredit
 *
 **/
SPROX_API_FUNC(Desfire_LimitedCredit2) (SPROX_PARAM  BYTE file_id, LONG amount)
{
  BYTE    comm_mode;
  WORD    access_rights;
  BYTE    write_only_access;
  BYTE    read_write_access;
  SPROX_RC   status;
  SPROX_DESFIRE_GET_CTX();

  /* we have to receive the communications mode first */
  status = SPROX_API_CALL(Desfire_GetFileSettings) (SPROX_PARAM_P  file_id, NULL, &comm_mode, &access_rights, NULL);
  if (status != DF_OPERATION_OK)
    return status;

  /* LimitedCredit is controlled by the fields w and r/w within the access_rights.         */
  /* Depending on the access_rights field (settings w and r/w) we have to decide whether   */
  /* we are able to communicate in the mode indicated by comm_mode.                        */
  /* If ctx->auth_key does neither match w nor r/w and one of this settings                */
  /* contains the value "ever" (0xE) communication has to be done in plain mode regardless */
  /* of the mode indicated by comm_mode.                                                   */
  write_only_access = (BYTE) ((access_rights & DF_WRITE_ONLY_ACCESS_MASK) >> DF_WRITE_ONLY_ACCESS_SHIFT);
  read_write_access = (BYTE) ((access_rights & DF_READ_WRITE_ACCESS_MASK) >> DF_READ_WRITE_ACCESS_SHIFT);

  if ((write_only_access != ctx->session_key_id)
   && (read_write_access != ctx->session_key_id)
   && ((write_only_access == 0x0E) || (read_write_access == 0x0E)))
  {
    comm_mode = DF_COMM_MODE_PLAIN;
  }

  /* Now execute the command */
  return SPROX_API_CALL(Desfire_LimitedCredit) (SPROX_PARAM_P  file_id, comm_mode, amount);
}

/**f* DesfireAPI/Credit
 *
 * NAME
 *   Credit
 *
 * DESCRIPTION
 *   Allows to increase a value stored in a Value File.
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_Credit(BYTE file_id,
 *                                     BYTE comm_mode,
 *                                     LONG amount);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_Credit(SPROX_INSTANCE rInst,
 *                                     BYTE file_id,
 *                                     BYTE comm_mode,
 *                                     LONG amount);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_Credit(SCARDHANDLE hCard,
 *                                     BYTE file_id,
 *                                     BYTE comm_mode,
 *                                     LONG amount);
 *
 * INPUTS
 *   BYTE file_id      : File IDentifier
 *   BYTE comm_mode    : file's communication settings (DF_COMM_MODE_PLAIN, DF_COMM_MODE_MACED,
 *                       DF_COMM_MODE_PLAIN2 or DF_COMM_MODE_ENCIPHERED)(see chapter 3.2 of
 *                       datasheet of mifare DesFire MF3ICD40 for more information)
 *   LONG amount       : amount to increase to the current value stored in the file. Only positive values allowed.
 *
 * RETURNS
 *   DF_OPERATION_OK   : success, data has been written
 *   Other code if internal or communication error has occured.
 *
 * NOTES
 *   The Credit command requires Authentication with the key specified for "Read&Write" access.
 *
 * SEE ALSO
 *   Credit2
 *
 **/
SPROX_API_FUNC(Desfire_Credit) (SPROX_PARAM  BYTE file_id, BYTE comm_mode, LONG amount)
{
  return SPROX_API_CALL(Desfire_ModifyValue) (SPROX_PARAM_P  DF_CREDIT, file_id, comm_mode, amount);
}

/**f* DesfireAPI/Credit2
 *
 * NAME
 *   Credit2
 *
 * DESCRIPTION
 *   Allows to increase a value stored in a Value File.
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_Credit2(BYTE file_id,
 *                                     LONG amount);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_Credit2(SPROX_INSTANCE rInst,
 *                                     BYTE file_id,
 *                                     LONG amount);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_Credit2(SCARDHANDLE hCard,
 *                                     BYTE file_id,
 *                                     LONG amount);
 *
 * INPUTS
 *   BYTE file_id      : File IDentifier
 *   LONG amount       : amount to increase to the current value stored in the file. Only positive values allowed.
 *
 * RETURNS
 *   DF_OPERATION_OK   : success, data has been written
 *   Other code if internal or communication error has occured.
 *
 * NOTES
 *   The Credit command requires Authentication with the key specified for "Read&Write" access.
 *
 * SEE ALSO
 *   Credit
 *
 **/
SPROX_API_FUNC(Desfire_Credit2) (SPROX_PARAM  BYTE file_id, LONG amount)
{
  BYTE    comm_mode;
  WORD    access_rights;
  BYTE    read_write_access;
  SPROX_RC   status;
  SPROX_DESFIRE_GET_CTX();

  /* we have to receive the communications mode first */
  status = SPROX_API_CALL(Desfire_GetFileSettings) (SPROX_PARAM_P  file_id, NULL, &comm_mode, &access_rights, NULL);
  if (status != DF_OPERATION_OK)
    return status;

  /* Credit is controlled by the field r/w within the access_rights.                    */
  /* Depending on the AccessRights field (setting r/w) we have to decide whether        */
  /* we are able to communicate in the mode indicated by comm_mode.                     */
  /* If ctx->auth_key does not match r/w and this setting contains the value            */
  /* "ever" (0xE), communication has to be done in plain mode regardless of the mode    */
  /* indicated by comm_mode.                                                            */
  read_write_access = (BYTE) ((access_rights & DF_READ_WRITE_ACCESS_MASK) >> DF_READ_WRITE_ACCESS_SHIFT);

  if ((read_write_access != ctx->session_key_id)
   && (read_write_access == 0x0E))
  {
    comm_mode = DF_COMM_MODE_PLAIN;
  }

  /* Now execute the command */
  return SPROX_API_CALL(Desfire_Credit) (SPROX_PARAM_P file_id, comm_mode, amount);
}

/**f* DesfireAPI/Debit
 *
 * NAME
 *   Debit
 *
 * DESCRIPTION
 *   Allows to decrease a value stored in a Value File.
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_Debit(BYTE file_id,
 *                                     BYTE comm_mode,
 *                                     LONG amount);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_Debit(SPROX_INSTANCE rInst,
 *                                     BYTE file_id,
 *                                     BYTE comm_mode,
 *                                     LONG amount);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_Debit(SCARDHANDLE hCard,
 *                                     BYTE file_id,
 *                                     BYTE comm_mode,
 *                                     LONG amount);
 *
 * INPUTS
 *   BYTE file_id      : File IDentifier
 *   BYTE comm_mode    : file's communication settings (DF_COMM_MODE_PLAIN, DF_COMM_MODE_MACED,
 *                       DF_COMM_MODE_PLAIN2 or DF_COMM_MODE_ENCIPHERED)(see chapter 3.2 of
 *                       datasheet of mifare DesFire MF3ICD40 for more information)
 *   LONG amount       : amount to decrease to the current value stored in the file. Only positive values allowed.
 *
 * RETURNS
 *   DF_OPERATION_OK   : success, data has been written
 *   Other code if internal or communication error has occured.
 *
 * NOTES
 *   The Credit command requires Authentication with the key specified for "Read", "Write" ord "Read&Write" access.
 *
 * SEE ALSO
 *   Debit2
 *
 **/
SPROX_API_FUNC(Desfire_Debit) (SPROX_PARAM  BYTE file_id, BYTE comm_mode, LONG amount)
{
  return SPROX_API_CALL(Desfire_ModifyValue) (SPROX_PARAM_P  DF_DEBIT, file_id, comm_mode, amount);
}

/**f* DesfireAPI/Debit2
 *
 * NAME
 *   Debit2
 *
 * DESCRIPTION
 *   Allows to decrease a value stored in a Value File.
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_Debit2(BYTE file_id,
 *                                     LONG amount);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_Debit2(SPROX_INSTANCE rInst,
 *                                     BYTE file_id,
 *                                     LONG amount);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_Debit2(SCARDHANDLE hCard,
 *                                     BYTE file_id,
 *                                     LONG amount);
 *
 * INPUTS
 *   BYTE file_id      : File IDentifier
 *   LONG amount       : amount to decrease to the current value stored in the file. Only positive values allowed.
 *
 * RETURNS
 *   DF_OPERATION_OK   : success, data has been written
 *   Other code if internal or communication error has occured.
 *
 * NOTES
 *   The Credit command requires Authentication with the key specified for "Read", "Write" ord "Read&Write" access.
 *
 * SEE ALSO
 *   Debit
 *
 **/
SPROX_API_FUNC(Desfire_Debit2) (SPROX_PARAM  BYTE file_id, LONG amount)
{
  BYTE    comm_mode;
  WORD    access_rights;
  BYTE    read_only_access;
  BYTE    write_only_access;
  BYTE    read_write_access;
  SPROX_RC   status;
  SPROX_DESFIRE_GET_CTX();

  /* we have to receive the communications mode first */
  status = SPROX_API_CALL(Desfire_GetFileSettings) (SPROX_PARAM_P  file_id, NULL, &comm_mode, &access_rights, NULL);
  if (status != DF_OPERATION_OK)
    return status;

  /* Debit is controlled by the fields r, w, r/w within the access_rights.                  */
  /* Depending on the access_rights field (settings r, w and r/w) we have to decide whether */
  /* we are able to communicate in the mode indicated by comm_mode.                         */
  /* If ctx->auth_key does neither match r, w nor r/w and one of this settings              */
  /* contains the value "ever" (0xE) communication has to be done in plain mode regardless  */
  /* of the mode indicated by comm_mode.                                                    */
  read_only_access  = (BYTE) ((access_rights & DF_READ_ONLY_ACCESS_MASK)  >> DF_READ_ONLY_ACCESS_SHIFT);
  write_only_access = (BYTE) ((access_rights & DF_WRITE_ONLY_ACCESS_MASK) >> DF_WRITE_ONLY_ACCESS_SHIFT);
  read_write_access = (BYTE) ((access_rights & DF_READ_WRITE_ACCESS_MASK) >> DF_READ_WRITE_ACCESS_SHIFT);

  if ((read_only_access  != ctx->session_key_id)
   && (write_only_access != ctx->session_key_id)
   && (read_write_access != ctx->session_key_id)
   && ((read_only_access == 0x0E) || (write_only_access == 0x0E) || (read_write_access == 0x0E)))
  {
    comm_mode = DF_COMM_MODE_PLAIN;
  }

  /* Now execute the command */
  return SPROX_API_CALL(Desfire_Debit) (SPROX_PARAM_P  file_id, comm_mode, amount);
}
