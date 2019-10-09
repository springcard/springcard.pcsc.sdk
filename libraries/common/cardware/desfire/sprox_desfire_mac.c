/**h* DesfireAPI/MAC
 *
 * NAME
 *   DesfireAPI :: MAC functions
 *
 * COPYRIGHT
 *   (c) 2009 SpringCard - www.springcard.com
 *
 * DESCRIPTION
 *   High-level implementation of the DESFIRE EV0 MAC scheme.
 *
 **/

#include "sprox_desfire_i.h"

//#define _DEBUG_MAC

static void ComputeMacBlocks(SPROX_PARAM  const BYTE data[], DWORD block_count, BYTE result[])
{
  DWORD i, j;
  BYTE  carry[16], buffer[16];
  SPROX_DESFIRE_GET_CTX_V();

#ifdef _DEBUG_MAC
  {
    DWORD i;
    printf("MAC(");
    for (i=0; i<8*block_count; i++)
      printf("%02X", data[i]);
    printf(")\n");
  }
#endif

  switch (ctx->session_type)
  {
    case KEY_LEGACY_DES :
      {
        memset(carry, 0x00, 8);       /* IV <- 00...00   */
        for (i = 0; i < block_count; i++)
        {
          memcpy(buffer, &data[8 * i], 8);
          for (j = 0; j < 8; j++)
            buffer[j] ^= carry[j];    /* P  <- P XOR IV  */
          TDES_Encrypt2(&ctx->cipher_context.tdes, carry, buffer); /* IV <- DES(P)    */
        }

        memcpy(result, carry, 8);
      }
      break;

    case KEY_LEGACY_3DES :
      {
        memset(carry, 0x00, 8);       /* IV <- 00...00   */
        for (i = 0; i < block_count; i++)
        {
          memcpy(buffer, &data[8 * i], 8);
          for (j = 0; j < 8; j++)
            buffer[j] ^= carry[j];    /* P  <- P XOR IV  */
          TDES_Encrypt2(&ctx->cipher_context.tdes, carry, buffer);  /* IV <- 3DES(P)   */
        }

        memcpy(result, carry, 8);
      }
      break;

    default :
      printf("INVALID FUNCTION CALL %s %d\n", __FILE__, __LINE__);
      break;
  }

#ifdef _DEBUG_MAC
  {
    DWORD i;
    printf("\t->");
    for (i=0; i<8; i++)
      printf("%02X", result[i]);
    printf(")\n");
  }
#endif
}

void Desfire_ComputeMac(SPROX_PARAM  const BYTE data[], DWORD length, BYTE mac[4])
{
  BYTE  result[16];
  DWORD block_size;
  SPROX_DESFIRE_GET_CTX_V();

  block_size = ctx->session_type == KEY_ISO_AES ? 16 : 8;

  if (length % block_size)
  {
    DWORD block_count;
    BYTE *buffer;

    block_count = 1 + (length / block_size);

    buffer = malloc(block_size * block_count);
    if (buffer == NULL)
      return;

    memset(buffer, 0, block_size * block_count);
    memcpy(buffer, data, length);

    ComputeMacBlocks(SPROX_PARAM_P  buffer, block_count, result);

    free(buffer);

  } else
  {
    ComputeMacBlocks(SPROX_PARAM_P  data, length / block_size, result);
  }

  if (mac != NULL)
    memcpy(mac, result, 4);
}

SPROX_RC Desfire_VerifyMacRecv(SPROX_PARAM  BYTE recv_buffer[], DWORD *recv_length)
{
  BYTE  mac[4];
  DWORD length;

#ifdef SPROX_DESFIRE_WITH_SAM
  SPROX_DESFIRE_GET_CTX();
#endif

  length = *recv_length;

  if (length < 5)
    return DFCARD_WRONG_LENGTH;

#ifdef SPROX_DESFIRE_WITH_SAM
  if (ctx->sam_session_active)
  {
    BOOL fKeepStatus = FALSE;
    LONG rc = SAM_VerifyMAC(ctx->sam_context.hSam, recv_buffer[0], &recv_buffer[1], length-1, 4, fKeepStatus);
    length -= 4;
    *recv_length = length;
    return rc;

  } else
 #endif
  {
    length -= 5;

    Desfire_ComputeMac(SPROX_PARAM_P  &recv_buffer[1], length, mac);

    if (memcmp(&recv_buffer[1 + length], mac, 4))
      return DFCARD_WRONG_MAC;

    /* Remove size of MAC */
    *recv_length -= 4;

    return DF_OPERATION_OK;
  }
}
