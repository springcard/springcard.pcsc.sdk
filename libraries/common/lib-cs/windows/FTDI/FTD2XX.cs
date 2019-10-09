using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace SpringCard.LibCs.Windows.FTDI
{
	public static class FTDI_FT422
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
		public static extern FT4222_STATUS FT4222_SetClock(IntPtr ftHandle, FT4222_ClockRate clk);

		[DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT4222_STATUS FT4222_GetClock(IntPtr ftHandle, ref FT4222_ClockRate clk);

		[DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT4222_STATUS FT4222_SPIMaster_Init(IntPtr ftHandle, FT4222_SPIMode ioLine, FT4222_SPIClock clock, FT4222_SPICPOL cpol, FT4222_SPICPHA cpha, Byte ssoMap);

		[DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT4222_STATUS FT4222_SPI_SetDrivingStrength(IntPtr ftHandle, SPI_DrivingStrength clkStrength, SPI_DrivingStrength ioStrength, SPI_DrivingStrength ssoStregth);

		[DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT4222_STATUS FT4222_SPIMaster_SingleReadWrite(IntPtr ftHandle, ref byte readBuffer, ref byte writeBuffer, ushort bufferSize, ref ushort sizeTransferred, bool isEndTransaction);

		[DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT4222_STATUS FT4222_I2CMaster_Init(IntPtr ftHandle, UInt32 kbps);

		[DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT4222_STATUS FT4222_I2CMaster_Read(IntPtr ftHandle, UInt16 deviceAddress, byte[] readBuffer, UInt16 bufferSize, ref UInt16 sizeTransferred);

		[DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT4222_STATUS FT4222_I2CMaster_Write(IntPtr ftHandle, UInt16 deviceAddress, byte[] readBuffer, UInt16 bufferSize, ref UInt16 sizeTransferred);

		[DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT4222_STATUS FT4222_I2CMaster_ReadEx(IntPtr ftHandle, UInt16 deviceAddress, byte flag, byte[] readBuffer, UInt16 bufferSize, ref UInt16 sizeTransferred);

		[DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT4222_STATUS FT4222_I2CMaster_WriteEx(IntPtr ftHandle, UInt16 deviceAddress, byte flag, byte[] readBuffer, UInt16 bufferSize, ref UInt16 sizeTransferred);

		[DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT4222_STATUS FT4222_I2CMaster_Reset(IntPtr ftHandle);

		[DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT4222_STATUS FT4222_I2CMaster_GetStatus(IntPtr ftHandle, ref FTDI_FT422.I2C_MasterStatus controllerStatus);


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
		};

		public enum FT4222_ClockRate
		{
			SYS_CLK_60 = 0,
			SYS_CLK_24,
			SYS_CLK_48,
			SYS_CLK_80,

		};

		public enum FT4222_SPIMode
		{
			SPI_IO_NONE = 0,
			SPI_IO_SINGLE = 1,
			SPI_IO_DUAL = 2,
			SPI_IO_QUAD = 4,

		};

		public enum FT4222_SPIClock
		{
			CLK_NONE = 0,
			CLK_DIV_2,      // 1/2   System Clock
			CLK_DIV_4,      // 1/4   System Clock
			CLK_DIV_8,      // 1/8   System Clock
			CLK_DIV_16,     // 1/16  System Clock
			CLK_DIV_32,     // 1/32  System Clock
			CLK_DIV_64,     // 1/64  System Clock
			CLK_DIV_128,    // 1/128 System Clock
			CLK_DIV_256,    // 1/256 System Clock
			CLK_DIV_512,    // 1/512 System Clock

		};

		public enum FT4222_SPICPOL
		{
			CLK_IDLE_LOW = 0,
			CLK_IDLE_HIGH = 1,
		};

		public enum FT4222_SPICPHA
		{
			CLK_LEADING = 0,
			CLK_TRAILING = 1,
		};

		public enum SPI_DrivingStrength
		{
			DS_4MA = 0,
			DS_8MA,
			DS_12MA,
			DS_16MA,
		};

		public enum I2C_MasterFlag
		{
			NONE = 0x80,
			START = 0x02,
			Repeated_START = 0x03,     // Repeated_START will not send master code in HS mode
			STOP = 0x04,
			START_AND_STOP = 0x06,      // START condition followed by SEND and STOP condition
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

	}
}
