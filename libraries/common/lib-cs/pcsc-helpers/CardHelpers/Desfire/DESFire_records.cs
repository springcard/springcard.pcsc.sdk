/*
 * Created by SharpDevelop.
 * User: Jerome Izaac
 * Date: 14/09/2017
 * Time: 16:20
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace SpringCard.PCSC.CardHelpers
{
  /// <summary>
  /// Description of DESFire_records.
  /// </summary>
  public partial class Desfire
  {
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
     *   SUInt16 SPROX_Desfire_ClearRecordFile(byte file_id);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SUInt16 SPROXx_Desfire_ClearRecordFile(SPROX_INSTANCE rInst,
     *                                     byte file_id);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_ClearRecordFile(SCARDHANDLE hCard,
     *                                     byte file_id);
     *
     * INPUTS
     *   byte file_id      : File IDentifier
     *
     * RETURNS
     *   DF_OPERATION_OK   : success, data has been written
     *   Other code if internal or communication error has occured.
     *
     * NOTES
     *   Full "Read&Write" permission on the file is necessary for executing this command
     *
     **/
    public long ClearRecordFile(byte file_id)
    {
      xfer_buffer[INF + 0] = DF_CLEAR_RECORD_FILE;
      xfer_buffer[INF + 1] = file_id;
      xfer_length = 2;
    
      /* response must be of one byte length if successfully terminated */
      return Command(1, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
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
     *   SUInt16 SPROX_Desfire_ReadRecords(byte file_id,
     *                                   byte comm_mode,
     *                                   UInt32 from_record,
     *                                   UInt32 max_record_count,
     *                                   UInt32 record_size,
     *                                   byte data[],
     *                                   UInt32 *record_count);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SUInt16 SPROXx_Desfire_ReadRecords(SPROX_INSTANCE rInst,
     *                                    byte file_id,
     *                                    byte comm_mode,
     *                                    UInt32 from_record,
     *                                    UInt32 max_record_count,
     *                                    UInt32 record_size,
     *                                    byte data[],
     *                                    UInt32 *record_count);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_ReadRecords(SCARDHANDLE hCard,
     *                                  byte file_id,
     *                                  byte comm_mode,
     *                                  UInt32 from_record,
     *                                  UInt32 max_record_count,
     *                                  UInt32 record_size,
     *                                  byte data[],
     *                                  UInt32 *record_count);
     *
     * INPUTS
     *   byte file_id           : File IDentifier
     *   byte comm_mode         : file's communication settings (DF_COMM_MODE_PLAIN, DF_COMM_MODE_MACED,
     *                            DF_COMM_MODE_PLAIN2 or DF_COMM_MODE_ENCIPHERED)(see chapter 3.2 of
     *                            datasheet of mifare DesFire MF3ICD40 for more information)
     *   UInt32 from_record      : offset of the newest record to read. Set to 0 for latest record
     *   UInt32 max_record_count : number of records to be read from the PICC. Set to 0 to read all records.
     *   UInt32 record_size      : size of the record in bytes
     *   byte data[]            : buffer to receive the data
     *   UInt32 *record_count    : actual number of records read
     *
     * RETURNS
     *   DF_OPERATION_OK        : success, data has been read
     *   Other code if internal or communication error has occured.
     *
     * SEE ALSO
     *   ReadRecords2
     *
     **/
    public long ReadRecords(byte file_id, byte comm_mode, UInt32 from_record, UInt32 max_record_count, UInt32 record_size, ref byte[] data, ref UInt32 record_count)
    {
      byte    file_type = 0;
      UInt32   done_size = 0;
      long   status;
    
      /* if a pointer was passed for retrieving the number of records read    */
      /* and no record size was specified, we have to get the record size via */
      /* the GetFileSettings command                                          */
      if (record_size == 0)
      {
        byte old_comm_mode;
        ushort access_rights;
        DF_FILE_SETTINGS file_settings;
        status = GetFileSettings(file_id, out file_type, out old_comm_mode, out access_rights, out file_settings);

        if (status != DF_OPERATION_OK)
          return status;
    
        /* if the file type indicates that this is not a record file */
        /* we are not able to proceed                                */
        if ((file_type != DF_LINEAR_RECORD_FILE) && (file_type != DF_CYCLIC_RECORD_FILE))
          return DFCARD_WRONG_FILE_TYPE;
    
        record_size = file_settings.RecordFile.eRecordSize;
    
        /* if eRecordSize becomes zero under any circumstances this     */
        /* would lead to a division by zero at the end of this function */
        /* thus making it impossible to proceed                         */
        if (record_size == 0)
          return DFCARD_WRONG_RECORD_SIZE;
      }
    
      /* Call the ReadFile function */
      status = ReadDataEx(DF_READ_RECORDS, file_id, comm_mode, from_record, max_record_count, record_size, ref data, ref done_size);
      if (status != DF_OPERATION_OK)
        return status;
    
      /* we have to check whether we received multiples of record_size   */
      /* if this is not the case this means that a format error occurred */
      if ((done_size % record_size) != 0)
        return DFCARD_WRONG_LENGTH;
    
      /* calculate the number of records if a pointer has been passed */
      record_count = done_size / record_size;
    
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
     *   SUInt16 SPROX_Desfire_ReadRecords2(byte file_id,
     *                                    UInt32 from_record,
     *                                    UInt32 max_record_count,
     *                                    UInt32 record_size,
     *                                    byte data[],
     *                                    UInt32 *record_count);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SUInt16 SPROXx_Desfire_ReadRecords2(SPROX_INSTANCE rInst,
     *                                     byte file_id,
     *                                     UInt32 from_record,
     *                                     UInt32 max_record_count,
     *                                     UInt32 record_size,
     *                                     byte data[],
     *                                     UInt32 *record_count);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_ReadRecords2(SCARDHANDLE hCard,
     *                                   byte file_id,
     *                                   byte comm_mode,
     *                                   UInt32 from_record,
     *                                   UInt32 max_record_count,
     *                                   UInt32 record_size,
     *                                   byte data[],
     *                                   UInt32 *record_count);
     *
     * INPUTS
     *   byte file_id           : File IDentifier
     *   UInt32 from_record      : offset of the newest record to read. Set to 0 for latest record
     *   UInt32 max_record_count : number of records to be read from the PICC. Set to 0 to read all records.
     *   UInt32 record_size      : size of the record in bytes
     *   byte data[]            : buffer to receive the data
     *   UInt32 *record_count    : actual number of records read
     *
     * RETURNS
     *   DF_OPERATION_OK        : success, data has been read
     *   Other code if internal or communication error has occured.
     *
     * SEE ALSO
     *   ReadRecords
     *
     **/
    public long ReadRecords2(byte file_id, UInt32 from_record, UInt32 max_record_count, UInt32 record_size, ref byte[] data, ref UInt32 record_count)
    {
      byte    comm_mode = 0;
      UInt16    access_rights = 0;
      byte    read_only_access;
      byte    read_write_access;
      long   status;

      /* we have to receive the communications mode first */
      byte file_type;
      DF_FILE_SETTINGS file_settings;
      status = GetFileSettings(file_id, out file_type, out comm_mode, out access_rights, out file_settings);
      if (status != DF_OPERATION_OK)
        return status;
    
      /* Depending on the AccessRights field (settings r and r/w) we have to decide whether     */
      /* we are able to communicate in the mode indicated by comm_mode.                         */
      /* If auth_key does neither match r nor r/w and one of this settings                      */
      /* contains the value "ever" (0x0E) communication has to be done in plain mode regardless */
      /* of the mode indicated by comm_mode.                                                    */
      read_only_access  = (byte) ((access_rights & DF_READ_ONLY_ACCESS_MASK)  >> DF_READ_ONLY_ACCESS_SHIFT);
      read_write_access = (byte) ((access_rights & DF_READ_WRITE_ACCESS_MASK) >> DF_READ_WRITE_ACCESS_SHIFT);
    
      if ((read_only_access  != session_key_id)
       && (read_write_access != session_key_id)
       && ((read_only_access == 0x0E) || (read_write_access == 0x0E)))
      {
        comm_mode = DF_COMM_MODE_PLAIN;
      }
    
      /* Now execute the command */
      return ReadRecords(file_id, comm_mode, from_record, max_record_count, record_size, ref data, ref record_count);
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
     *   SUInt16 SPROX_Desfire_WriteRecord(byte file_id,
     *                                     byte comm_mode,
     *                                     UInt32 from_offset,
     *                                     UInt32 size,
     *                                     const byte data[]);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SUInt16 SPROXx_Desfire_WriteRecord(SPROX_INSTANCE rInst,
     *                                     byte file_id,
     *                                     byte comm_mode,
     *                                     UInt32 from_offset,
     *                                     UInt32 size,
     *                                     const byte data[]);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_WriteRecord(SCARDHANDLE hCard,
     *                                     byte file_id,
     *                                     byte comm_mode,
     *                                     UInt32 from_offset,
     *                                     UInt32 size,
     *                                     const byte data[]);
     *
     * INPUTS
     *   byte file_id      : File IDentifier
     *   byte comm_mode    : file's communication settings (DF_COMM_MODE_PLAIN, DF_COMM_MODE_MACED,
     *                       DF_COMM_MODE_PLAIN2 or DF_COMM_MODE_ENCIPHERED)(see chapter 3.2 of
     *                       datasheet of mifare DesFire MF3ICD40 for more information)
     *   UInt32 from_offset : offset within one single record in bytes
     *   UInt32 size        : size data to be written in bytes
     *   byte data[]       : buffer containing the data to write
     *
     * RETURNS
     *   DF_OPERATION_OK   : success, data has been written
     *   Other code if internal or communication error has occured.
     *
     * SEE ALSO
     *   WriteRecord2
     *
     **/
    public long WriteRecord(byte file_id, byte comm_mode, UInt32 from_offset, UInt32 size, byte[] data)
    {
      return WriteDataEx(DF_WRITE_RECORD, file_id, comm_mode, from_offset, size, data);
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
     *   SUInt16 SPROX_Desfire_WriteRecord2(byte file_id,
     *                                     UInt32 from_offset,
     *                                     UInt32 size,
     *                                     const byte data[]);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SUInt16 SPROXx_Desfire_WriteRecord2(SPROX_INSTANCE rInst,
     *                                     byte file_id,
     *                                     UInt32 from_offset,
     *                                     UInt32 size,
     *                                     const byte data[]);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_WriteRecord2(SCARDHANDLE hCard,
     *                                     byte file_id,
     *                                     UInt32 from_offset,
     *                                     UInt32 size,
     *                                     const byte data[]);
     *
     * INPUTS
     *   byte file_id      : File IDentifier
     *   UInt32 from_offset : offset within one single record in bytes
     *   UInt32 size        : size data to be written in bytes
     *   byte data[]       : buffer containing the data to write
     *
     * RETURNS
     *   DF_OPERATION_OK   : success, data has been written
     *   Other code if internal or communication error has occured.
     *
     * SEE ALSO
     *   WriteRecord
     *
     **/
    public long WriteRecord2(byte file_id, UInt32 from_offset, UInt32 size, byte[] data)
    {
      byte    comm_mode = 0;
      UInt16  access_rights=0;
      byte    write_only_access;
      byte    read_write_access;
      long    status;

      /* we have to receive the communications mode first */
      byte file_type;
      DF_FILE_SETTINGS file_settings;
      status = GetFileSettings(file_id, out file_type, out comm_mode, out access_rights, out file_settings);
      if (status != DF_OPERATION_OK)
        return status;
    
      /* Depending on the access_rights field (settings w and r/w) we have to decide whether   */
      /* we are able to communicate in the mode indicated by comm_mode.                        */
      /* If auth_key does neither match w nor r/w and one of this settings                */
      /* contains the value "ever" (0xE) communication has to be done in plain mode regardless */
      /* of the mode indicated by comm_mode.                                                   */
      write_only_access = (byte) ((access_rights & DF_WRITE_ONLY_ACCESS_MASK) >> DF_WRITE_ONLY_ACCESS_SHIFT);
      read_write_access = (byte) ((access_rights & DF_READ_WRITE_ACCESS_MASK) >> DF_READ_WRITE_ACCESS_SHIFT);
    
      if ((write_only_access != session_key_id)
       && (read_write_access != session_key_id)
       && ((write_only_access == 0x0E) || (read_write_access == 0x0E)))
      {
        comm_mode = DF_COMM_MODE_PLAIN;
      }
    
      /* Now execute the command */
      return WriteRecord(file_id, comm_mode, from_offset, size, data);
    }

    
  }
}
