/**h* SpringProxAPI/CalypsoRadio
 *
 * NAME
 *   SpringProxAPI :: Working with Calypso cards (low level communication using Innovatron protocol)
 *
 * NOTES
 *   Most parameters of the SPROX_Calypso_xxx functions directly map to Calypso
 *   card internal functions.
 *   Please refer to Calypso official documentation for any details.
 * 
 **/
#include "sprox_calypso_i.h"
#include "products/springprox/api/springprox.h"

#ifdef USE_MICORE2
  #include "pcd_regs.h"
#else
  #include "hardware/micore1/micore1_regs.h"
#endif

#define CALYPSO_CMD_R      0x00
#define CALYPSO_CMD_RR     0x01
#define CALYPSO_CMD_DISC   0x03
#define CALYPSO_CMD_REPGEN 0x07
#define CALYPSO_CMD_ATTRIB 0x0F
#define CALYPSO_CMD_APGEN  0x0B
#define CALYPSO_CMD_REC    0x40
#define CALYPSO_CMD_RA     0xC0

#define CALYPSO_EXCHANGE_TMO     0x8000

//#define _DEBUG

SPROX_CALYPSO_FUNC_EX(ConfigureForCalypso) (SPROX_PARAM_V)
{
  SWORD   rc;
  SPROX_CALYPSO_GET_CTX();

  SPROX_API_CALL(SetConfig) (SPROX_PARAM_P  CFG_MODE_ISO_14443_A);
  rc = SPROX_API_CALL(SetConfig) (SPROX_PARAM_P  CFG_MODE_ISO_14443_B);
  if (rc)
    return rc;

#ifndef USE_MICORE2
  rc = SPROX_API_CALL(WriteRCRegister) (SPROX_PARAM_P  RegTypeBFraming, 0x23);
  if (rc)
    return rc;

  rc = SPROX_API_CALL(WriteRCRegister) (SPROX_PARAM_P  RegBPSKDemControl, 0xFE);
  if (rc)
    return rc;
#endif



  return SPROX_API_CALL(ControlRF) (SPROX_PARAM_P  TRUE);
}

SPROX_CALYPSO_FUNC_EX(Calypso_APGEN) (SPROX_PARAM  BYTE pcb_addr,
                                                   BOOL long_apgen,
                                                   BOOL with_atr,
                                                   BYTE occupancy,
                                                   BYTE repgen[],
                                                   BYTE *repgen_len,
                                                   BYTE picc_uid[4])
{
  BYTE    buffer[128];
  WORD    length;
  SWORD   rc;
  SPROX_CALYPSO_GET_CTX();

  memset(buffer, 0xCC, sizeof(buffer));

  length = sizeof(buffer);

  buffer[0] = (pcb_addr & 0x0F);
  buffer[1] = CALYPSO_CMD_APGEN;
  buffer[2] = (occupancy & 0x3F);
  if (long_apgen)
  {
    buffer[3] = 0x00;
    if (with_atr)
      buffer[3] |= 0x80;        // ATR requested
    rc = SPROX_API_CALL(B_Exchange) (SPROX_PARAM_P  buffer,
                                                    4 + 2,
                                                    buffer,
                                                    &length,
                                                    TRUE,
                                                    CALYPSO_EXCHANGE_TMO);
  } else
  {
    buffer[2] |= 0x40;          // short APGEN

    rc = SPROX_API_CALL(B_Exchange) (SPROX_PARAM_P  buffer,
                                                    3 + 2,
                                                    buffer,
                                                    &length,
                                                    TRUE,
                                                    CALYPSO_EXCHANGE_TMO);
  }

  if (!rc)
  {
    if (length >= 8)            // 2 for CRC, 2 for addr and cmd, 4 for UID
    {
      length -= 2;              // Remove CRC

      // Check address            
      if (buffer[0] != pcb_addr)
        return CALYPSO_ERR_WRONG_ADDR;

      // Check command, must be REPGEN
      if (buffer[1] != CALYPSO_CMD_REPGEN)
        return CALYPSO_ERR_WRONG_RESP;

      // Copy UID
      if (picc_uid != NULL)
        memcpy(picc_uid, &buffer[2], 4);
    } else
      return CALYPSO_ERR_WRONG_LENGTH;  // REPGEN too short

    // Copy the whole REPGEN frame
    if ((repgen != NULL) && (repgen_len != NULL))
    {
      length -= 2;              // Remove addr and cmd

      if (length > *repgen_len)
      {
        length = *repgen_len;
        rc = CALYPSO_ERR_OVERFLOW;
      }
      memcpy(repgen, &buffer[2], length);
      *repgen_len = (BYTE) length;
    }
    // After APGEN we start with a new sequence number
    ctx->picc_seq = 1;
  }

  return rc;
}

SPROX_CALYPSO_FUNC_EX(Calypso_ATTRIB) (SPROX_PARAM  BYTE pcd_addr,
                                                    BYTE picc_addr,
                                                    BYTE picc_uid[4])
{
  BYTE    buffer[254];
  WORD    length;
  SWORD   rc;
  SPROX_CALYPSO_GET_CTX();

  memset(buffer, 0xCC, sizeof(buffer));

  // Build frame header
  buffer[0] = ((picc_addr & 0x0F) << 4) | (pcd_addr & 0x0F);
  buffer[1] = CALYPSO_CMD_ATTRIB;
  // Append card's UID
  memcpy(&buffer[2], picc_uid, 4);

  length = sizeof(buffer);

  rc = SPROX_API_CALL(B_Exchange) (SPROX_PARAM_P  buffer,
                                                  8,  // 2 for CRC, 2 for addr and cmd, 4 for picc_uid
                                                  buffer,
                                                  &length,
                                                  TRUE,
                                                  CALYPSO_EXCHANGE_TMO);

  if (!rc)
  {
    if (length == 4)            // 2 for CRC, 2 for addr and cmd
    {
      length -= 2;

      // Check address
      if (buffer[0] != (((picc_addr & 0x0F) << 4) | (pcd_addr & 0x0F)))
        return CALYPSO_ERR_WRONG_ADDR;
      // Check command, must be RR
      if (buffer[1] != CALYPSO_CMD_RR)
      {
        {
          sprox_calypso_trace("SPROX_Calypso_ATTRIB: %02X%02X\n", buffer[0], buffer[1]);
        }
        return CALYPSO_ERR_WRONG_RESP;
      }
    } else
      return CALYPSO_ERR_WRONG_LENGTH;  // RR too short or too long
  }

  return rc;
}

SPROX_CALYPSO_FUNC_EX(Calypso_COM_R) (SPROX_PARAM  BYTE pcd_addr,
                                                   BYTE picc_addr,
                                                   const BYTE iso_in_data[],
                                                   BYTE iso_in_data_len,
                                                   BYTE iso_out_data[],
                                                   BYTE *iso_out_data_len)
{
  BYTE    buffer[254];
  WORD    length;
  SWORD   rc;
  SPROX_CALYPSO_GET_CTX();

  memset(buffer, 0xCC, sizeof(buffer));

  // Build frame header
  buffer[0] = ((picc_addr & 0x0F) << 4) | (pcd_addr & 0x0F);
  buffer[1] = CALYPSO_CMD_R | (ctx->picc_seq << 1);
  buffer[2] = iso_in_data_len + 1;
  // Append ISO data
  memcpy(&buffer[3], iso_in_data, iso_in_data_len);

  length = sizeof(buffer);

  rc = SPROX_API_CALL(B_Exchange) (SPROX_PARAM_P  buffer,
                                                  (WORD) (3 + iso_in_data_len + 2), // 2 for CRC, 2 for addr and cmd, 1 for data_len
                                                  buffer,
                                                  &length,
                                                  TRUE,
                                                  CALYPSO_EXCHANGE_TMO);

  if (!rc)
  {
    if (length >= 4)            // 2 for CRC, 2 for addr and cmd
    {
      length -= 2;

      // Check address
      if (buffer[0] != (((picc_addr & 0x0F) << 4) | (pcd_addr & 0x0F)))
        return CALYPSO_ERR_WRONG_ADDR;
      // Check command, must be REC
      if (buffer[1] != (CALYPSO_CMD_REC | (ctx->picc_seq << 1)))
        return CALYPSO_ERR_WRONG_RESP;
    } else
      return CALYPSO_ERR_WRONG_LENGTH;  // REC too short

    if (length >= 5)
    {
      // Copy the RESPONSE
      if ((iso_out_data != NULL) && (iso_out_data_len != NULL))
      {
        length -= 2;            // Remove addr and cmd

        // Check received length against claimed length
        if (length != buffer[2])
          rc = CALYPSO_ERR_WRONG_LENGTH;

        length -= 1;            // Remove len

        if (length > *iso_out_data_len)
        {
          length = *iso_out_data_len;
          rc = CALYPSO_ERR_OVERFLOW;
        }
        memcpy(iso_out_data, &buffer[3], length);
        *iso_out_data_len = (BYTE) length;
      }
    } else
    {
      // No data in RESPONSE
      if (iso_out_data_len != NULL)
        *iso_out_data_len = 0;
    }
  }

  if (rc == 0)
  {
    ctx->picc_seq++;
    ctx->picc_seq %= 8;
  } else
  {
    ctx->picc_seq = 0;
  }

  return rc;
}

SPROX_CALYPSO_FUNC_EX(Calypso_COM_RA) (SPROX_PARAM  BYTE pcd_addr,
                                                    BYTE picc_addr,
                                                    const BYTE picc_uid[4],
                                                    const BYTE iso_in_data[],
                                                    BYTE iso_in_data_len,
                                                    BYTE iso_out_data[],
                                                    BYTE *iso_out_data_len)
{
  BYTE    buffer[254];
  WORD    length;
  SWORD   rc;
  SPROX_CALYPSO_GET_CTX();

  memset(buffer, 0xCC, sizeof(buffer));

  // Build frame header
  buffer[0] = ((picc_addr & 0x0F) << 4) | (pcd_addr & 0x0F);
  buffer[1] = CALYPSO_CMD_RA | (ctx->picc_seq << 1);
  buffer[2] = iso_in_data_len + 1;
  // Append ISO data
  memcpy(&buffer[3], iso_in_data, iso_in_data_len);
  // Append UID
  memcpy(&buffer[3 + iso_in_data_len], picc_uid, 4);

  length = sizeof(buffer);

  rc = SPROX_API_CALL(B_Exchange) (SPROX_PARAM_P  buffer,
                                                  (WORD) (7 + iso_in_data_len + 2), // 2 for CRC, 2 for addr and cmd, 1 for data_len, 4 for picc_uid
                                                  buffer,
                                                  &length,
                                                  TRUE,
                                                  CALYPSO_EXCHANGE_TMO);

  if (!rc)
  {
    if (length >= 4)            // 2 for CRC, 2 for addr and cmd
    {
      length -= 2;

      // Check address
      if (buffer[0] != (((picc_addr & 0x0F) << 4) | (pcd_addr & 0x0F)))
        return CALYPSO_ERR_WRONG_ADDR;
      // Check command, must be REC
      if (buffer[1] != (CALYPSO_CMD_REC | (ctx->picc_seq << 1)))
        return CALYPSO_ERR_WRONG_RESP;
    } else
      return CALYPSO_ERR_WRONG_LENGTH;  // REC too short

    if (length >= 5)
    {
      // Copy the RESPONSE
      if ((iso_out_data != NULL) && (iso_out_data_len != NULL))
      {
        length -= 2;            // Remove addr and cmd

        // Check received length against claimed length
        if (length != buffer[2])
          rc = CALYPSO_ERR_WRONG_LENGTH;

        length -= 1;            // Remove len

        if (length > *iso_out_data_len)
        {
          length = *iso_out_data_len;
          rc = CALYPSO_ERR_OVERFLOW;
        }
        memcpy(iso_out_data, &buffer[3], length);
        *iso_out_data_len = (BYTE) length;
      }
    } else
    {
      // No data in RESPONSE
      if (iso_out_data_len != NULL)
        *iso_out_data_len = 0;
    }
  }

  if (rc == 0)
  {
    ctx->picc_seq++;
    ctx->picc_seq %= 8;
  } else
  {
    ctx->picc_seq = 0;
  }

  return rc;
}

SPROX_CALYPSO_FUNC_EX(Calypso_DISC) (SPROX_PARAM  BYTE pcd_addr, BYTE picc_addr)
{
  BYTE    buffer[254];
  WORD    length;
  SWORD   rc;
  SPROX_CALYPSO_GET_CTX();

  memset(buffer, 0xCC, sizeof(buffer));

  // Build frame header
  buffer[0] = ((picc_addr & 0x0F) << 4) | (pcd_addr & 0x0F);
  buffer[1] = CALYPSO_CMD_DISC;

  length = sizeof(buffer);

  rc = SPROX_API_CALL(B_Exchange) (SPROX_PARAM_P  buffer,
                                                  2 + 2,  // 2 for CRC, 2 for addr and cmd
                                                  buffer,
                                                  &length,
                                                  TRUE,
                                                  CALYPSO_EXCHANGE_TMO);

  if (!rc)
  {
    if (length == 4)            // 2 for CRC, 2 for addr and cmd
    {
      // Check address
      if (buffer[0] != (((picc_addr & 0x0F) << 4) | (pcd_addr & 0x0F)))
        return CALYPSO_ERR_WRONG_ADDR;
      // Check command, must be RR
      if (buffer[1] != CALYPSO_CMD_RR)
        return CALYPSO_ERR_WRONG_RESP;
    } else
      return CALYPSO_ERR_WRONG_LENGTH;  // RR too short or too long
  }

  return rc;
}

SPROX_CALYPSO_FUNC_EX(Calypso_WAIT) (SPROX_PARAM  BYTE pcd_addr, BYTE picc_addr)
{
  SPROX_CALYPSO_GET_CTX();
  return MI_UNKNOWN_FUNCTION;
}



