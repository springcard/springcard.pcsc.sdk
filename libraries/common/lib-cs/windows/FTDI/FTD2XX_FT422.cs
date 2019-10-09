/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 26/07/2017
 * Time: 16:19
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace SpringCard.LibCs.Windows.FTDI
{
	public class FTDI_FT422
	{
		//**************************************************************************
		//
		// FUNCTION IMPORTS FROM FTD2XX DLL
		//
		//**************************************************************************

		[DllImport("ftd2xx.dll")]
		public static extern FTDI.FT_STATUS FT_CreateDeviceInfoList(ref UInt32 numdevs);

		[DllImport("ftd2xx.dll")]
		public static extern FTDI.FT_STATUS FT_GetDeviceInfoDetail(UInt32 index, ref UInt32 flags, ref FTDI.FT_DEVICE chiptype, ref UInt32 id, ref UInt32 locid, byte[] serialnumber, byte[] description, ref IntPtr ftHandle);

		//[DllImportAttribute("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		[DllImport("ftd2xx.dll")]
		public static extern FTDI.FT_STATUS FT_OpenEx(uint pvArg1, int dwFlags, ref IntPtr ftHandle);

		//[DllImportAttribute("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		[DllImport("ftd2xx.dll")]
		public static extern FTDI.FT_STATUS FT_Close(IntPtr ftHandle);


		public const byte FT_OPEN_BY_SERIAL_NUMBER = 1;
		public const byte FT_OPEN_BY_DESCRIPTION = 2;
		public const byte FT_OPEN_BY_LOCATION = 4;

		//**************************************************************************
		//
		// FUNCTION IMPORTS FROM LIBFT4222 DLL
		//
		//**************************************************************************

		[DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT4222_STATUS FT4222_I2CMaster_Init(IntPtr ftHandle, uint speed);

		[DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT4222_STATUS FT4222_I2CMaster_Write(IntPtr ftHandle, ushort slaveAddress, byte[] buffer, ushort bytesToWrite, ref ushort sizeTransferred);

		[DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT4222_STATUS FT4222_I2CMaster_Read(IntPtr ftHandle, ushort slaveAddress, byte[] buffer, ushort bytesToRead, ref ushort sizeTransferred);

		[DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT4222_STATUS FT4222_I2CMaster_GetStatus(IntPtr ftHandle, ref I2C_MasterStatus controllerStatus);

		[DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT4222_STATUS FT4222_I2CMaster_WriteEx(IntPtr ftHandle, ushort deviceAddress, byte flag, byte[] buffer, ushort bytesToWrite, ref ushort sizeTransferred);

		[DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT4222_STATUS FT4222_I2CMaster_ReadEx(IntPtr ftHandle, ushort deviceAddress, byte flag, byte[] buffer, ushort bytesToRead, ref ushort sizeTransferred);

		[DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT4222_STATUS FT4222_I2CMaster_Reset(IntPtr ftHandle);
		
		public enum I2C_MasterFlag
		{
			NONE = 0x80,
			START = 0x02,
			Repeated_START = 0x03,     // Repeated_START will not send master code in HS mode
			STOP  = 0x04,
			START_AND_STOP = 0x06      // START condition followed by SEND and STOP condition
		};

		public enum I2C_MasterStatus : byte
		{
			CONTROLLER_BUSY = 0x01,
			ERROR = 0x02,
			NO_ADDR_ACK = 0x04,
			NO_DATA_ACK = 0x08,
			ARBITRATION_LOST = 0x10,
			CONTROLLER_IDLE = 0x20,
			BUS_BUSY = 0x40
		};
		
		// FT4222 Device status
		public enum FT4222_STATUS
		{
			FT4222_OK,
			FT4222_INVALID_HANDLE,
			FT4222_DEVICE_NOT_FOUND,
			FT4222_DEVICE_NOT_OPENED,
			FT4222_IO_ERROR,
			FT4222_INSUFFICIENT_RESOURCES,
			FT4222_INVALID_PARAMETER,
			FT4222_INVALID_BAUD_RATE,
			FT4222_DEVICE_NOT_OPENED_FOR_ERASE,
			FT4222_DEVICE_NOT_OPENED_FOR_WRITE,
			FT4222_FAILED_TO_WRITE_DEVICE,
			FT4222_EEPROM_READ_FAILED,
			FT4222_EEPROM_WRITE_FAILED,
			FT4222_EEPROM_ERASE_FAILED,
			FT4222_EEPROM_NOT_PRESENT,
			FT4222_EEPROM_NOT_PROGRAMMED,
			FT4222_INVALID_ARGS,
			FT4222_NOT_SUPPORTED,
			FT4222_OTHER_ERROR,
			FT4222_DEVICE_LIST_NOT_READY,

			FT4222_DEVICE_NOT_SUPPORTED = 1000,        // FT_STATUS extending message
			FT4222_CLK_NOT_SUPPORTED,     // spi master do not support 80MHz/CLK_2
			FT4222_VENDER_CMD_NOT_SUPPORTED,
			FT4222_IS_NOT_SPI_MODE,
			FT4222_IS_NOT_I2C_MODE,
			FT4222_IS_NOT_SPI_SINGLE_MODE,
			FT4222_IS_NOT_SPI_MULTI_MODE,
			FT4222_WRONG_I2C_ADDR,
			FT4222_INVAILD_FUNCTION,
			FT4222_INVALID_POINTER,
			FT4222_EXCEEDED_MAX_TRANSFER_SIZE,
			FT4222_FAILED_TO_READ_DEVICE,
			FT4222_I2C_NOT_SUPPORTED_IN_THIS_MODE,
			FT4222_GPIO_NOT_SUPPORTED_IN_THIS_MODE,
			FT4222_GPIO_EXCEEDED_MAX_PORTNUM,
			FT4222_GPIO_WRITE_NOT_SUPPORTED,
			FT4222_GPIO_PULLUP_INVALID_IN_INPUTMODE,
			FT4222_GPIO_PULLDOWN_INVALID_IN_INPUTMODE,
			FT4222_GPIO_OPENDRAIN_INVALID_IN_OUTPUTMODE,
			FT4222_INTERRUPT_NOT_SUPPORTED,
			FT4222_GPIO_INPUT_NOT_SUPPORTED,
			FT4222_EVENT_NOT_SUPPORTED,
			
			FT4222_I2CM_ARB_LOST = 2000,
			FT4222_I2CM_ADDRESS_NACK,
			FT4222_I2CM_DATA_NACK,
			FT4222_I2CM_OTHER_ERROR,
			FT4222_I2CM_TIMEOUT,
		};
		
        public const byte I2CM_CONTROLLER_BUSY = 0x01;
        public const byte I2CM_ERROR = 0x02;
        public const byte I2CM_ADDRESS_NACK = 0x04;
        public const byte I2CM_DATA_NACK = 0x08;
        public const byte I2CM_ARB_LOST = 0x10;
        public const byte I2CM_IDLE = 0x20;
        public const byte I2CM_BUS_BUSY = 0x40;
		
		public static FT4222_STATUS FT4222_I2CMaster_Wait(IntPtr ftHandle)
		{
			FT4222_STATUS ft42Status = 0;
            I2C_MasterStatus i2cStatus = 0;
			int maxWait = 200;
			
			for (int i=0; i<maxWait; i++)
			{
				i2cStatus = 0;			
	
				ft42Status = FT4222_I2CMaster_GetStatus(ftHandle, ref i2cStatus);
				if (ft42Status != FT4222_STATUS.FT4222_OK)
				{
					Console.WriteLine("FT4222_I2CMaster_GetStatus: " + ft42Status);
					return ft42Status;				
				}
				
				/* I2C Master Controller Status
				 *   bit 0 = controller busy: all other status bits invalid
				 *   bit 1 = error condition
				 *   bit 2 = slave address was not acknowledged during last operation
				 *   bit 3 = data not acknowledged during last operation
				 *   bit 4 = arbitration lost during last operation
				 *   bit 5 = controller idle
				 *   bit 6 = bus busy
				 */
				if ((i2cStatus& I2C_MasterStatus.CONTROLLER_BUSY) != 0x00)
				{
					Thread.Sleep(5);					
					continue;
				}
				
				if ((i2cStatus & I2C_MasterStatus.BUS_BUSY) != 0x00)
				{
					Thread.Sleep(5);					
					continue;
				}

				if ((i2cStatus & I2C_MasterStatus.ARBITRATION_LOST) != 0x00)
					return FT4222_STATUS.FT4222_I2CM_ARB_LOST;

				if ((i2cStatus & I2C_MasterStatus.NO_ADDR_ACK) != 0x00)
					return FT4222_STATUS.FT4222_I2CM_ADDRESS_NACK;
				
				if ((i2cStatus & I2C_MasterStatus.NO_DATA_ACK) != 0x00)
					return FT4222_STATUS.FT4222_I2CM_DATA_NACK;
				
				if ((i2cStatus & I2C_MasterStatus.ERROR) != 0x00)
					return FT4222_STATUS.FT4222_I2CM_OTHER_ERROR;				
				
				if ((i2cStatus & I2C_MasterStatus.CONTROLLER_IDLE) != 0x00)
					return FT4222_STATUS.FT4222_OK;
			}
			
			return FT4222_STATUS.FT4222_I2CM_TIMEOUT;
		}
	}
}
