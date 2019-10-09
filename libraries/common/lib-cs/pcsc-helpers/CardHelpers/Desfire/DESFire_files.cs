/*
 * Created by SharpDevelop.
 * User: Jerome Izaac
 * Date: 13/09/2017
 * Time: 15:49
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace SpringCard.PCSC.CardHelpers
{
  /// <summary>
  /// Description of DESFire_files.
  /// </summary>
  public partial class Desfire
  {
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

    public void PrepareCreateFileCommand(byte create_cmd, byte file_id, Int32 iso_ef_id, byte comm_mode, UInt16 access_rights)
    {
      xfer_length = 0;
    
      /* Create the info block containing the command code and the given parameters. */
      xfer_buffer[xfer_length++] = create_cmd;
    
      xfer_buffer[xfer_length++] = file_id;
    
      if ( iso_ef_id != (Int32) (-1) )
      {
        xfer_buffer[xfer_length++] = (byte) (iso_ef_id & 0x00FF); iso_ef_id >>= 8;
        xfer_buffer[xfer_length++] = (byte) (iso_ef_id & 0x00FF);
      }
    
      xfer_buffer[xfer_length++] = comm_mode;
    
      xfer_buffer[xfer_length++] = (byte) (access_rights & 0x00FF); access_rights >>= 8;
      xfer_buffer[xfer_length++] = (byte) (access_rights & 0x00FF);
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
     *   SUInt16 SPROX_Desfire_CreateStdDataFile(byte file_id,
     *                                         byte comm_mode,
     *                                         UInt16 access_rights,
     *                                         UInt32 file_size);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SUInt16 SPROXx_Desfire_CreateStdDataFile(SPROX_INSTANCE rInst,
     *                                          byte file_id,
     *                                          byte comm_mode,
     *                                          UInt16 access_rights,
     *                                          UInt32 file_size);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_CreateStdDataFile(SCARDHANDLE hCard,
     *                                        byte file_id,
     *                                        byte comm_mode,
     *                                        UInt16 access_rights,
     *                                        UInt32 file_size);
     *
     * INPUTS
     *   byte file_id                : DESFIRE File IDentifier
     *   byte comm_mode              : file's Communication Settings
     *   UInt16 access_rights          : file's Access Rights
     *   UInt32 file_size             : size of the file in bytes
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
    public long CreateStdDataFile(byte file_id, byte comm_mode, UInt16 access_rights, UInt32 file_size)
    {
      PrepareCreateFileCommand(DF_CREATE_STD_DATA_FILE, file_id, (Int32) (-1), comm_mode, access_rights);
    
      xfer_buffer[xfer_length++] = (byte) (file_size & 0x00FF); file_size >>= 8;
      xfer_buffer[xfer_length++] = (byte) (file_size & 0x00FF); file_size >>= 8;
      xfer_buffer[xfer_length++] = (byte) (file_size & 0x00FF);
    
      return Command(0, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
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
     *   SUInt16 SPROX_Desfire_CreateIsoStdDataFile(byte file_id,
     *                                            UInt16 iso_ef_id,
     *                                            byte comm_mode,
     *                                            UInt16 access_rights,
     *                                            UInt32 file_size);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SUInt16 SPROXx_Desfire_CreateIsoStdDataFile(SPROX_INSTANCE rInst,
     *                                             UInt16 iso_ef_id,
     *                                             byte file_id,
     *                                             byte comm_mode,
     *                                             UInt16 access_rights,
     *                                             UInt32 file_size);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_CreateIsoStdDataFile(SCARDHANDLE hCard,
     *                                           UInt16 iso_ef_id,
     *                                           byte file_id,
     *                                           byte comm_mode,
     *                                           UInt16 access_rights,
     *                                           UInt32 file_size);
     *
     * INPUTS
     *   byte file_id                : DESFIRE File IDentifier
     *   UInt16 iso_ef_id              : IDentifier of the EF for ISO 7816-4 applications
     *   byte comm_mode              : file's Communication Settings
     *   UInt16 access_rights          : file's Access Rights
     *   UInt32 file_size             : size of the file in bytes
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
    public long CreateIsoStdDataFile(byte file_id, UInt16 iso_ef_id, byte comm_mode, UInt16 access_rights, UInt32 file_size)
    {
      PrepareCreateFileCommand(DF_CREATE_STD_DATA_FILE, file_id, iso_ef_id, comm_mode, access_rights);
    
      xfer_buffer[xfer_length++] = (byte) (file_size & 0x00FF); file_size >>= 8;
      xfer_buffer[xfer_length++] = (byte) (file_size & 0x00FF); file_size >>= 8;
      xfer_buffer[xfer_length++] = (byte) (file_size & 0x00FF);
    
      return Command(0, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
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
     *   SUInt16 SPROX_Desfire_CreateBackupDataFile(byte file_id,
     *                                            byte comm_mode,
     *                                            UInt16 access_rights,
     *                                            UInt32 file_size);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SUInt16 SPROXx_Desfire_CreateBackupDataFile(SPROX_INSTANCE rInst,
     *                                             byte file_id,
     *                                             byte comm_mode,
     *                                             UInt16 access_rights,
     *                                             UInt32 file_size);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_CreateBackupDataFile(SCARDHANDLE hCard,
     *                                           byte file_id,
     *                                           byte comm_mode,
     *                                           UInt16 access_rights,
     *                                           UInt32 file_size);
     *
     * INPUTS
     *   byte file_id                : DESFIRE File IDentifier
     *   byte comm_mode              : file's Communication Settings
     *   UInt16 access_rights          : file's Access Rights
     *   UInt32 file_size             : size of the file in bytes
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
    public long CreateBackupDataFile(byte file_id, byte comm_mode, UInt16 access_rights, UInt32 file_size)
    {
      PrepareCreateFileCommand(DF_CREATE_BACKUP_DATA_FILE, file_id, (Int32) (-1), comm_mode, access_rights);
    
      xfer_buffer[xfer_length++] = (byte) (file_size & 0x000000FF); file_size >>= 8;
      xfer_buffer[xfer_length++] = (byte) (file_size & 0x000000FF); file_size >>= 8;
      xfer_buffer[xfer_length++] = (byte) (file_size & 0x000000FF);
    
      return Command(0, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
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
     *   SUInt16 SPROX_Desfire_CreateIsoBackupDataFile(byte file_id,
     *                                               UInt16 iso_ef_id,
     *                                               byte comm_mode,
     *                                               UInt16 access_rights,
     *                                               UInt32 file_size);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SUInt16 SPROXx_Desfire_CreateIsoBackupDataFile(SPROX_INSTANCE rInst,
     *                                                UInt16 iso_ef_id,
     *                                                byte file_id,
     *                                                byte comm_mode,
     *                                                UInt16 access_rights,
     *                                                UInt32 file_size);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_CreateIsoBackupDataFile(SCARDHANDLE hCard,
     *                                              UInt16 iso_ef_id,
     *                                              byte file_id,
     *                                              byte comm_mode,
     *                                              UInt16 access_rights,
     *                                              UInt32 file_size);
     *
     * INPUTS
     *   byte file_id                : DESFIRE File IDentifier
     *   UInt16 iso_ef_id              : IDentifier of the EF for ISO 7816-4 applications
     *   byte comm_mode              : file's Communication Settings
     *   UInt16 access_rights          : file's Access Rights
     *   UInt32 file_size             : size of the file in bytes
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
    public long CreateIsoBackupDataFile(byte file_id, UInt16 iso_ef_id, byte comm_mode, UInt16 access_rights, UInt32 file_size)
    {
      PrepareCreateFileCommand(DF_CREATE_BACKUP_DATA_FILE, file_id, iso_ef_id, comm_mode, access_rights);
    
      xfer_buffer[xfer_length++] = (byte) (file_size & 0x000000FF); file_size >>= 8;
      xfer_buffer[xfer_length++] = (byte) (file_size & 0x000000FF); file_size >>= 8;
      xfer_buffer[xfer_length++] = (byte) (file_size & 0x000000FF);
    
      return Command(0, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
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
     *   SUInt16 SPROX_Desfire_CreateValueFile(byte file_id,
     *                                       byte comm_mode,
     *                                       UInt16 access_rights,
     *                                       LONG lower_limit,
     *                                       LONG upper_limit,
     *                                       LONG initial_value,
     *                                       byte limited_credit_enabled);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SUInt16 SPROXx_Desfire_CreateValueFile(SPROX_INSTANCE rInst,
     *                                        byte file_id,
     *                                        byte comm_mode,
     *                                        UInt16 access_rights,
     *                                        LONG lower_limit,
     *                                        LONG upper_limit,
     *                                        LONG initial_value,
     *                                        byte limited_credit_enabled);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_CreateValueFile(SCARDHANDLE hCard,
     *                                      byte file_id,
     *                                      byte comm_mode,
     *                                      UInt16 access_rights,
     *                                      LONG lower_limit,
     *                                      LONG upper_limit,
     *                                      LONG initial_value,
     *                                      byte limited_credit_enabled);
     *
     * INPUTS
     *   byte file_id                : DESFIRE File IDentifier
     *   byte comm_mode              : file's Communication Settings
     *   UInt16 access_rights          : file's Access Rights
     *   LONG lower_limit            : lower limit which is valid for this file. The lower limit marks
     *                                 the boundary which must not be passed by a Debit calculation on
     *                                 the current value
     *   LONG upper_limit            : the upper limit which sets the boundary in the same manner but for
     *                                 the Credit operation
     *   LONG initial_value          : specifies the initial value of the value file
     *   byte limited_credit_enabled : activation of the LimitedCredit feature (0 disabled, 1 enabled)
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
    public long CreateValueFile(byte file_id, byte comm_mode, UInt16 access_rights, long lower_limit, long upper_limit, long initial_value, byte limited_credit_enabled)
    {
      PrepareCreateFileCommand(DF_CREATE_VALUE_FILE, file_id, -1, comm_mode, access_rights);
    
      xfer_buffer[xfer_length++] = (byte) (lower_limit & 0x000000FF); lower_limit >>= 8;
      xfer_buffer[xfer_length++] = (byte) (lower_limit & 0x000000FF); lower_limit >>= 8;
      xfer_buffer[xfer_length++] = (byte) (lower_limit & 0x000000FF); lower_limit >>= 8;
      xfer_buffer[xfer_length++] = (byte) (lower_limit & 0x000000FF);
    
      xfer_buffer[xfer_length++] = (byte) (upper_limit & 0x000000FF); upper_limit >>= 8;
      xfer_buffer[xfer_length++] = (byte) (upper_limit & 0x000000FF); upper_limit >>= 8;
      xfer_buffer[xfer_length++] = (byte) (upper_limit & 0x000000FF); upper_limit >>= 8;
      xfer_buffer[xfer_length++] = (byte) (upper_limit & 0x000000FF);
    
      xfer_buffer[xfer_length++] = (byte) (initial_value & 0x000000FF); initial_value >>= 8;
      xfer_buffer[xfer_length++] = (byte) (initial_value & 0x000000FF); initial_value >>= 8;
      xfer_buffer[xfer_length++] = (byte) (initial_value & 0x000000FF); initial_value >>= 8;
      xfer_buffer[xfer_length++] = (byte) (initial_value & 0x000000FF);
    
      xfer_buffer[xfer_length++] = limited_credit_enabled;
    
      return Command(0, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
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
     *   SUInt16 SPROX_Desfire_CreateLinearRecordFile(byte file_id,
     *                                     byte comm_mode,
     *                                     UInt16 access_rights,
     *                                     UInt32 record_size,
     *                                     UInt32 max_records);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SUInt16 SPROXx_Desfire_CreateLinearRecordFile(SPROX_INSTANCE rInst,
     *                                     byte file_id,
     *                                     byte comm_mode,
     *                                     UInt16 access_rights,
     *                                     UInt32 record_size,
     *                                     UInt32 max_records);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_CreateLinearRecordFile(SCARDHANDLE hCard,
     *                                     byte file_id,
     *                                     byte comm_mode,
     *                                     UInt16 access_rights,
     *                                     UInt32 record_size,
     *                                     UInt32 max_records);
     *
     * INPUTS
     *   byte file_id                : DESFIRE File IDentifier
     *   byte comm_mode              : file's Communication Settings
     *   UInt16 access_rights          : file's Access Rights
     *   UInt32 record_size           : size of one single record in bytes
     *   UInt32 max_records           : maximum number of records
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
    public long CreateLinearRecordFile(byte file_id, byte comm_mode, UInt16 access_rights, UInt32 record_size, UInt32 max_records)
    {
      PrepareCreateFileCommand(DF_CREATE_LINEAR_RECORD_FILE, file_id, -1, comm_mode, access_rights);
    
      xfer_buffer[xfer_length++] = (byte) (record_size & 0x000000FF); record_size >>= 8;
      xfer_buffer[xfer_length++] = (byte) (record_size & 0x000000FF); record_size >>= 8;
      xfer_buffer[xfer_length++] = (byte) (record_size & 0x000000FF);
    
      xfer_buffer[xfer_length++] = (byte) (max_records & 0x000000FF); max_records >>= 8;
      xfer_buffer[xfer_length++] = (byte) (max_records & 0x000000FF); max_records >>= 8;
      xfer_buffer[xfer_length++] = (byte) (max_records & 0x000000FF);
    
      /* Communicate the info block to the card and check the operation's return status. */
      return Command(0, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
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
     *   SUInt16 SPROX_Desfire_CreateIsoLinearRecordFile(byte file_id,
     *                                                 UInt16 iso_ef_id,
     *                                                 byte comm_mode,
     *                                                 UInt16 access_rights,
     *                                                 UInt32 record_size,
     *                                                 UInt32 max_records);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SUInt16 SPROXx_Desfire_CreateIsoLinearRecordFile(SPROX_INSTANCE rInst,
     *                                                  UInt16 iso_ef_id,
     *                                                  byte file_id,
     *                                                  byte comm_mode,
     *                                                  UInt16 access_rights,
     *                                                  UInt32 record_size,
     *                                                  UInt32 max_records);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_CreateIsoLinearRecordFile(SCARDHANDLE hCard,
     *                                                UInt16 iso_ef_id,
     *                                                byte file_id,
     *                                                byte comm_mode,
     *                                                UInt16 access_rights,
     *                                                UInt32 record_size,
     *                                                UInt32 max_records);
     *
     * INPUTS
     *   byte file_id                : DESFIRE File IDentifier
     *   UInt16 iso_ef_id              : IDentifier of the EF for ISO 7816-4 applications
     *   byte comm_mode              : file's Communication Settings
     *   UInt16 access_rights          : file's Access Rights
     *   UInt32 record_size           : size of one single record in bytes
     *   UInt32 max_records           : maximum number of records
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
    public long CreateIsoLinearRecordFile(byte file_id, UInt16 iso_ef_id, byte comm_mode, UInt16 access_rights, UInt32 record_size, UInt32 max_records)
    {
      PrepareCreateFileCommand(DF_CREATE_LINEAR_RECORD_FILE, file_id, iso_ef_id, comm_mode, access_rights);
    
      xfer_buffer[xfer_length++] = (byte) (record_size & 0x000000FF); record_size >>= 8;
      xfer_buffer[xfer_length++] = (byte) (record_size & 0x000000FF); record_size >>= 8;
      xfer_buffer[xfer_length++] = (byte) (record_size & 0x000000FF);
    
      xfer_buffer[xfer_length++] = (byte) (max_records & 0x000000FF); max_records >>= 8;
      xfer_buffer[xfer_length++] = (byte) (max_records & 0x000000FF); max_records >>= 8;
      xfer_buffer[xfer_length++] = (byte) (max_records & 0x000000FF);
    
      /* Communicate the info block to the card and check the operation's return status. */
      return Command(0, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
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
     *   SUInt16 SPROX_Desfire_CreateCyclicRecordFile(byte file_id,
     *                                              byte comm_mode,
     *                                              UInt16 access_rights,
     *                                              UInt32 record_size,
     *                                              UInt32 max_records);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SUInt16 SPROXx_Desfire_CreateCyclicRecordFile(SPROX_INSTANCE rInst,
     *                                               byte file_id,
     *                                               byte comm_mode,
     *                                               UInt16 access_rights,
     *                                               UInt32 record_size,
     *                                               UInt32 max_records);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_CreateCyclicRecordFile(SCARDHANDLE hCard,
     *                                             byte file_id,
     *                                             byte comm_mode,
     *                                             UInt16 access_rights,
     *                                             UInt32 record_size,
     *                                             UInt32 max_records);
     *
     * INPUTS
     *   byte file_id                : DESFIRE File IDentifier
     *   byte comm_mode              : file's Communication Settings
     *   UInt16 access_rights          : file's Access Rights
     *   UInt32 file_size             : size of the file in bytes
     *   UInt32 record_size           : size of one single record in bytes
     *   UInt32 max_records           : maximum number of records
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
    public long CreateCyclicRecordFile(byte file_id, byte comm_mode, UInt16 access_rights, UInt32 record_size, UInt32 max_records)
    {
      PrepareCreateFileCommand(DF_CREATE_CYCLIC_RECORD_FILE, file_id, -1, comm_mode, access_rights);
    
      xfer_buffer[xfer_length++] = (byte) (record_size & 0x000000FF); record_size >>= 8;
      xfer_buffer[xfer_length++] = (byte) (record_size & 0x000000FF); record_size >>= 8;
      xfer_buffer[xfer_length++] = (byte) (record_size & 0x000000FF);
    
      xfer_buffer[xfer_length++] = (byte) (max_records & 0x000000FF); max_records >>= 8;
      xfer_buffer[xfer_length++] = (byte) (max_records & 0x000000FF); max_records >>= 8;
      xfer_buffer[xfer_length++] = (byte) (max_records & 0x000000FF);
    
      /* Communicate the info block to the card and check the operation's return status. */
      return Command(0, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
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
     *   SUInt16 SPROX_Desfire_CreateIsoCyclicRecordFile(byte file_id,
     *                                                 UInt16 iso_ef_id,
     *                                                 byte comm_mode,
     *                                                 UInt16 access_rights,
     *                                                 UInt32 record_size,
     *                                                 UInt32 max_records);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SUInt16 SPROXx_Desfire_CreateIsoCyclicRecordFile(SPROX_INSTANCE rInst,
     *                                                  UInt16 iso_ef_id,
     *                                                  byte file_id,
     *                                                  byte comm_mode,
     *                                                  UInt16 access_rights,
     *                                                  UInt32 record_size,
     *                                                  UInt32 max_records);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_CreateIsoCyclicRecordFile(SCARDHANDLE hCard,
     *                                                UInt16 iso_ef_id,
     *                                                byte file_id,
     *                                                byte comm_mode,
     *                                                UInt16 access_rights,
     *                                                UInt32 record_size,
     *                                                UInt32 max_records);
     *
     * INPUTS
     *   byte file_id                : DESFIRE File IDentifier
     *   UInt16 iso_ef_id              : IDentifier of the EF for ISO 7816-4 applications
     *   byte comm_mode              : file's Communication Settings
     *   UInt16 access_rights          : file's Access Rights
     *   UInt32 file_size             : size of the file in bytes
     *   UInt32 record_size           : size of one single record in bytes
     *   UInt32 max_records           : maximum number of records
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
    public long CreateIsoCyclicRecordFile(byte file_id, UInt16 iso_ef_id, byte comm_mode, UInt16 access_rights, UInt32 record_size, UInt32 max_records)
    {
      PrepareCreateFileCommand(DF_CREATE_CYCLIC_RECORD_FILE, file_id, iso_ef_id, comm_mode, access_rights);
    
      xfer_buffer[xfer_length++] = (byte) (record_size & 0x000000FF); record_size >>= 8;
      xfer_buffer[xfer_length++] = (byte) (record_size & 0x000000FF); record_size >>= 8;
      xfer_buffer[xfer_length++] = (byte) (record_size & 0x000000FF);
    
      xfer_buffer[xfer_length++] = (byte) (max_records & 0x000000FF); max_records >>= 8;
      xfer_buffer[xfer_length++] = (byte) (max_records & 0x000000FF); max_records >>= 8;
      xfer_buffer[xfer_length++] = (byte) (max_records & 0x000000FF);
    
      /* Communicate the info block to the card and check the operation's return status. */
      return Command(0, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
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
     *   SUInt16 SPROX_Desfire_DeleteFile(byte file_id);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SUInt16 SPROXx_Desfire_DeleteFile(SPROX_INSTANCE rInst,
     *                                   byte file_id);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_DeleteFile(SCARDHANDLE hCard,
     *                                 byte file_id);
     *
     * INPUTS
     *   byte file_id                : DESFIRE File IDentifier
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
    public long DeleteFile(byte file_id)
    {
      /* Create the info block containing the command code and the given parameters. */
      xfer_buffer[INF + 0] = DF_DELETE_FILE;
      xfer_buffer[INF + 1] = file_id;
      xfer_length = 2;
    
      /* Communicate the info block to the card and check the operation's return status. */
      return Command(0, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
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
     *       UInt32   eFileSize;             //user file size
     *     } stDataFileSettings;
     *
     *     struct
     *     {
     *       LONG    lLowerLimit;           // lower limit of the file
     *       LONG    lUpperLimit;           // upper limit of the file
     *       UInt32   eLimitedCredit;        // limited credit value
     *       byte    bLimitedCreditEnabled; // limited credit enabled
     *     } stValueFileSettings;
     *
     *     struct
     *     {
     *       UInt32   eRecordSize;           // record size
     *       UInt32   eMaxNRecords;          // maximum number of records
     *       UInt32   eCurrNRecords;         // current number of records
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
     *   SUInt16 SPROX_Desfire_GetFileSettings(byte file_id,
     *                                     byte *file_type,
     *                                     byte *comm_mode,
     *                                     UInt16 *access_rights,
     *                                     DF_ADDITIONAL_FILE_SETTINGS *additionnal_settings);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SUInt16 SPROXx_Desfire_GetFileSettings(SPROX_INSTANCE rInst,
     *                                     byte file_id,
     *                                     byte *file_type,
     *                                     byte *comm_mode,
     *                                     UInt16 *access_rights,
     *                                     DF_ADDITIONAL_FILE_SETTINGS *additionnal_settings);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_GetFileSettings(SCARDHANDLE hCard,
     *                                     byte file_id,
     *                                     byte *file_type,
     *                                     byte *comm_mode,
     *                                     UInt16 *access_rights,
     *                                     DF_ADDITIONAL_FILE_SETTINGS *additionnal_settings);
     *
     * INPUTS
     *   byte file_id                : DESFIRE File IDentifier
     *   byte *file_type             : type of file (DF_STANDARD_DATA_FILE, DF_BACKUP_DATA_FILE,
     *                                 DF_VALUE_FILE, DF_LINEAR_RECORD_FILE or DF_CYCLIC_RECORD_FILE)
     *   byte *comm_mode             : file's Communication Settings
     *   UInt16 *access_rights         : file's Access Rights
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
    
    public long GetFileSettings(byte file_id, out byte file_type, out byte comm_mode, out UInt16 access_rights, out DF_FILE_SETTINGS additionnal_settings)
    {
      long   status;
      UInt32   temp;
      
      file_type = 0;
      comm_mode = 0;
      access_rights = 0;
      additionnal_settings = null;
    
      // create command code and append FileID parameter 
      xfer_buffer[INF + 0] = DF_GET_FILE_SETTINGS;
      xfer_buffer[INF + 1] = file_id;
      xfer_length = 2;
    
      // don't do any implicit length check on the response
      // this has to be resolved depending on the file type 
      status = Command(0, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
      if (status != DF_OPERATION_OK)
        return status;
    
      // there is a fixed section of the response which is common to all file types 
      // therefore the response must at least be of this fixed length              
      // Note the status byte had to be extract!                                  
      xfer_length--;                 // subtract 1 byte for the received status of desfire card
      if (xfer_length < DF_FILE_SETTINGS_COMMON_LENGTH)
        return DFCARD_WRONG_LENGTH;
    
      // extract the file's type 
      file_type = xfer_buffer[INF + 1];
    
      // extract the file's communication mode
      comm_mode = xfer_buffer[INF + 2];
    
      //extract the file's access rights 
      access_rights = xfer_buffer[INF + 4];
      access_rights <<= 8;
      access_rights += xfer_buffer[INF + 3];
    
      // the rest of the buffer contains file type specific information      
      // therefore we have to resolve it according to the received file type 
      xfer_length -= DF_FILE_SETTINGS_COMMON_LENGTH; // subtract for the fixed section 
      switch (file_type)
      {
        case DF_STANDARD_DATA_FILE:
        case DF_BACKUP_DATA_FILE:
    
          if (xfer_length != DF_DATA_FILE_SETTINGS_LENGTH)
            return DFCARD_WRONG_LENGTH;
          
          DF_DATA_FILE_SETTINGS data_settings = new DF_DATA_FILE_SETTINGS();          
    
          // extract the file size 
          temp   = xfer_buffer[INF + 7];
          temp <<= 8;
          temp += xfer_buffer[INF + 6];
          temp <<= 8;
          temp += xfer_buffer[INF + 5];
          
          data_settings.eFileSize = temp;
          
          additionnal_settings = new DF_FILE_SETTINGS(data_settings);          
          break;
    
        case DF_VALUE_FILE:
    
          if (xfer_length != DF_VALUE_FILE_SETTINGS_LENGTH)
            return DFCARD_WRONG_LENGTH;
          
          DF_VALUE_FILE_SETTINGS value_settings = new DF_VALUE_FILE_SETTINGS();
    
          // extract the lower limit 
          temp   = xfer_buffer[INF + 8];
          temp <<= 8;
          temp += xfer_buffer[INF + 7];
          temp <<= 8;
          temp += xfer_buffer[INF + 6];
          temp <<= 8;
          temp += xfer_buffer[INF + 5];
          value_settings.lLowerLimit = (Int32) temp;
    
          // extract the upper limit 
          temp   = xfer_buffer[INF + 12];
          temp <<= 8;
          temp += xfer_buffer[INF + 11];
          temp <<= 8;
          temp += xfer_buffer[INF + 10];
          temp <<= 8;
          temp += xfer_buffer[INF + 9];
          value_settings.lUpperLimit = (Int32) temp;
    
          // extract the limited credit value
          temp = xfer_buffer[INF + 16];
          temp <<= 8;
          temp += xfer_buffer[INF + 15];
          temp <<= 8;
          temp += xfer_buffer[INF + 14];
          temp <<= 8;
          temp += xfer_buffer[INF + 13];
          value_settings.eLimitedCredit = temp;
    
          // limited Credit enabled ?
          value_settings.bLimitedCreditEnabled = xfer_buffer[INF + 17];
          
          additionnal_settings = new DF_FILE_SETTINGS(value_settings);
          break;
    
        case DF_LINEAR_RECORD_FILE:
        case DF_CYCLIC_RECORD_FILE:
    
          if (xfer_length != DF_RECORD_FILE_SETTINGS_LENGTH)
            return DFCARD_WRONG_LENGTH;
          
          DF_RECORD_FILE_SETTINGS record_settings = new DF_RECORD_FILE_SETTINGS();
    
          // extract the number of entire records 
          temp = xfer_buffer[INF + 7];
          temp <<= 8;
          temp += xfer_buffer[INF + 6];
          temp <<= 8;
          temp += xfer_buffer[INF + 5];
          record_settings.eRecordSize = temp;
    
          // get the size of one record 
          temp = xfer_buffer[INF + 10];
          temp <<= 8;
          temp += xfer_buffer[INF + 9];
          temp <<= 8;
          temp += xfer_buffer[INF + 8];
          record_settings.eMaxNRecords = temp;
    
          // get the current number of records 
          temp = xfer_buffer[INF + 13];
          temp <<= 8;
          temp += xfer_buffer[INF + 12];
          temp <<= 8;
          temp += xfer_buffer[INF + 11];
          record_settings.eCurrNRecords = temp;
    
          additionnal_settings = new DF_FILE_SETTINGS(record_settings);
          break;
    
        default:
          // Unknown file type 
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
     *   SUInt16 SPROX_Desfire_ChangeFileSettings(byte file_id,
     *                                     byte comm_mode,
     *                                     UInt16 access_rights);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SUInt16 SPROXx_Desfire_ChangeFileSettings(SPROX_INSTANCE rInst,
     *                                     byte file_id,
     *                                     byte comm_mode,
     *                                     UInt16 access_rights);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_ChangeFileSettings(SCARDHANDLE hCard,
     *                                     byte file_id,
     *                                     byte comm_mode,
     *                                     UInt16 access_rights);
     *
     * INPUTS
     *   byte file_id                : DESFIRE File IDentifier
     *   byte comm_mode              : file's Communication Settings
     *   UInt16 access_rights          : file's Access Rights
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
    
    public long ChangeFileSettings(byte file_id, byte comm_mode, UInt16 access_rights)
    {
      UInt16    comm_encrypted;
      long   status;
      UInt16    old_access_rights=0;
    
      // Before being able to send the new file settings to the PICC, we need to know whether 
      // the current access rights setting requires the communication to be encrypted.       
      byte file_type;
      byte old_comm_mode;
      DF_FILE_SETTINGS file_settings;      
      status = GetFileSettings(file_id, out file_type, out old_comm_mode, out old_access_rights, out file_settings);
      if (status != DF_OPERATION_OK)
        return status;
    
      comm_encrypted = (ushort) (old_access_rights & DF_CHANGE_RIGHTS_ACCESS_MASK);
    
      // Create the info block containing the command code and the given parameters. 
      xfer_length = 0;
    
      xfer_buffer[xfer_length++] = DF_CHANGE_FILE_SETTINGS;
      xfer_buffer[xfer_length++] = file_id;
      xfer_buffer[xfer_length++] = comm_mode;
    
      xfer_buffer[xfer_length++] = (byte) (access_rights & 0x00FF); access_rights >>= 8;
      xfer_buffer[xfer_length++] = (byte) (access_rights & 0x00FF);
    
      if (comm_encrypted != 0x0E)
      {
        // If encrypted communication is requested, append a CRC, computed over the communication mode         
        // and access rights arguments (3bytes), to the command string. Then 'Encrypt' these arguments and the 
        // CRC to ensure that only an authenticated PCD can change the settings.                              
        XferAppendCrc(2);
        XferCipherSend(2);

        // Communicate the info block to the card and check the operation's return status.
        return Command(0, CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
    
      } else
      {
        // Communicate the info block to the card and check the operation's return status.
        return Command(0, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
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
     *   SUInt16 SPROX_Desfire_GetFileIDs(byte fid_max_count,
     *                                     byte fid_list[],
     *                                     byte *fid_count);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SUInt16 SPROXx_Desfire_GetFileIDs(SPROX_INSTANCE rInst,
     *                                     byte fid_max_count,
     *                                     byte fid_list[],
     *                                     byte *fid_count);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_GetFileIDs(SCARDHANDLE hCard,
     *                                     byte fid_max_count,
     *                                     byte fid_list[],
     *                                     byte *fid_count);
     *
     * INPUTS
     *   byte fid_max_count          : maximum number of DESFIRE File IDentifiers
     *   byte fid_list[]             : DESFIRE File IDentifiers list
     *   byte *fid_count             : number of DESFIRE File IDentifiers in the selected application
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
    public long GetFileIDs(byte fid_max_count, byte[] fid_list, ref byte fid_count)
    {
      byte i;
      long   status;

      /* reset the number of found FileIDs */
      fid_count = 0;
    
      /* create the info block containing the command code */
      xfer_buffer[INF + 0] = DF_GET_FILE_IDS;
      xfer_length = 1;
    
      /* this command terminates within a single frame (no additional frames necessary) */
      status = Command(0, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
      if (status != DF_OPERATION_OK)
        return status;
    
      for (i = 0; (byte) (INF + 1 + i) < xfer_length; i++)
        if ((fid_list != null) && (i < fid_max_count))
          fid_list[i] = xfer_buffer[INF + 1 + i];

      fid_count = i;
    
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
     *   SUInt16 SPROX_Desfire_GetIsoFileIDs(byte fid_max_count,
     *                                     UInt16 fid_list[],
     *                                     byte *fid_count);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SUInt16 SPROXx_Desfire_GetIsoFileIDs(SPROX_INSTANCE rInst,
     *                                      byte fid_max_count,
     *                                      UInt16 fid_list[],
     *                                      byte *fid_count);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_GetIsoFileIDs(SCARDHANDLE hCard,
     *                                    byte fid_max_count,
     *                                    UInt16 fid_list[],
     *                                    byte *fid_count);
     *
     * INPUTS
     *   byte fid_max_count          : maximum number of File IDentifiers
     *   UInt16 fid_list[]             : File IDentifiers list
     *   byte *fid_count             : number of File IDentifiers in the selected application
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
    public long GetIsoFileIDs(byte fid_max_count, UInt16[] fid_list, ref byte fid_count)
    {
      byte i, c = 0;
      long   status;

      /* reset the number of found FileIDs */
      fid_count = 0;
    
      /* create the info block containing the command code */
      xfer_buffer[INF + 0] = DF_GET_ISO_FILE_IDS;
      xfer_length = 1;
    
    again:
    
      status = Command(0, WANTS_OPERATION_OK | WANTS_ADDITIONAL_FRAME);
      if (status != DF_OPERATION_OK)
        return status;
    
      for (i = 0; (byte) (INF + 1 + i) < xfer_length; i+=2)
      {
        if ((fid_list != null) && (c < fid_max_count))
        {
          fid_list[c]   = xfer_buffer[INF + 2 + i];
          fid_list[c] <<= 8;
          fid_list[c]  |= xfer_buffer[INF + 1 + i];
        }
        c++;
      }
    
      if (xfer_buffer[INF + 0] == DF_ADDITIONAL_FRAME)
      {
        xfer_length = 1;
        goto again;
      }
    
      fid_count = c;
    
      if (c > fid_max_count)
        return DFCARD_OVERFLOW;
    
      return DF_OPERATION_OK;
    }

  }
}
