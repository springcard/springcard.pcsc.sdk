/**h* DesfireAPI/Records
 *
 * NAME
 *   DesfireAPI :: Linear or cyclic file related functions
 *
 * COPYRIGHT
 *   (c) 2009 SpringCard - www.springcard.com
 *
 * DESCRIPTION
 *   Implementation of DESFIRE functions to handle structured files.
 *
 **/
#include "sprox_desfire_i.h"

/**f* DesfireAPI/ClearRecordFile
 *
 * NAME
 *   Desfire_ClearRecordFile
 *
 * DESCRIPTION
 *   Allows to reset a Cyclic or Linear Record File to the empty state
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_ClearRecordFile(BYTE file_id);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_ClearRecordFile(SPROX_INSTANCE rInst,
 *                                     BYTE file_id);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_ClearRecordFile(SCARDHANDLE hCard,
 *                                     BYTE file_id);
 *
 * INPUTS
 *   BYTE file_id      : File IDentifier
 *
 * RETURNS
 *   DF_OPERATION_OK   : success, data has been written
 *   Other code if internal or communication error has occured.
 *
 * NOTES
 *   Full "Read&Write" permission on the file is necessary for executing this command
 *
 **/
SPROX_API_FUNC(Desfire_ClearRecordFile) (SPROX_PARAM  BYTE file_id)
{
  SPROX_DESFIRE_GET_CTX();
  ctx->xfer_buffer[INF + 0] = DF_CLEAR_RECORD_FILE;
  ctx->xfer_buffer[INF + 1] = file_id;
  ctx->xfer_length = 2;

  /* response must be of one byte length if successfully terminated */
  return SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  1, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
}

/**f* DesfireAPI/ReadRecords
 *
 * NAME
 *   ReadRecords
 *
 * DESCRIPTION
 *   Allows to read data out a set of complete records from a Cyclic or Linear Record File.
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_ReadRecords(BYTE file_id,
 *                                   BYTE comm_mode,
 *                                   DWORD from_record,
 *                                   DWORD max_record_count,
 *                                   DWORD record_size,
 *                                   BYTE data[],
 *                                   DWORD *record_count);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_ReadRecords(SPROX_INSTANCE rInst,
 *                                    BYTE file_id,
 *                                    BYTE comm_mode,
 *                                    DWORD from_record,
 *                                    DWORD max_record_count,
 *                                    DWORD record_size,
 *                                    BYTE data[],
 *                                    DWORD *record_count);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_ReadRecords(SCARDHANDLE hCard,
 *                                  BYTE file_id,
 *                                  BYTE comm_mode,
 *                                  DWORD from_record,
 *                                  DWORD max_record_count,
 *                                  DWORD record_size,
 *                                  BYTE data[],
 *                                  DWORD *record_count);
 *
 * INPUTS
 *   BYTE file_id           : File IDentifier
 *   BYTE comm_mode         : file's communication settings (DF_COMM_MODE_PLAIN, DF_COMM_MODE_MACED,
 *                            DF_COMM_MODE_PLAIN2 or DF_COMM_MODE_ENCIPHERED)(see chapter 3.2 of
 *                            datasheet of mifare DesFire MF3ICD40 for more information)
 *   DWORD from_record      : offset of the newest record to read. Set to 0 for latest record
 *   DWORD max_record_count : number of records to be read from the PICC. Set to 0 to read all records.
 *   DWORD record_size      : size of the record in bytes
 *   BYTE data[]            : buffer to receive the data
 *   DWORD *record_count    : actual number of records read
 *
 * RETURNS
 *   DF_OPERATION_OK        : success, data has been read
 *   Other code if internal or communication error has occured.
 *
 * SEE ALSO
 *   ReadRecords2
 *
 **/
SPROX_API_FUNC(Desfire_ReadRecords) (SPROX_PARAM  BYTE file_id, BYTE comm_mode, DWORD from_record, DWORD max_record_count, DWORD record_size, BYTE data[], DWORD *record_count)
{
  BYTE    file_type;
  DWORD   done_size;
  DF_ADDITIONAL_FILE_SETTINGS file_settings;
  SPROX_RC   status;

  memset(&file_settings, 0x00, sizeof(file_settings));

  /* if a pointer was passed for retrieving the number of records read    */
  /* and no record size was specified, we have to get the record size via */
  /* the GetFileSettings command                                          */
  if ((record_count != NULL) && (record_size == 0))
  {
    status = SPROX_API_CALL(Desfire_GetFileSettings) (SPROX_PARAM_P  file_id, &file_type, 0, 0, &file_settings);
    if (status != DF_OPERATION_OK)
      return status;

    /* if the file type indicates that this is not a record file */
    /* we are not able to proceed                                */
    if ((file_type != DF_LINEAR_RECORD_FILE) && (file_type != DF_CYCLIC_RECORD_FILE))
      return DFCARD_WRONG_FILE_TYPE;

    record_size = file_settings.stRecordFileSettings.eRecordSize;

    /* if eRecordSize becomes zero under any circumstances this     */
    /* would lead to a division by zero at the end of this function */
    /* thus making it impossible to proceed                         */
    if (record_size == 0)
      return DFCARD_WRONG_RECORD_SIZE;
  }

  /* Call the ReadFile function */
  status = SPROX_API_CALL(Desfire_ReadDataEx) (SPROX_PARAM_P  DF_READ_RECORDS, file_id, comm_mode, from_record, max_record_count, record_size, data, &done_size);
  if (status != DF_OPERATION_OK)
    return status;

  /* we have to check whether we received multiples of record_size   */
  /* if this is not the case this means that a format error occurred */
  if ((done_size % record_size) != 0)
    return DFCARD_WRONG_LENGTH;

  /* calculate the number of records if a pointer has been passed */
  if (record_count != NULL)
    *record_count = done_size / record_size;

  return DF_OPERATION_OK;
}

/**f* DesfireAPI/ReadRecords2
 *
 * NAME
 *   ReadRecords2
 *
 * DESCRIPTION
 *   Allows to read data out a set of complete records from a Cyclic or Linear Record File.
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_ReadRecords2(BYTE file_id,
 *                                    DWORD from_record,
 *                                    DWORD max_record_count,
 *                                    DWORD record_size,
 *                                    BYTE data[],
 *                                    DWORD *record_count);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_ReadRecords2(SPROX_INSTANCE rInst,
 *                                     BYTE file_id,
 *                                     DWORD from_record,
 *                                     DWORD max_record_count,
 *                                     DWORD record_size,
 *                                     BYTE data[],
 *                                     DWORD *record_count);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_ReadRecords2(SCARDHANDLE hCard,
 *                                   BYTE file_id,
 *                                   BYTE comm_mode,
 *                                   DWORD from_record,
 *                                   DWORD max_record_count,
 *                                   DWORD record_size,
 *                                   BYTE data[],
 *                                   DWORD *record_count);
 *
 * INPUTS
 *   BYTE file_id           : File IDentifier
 *   DWORD from_record      : offset of the newest record to read. Set to 0 for latest record
 *   DWORD max_record_count : number of records to be read from the PICC. Set to 0 to read all records.
 *   DWORD record_size      : size of the record in bytes
 *   BYTE data[]            : buffer to receive the data
 *   DWORD *record_count    : actual number of records read
 *
 * RETURNS
 *   DF_OPERATION_OK        : success, data has been read
 *   Other code if internal or communication error has occured.
 *
 * SEE ALSO
 *   ReadRecords
 *
 **/
SPROX_API_FUNC(Desfire_ReadRecords2) (SPROX_PARAM  BYTE file_id, DWORD from_record, DWORD max_record_count, DWORD record_size, BYTE data[], DWORD *record_count)
{
  BYTE    comm_mode;
  WORD    access_rights;
  BYTE    read_only_access;
  BYTE    read_write_access;
  SPROX_RC   status;
  SPROX_DESFIRE_GET_CTX();

  /* we have to receive the communications mode first */
  status = SPROX_API_CALL(Desfire_GetFileSettings) (SPROX_PARAM_P  file_id, NULL, &comm_mode, &access_rights, NULL);
  if (status != DF_OPERATION_OK)
    return status;

  /* Depending on the AccessRights field (settings r and r/w) we have to decide whether     */
  /* we are able to communicate in the mode indicated by comm_mode.                         */
  /* If ctx->auth_key does neither match r nor r/w and one of this settings                 */
  /* contains the value "ever" (0x0E) communication has to be done in plain mode regardless */
  /* of the mode indicated by comm_mode.                                                    */
  read_only_access  = (BYTE) ((access_rights & DF_READ_ONLY_ACCESS_MASK)  >> DF_READ_ONLY_ACCESS_SHIFT);
  read_write_access = (BYTE) ((access_rights & DF_READ_WRITE_ACCESS_MASK) >> DF_READ_WRITE_ACCESS_SHIFT);

  if ((read_only_access  != ctx->session_key_id)
   && (read_write_access != ctx->session_key_id)
   && ((read_only_access == 0x0E) || (read_write_access == 0x0E)))
  {
    comm_mode = DF_COMM_MODE_PLAIN;
  }

  /* Now execute the command */
  return SPROX_API_CALL(Desfire_ReadRecords) (SPROX_PARAM_P  file_id, comm_mode, from_record, max_record_count, record_size, data, record_count);
}

/**f* DesfireAPI/WriteRecord
 *
 * NAME
 *   WriteRecord
 *
 * DESCRIPTION
 *   Allows to write data to a record in a Cyclic or Linear Record File.
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_WriteRecord(BYTE file_id,
 *                                     BYTE comm_mode,
 *                                     DWORD from_offset,
 *                                     DWORD size,
 *                                     const BYTE data[]);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_WriteRecord(SPROX_INSTANCE rInst,
 *                                     BYTE file_id,
 *                                     BYTE comm_mode,
 *                                     DWORD from_offset,
 *                                     DWORD size,
 *                                     const BYTE data[]);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_WriteRecord(SCARDHANDLE hCard,
 *                                     BYTE file_id,
 *                                     BYTE comm_mode,
 *                                     DWORD from_offset,
 *                                     DWORD size,
 *                                     const BYTE data[]);
 *
 * INPUTS
 *   BYTE file_id      : File IDentifier
 *   BYTE comm_mode    : file's communication settings (DF_COMM_MODE_PLAIN, DF_COMM_MODE_MACED,
 *                       DF_COMM_MODE_PLAIN2 or DF_COMM_MODE_ENCIPHERED)(see chapter 3.2 of
 *                       datasheet of mifare DesFire MF3ICD40 for more information)
 *   DWORD from_offset : offset within one single record in bytes
 *   DWORD size        : size data to be written in bytes
 *   BYTE data[]       : buffer containing the data to write
 *
 * RETURNS
 *   DF_OPERATION_OK   : success, data has been written
 *   Other code if internal or communication error has occured.
 *
 * SEE ALSO
 *   WriteRecord2
 *
 **/
SPROX_API_FUNC(Desfire_WriteRecord) (SPROX_PARAM  BYTE file_id, BYTE comm_mode, DWORD from_offset, DWORD size, const BYTE data[])
{
  return SPROX_API_CALL(Desfire_WriteDataEx) (SPROX_PARAM_P  DF_WRITE_RECORD, file_id, comm_mode, from_offset, size, data);
}

/**f* DesfireAPI/WriteRecord2
 *
 * NAME
 *   WriteRecord2
 *
 * DESCRIPTION
 *   Allows to write data to a record in a Cyclic or Linear Record File.
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_WriteRecord2(BYTE file_id,
 *                                     DWORD from_offset,
 *                                     DWORD size,
 *                                     const BYTE data[]);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_WriteRecord2(SPROX_INSTANCE rInst,
 *                                     BYTE file_id,
 *                                     DWORD from_offset,
 *                                     DWORD size,
 *                                     const BYTE data[]);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_WriteRecord2(SCARDHANDLE hCard,
 *                                     BYTE file_id,
 *                                     DWORD from_offset,
 *                                     DWORD size,
 *                                     const BYTE data[]);
 *
 * INPUTS
 *   BYTE file_id      : File IDentifier
 *   DWORD from_offset : offset within one single record in bytes
 *   DWORD size        : size data to be written in bytes
 *   BYTE data[]       : buffer containing the data to write
 *
 * RETURNS
 *   DF_OPERATION_OK   : success, data has been written
 *   Other code if internal or communication error has occured.
 *
 * SEE ALSO
 *   WriteRecord
 *
 **/
SPROX_API_FUNC(Desfire_WriteRecord2) (SPROX_PARAM  BYTE file_id, DWORD from_offset, DWORD size, const BYTE data[])
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
  return SPROX_API_CALL(Desfire_WriteRecord) (SPROX_PARAM_P  file_id, comm_mode, from_offset, size, data);
}
