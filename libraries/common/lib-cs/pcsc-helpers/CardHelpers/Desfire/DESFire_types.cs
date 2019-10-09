/*
 * Created by SharpDevelop.
 * User: Jerome Izaac
 * Date: 08/09/2017
 * Time: 10:04
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Runtime.InteropServices;

namespace SpringCard.PCSC.CardHelpers
{
  /// <summary>
  /// Description of DESFire_types.
  /// </summary>
  public partial class Desfire
  {

    /*
     * DF_VERSION_INFO
     * ---------------
     * Structure for returning the information supplied by the GetVersion command.
     */

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct DF_VERSION_INFO
    {
		public byte    bHwVendorID;
		public byte    bHwType;
		public byte    bHwSubType;
		public byte    bHwMajorVersion;
		public byte    bHwMinorVersion;
		public byte    bHwStorageSize;
		public byte    bHwProtocol;
		public byte    bSwVendorID;
		public byte    bSwType;
		public byte    bSwSubType;
		public byte    bSwMajorVersion;
		public byte    bSwMinorVersion;
		public byte    bSwStorageSize;
		public byte    bSwProtocol;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
		public byte[]  abUid;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
		public byte[]  abBatchNo;
		public byte    bProductionCW;
		public byte    bProductionYear;
    } 

   
    /*
     * DF_ADDITIONAL_FILE_SETTINGS
     * ---------------------------
     * Union for returning the information supplied by the GetFileSettings command.
     * Use stDataFileSettings for Standard Data Files and Backup Data Files.
     * Use stValueFileSettings for Value Files.
     * Use stRecordFileSettings for Linear Record Files and Cyclic Record Files.
     */
     
	[StructLayout(LayoutKind.Sequential, Pack=1)]   
    public struct DF_DATA_FILE_SETTINGS
    {
      public UInt32  eFileSize;
    }
    
  	[StructLayout(LayoutKind.Sequential, Pack=1)]
  	public struct DF_VALUE_FILE_SETTINGS
  	{
      public Int32   lLowerLimit;
      public Int32   lUpperLimit;
      public UInt32  eLimitedCredit;
      public byte    bLimitedCreditEnabled;
  	}

  	[StructLayout(LayoutKind.Sequential, Pack=1)]
  	public struct DF_RECORD_FILE_SETTINGS
  	{
      public UInt32  eRecordSize;
      public UInt32  eMaxNRecords;
      public UInt32  eCurrNRecords;
    };
    
  	[StructLayout(LayoutKind.Sequential, Pack=1)]
  	public class DF_RAW_FILE_SETTINGS
  	{
  		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
  		public byte[] buffer;
  	}
  	
  	public class DF_FILE_SETTINGS
  	{
  		public enum FileSettingsType { Data, Value, Record };
  		public FileSettingsType Type { get; private set; }
  		public DF_DATA_FILE_SETTINGS DataFile { get; private set; }
  		public DF_FILE_SETTINGS(DF_DATA_FILE_SETTINGS DataFile)
  		{
  			this.DataFile = DataFile;
  			this.Type = FileSettingsType.Data;
  		}
  		public DF_VALUE_FILE_SETTINGS ValueFile { get; private set; }
  		public DF_FILE_SETTINGS(DF_VALUE_FILE_SETTINGS ValueFile)
  		{
  			this.ValueFile = ValueFile;
  			this.Type = FileSettingsType.Value;
  		}  		
  		public DF_RECORD_FILE_SETTINGS RecordFile { get; private set; }
  		public DF_FILE_SETTINGS(DF_RECORD_FILE_SETTINGS RecordFile)
  		{
  			this.RecordFile = RecordFile;
  			this.Type = FileSettingsType.Record;
  		}  		
  	}
   

    /*
     * DF_ISO_APPLICATION_ST
     * ---------------------
     * Structure for the GetIsoDFNames command
     */
    
    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct DF_ISO_APPLICATION_ST
    {
      public UInt32 dwAid;
      public UInt16  wIsoId;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
      public byte[]  abIsoName;
      public byte  bIsoNameLen;
    }


    
  
  }
}
