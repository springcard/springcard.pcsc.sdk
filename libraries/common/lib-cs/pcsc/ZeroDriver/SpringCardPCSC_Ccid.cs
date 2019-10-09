/**h* SpringCard/PCSC_CcidOver
 *
 **/
using System;
using SpringCard.PCSC;

namespace SpringCard.PCSC.ZeroDriver
{
	public abstract class CCID
	{
		#region CCID constants
		public const byte EP_Control_To_RDR = 0x00;
		public const byte EP_Control_To_PC = 0x80;
		public const byte EP_Bulk_RDR_To_PC = 0x81;
		public const byte EP_Bulk_PC_To_RDR = 0x02;
		public const byte EP_Interrupt = 0x83;
		
		public const byte PC_TO_RDR_ICCPOWERON = 0x62;
		public const byte PC_TO_RDR_ICCPOWEROFF = 0x63;
		public const byte PC_TO_RDR_GETSLOTSTATUS = 0x65;
		public const byte PC_TO_RDR_XFRBLOCK = 0x6F;
		public const byte PC_TO_RDR_ESCAPE = 0x6B;
		
		public const byte RDR_TO_PC_DATABLOCK = 0x80;
		public const byte RDR_TO_PC_SLOTSTATUS = 0x81;
		public const byte RDR_TO_PC_ESCAPE = 0x83;
		
		public const byte GET_STATE = 0x00;
		public const byte GET_DESCRIPTOR = 0x06;
		public const byte SET_CONFIGURATION = 0x09;
		
		public const byte RDR_TO_PC_NOTIFYSLOTCHANGE = 0x50;
		#endregion
		
		#region CCID blocks

		public class PC_to_RDR_Block
		{
			public byte Message;
			public byte Slot;
			public byte Sequence;
			public byte Param1;
			public byte Param2;
			public byte Param3;
			public byte[] Data;
			
			public PC_to_RDR_Block(byte[] buffer)
			{
				this.Message = buffer[0];
				this.Slot = buffer[5];
				this.Sequence = buffer[6];
				this.Param1 = buffer[7];
				this.Param2 = buffer[8];
				this.Param3 = buffer[9];
				if (buffer.Length > 10)
				{
					this.Data = new byte[buffer.Length - 10];
					Array.Copy(buffer, 10, this.Data, 0, this.Data.Length);
				}
			}
		}
		
		public class RDR_to_PC_Block
		{
			public byte Message;
			public byte Slot;
			public byte Sequence;
			public byte Status;
			public byte Error;
			public byte Chain;
			public byte[] Data;
            public bool Secure;
			
			public RDR_to_PC_Block(byte[] buffer)
			{
				this.Message = buffer[0];
				uint Length = 0;
                Secure = ((buffer[4] & 0x80) != 0) ? true : false;
                Length += buffer[4]; Length *= 256;
				Length += buffer[3]; Length *= 256;
				Length += buffer[2]; Length *= 256;
				Length += buffer[1];
				this.Slot = buffer[5];
				this.Sequence = buffer[6];
				this.Status = buffer[7];
				this.Error = buffer[8];
				this.Chain = buffer[9];
				if (Length > 0)
				{
					this.Data = new byte[Length];
					Array.Copy(buffer, 10, this.Data, 0, Length);
				}
			}	

        }

		#endregion
	}
}
