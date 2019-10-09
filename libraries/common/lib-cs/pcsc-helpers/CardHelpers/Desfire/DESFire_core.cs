/*
 * Created by SharpDevelop.
 * User: Jerome Izaac
 * Date: 08/09/2017
 * Time: 14:03
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using SpringCard.LibCs;

namespace SpringCard.PCSC.CardHelpers
{
  /// <summary>
  /// Description of DESFire_core.
  /// </summary>
  public partial class Desfire
  {
    long Command(UInt16 accept_len, int flags)
    { 
      long status;

      if (xfer_length > DF_MAX_INFO_FRAME_SIZE)
        return DFCARD_LIB_CALL_ERROR;
      
      if ((session_type & KEY_ISO_MODE) == KEY_ISO_MODE)
        if ((flags & COMPUTE_COMMAND_CMAC) == COMPUTE_COMMAND_CMAC)
          XferCmacSend( ((flags & APPEND_COMMAND_CMAC) == APPEND_COMMAND_CMAC) ? true : false);

      
      status = Exchange();
      
      if (status != DF_OPERATION_OK)
        return status;

      /* Check whether the PICC's response consists of at least one byte.         */
      /* If the response is empty, we can not even determine the PICC's status.   */
      /* Therefore an empty response is always a length error.                    */
      if ((xfer_length < 1) || (xfer_length > MAX_INFO_FRAME_SIZE))
      {
        /* Error: block with inappropriate number of bytes received from the PICC. */
        return DFCARD_WRONG_LENGTH;
      }
      
      /*  Check the status byte received from the PICC.
      Depending on which of the flags (sgbWANTS_OPERATION_OK and sgbWANTS_ADDITIONAL_FRAME)
      are set in the bExpectedStatusFlags parameter, we accept either the status byte
      sgbOPERATION_OK or the status byte sgbDF_ADDITIONAL_FRAME or both of them. */
      if (!(((xfer_buffer[0] == DF_OPERATION_OK) && ((flags & WANTS_OPERATION_OK) != 0)) ||
            ((xfer_buffer[0] == DF_ADDITIONAL_FRAME) && ((flags & WANTS_ADDITIONAL_FRAME) != 0 ))))
      {
        /* Error: error or unexpected status received from the PICC. */
        return (DFCARD_ERROR - xfer_buffer[0]);
      }
      
      if ((session_type & KEY_ISO_MODE) != 0)
      {
        if ((flags & (CHECK_RESPONSE_CMAC|LOOSE_RESPONSE_CMAC)) != 0)
        {
          /* Go into CMAC stuff */
          /* If authenticated with SAM, Desfire_XferCmacRecv uses the SAM*/
          if (XferCmacRecv() != DF_OPERATION_OK)
          {
            if ((flags & LOOSE_RESPONSE_CMAC) != 0)
            {
              //memcpy(ctx->init_vector, &ctx->xfer_buffer[ctx->xfer_length], 8);
              Array.ConstrainedCopy(xfer_buffer, (int) xfer_length, init_vector, 0, 8);
            } else
            {
              return DFCARD_WRONG_MAC;
            }
          }
        }
      }
      
      /* Finally check the length of the response. */
      if ((accept_len != 0) && (accept_len != xfer_length))
      {
        /* Error: block with inappropriate number of bytes received from the PICC. */
        return DFCARD_WRONG_LENGTH;
      }
      
      return DF_OPERATION_OK;
    }
    
    long Exchange()
    {
      byte    card_status;
      UInt16  resp_data_len;

      byte[]   send_buffer = new byte[256];
      UInt16   send_length;
      byte[]   recv_buffer = new byte[256];
      UInt16   recv_length = (UInt16) recv_buffer.Length;
      
      if (xfer_length > 200)
        return SCARD_E_INSUFFICIENT_BUFFER;
      
      /* Build the APDU */
      /* -------------- */
      if (isoWrapping == DF_ISO_WRAPPING_CARD)
      {
        UInt32 l = (UInt32) send_buffer.Length;
        if (!BuildIsoApdu(xfer_buffer, xfer_length, send_buffer, ref l, false))
          return DFCARD_LIB_CALL_ERROR;
        
        send_length = (ushort) l;
      
      } else
      if (isoWrapping == DF_ISO_WRAPPING_READER)
      {
        UInt32 l = (UInt32) send_buffer.Length;
        
        if (!BuildIsoApdu(xfer_buffer, xfer_length, send_buffer, ref l, true))
          return DFCARD_LIB_CALL_ERROR;
        
        send_length = (ushort) l;
      
      } else
      {
        Array.ConstrainedCopy(xfer_buffer, 0, send_buffer, 0, (int) xfer_length);
        send_length = (ushort) xfer_length;
      }
      
      
      byte[] to_send = new byte[send_length];
      Array.ConstrainedCopy(send_buffer, 0, to_send, 0, send_length);
      
      Logger.Debug("<< " + BinConvert.ToHex(to_send));

      recv_buffer = transmitter.Transmit(to_send);
            
      if (recv_buffer != null)
      {
	      Logger.Debug(">> " + BinConvert.ToHex(recv_buffer));      
	      recv_length = (ushort) recv_buffer.Length;
      } else
      {
      	Logger.Debug(">> ERROR");
      	return -1;
      }
      
      /* Check status word */
      /* ----------------- */
      
      if (isoWrapping == DF_ISO_WRAPPING_CARD)
      {
        if (recv_length < 2)
          return DFCARD_PCSC_BAD_RESP_LEN;
        
        /* SW1 must be 91YY */
        if (recv_buffer[recv_length-2] != 0x91)
          return DFCARD_PCSC_BAD_RESP_SW;
        
        /* SW2 is the card status */
        card_status  = recv_buffer[recv_length-1];
        
        /* Retrieve card's data */
        resp_data_len = (UInt16) (recv_length - 2);

        /* Remember received size */
        xfer_length = (uint) (resp_data_len + 1);
        
        /* Copy the buffer */
        Array.ConstrainedCopy(recv_buffer, 0, xfer_buffer, 1, (int) xfer_length);
      
      } else
      if (isoWrapping == DF_ISO_WRAPPING_READER)
      {
        /* SW must be 9000 */
        UInt16 sw;
        
        if (recv_length < 2)
          return DFCARD_PCSC_BAD_RESP_LEN;
        
        sw = (ushort) ((recv_buffer[recv_length-2] * 0x0100) | recv_buffer[recv_length-1]);

        
        switch (sw)
        {
          case 0x9000 : break;
          
          case 0x6F01 :
          case 0x6F3D :
          case 0x6F51 :
          case 0x6F52 :
            return SCARD_W_REMOVED_CARD;
          
          case 0x6F02 :
          case 0x6F3E :
            return SCARD_E_COMM_DATA_LOST;
          
          case 0x6F47 :
            return SCARD_E_CARD_UNSUPPORTED;
          
          default     : return DFCARD_PCSC_BAD_RESP_SW;
        }
        
        if (recv_length < 3)
          return DFCARD_WRONG_LENGTH;
        
        /* Card status word is at the beginning of the buffer */
        card_status   = recv_buffer[0];
        
        /* Retrieve card's data */
        resp_data_len = (UInt16) (recv_length - 3);
        
        /* Remember received size */
        xfer_length = (uint) (resp_data_len + 1);
        
        /* Copy the buffer */
        Array.ConstrainedCopy(recv_buffer, 1, xfer_buffer, 1, (int) xfer_length);
              
      } else
      {
        if (recv_length < 1)
          return DFCARD_WRONG_LENGTH;
        
        card_status   = recv_buffer[0];
        resp_data_len = (UInt16) (recv_length - 1);

        /* Remember received size */
        xfer_length = (uint) (resp_data_len + 1);
        
        /* Copy the buffer */
        Array.ConstrainedCopy(recv_buffer, 0, xfer_buffer, 1, (int) xfer_length);
      }
      

      /* Check whether the PICC's response consists of at least one byte.
      If the response is empty, we can not even determine the PICC's status.
      Therefore an empty response is always a length error. */
      if ((xfer_length < 1) || (xfer_length > MAX_INFO_FRAME_SIZE))
      {
        /* Error: block with inappropriate number of bytes received from the PICC. */
        return DFCARD_WRONG_LENGTH;
      }
      
      /* Copy the buffer */
      xfer_buffer[0] = card_status;
      
      return DF_OPERATION_OK;
    }

  }
}
