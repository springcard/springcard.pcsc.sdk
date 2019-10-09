/**h* DesfireAPI/Files
 *
 * NAME
 *   DesfireAPI :: File management functions
 *
 * COPYRIGHT
 *   (c) 2009 SpringCard - www.springcard.com
 *
 * DESCRIPTION
 *   Implementation of management functions to create or delete
 *   files withing a DESFIRE application.
 *
 **/
#include "sprox_desfire_i.h"


static void Desfire_PrepareCreateFileCommand(SPROX_PARAM  BYTE create_cmd, BYTE file_id, DWORD iso_ef_id, BYTE comm_mode, WORD access_rights)
{
  SPROX_DESFIRE_GET_CTX_V();

  ctx->xfer_length = 0;

  /* Create the info block containing the command code and the given parameters. */
  ctx->xfer_buffer[ctx->xfer_length++] = create_cmd;

  ctx->xfer_buffer[ctx->xfer_length++] = file_id;

  if (iso_ef_id != (DWORD) -1)
  {
    ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (iso_ef_id & 0x00FF); iso_ef_id >>= 8;
    ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (iso_ef_id & 0x00FF);
  }

  ctx->xfer_buffer[ctx->xfer_length++] = comm_mode;

  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (access_rights & 0x00FF); access_rights >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (access_rights & 0x00FF);
}

/**f* DesfireAPI/CreateStdDataFile
 *
 * NAME
 *   CreateStdDataFile
 *
 * DESCRIPTION
 *   Creates files for the storage of plain unformatted user data within an existing application
 *   on the DESFIRE card
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_CreateStdDataFile(BYTE file_id,
 *                                         BYTE comm_mode,
 *                                         WORD access_rights,
 *                                         DWORD file_size);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_CreateStdDataFile(SPROX_INSTANCE rInst,
 *                                          BYTE file_id,
 *                                          BYTE comm_mode,
 *                                          WORD access_rights,
 *                                          DWORD file_size);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_CreateStdDataFile(SCARDHANDLE hCard,
 *                                        BYTE file_id,
 *                                        BYTE comm_mode,
 *                                        WORD access_rights,
 *                                        DWORD file_size);
 *
 * INPUTS
 *   BYTE file_id                : DESFIRE File IDentifier
 *   BYTE comm_mode              : file's Communication Settings
 *   WORD access_rights          : file's Access Rights
 *   DWORD file_size             : size of the file in bytes
 *
 * RETURNS
 *   DF_OPERATION_OK    : creation succeeded
 *   Other code if internal or communication error has occured.
 *
 * NOTES
 *   Please refer to DESFIRE datasheet for details regarding this function and its
 *   acceptable parameters.
 *
 * SEE ALSO
 *   CreateBackupDataFile
 *   CreateValueFile
 *   CreateLinearRecordFile
 *   CreateCyclicRecordFile
 *   DeleteFile
 *
 **/
SPROX_API_FUNC(Desfire_CreateStdDataFile) (SPROX_PARAM  BYTE file_id, BYTE comm_mode, WORD access_rights, DWORD file_size)
{
  SPROX_DESFIRE_GET_CTX();

  Desfire_PrepareCreateFileCommand(SPROX_PARAM_P  DF_CREATE_STD_DATA_FILE, file_id, (DWORD) -1, comm_mode, access_rights);

  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (file_size & 0x00FF); file_size >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (file_size & 0x00FF); file_size >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (file_size & 0x00FF);

  return SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  0, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
}

/**f* DesfireAPI/CreateIsoStdDataFile
 *
 * NAME
 *   CreateIsoStdDataFile
 *
 * DESCRIPTION
 *   Creates files for the storage of plain unformatted user data within an existing application
 *   on the DESFIRE card.
 *   Using this function, an ISO EF IDentifier is specified as well as a legacy DESFIRE File ID.
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_CreateIsoStdDataFile(BYTE file_id,
 *                                            WORD iso_ef_id,
 *                                            BYTE comm_mode,
 *                                            WORD access_rights,
 *                                            DWORD file_size);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_CreateIsoStdDataFile(SPROX_INSTANCE rInst,
 *                                             WORD iso_ef_id,
 *                                             BYTE file_id,
 *                                             BYTE comm_mode,
 *                                             WORD access_rights,
 *                                             DWORD file_size);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_CreateIsoStdDataFile(SCARDHANDLE hCard,
 *                                           WORD iso_ef_id,
 *                                           BYTE file_id,
 *                                           BYTE comm_mode,
 *                                           WORD access_rights,
 *                                           DWORD file_size);
 *
 * INPUTS
 *   BYTE file_id                : DESFIRE File IDentifier
 *   WORD iso_ef_id              : IDentifier of the EF for ISO 7816-4 applications
 *   BYTE comm_mode              : file's Communication Settings
 *   WORD access_rights          : file's Access Rights
 *   DWORD file_size             : size of the file in bytes
 *
 * RETURNS
 *   DF_OPERATION_OK    : creation succeeded
 *   Other code if internal or communication error has occured.
 *
 * NOTES
 *   Please refer to DESFIRE datasheet for details regarding this function and its
 *   acceptable parameters.
 *
 * SEE ALSO
 *   CreateBackupDataFile
 *   CreateValueFile
 *   CreateLinearRecordFile
 *   CreateCyclicRecordFile
 *   DeleteFile
 *
 **/
SPROX_API_FUNC(Desfire_CreateIsoStdDataFile) (SPROX_PARAM  BYTE file_id, WORD iso_ef_id, BYTE comm_mode, WORD access_rights, DWORD file_size)
{
  SPROX_DESFIRE_GET_CTX();

  Desfire_PrepareCreateFileCommand(SPROX_PARAM_P  DF_CREATE_STD_DATA_FILE, file_id, iso_ef_id, comm_mode, access_rights);

  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (file_size & 0x00FF); file_size >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (file_size & 0x00FF); file_size >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (file_size & 0x00FF);

  return SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  0, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
}

/**f* DesfireAPI/CreateBackupDataFile
 *
 * NAME
 *   CreateBackupDataFile
 *
 * DESCRIPTION
 *   Creates files for the storage of plain unformatted user data within an existing application
 *   on the DESFIRE card, additionally supporting the feature of integrated backup mechanism
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_CreateBackupDataFile(BYTE file_id,
 *                                            BYTE comm_mode,
 *                                            WORD access_rights,
 *                                            DWORD file_size);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_CreateBackupDataFile(SPROX_INSTANCE rInst,
 *                                             BYTE file_id,
 *                                             BYTE comm_mode,
 *                                             WORD access_rights,
 *                                             DWORD file_size);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_CreateBackupDataFile(SCARDHANDLE hCard,
 *                                           BYTE file_id,
 *                                           BYTE comm_mode,
 *                                           WORD access_rights,
 *                                           DWORD file_size);
 *
 * INPUTS
 *   BYTE file_id                : DESFIRE File IDentifier
 *   BYTE comm_mode              : file's Communication Settings
 *   WORD access_rights          : file's Access Rights
 *   DWORD file_size             : size of the file in bytes
 *
 * RETURNS
 *   DF_OPERATION_OK    : Backup Data File  succeeded
 *   Other code if internal or communication error has occured.
 *
 * NOTES
 *   Please refer to DESFIRE datasheet for details regarding this function and its
 *   acceptable parameters.
 *
 * SEE ALSO
 *   CreateStdDataFile
 *   CreateValueFile
 *   CreateLinearRecordFile
 *   CreateCyclicRecordFile
 *   DeleteFile
 *
 **/
SPROX_API_FUNC(Desfire_CreateBackupDataFile) (SPROX_PARAM  BYTE file_id, BYTE comm_mode, WORD access_rights, DWORD file_size)
{
  SPROX_DESFIRE_GET_CTX();

  Desfire_PrepareCreateFileCommand(SPROX_PARAM_P  DF_CREATE_BACKUP_DATA_FILE, file_id, (DWORD) -1, comm_mode, access_rights);

  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (file_size & 0x000000FF); file_size >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (file_size & 0x000000FF); file_size >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (file_size & 0x000000FF);

  return SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  0, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
}

/**f* DesfireAPI/CreateIsoBackupDataFile
 *
 * NAME
 *   CreateIsoBackupDataFile
 *
 * DESCRIPTION
 *   Creates files for the storage of plain unformatted user data within an existing application
 *   on the DESFIRE card, additionally supporting the feature of integrated backup mechanism.
 *   Using this function, an ISO EF IDentifier is specified as well as a legacy DESFIRE File ID.
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_CreateIsoBackupDataFile(BYTE file_id,
 *                                               WORD iso_ef_id,
 *                                               BYTE comm_mode,
 *                                               WORD access_rights,
 *                                               DWORD file_size);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_CreateIsoBackupDataFile(SPROX_INSTANCE rInst,
 *                                                WORD iso_ef_id,
 *                                                BYTE file_id,
 *                                                BYTE comm_mode,
 *                                                WORD access_rights,
 *                                                DWORD file_size);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_CreateIsoBackupDataFile(SCARDHANDLE hCard,
 *                                              WORD iso_ef_id,
 *                                              BYTE file_id,
 *                                              BYTE comm_mode,
 *                                              WORD access_rights,
 *                                              DWORD file_size);
 *
 * INPUTS
 *   BYTE file_id                : DESFIRE File IDentifier
 *   WORD iso_ef_id              : IDentifier of the EF for ISO 7816-4 applications
 *   BYTE comm_mode              : file's Communication Settings
 *   WORD access_rights          : file's Access Rights
 *   DWORD file_size             : size of the file in bytes
 *
 * RETURNS
 *   DF_OPERATION_OK    : Backup Data File  succeeded
 *   Other code if internal or communication error has occured.
 *
 * NOTES
 *   Please refer to DESFIRE datasheet for details regarding this function and its
 *   acceptable parameters.
 *
 * SEE ALSO
 *   CreateStdDataFile
 *   CreateValueFile
 *   CreateLinearRecordFile
 *   CreateCyclicRecordFile
 *   DeleteFile
 *
 **/
SPROX_API_FUNC(Desfire_CreateIsoBackupDataFile) (SPROX_PARAM  BYTE file_id, WORD iso_ef_id, BYTE comm_mode, WORD access_rights, DWORD file_size)
{
  SPROX_DESFIRE_GET_CTX();

  Desfire_PrepareCreateFileCommand(SPROX_PARAM_P  DF_CREATE_BACKUP_DATA_FILE, file_id, iso_ef_id, comm_mode, access_rights);

  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (file_size & 0x000000FF); file_size >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (file_size & 0x000000FF); file_size >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (file_size & 0x000000FF);

  return SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  0, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
}

/**f* DesfireAPI/CreateValueFile
 *
 * NAME
 *   CreateValueFile
 *
 * DESCRIPTION
 *   Creates files for storage and manipulation of 32bit signed integer values within an existing
 *   application on the DESFIRE card
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_CreateValueFile(BYTE file_id,
 *                                       BYTE comm_mode,
 *                                       WORD access_rights,
 *                                       LONG lower_limit,
 *                                       LONG upper_limit,
 *                                       LONG initial_value,
 *                                       BYTE limited_credit_enabled);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_CreateValueFile(SPROX_INSTANCE rInst,
 *                                        BYTE file_id,
 *                                        BYTE comm_mode,
 *                                        WORD access_rights,
 *                                        LONG lower_limit,
 *                                        LONG upper_limit,
 *                                        LONG initial_value,
 *                                        BYTE limited_credit_enabled);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_CreateValueFile(SCARDHANDLE hCard,
 *                                      BYTE file_id,
 *                                      BYTE comm_mode,
 *                                      WORD access_rights,
 *                                      LONG lower_limit,
 *                                      LONG upper_limit,
 *                                      LONG initial_value,
 *                                      BYTE limited_credit_enabled);
 *
 * INPUTS
 *   BYTE file_id                : DESFIRE File IDentifier
 *   BYTE comm_mode              : file's Communication Settings
 *   WORD access_rights          : file's Access Rights
 *   LONG lower_limit            : lower limit which is valid for this file. The lower limit marks
 *                                 the boundary which must not be passed by a Debit calculation on
 *                                 the current value
 *   LONG upper_limit            : the upper limit which sets the boundary in the same manner but for
 *                                 the Credit operation
 *   LONG initial_value          : specifies the initial value of the value file
 *   BYTE limited_credit_enabled : activation of the LimitedCredit feature (0 disabled, 1 enabled)
 *
 * RETURNS
 *   DF_OPERATION_OK    : file created
 *   Other code if internal or communication error has occured.
 *
 * NOTES
 *   Please refer to DESFIRE datasheet for details regarding this function and its
 *   acceptable parameters.
 *
 * SEE ALSO
 *   CreateStdDataFile
 *   CreateBackupDataFile
 *   CreateLinearRecordFile
 *   CreateCyclicRecordFile
 *   DeleteFile
 *
 **/
SPROX_API_FUNC(Desfire_CreateValueFile) (SPROX_PARAM  BYTE file_id, BYTE comm_mode, WORD access_rights, LONG lower_limit, LONG upper_limit, LONG initial_value, BYTE limited_credit_enabled)
{
  SPROX_DESFIRE_GET_CTX();

  Desfire_PrepareCreateFileCommand(SPROX_PARAM_P  DF_CREATE_VALUE_FILE, file_id, (DWORD) -1, comm_mode, access_rights);

  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (lower_limit & 0x000000FF); lower_limit >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (lower_limit & 0x000000FF); lower_limit >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (lower_limit & 0x000000FF); lower_limit >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (lower_limit & 0x000000FF);

  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (upper_limit & 0x000000FF); upper_limit >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (upper_limit & 0x000000FF); upper_limit >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (upper_limit & 0x000000FF); upper_limit >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (upper_limit & 0x000000FF);

  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (initial_value & 0x000000FF); initial_value >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (initial_value & 0x000000FF); initial_value >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (initial_value & 0x000000FF); initial_value >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (initial_value & 0x000000FF);

  ctx->xfer_buffer[ctx->xfer_length++] = limited_credit_enabled;

  return SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  0, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
}

/**f* DesfireAPI/CreateLinearRecordFile
 *
 * NAME
 *   CreateLinearRecordFile
 *
 * DESCRIPTION
 *   Creates a linear record file
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_CreateLinearRecordFile(BYTE file_id,
 *                                     BYTE comm_mode,
 *                                     WORD access_rights,
 *                                     DWORD record_size,
 *                                     DWORD max_records);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_CreateLinearRecordFile(SPROX_INSTANCE rInst,
 *                                     BYTE file_id,
 *                                     BYTE comm_mode,
 *                                     WORD access_rights,
 *                                     DWORD record_size,
 *                                     DWORD max_records);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_CreateLinearRecordFile(SCARDHANDLE hCard,
 *                                     BYTE file_id,
 *                                     BYTE comm_mode,
 *                                     WORD access_rights,
 *                                     DWORD record_size,
 *                                     DWORD max_records);
 *
 * INPUTS
 *   BYTE file_id                : DESFIRE File IDentifier
 *   BYTE comm_mode              : file's Communication Settings
 *   WORD access_rights          : file's Access Rights
 *   DWORD record_size           : size of one single record in bytes
 *   DWORD max_records           : maximum number of records
 *
 * RETURNS
 *   DF_OPERATION_OK    : file created successfully
 *   Other code if internal or communication error has occured.
 *
 * NOTES
 *   Please refer to DESFIRE datasheet for details regarding this function and its
 *   acceptable parameters.
 *
 * SEE ALSO
 *   CreateStdDataFile
 *   CreateBackupDataFile
 *   CreateValueFile
 *   CreateCyclicRecordFile
 *   DeleteFile
 *
 **/
SPROX_API_FUNC(Desfire_CreateLinearRecordFile) (SPROX_PARAM  BYTE file_id, BYTE comm_mode, WORD access_rights, DWORD record_size, DWORD max_records)
{
  SPROX_DESFIRE_GET_CTX();

  Desfire_PrepareCreateFileCommand(SPROX_PARAM_P  DF_CREATE_LINEAR_RECORD_FILE, file_id, (DWORD) -1, comm_mode, access_rights);

  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (record_size & 0x000000FF); record_size >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (record_size & 0x000000FF); record_size >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (record_size & 0x000000FF);

  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (max_records & 0x000000FF); max_records >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (max_records & 0x000000FF); max_records >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (max_records & 0x000000FF);

  /* Communicate the info block to the card and check the operation's return status. */
  return SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  0, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
}

/**f* DesfireAPI/CreateIsoLinearRecordFile
 *
 * NAME
 *   CreateIsoLinearRecordFile
 *
 * DESCRIPTION
 *   Creates a linear record file.
 *   Using this function, an ISO EF IDentifier is specified as well as a legacy DESFIRE File ID.
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_CreateIsoLinearRecordFile(BYTE file_id,
 *                                                 WORD iso_ef_id,
 *                                                 BYTE comm_mode,
 *                                                 WORD access_rights,
 *                                                 DWORD record_size,
 *                                                 DWORD max_records);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_CreateIsoLinearRecordFile(SPROX_INSTANCE rInst,
 *                                                  WORD iso_ef_id,
 *                                                  BYTE file_id,
 *                                                  BYTE comm_mode,
 *                                                  WORD access_rights,
 *                                                  DWORD record_size,
 *                                                  DWORD max_records);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_CreateIsoLinearRecordFile(SCARDHANDLE hCard,
 *                                                WORD iso_ef_id,
 *                                                BYTE file_id,
 *                                                BYTE comm_mode,
 *                                                WORD access_rights,
 *                                                DWORD record_size,
 *                                                DWORD max_records);
 *
 * INPUTS
 *   BYTE file_id                : DESFIRE File IDentifier
 *   WORD iso_ef_id              : IDentifier of the EF for ISO 7816-4 applications
 *   BYTE comm_mode              : file's Communication Settings
 *   WORD access_rights          : file's Access Rights
 *   DWORD record_size           : size of one single record in bytes
 *   DWORD max_records           : maximum number of records
 *
 * RETURNS
 *   DF_OPERATION_OK    : file created successfully
 *   Other code if internal or communication error has occured.
 *
 * NOTES
 *   Please refer to DESFIRE datasheet for details regarding this function and its
 *   acceptable parameters.
 *
 * SEE ALSO
 *   CreateStdDataFile
 *   CreateBackupDataFile
 *   CreateValueFile
 *   CreateCyclicRecordFile
 *   DeleteFile
 *
 **/
SPROX_API_FUNC(Desfire_CreateIsoLinearRecordFile) (SPROX_PARAM  BYTE file_id, WORD iso_ef_id, BYTE comm_mode, WORD access_rights, DWORD record_size, DWORD max_records)
{
  SPROX_DESFIRE_GET_CTX();

  Desfire_PrepareCreateFileCommand(SPROX_PARAM_P  DF_CREATE_LINEAR_RECORD_FILE, file_id, iso_ef_id, comm_mode, access_rights);

  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (record_size & 0x000000FF); record_size >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (record_size & 0x000000FF); record_size >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (record_size & 0x000000FF);

  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (max_records & 0x000000FF); max_records >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (max_records & 0x000000FF); max_records >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (max_records & 0x000000FF);

  /* Communicate the info block to the card and check the operation's return status. */
  return SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  0, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
}

/**f* DesfireAPI/CreateCyclicRecordFile
 *
 * NAME
 *   CreateCyclicRecordFile
 *
 * DESCRIPTION
 *   Creates a cyclic record file
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_CreateCyclicRecordFile(BYTE file_id,
 *                                              BYTE comm_mode,
 *                                              WORD access_rights,
 *                                              DWORD record_size,
 *                                              DWORD max_records);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_CreateCyclicRecordFile(SPROX_INSTANCE rInst,
 *                                               BYTE file_id,
 *                                               BYTE comm_mode,
 *                                               WORD access_rights,
 *                                               DWORD record_size,
 *                                               DWORD max_records);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_CreateCyclicRecordFile(SCARDHANDLE hCard,
 *                                             BYTE file_id,
 *                                             BYTE comm_mode,
 *                                             WORD access_rights,
 *                                             DWORD record_size,
 *                                             DWORD max_records);
 *
 * INPUTS
 *   BYTE file_id                : DESFIRE File IDentifier
 *   BYTE comm_mode              : file's Communication Settings
 *   WORD access_rights          : file's Access Rights
 *   DWORD file_size             : size of the file in bytes
 *   DWORD record_size           : size of one single record in bytes
 *   DWORD max_records           : maximum number of records
 *
 * RETURNS
 *   DF_OPERATION_OK    : file created successfully
 *   Other code if internal or communication error has occured.
 *
 * NOTES
 *   Please refer to DESFIRE datasheet for details regarding this function and its
 *   acceptable parameters.
 *
 * SEE ALSO
 *   CreateStdDataFile
 *   CreateBackupDataFile
 *   CreateValueFile
 *   CreateLinearRecordFile
 *   DeleteFile
 *
 **/
SPROX_API_FUNC(Desfire_CreateCyclicRecordFile) (SPROX_PARAM  BYTE file_id, BYTE comm_mode, WORD access_rights, DWORD record_size, DWORD max_records)
{
  SPROX_DESFIRE_GET_CTX();

  Desfire_PrepareCreateFileCommand(SPROX_PARAM_P  DF_CREATE_CYCLIC_RECORD_FILE, file_id, (DWORD) -1, comm_mode, access_rights);

  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (record_size & 0x000000FF); record_size >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (record_size & 0x000000FF); record_size >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (record_size & 0x000000FF);

  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (max_records & 0x000000FF); max_records >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (max_records & 0x000000FF); max_records >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (max_records & 0x000000FF);

  /* Communicate the info block to the card and check the operation's return status. */
  return SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  0, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
}

/**f* DesfireAPI/CreateIsoCyclicRecordFile
 *
 * NAME
 *   CreateIsoCyclicRecordFile
 *
 * DESCRIPTION
 *   Creates a cyclic record file.
 *   Using this function, an ISO EF IDentifier is specified as well as a legacy DESFIRE File ID.
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_CreateIsoCyclicRecordFile(BYTE file_id,
 *                                                 WORD iso_ef_id,
 *                                                 BYTE comm_mode,
 *                                                 WORD access_rights,
 *                                                 DWORD record_size,
 *                                                 DWORD max_records);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_CreateIsoCyclicRecordFile(SPROX_INSTANCE rInst,
 *                                                  WORD iso_ef_id,
 *                                                  BYTE file_id,
 *                                                  BYTE comm_mode,
 *                                                  WORD access_rights,
 *                                                  DWORD record_size,
 *                                                  DWORD max_records);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_CreateIsoCyclicRecordFile(SCARDHANDLE hCard,
 *                                                WORD iso_ef_id,
 *                                                BYTE file_id,
 *                                                BYTE comm_mode,
 *                                                WORD access_rights,
 *                                                DWORD record_size,
 *                                                DWORD max_records);
 *
 * INPUTS
 *   BYTE file_id                : DESFIRE File IDentifier
 *   WORD iso_ef_id              : IDentifier of the EF for ISO 7816-4 applications
 *   BYTE comm_mode              : file's Communication Settings
 *   WORD access_rights          : file's Access Rights
 *   DWORD file_size             : size of the file in bytes
 *   DWORD record_size           : size of one single record in bytes
 *   DWORD max_records           : maximum number of records
 *
 * RETURNS
 *   DF_OPERATION_OK    : file created successfully
 *   Other code if internal or communication error has occured.
 *
 * NOTES
 *   Please refer to DESFIRE datasheet for details regarding this function and its
 *   acceptable parameters.
 *
 * SEE ALSO
 *   CreateStdDataFile
 *   CreateBackupDataFile
 *   CreateValueFile
 *   CreateLinearRecordFile
 *   DeleteFile
 *
 **/
SPROX_API_FUNC(Desfire_CreateIsoCyclicRecordFile) (SPROX_PARAM  BYTE file_id, WORD iso_ef_id, BYTE comm_mode, WORD access_rights, DWORD record_size, DWORD max_records)
{
  SPROX_DESFIRE_GET_CTX();

  Desfire_PrepareCreateFileCommand(SPROX_PARAM_P  DF_CREATE_CYCLIC_RECORD_FILE, file_id, iso_ef_id, comm_mode, access_rights);

  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (record_size & 0x000000FF); record_size >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (record_size & 0x000000FF); record_size >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (record_size & 0x000000FF);

  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (max_records & 0x000000FF); max_records >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (max_records & 0x000000FF); max_records >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (max_records & 0x000000FF);

  /* Communicate the info block to the card and check the operation's return status. */
  return SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  0, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
}

/**f* DesfireAPI/DeleteFile
 *
 * NAME
 *   DeleteFile
 *
 * DESCRIPTION
 *   Permanently deactivates a file within the file directory of the currently selected application
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_DeleteFile(BYTE file_id);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_DeleteFile(SPROX_INSTANCE rInst,
 *                                   BYTE file_id);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_DeleteFile(SCARDHANDLE hCard,
 *                                 BYTE file_id);
 *
 * INPUTS
 *   BYTE file_id                : DESFIRE File IDentifier
 *
 * RETURNS
 *   DF_OPERATION_OK    : File deleted successfully
 *   Other code if internal or communication error has occured.
 *
 * NOTES
 *   Please refer to DESFIRE datasheet for details regarding this function and its
 *   acceptable parameters.
 *
 * SEE ALSO
 *   CreateStdDataFile
 *   CreateBackupDataFile
 *   CreateValueFile
 *   CreateLinearRecordFile
 *   CreateCyclicRecordFile
 *
 **/
SPROX_API_FUNC(Desfire_DeleteFile) (SPROX_PARAM  BYTE file_id)
{
  SPROX_DESFIRE_GET_CTX();

  /* Create the info block containing the command code and the given parameters. */
  ctx->xfer_buffer[INF + 0] = DF_DELETE_FILE;
  ctx->xfer_buffer[INF + 1] = file_id;
  ctx->xfer_length = 2;

  /* Communicate the info block to the card and check the operation's return status. */
  return SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  0, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
}


/**t* DesfireAPI/DF_ADDITIONAL_FILE_SETTINGS
 *
 * NAME
 *   DF_ADDITIONAL_FILE_SETTINGS
 *
 * DESCRIPTION
 *   Union for returning the information supplied by the GetFileSettings command.
 *   Use stDataFileSettings for Standard Data Files and Backup Data Files.
 *   Use stValueFileSettings for Value Files.
 *   Use stRecordFileSettings for Linear Record Files and Cyclic Record Files.
 *
 * SOURCE
 *   typedef union
 *   {
 *     struct
 *     {
 *       DWORD   eFileSize;             //user file size
 *     } stDataFileSettings;
 *
 *     struct
 *     {
 *       LONG    lLowerLimit;           // lower limit of the file
 *       LONG    lUpperLimit;           // upper limit of the file
 *       DWORD   eLimitedCredit;        // limited credit value
 *       BYTE    bLimitedCreditEnabled; // limited credit enabled
 *     } stValueFileSettings;
 *
 *     struct
 *     {
 *       DWORD   eRecordSize;           // record size
 *       DWORD   eMaxNRecords;          // maximum number of records
 *       DWORD   eCurrNRecords;         // current number of records
 *     } stRecordFileSettings;
 *   } DF_ADDITIONAL_FILE_SETTINGS;
 *
 **/

/**f* DesfireAPI/GetFileSettings
 *
 * NAME
 *   GetFileSettings
 *
 * DESCRIPTION
 *   Get information on the properties of a specific file
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_GetFileSettings(BYTE file_id,
 *                                     BYTE *file_type,
 *                                     BYTE *comm_mode,
 *                                     WORD *access_rights,
 *                                     DF_ADDITIONAL_FILE_SETTINGS *additionnal_settings);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_GetFileSettings(SPROX_INSTANCE rInst,
 *                                     BYTE file_id,
 *                                     BYTE *file_type,
 *                                     BYTE *comm_mode,
 *                                     WORD *access_rights,
 *                                     DF_ADDITIONAL_FILE_SETTINGS *additionnal_settings);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_GetFileSettings(SCARDHANDLE hCard,
 *                                     BYTE file_id,
 *                                     BYTE *file_type,
 *                                     BYTE *comm_mode,
 *                                     WORD *access_rights,
 *                                     DF_ADDITIONAL_FILE_SETTINGS *additionnal_settings);
 *
 * INPUTS
 *   BYTE file_id                : DESFIRE File IDentifier
 *   BYTE *file_type             : type of file (DF_STANDARD_DATA_FILE, DF_BACKUP_DATA_FILE,
 *                                 DF_VALUE_FILE, DF_LINEAR_RECORD_FILE or DF_CYCLIC_RECORD_FILE)
 *   BYTE *comm_mode             : file's Communication Settings
 *   WORD *access_rights         : file's Access Rights
 *   DF_ADDITIONAL_FILE_SETTINGS *additionnal_settings : information file
 *
 * RETURNS
 *   DF_OPERATION_OK    : operation succeeded
 *   Other code if internal or communication error has occured.
 *
 * NOTES
 *   Please refer to DESFIRE datasheet for details regarding this function and the
 *   returned values.
 *
 * SEE ALSO
 *   GetFileIDs
 *   ChangeFileSettings
 *
 **/
SPROX_API_FUNC(Desfire_GetFileSettings) (SPROX_PARAM  BYTE file_id, BYTE *file_type, BYTE *comm_mode, WORD *access_rights, DF_ADDITIONAL_FILE_SETTINGS *additionnal_settings)
{
  BYTE    l_file_type;
  SPROX_RC   status;
  DWORD   temp;
  SPROX_DESFIRE_GET_CTX();

  if (file_type == NULL)
    file_type = &l_file_type;

  /* create command code and append FileID parameter */
  ctx->xfer_buffer[INF + 0] = DF_GET_FILE_SETTINGS;
  ctx->xfer_buffer[INF + 1] = file_id;
  ctx->xfer_length = 2;

  /* don't do any implicit length check on the response */
  /* this has to be resolved depending on the file type */
  status = SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  0, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
  if (status != DF_OPERATION_OK)
    return status;

  /* there is a fixed section of the response which is common to all file types */
  /* therefore the response must at least be of this fixed length               */
  /* Note the status byte had to be extract!                                    */
  ctx->xfer_length--;                 /* subtract 1 byte for the received status of desfire card */
  if (ctx->xfer_length < DF_FILE_SETTINGS_COMMON_LENGTH)
    return DFCARD_WRONG_LENGTH;

  /* extract the file's type */
  *file_type = ctx->xfer_buffer[INF + 1];

  /* extract the file's communication mode */
  if (comm_mode != NULL)
    *comm_mode = ctx->xfer_buffer[INF + 2];

  /* extract the file's access rights */
  if (access_rights != NULL)
  {
    *access_rights = ctx->xfer_buffer[INF + 4];
    *access_rights <<= 8;
    *access_rights += ctx->xfer_buffer[INF + 3];
  }

  /* if no pointer to a union for holding the file settings has been passed */
  /* we empty the buffer and return with success                            */
  if (additionnal_settings == NULL)
    return DF_OPERATION_OK;

  /* the rest of the buffer contains file type specific information      */
  /* therefore we have to resolve it according to the received file type */
  ctx->xfer_length -= DF_FILE_SETTINGS_COMMON_LENGTH; /* subtract for the fixed section */
  switch (*file_type)
  {
    case DF_STANDARD_DATA_FILE:
    case DF_BACKUP_DATA_FILE:

      if (ctx->xfer_length != DF_DATA_FILE_SETTINGS_LENGTH)
        return DFCARD_WRONG_LENGTH;

      /* extract the file size */
      temp   = ctx->xfer_buffer[INF + 7];
      temp <<= 8;
      temp += ctx->xfer_buffer[INF + 6];
      temp <<= 8;
      temp += ctx->xfer_buffer[INF + 5];
      additionnal_settings->stDataFileSettings.eFileSize = temp;
      break;

    case DF_VALUE_FILE:

      if (ctx->xfer_length != DF_VALUE_FILE_SETTINGS_LENGTH)
        return DFCARD_WRONG_LENGTH;

      /* extract the lower limit */
      temp   = ctx->xfer_buffer[INF + 8];
      temp <<= 8;
      temp += ctx->xfer_buffer[INF + 7];
      temp <<= 8;
      temp += ctx->xfer_buffer[INF + 6];
      temp <<= 8;
      temp += ctx->xfer_buffer[INF + 5];
      additionnal_settings->stValueFileSettings.lLowerLimit = temp;

      /* extract the upper limit */
      temp   = ctx->xfer_buffer[INF + 12];
      temp <<= 8;
      temp += ctx->xfer_buffer[INF + 11];
      temp <<= 8;
      temp += ctx->xfer_buffer[INF + 10];
      temp <<= 8;
      temp += ctx->xfer_buffer[INF + 9];
      additionnal_settings->stValueFileSettings.lUpperLimit = temp;

      /* extract the limited credit value; */
      temp = ctx->xfer_buffer[INF + 16];
      temp <<= 8;
      temp += ctx->xfer_buffer[INF + 15];
      temp <<= 8;
      temp += ctx->xfer_buffer[INF + 14];
      temp <<= 8;
      temp += ctx->xfer_buffer[INF + 13];
      additionnal_settings->stValueFileSettings.eLimitedCredit = temp;

      /* limited Credit enabled ? */
      additionnal_settings->stValueFileSettings.bLimitedCreditEnabled = ctx->xfer_buffer[INF + 17];
      break;

    case DF_LINEAR_RECORD_FILE:
    case DF_CYCLIC_RECORD_FILE:

      if (ctx->xfer_length != DF_RECORD_FILE_SETTINGS_LENGTH)
        return DFCARD_WRONG_LENGTH;

      /* extract the number of entire records */
      temp = ctx->xfer_buffer[INF + 7];
      temp <<= 8;
      temp += ctx->xfer_buffer[INF + 6];
      temp <<= 8;
      temp += ctx->xfer_buffer[INF + 5];
      additionnal_settings->stRecordFileSettings.eRecordSize = temp;

      /* get the size of one record */
      temp = ctx->xfer_buffer[INF + 10];
      temp <<= 8;
      temp += ctx->xfer_buffer[INF + 9];
      temp <<= 8;
      temp += ctx->xfer_buffer[INF + 8];
      additionnal_settings->stRecordFileSettings.eMaxNRecords = temp;

      /* get the current number of records */
      temp = ctx->xfer_buffer[INF + 13];
      temp <<= 8;
      temp += ctx->xfer_buffer[INF + 12];
      temp <<= 8;
      temp += ctx->xfer_buffer[INF + 11];
      additionnal_settings->stRecordFileSettings.eCurrNRecords = temp;

      break;

    default:
      /* Unknown file type */
      return DFCARD_WRONG_FILE_TYPE;
  }

  return DF_OPERATION_OK;
}

/**f* DesfireAPI/ChangeFileSettings
 *
 * NAME
 *   ChangeFileSettings
 *
 * DESCRIPTION
 *   Changes the access parameters of an existing file
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_ChangeFileSettings(BYTE file_id,
 *                                     BYTE comm_mode,
 *                                     WORD access_rights);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_ChangeFileSettings(SPROX_INSTANCE rInst,
 *                                     BYTE file_id,
 *                                     BYTE comm_mode,
 *                                     WORD access_rights);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_ChangeFileSettings(SCARDHANDLE hCard,
 *                                     BYTE file_id,
 *                                     BYTE comm_mode,
 *                                     WORD access_rights);
 *
 * INPUTS
 *   BYTE file_id                : DESFIRE File IDentifier
 *   BYTE comm_mode              : file's Communication Settings
 *   WORD access_rights          : file's Access Rights
 *
 * RETURNS
 *   DF_OPERATION_OK    : change succeeded
 *   Other code if internal or communication error has occured.
 *
 * NOTES
 *   Please refer to DESFIRE datasheet for details regarding this function and its
 *   acceptable parameters.
 *
 * SEE ALSO
 *   GetFileIDs
 *   GetFileSettings
 *
 **/
SPROX_API_FUNC(Desfire_ChangeFileSettings) (SPROX_PARAM  BYTE file_id, BYTE comm_mode, WORD access_rights)
{
  BOOL    comm_encrypted;
  SPROX_RC   status;
  WORD    old_access_rights;
  SPROX_DESFIRE_GET_CTX();

  /* Before being able to send the new file settings to the PICC, we need to know whether */
  /* the current access rights setting requires the communication to be encrypted.        */

  status = SPROX_API_CALL(Desfire_GetFileSettings) (SPROX_PARAM_P  file_id, NULL, NULL, &old_access_rights, NULL);
  if (status != DF_OPERATION_OK)
    return status;

  comm_encrypted = (old_access_rights & DF_CHANGE_RIGHTS_ACCESS_MASK);

  /* Create the info block containing the command code and the given parameters. */
  ctx->xfer_length = 0;

  ctx->xfer_buffer[ctx->xfer_length++] = DF_CHANGE_FILE_SETTINGS;
  ctx->xfer_buffer[ctx->xfer_length++] = file_id;
  ctx->xfer_buffer[ctx->xfer_length++] = comm_mode;

  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (access_rights & 0x00FF); access_rights >>= 8;
  ctx->xfer_buffer[ctx->xfer_length++] = (BYTE) (access_rights & 0x00FF);

  if (comm_encrypted != 0x0E)
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
        /* Non-ISO authenticated: don't send command  */
        SAM_EncipherData(ctx->sam_context.hSam, &ctx->xfer_buffer[2], ctx->xfer_length-2, 0, pbOut, &dwOutLength);
      }

      ctx->xfer_length = 0;
      ctx->xfer_buffer[ctx->xfer_length++] = DF_CHANGE_FILE_SETTINGS;
      ctx->xfer_buffer[ctx->xfer_length++] = file_id;
      for (i=0; i<dwOutLength; i++)
        ctx->xfer_buffer[ctx->xfer_length++] = pbOut[i];

    } else
#endif
    {
      /* If encrypted communication is requested, append a CRC, computed over the communication mode         */
      /* and access rights arguments (3bytes), to the command string. Then 'Encrypt' these arguments and the */
      /* CRC to ensure that only an authenticated PCD can change the settings.                               */
      Desfire_XferAppendCrc(SPROX_PARAM_P  2);
      Desfire_XferCipherSend(SPROX_PARAM_P  2);
    }

    /* Communicate the info block to the card and check the operation's return status. */
    return SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  0, CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);

  } else
  {
    /* Communicate the info block to the card and check the operation's return status. */
    return SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  0, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
  }
}


/**f* DesfireAPI/GetFileIDs
 *
 * NAME
 *   GetFileIDs
 *
 * DESCRIPTION
 *   Returns the DESFIRE File IDentifiers of all active files within the currently selected application
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_GetFileIDs(BYTE fid_max_count,
 *                                     BYTE fid_list[],
 *                                     BYTE *fid_count);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_GetFileIDs(SPROX_INSTANCE rInst,
 *                                     BYTE fid_max_count,
 *                                     BYTE fid_list[],
 *                                     BYTE *fid_count);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_GetFileIDs(SCARDHANDLE hCard,
 *                                     BYTE fid_max_count,
 *                                     BYTE fid_list[],
 *                                     BYTE *fid_count);
 *
 * INPUTS
 *   BYTE fid_max_count          : maximum number of DESFIRE File IDentifiers
 *   BYTE fid_list[]             : DESFIRE File IDentifiers list
 *   BYTE *fid_count             : number of DESFIRE File IDentifiers in the selected application
 *
 * RETURNS
 *   DF_OPERATION_OK    : operation succeeded
 *   Other code if internal or communication error has occured.
 *
 * NOTES
 *   Please refer to DESFIRE datasheet for details regarding this function and the
 *   returned values.
 *
 * SEE ALSO
 *   GetFileSettings
 *   ChangeFileSettings
 *
 **/
SPROX_API_FUNC(Desfire_GetFileIDs) (SPROX_PARAM  BYTE fid_max_count, BYTE fid_list[], BYTE *fid_count)
{
	BYTE i;
  SPROX_RC   status;
  SPROX_DESFIRE_GET_CTX();

  /* reset the number of found FileIDs */
	if (fid_count != NULL)
    *fid_count = 0;

  /* create the info block containing the command code */
  ctx->xfer_buffer[INF + 0] = DF_GET_FILE_IDS;
  ctx->xfer_length = 1;

  /* this command terminates within a single frame (no additional frames necessary) */
  status = SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  0, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
  if (status != DF_OPERATION_OK)
    return status;

  for (i = 0; (BYTE) (INF + 1 + i) < ctx->xfer_length; i++)
  {
		if ((fid_list != NULL) && (i < fid_max_count))
		  fid_list[i] = ctx->xfer_buffer[INF + 1 + i];
  }

	if (fid_count != NULL)
		*fid_count = i;

  if (i > fid_max_count)
    return DFCARD_OVERFLOW;

  return DF_OPERATION_OK;
}

/**f* DesfireAPI/GetIsoFileIDs
 *
 * NAME
 *   GetIsoFileIDs
 *
 * DESCRIPTION
 *   Returns the File IDentifiers of all active files within the currently selected application
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_GetIsoFileIDs(BYTE fid_max_count,
 *                                     WORD fid_list[],
 *                                     BYTE *fid_count);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_GetIsoFileIDs(SPROX_INSTANCE rInst,
 *                                      BYTE fid_max_count,
 *                                      WORD fid_list[],
 *                                      BYTE *fid_count);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_GetIsoFileIDs(SCARDHANDLE hCard,
 *                                    BYTE fid_max_count,
 *                                    WORD fid_list[],
 *                                    BYTE *fid_count);
 *
 * INPUTS
 *   BYTE fid_max_count          : maximum number of File IDentifiers
 *   WORD fid_list[]             : File IDentifiers list
 *   BYTE *fid_count             : number of File IDentifiers in the selected application
 *
 * RETURNS
 *   DF_OPERATION_OK    : operation succeeded
 *   Other code if internal or communication error has occured.
 *
 * SEE ALSO
 *   GetFileIDs
 *   GetIsoDFNames
 *
 **/
SPROX_API_FUNC(Desfire_GetIsoFileIDs) (SPROX_PARAM  BYTE fid_max_count, WORD fid_list[], BYTE *fid_count)
{
	BYTE i, c = 0;
  SPROX_RC   status;
  SPROX_DESFIRE_GET_CTX();

  /* reset the number of found FileIDs */
	if (fid_count != NULL)
    *fid_count = 0;

  /* create the info block containing the command code */
  ctx->xfer_buffer[INF + 0] = DF_GET_ISO_FILE_IDS;
  ctx->xfer_length = 1;

again:

  status = SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  0, WANTS_OPERATION_OK | WANTS_ADDITIONAL_FRAME);
  if (status != DF_OPERATION_OK)
    return status;

  for (i = 0; (BYTE) (INF + 1 + i) < ctx->xfer_length; i+=2)
  {
		if ((fid_list != NULL) && (c < fid_max_count))
		{
		  fid_list[c]   = ctx->xfer_buffer[INF + 2 + i];
      fid_list[c] <<= 8;
			fid_list[c]  |= ctx->xfer_buffer[INF + 1 + i];
    }
		c++;
  }

  if (ctx->xfer_buffer[INF + 0] == DF_ADDITIONAL_FRAME)
  {
    ctx->xfer_length = 1;
    goto again;
  }

	if (fid_count != NULL)
		*fid_count = c;

  if (c > fid_max_count)
    return DFCARD_OVERFLOW;

  return DF_OPERATION_OK;
}
