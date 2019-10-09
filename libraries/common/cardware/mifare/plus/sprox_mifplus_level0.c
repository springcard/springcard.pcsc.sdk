/**h* MifPlusAPI/Level0
 *
 * NAME
 *   MifPlusAPI :: Implementation of Level 0 card functions
 *
 * COPYRIGHT
 *   (c) 2011 SpringCard - www.springcard.com
 *
 * DESCRIPTION
 *   Mifare Plus S and X Level 0 functions.
 *
 **/
#include "sprox_mifplus_i.h"

/**f* MifPlusAPI/WritePerso
 *
 * NAME
 *   WritePerso
 *
 * DESCRIPTION
 *   Write 16 bytes into a Mifare Plus card running at Level 0.
 *
 * SYNOPSIS
 *
 *   [[sprox_mifplus.dll]]
 *   SWORD SPROX_MifPlus_WritePerso(WORD address,
 *                                  const BYTE value[16]);
 *
 *   [[sprox_mifplus_ex.dll]]
 *   SWORD SPROXx_MifPlus_WritePerso(SPROX_INSTANCE rInst,
 *                                   WORD address,
 *                                   const BYTE value[16]);
 *
 *   [[pcsc_mifplusdll]]
 *   LONG  SCardMifPlus_WritePerso(SCARDHANDLE hCard,
 *                                 BYTE address,
 *                                 const BYTE value[16]);
 *
 * INPUTS
 *   BYTE address        : address of the first page to be written
 *   const BYTE data[16] : new data
 *
 * SEE ALSO
 *   CommitPerso
 *
 **/
SPROX_API_FUNC(MifPlus_WritePerso) (SPROX_PARAM  WORD address, const BYTE value[16])
{
  SPROX_RC rc;
  WORD rlen;
  BYTE buffer[64];
	SPROX_MIFPLUS_GET_CTX();

  if (value == NULL)
    return MFP_LIB_CALL_ERROR;

  /* WritePerso */
  buffer[0] = MFP_CMD_WRITE_PERSO;
  buffer[1] = (BYTE)  address; /* LSB first */
	buffer[2] = (BYTE) (address >> 8);
  memcpy(&buffer[3], value, 16);

  rc = SPROX_API_CALL(MifPlus_Command) (SPROX_PARAM_P  buffer, 19, buffer, sizeof(buffer), &rlen);
	if (rc != MFP_SUCCESS)
	  goto done;

  /* Check answer - shall be a one byte status - which one ? */
	rc = MifPlus_Result(buffer, rlen, 0);

done:  
  return rc;
}

/**f* MifPlusAPI/CommitPerso
 *
 * NAME
 *   CommitPerso
 *
 * DESCRIPTION
 *   Leave Level 0 for Level 1
 *
 * SYNOPSIS
 *
 *   [[sprox_mifplus.dll]]
 *   SWORD SPROX_MifPlus_CommitPerso(void);
 *
 *   [[sprox_mifplus_ex.dll]]
 *   SWORD SPROXx_MifPlus_CommitPerso(SPROX_INSTANCE rInst);
 *
 *   [[pcsc_mifplusdll]]
 *   LONG  SCardMifPlus_CommitPerso(SCARDHANDLE hCard);
 *
 * INPUTS
 *   none
 *
 * SEE ALSO
 *   WritePerso
 *
 **/
SPROX_API_FUNC(MifPlus_CommitPerso) (SPROX_PARAM_V)
{
  SPROX_RC rc;
  WORD rlen;
  BYTE buffer[64];
	SPROX_MIFPLUS_GET_CTX();

  /* WritePerso */
  buffer[0] = MFP_CMD_COMMIT_PERSO;

  rc = SPROX_API_CALL(MifPlus_Command) (SPROX_PARAM_P  buffer, 1, buffer, sizeof(buffer), &rlen);
	if (rc != MFP_SUCCESS)
	  goto done;

  /* Check answer - shall be a one byte status - which one ? */
	rc = MifPlus_Result(buffer, rlen, 0);

done:  
  return rc;
}
