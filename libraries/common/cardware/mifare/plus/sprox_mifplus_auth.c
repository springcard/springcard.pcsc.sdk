/**h* MifPlusAPI/Authentication
 *
 * NAME
 *   MifPlusAPI :: Authentication functions
 *
 * COPYRIGHT
 *   (c) 2011 SpringCard - www.springcard.com
 *
 * DESCRIPTION
 *   Implementation of the Mifare Plus S and X authentication functions.
 *
 **/
#include "sprox_mifplus_i.h"

SPROX_API_FUNC(MifPlus_FirstAuthenticate_Step1) (SPROX_PARAM  WORD address, const BYTE pcd_cap[], BYTE pcd_cap_len, BYTE rnd_picc[16])
{
  SPROX_RC rc;
  WORD rlen;
  BYTE buffer[64];
	SPROX_MIFPLUS_GET_CTX();

  if (((pcd_cap == NULL) && (pcd_cap_len != 0)) || (pcd_cap_len > 6))
    return MFP_LIB_CALL_ERROR;
  if (rnd_picc == NULL)
    return MFP_LIB_CALL_ERROR;

	memset(buffer, 0, sizeof(buffer));

  buffer[0] = MFP_CMD_FIRST_AUTHENTICATE;
  buffer[1] = (BYTE)  address; /* LSB first */
	buffer[2] = (BYTE) (address >> 8);
	buffer[3] = pcd_cap_len;
	if (pcd_cap_len)
	  memcpy(&buffer[4], pcd_cap, pcd_cap_len);

#ifdef MIFPLUS_DEBUG
  {
    WORD i;
    printf("FirstAuthenticate1 < ");
    for (i=0; i<(4+pcd_cap_len); i++)
      printf("%02X", buffer[i]);
    printf("\n");
  }
#endif
    
  rc = SPROX_API_CALL(MifPlus_Command) (SPROX_PARAM_P  buffer, (WORD) (4 + pcd_cap_len), buffer, sizeof(buffer), &rlen);
	if (rc != MFP_SUCCESS)
	  goto done;

#ifdef MIFPLUS_DEBUG
  {
    WORD i;
    printf("FirstAuthenticate1 > ");
    for (i=0; i<rlen; i++)
      printf("%02X", buffer[i]);
    printf("\n");
  }
#endif
    
  /* Check answer - shall be status + 16 bytes */
  rc = MifPlus_Result(buffer, rlen, 16);
	if (rc != MFP_SUCCESS)
    goto done;

	/* Retrieve card's RndB */
	/* -------------------- */

	memcpy(rnd_picc, &buffer[1], 16);
	AES_Decrypt(&ctx->main_cipher, rnd_picc);
  
#ifdef MIFPLUS_DEBUG
  {
    BYTE i;
    printf("RndPicc=");
    for (i=0; i<16; i++)
      printf("%02X", rnd_picc[i]);
    printf("\n");
  }
#endif
 
done:
  return rc;
}

SPROX_API_FUNC(MifPlus_FirstAuthenticate_Step2) (SPROX_PARAM  BYTE rnd_picc[16], const BYTE rnd_pcd[16], const BYTE pcd_cap[], BYTE pcd_cap_len, BYTE picc_cap[6], BYTE transaction_id[4])
{
  SPROX_RC rc;
  WORD rlen;
  BYTE i;
  BYTE rnd_pcd_e[16];
  BYTE rnd_picc_e[16];
	BYTE vector[16];
  BYTE buffer[64];
	SPROX_MIFPLUS_GET_CTX();

  if ((rnd_picc == NULL) || (rnd_pcd == NULL))
    return MFP_LIB_CALL_ERROR;
  if (((pcd_cap == NULL) && (pcd_cap_len != 0)) || (pcd_cap_len > 6))
    return MFP_LIB_CALL_ERROR;
    
#ifdef MIFPLUS_DEBUG
  {
    WORD i;
    printf("RndPcd > ");
    for (i=0; i<16; i++)
      printf("%02X", rnd_pcd[i]);
    printf("\n");
  }
#endif    
   
  /* Encrypt RnA */
  /* ----------- */

  memcpy(rnd_pcd_e, rnd_pcd, 16);
  AES_Encrypt(&ctx->main_cipher, rnd_pcd_e);
  
#ifdef MIFPLUS_DEBUG
  {
    WORD i;
    printf("RndPcdE > ");
    for (i=0; i<16; i++)
      printf("%02X", rnd_pcd_e[i]);
    printf("\n");
  }
#endif    
  
  memcpy(vector, rnd_pcd_e, 16);   
   
	/* Compute RnB', rotating RndB to the right */
  /* ---------------------------------------- */
  for (i=0; i<15; i++)
    rnd_picc_e[i] = rnd_picc[i+1];
  rnd_picc_e[15] = rnd_picc[0];
  
#ifdef MIFPLUS_DEBUG
  {
    WORD i;
    printf("RndPiccE > ");
    for (i=0; i<16; i++)
      printf("%02X", rnd_picc_e[i]);
    printf("\n");
  }
#endif      

	/* Encrypt RnB' */
  /* ------------ */
  for (i=0; i<16; i++)
    rnd_picc_e[i] ^= vector[i];
  AES_Encrypt(&ctx->main_cipher, rnd_picc_e);

  /* Build the command buffer */
  /* ------------------------ */

  buffer[0] = MFP_CMD_AUTHENTICATE_PART_2;
  memcpy(&buffer[1],  rnd_pcd_e, 16);
  memcpy(&buffer[17], rnd_picc_e, 16);

#ifdef MIFPLUS_DEBUG
  {
    WORD i;
    printf("FirstAuthenticate2 < ");
    for (i=0; i<33; i++)
      printf("%02X", buffer[i]);
    printf("\n");
  }
#endif
  
  rc = SPROX_API_CALL(MifPlus_Command) (SPROX_PARAM_P  buffer, 33, buffer, sizeof(buffer), &rlen);
	if (rc != MFP_SUCCESS)
	  goto done;

  /* Check answer - shall be status + 32 bytes */
  rc = MifPlus_Result(buffer, rlen, 32);
	if (rc != MFP_SUCCESS)
    goto done;

#ifdef MIFPLUS_DEBUG
  {
    WORD i;
    printf("FirstAuthenticate2 > ");
    for (i=0; i<rlen; i++)
      printf("%02X", buffer[i]);
    printf("\n");
  }
#endif
    
  /* Decrypt the answer */
	memcpy(vector, &buffer[1], 16);
	AES_Decrypt(&ctx->main_cipher, &buffer[1]);
  
#ifdef MIFPLUS_DEBUG
  {
    WORD i;
    printf("Buffer[1] > ");
    for (i=0; i<16; i++)
      printf("%02X", buffer[1+i]);
    printf("\n");
  }
#endif    
  
  AES_Decrypt(&ctx->main_cipher, &buffer[17]);
  
#ifdef MIFPLUS_DEBUG
  {
    WORD i;
    printf("Buffer[17] > ");
    for (i=0; i<16; i++)
      printf("%02X", buffer[17+i]);
    printf("\n");
  }
#endif    
  
	for (i=0; i<16; i++)
	  buffer[17+i] ^= vector[i];

#ifdef MIFPLUS_DEBUG
  {
    WORD i;
    printf("           > ");
    for (i=0; i<16; i++)
      printf("%02X", buffer[17+i]);
    printf("\n");
  }
#endif    
    
  /* Compare RndA' with RndA */
  /* ----------------------- */

	memcpy(rnd_pcd_e, &buffer[5], 16);
  if (memcmp(&rnd_pcd_e[0], &rnd_pcd[1], 15) || (rnd_pcd_e[15] != rnd_pcd[0]))
  {
	  /* Authentication failed */
    rc = MFP_ERROR_CARD_AUTH;
    goto done;
  }

	/* Compare pcd_cap */
	/* --------------- */

	if (pcd_cap_len)
	{
		if (memcmp(&buffer[27], pcd_cap, pcd_cap_len))
		{
			/* Authentication failed */
			rc = MFP_ERROR_CARD_AUTH;
			goto done;
		}
	}

	/* Get the Transaction Identifier */
	/* ----------------------------- */

	if (transaction_id != NULL)
	{
	  memcpy(transaction_id, &buffer[1], 4);
	}

	/* Get the PICC cap */
	/* ---------------- */
	if (picc_cap != NULL)
	{
    memcpy(picc_cap, &buffer[21], 6);
	}
  
done:
  return rc;
}

/**f* MifPlusAPI/FirstAuthenticate
 *
 * NAME
 *   FirstAuthenticate
 *
 * DESCRIPTION
 *   Perform authentication using the specified AES key on the currently selected
 *   Mifare Plus card.
 *
 * SYNOPSIS
 *
 *   [[sprox_mifplus.dll]]
 *   SWORD SPROX_MifPlus_FirstAuthenticate(WORD key_address,
 *                                         const BYTE key_value[16]
 *                                         const BYTE pcd_cap[],
 *                                         BYTE pcd_cap_len,
 *                                         BYTE picc_cap[6]);
 *
 *   [[sprox_mifplus_ex.dll]]
 *   SWORD SPROXx_MifPlus_FirstAuthenticate(SPROX_INSTANCE rInst,
 *                                          WORD key_address,
 *                                          const BYTE key_value[16],
 *                                          const BYTE pcd_cap[],
 *                                          BYTE pcd_cap_len,
 *                                          BYTE picc_cap[6]);
 *
 *   [[pcsc_mifplus.dll]]
 *   LONG  SCardMifPlus_FirstAuthenticate(SCARDHANDLE hCard,
 *                                        WORD key_address,
 *                                        const BYTE key_value[16],
 *                                        const BYTE pcd_cap[],
 *                                        BYTE pcd_cap_len,
 *                                        BYTE picc_cap[6]);
 *
 * INPUTS
 *   WORD key_address               : the address of the key within the card
 *   const BYTE key_value[16]       : 16-byte key (AES)
 *   const BYTE pcd_cap[]           : PCDcap2 field (to be provided by the application)
 *   BYTE pcd_cap_len               : size of pcd_cap (0 to 6)
 *   BYTE picc_cap[6]               : upon success, the PICCcap2 field returned by the card (6 bytes)
 *
 * SEE ALSO
 *   FollowingAuthenticate
 *
 **/
SPROX_API_FUNC(MifPlus_FirstAuthenticate) (SPROX_PARAM  WORD key_address, const BYTE key_value[16], const BYTE pcd_cap[], BYTE pcd_cap_len, BYTE picc_cap[6])
{
  SPROX_RC rc;
	BYTE rnd_picc[16], rnd_pcd[16];
#ifdef _USE_PCSC
  BOOL resume_tracking = FALSE;
#endif
	SPROX_MIFPLUS_GET_CTX();

#ifdef _USE_PCSC
  if (!ctx->tcl)
  {
    /* When running at Mifare level (!T=CL) the tracking must be halted so that no */
    /* wake-up command could come in between the two steps of the authentication   */
    rc = SPROX_API_CALL(MifPlus_SuspendTracking) (SPROX_PARAM_PV);
    if (rc != MFP_SUCCESS)
      goto done;
    resume_tracking = TRUE;
  }
#endif

#ifdef MIFPLUS_DEBUG
  {
    WORD i;
    printf("FirstAuthenticate %04X, key=", key_address);
    for (i=0; i<16; i++)
      printf("%02X", key_value[i]);
    printf("\n");
  }
#endif

  ctx->in_session = FALSE;
		
  AES_Init(&ctx->main_cipher, key_value);  
  
	rc = SPROX_API_CALL(MifPlus_FirstAuthenticate_Step1) (SPROX_PARAM_P  key_address, pcd_cap, pcd_cap_len, rnd_picc);
  if (rc != MFP_SUCCESS)
    goto done;

  GetRandomBytes(SPROX_PARAM_P  rnd_pcd, 16);
  
#ifdef MIFPLUS_DEBUG
  memset(rnd_pcd, 0xAA, 16);
#endif        

#ifdef _WITH_SELFTEST
  if (in_selftest)
    GetRandomBytes_Hook(SPROX_PARAM_P  rnd_pcd, 16);
#endif
 
	rc = SPROX_API_CALL(MifPlus_FirstAuthenticate_Step2) (SPROX_PARAM_P  rnd_picc, rnd_pcd, pcd_cap, pcd_cap_len, picc_cap, ctx->transaction_id);
  if (rc != MFP_SUCCESS)
    goto done;

	MifPlus_ComputeKey(SPROX_PARAM_P  rnd_pcd, rnd_picc, MFP_CONSTANT_KEY_ENC, ctx->session_enc_key);

#ifdef MIFPLUS_DEBUG
  {
    WORD i;
    printf("ENC Key =");
    for (i=0; i<16; i++)
      printf("%02X", ctx->session_enc_key[i]);
    printf("\n");
  }
#endif

	MifPlus_ComputeKey(SPROX_PARAM_P  rnd_pcd, rnd_picc, MFP_CONSTANT_KEY_MAC, ctx->session_mac_key);

#ifdef MIFPLUS_DEBUG
  {
    WORD i;
    printf("MAC Key =");
    for (i=0; i<16; i++)
      printf("%02X", ctx->session_mac_key[i]);
    printf("\n");
  }
#endif

	AES_Init(&ctx->main_cipher, ctx->session_enc_key);  
	MifPlus_InitCmac(SPROX_PARAM_P  ctx->session_mac_key);



	ctx->read_counter  = 0;
	ctx->write_counter = 0;

  ctx->in_session = TRUE;

done:
 #ifdef _USE_PCSC
  if (resume_tracking)
  {
    SPROX_API_CALL(MifPlus_ResumeTracking) (SPROX_PARAM_PV);
  }
#endif
  return rc;
}


SPROX_API_FUNC(MifPlus_FollowingAuthenticate_Step1) (SPROX_PARAM  WORD address, BYTE rnd_picc[16])
{
  SPROX_RC rc;
  WORD rlen;
  BYTE buffer[64];
	BYTE vector[16];
	BYTE i;
	SPROX_MIFPLUS_GET_CTX();

  if (rnd_picc == NULL)
    return MFP_LIB_CALL_ERROR;

  buffer[0] = MFP_CMD_FOLLOWING_AUTHENTICATE;
  buffer[1] = (BYTE)  address; /* LSB first */
	buffer[2] = (BYTE) (address >> 8);

  rc = SPROX_API_CALL(MifPlus_Command) (SPROX_PARAM_P  buffer, 3, buffer, sizeof(buffer), &rlen);
	if (rc != MFP_SUCCESS)
	  goto done;

  /* Check answer - shall be status + 16 bytes */
  rc = MifPlus_Result(buffer, rlen, 16);
	if (rc != MFP_SUCCESS)
    goto done;

	/* Retrieve card's RndB */
	/* -------------------- */

	MifPlus_GetCbcVector_Response(SPROX_PARAM_P  vector);

	memcpy(rnd_picc, &buffer[1], 16);
	AES_Decrypt(&ctx->main_cipher, rnd_picc);
	for (i=0; i<16; i++)
	  rnd_picc[i] ^= vector[i];
 
done:
  return rc;
}

SPROX_API_FUNC(MifPlus_FollowingAuthenticate_Step2) (SPROX_PARAM  BYTE rnd_picc[16], const BYTE rnd_pcd[16])
{
  SPROX_RC rc;
  WORD rlen;
  BYTE i;
  BYTE rnd_pcd_e[16];
  BYTE rnd_picc_e[16];
	BYTE vector[16];
  BYTE buffer[64];
	SPROX_MIFPLUS_GET_CTX();

  if ((rnd_picc == NULL) || (rnd_pcd == NULL))
    return MFP_LIB_CALL_ERROR;

  /* Encrypt RnA */
  /* ----------- */

	MifPlus_GetCbcVector_Command(SPROX_PARAM_P  vector);

  memcpy(rnd_pcd_e, rnd_pcd, 16);
  for (i=0; i<16; i++)
    rnd_pcd_e[i] ^= vector[i];
  AES_Encrypt(&ctx->main_cipher, rnd_pcd_e);
  memcpy(vector, rnd_pcd_e, 16);
   
	/* Compute RnB', rotating RndB to the right */
  /* ---------------------------------------- */
  for (i=0; i<15; i++)
    rnd_picc_e[i] = rnd_picc[i+1];
  rnd_picc_e[15] = rnd_picc[0];

	/* Encrypt RnB' */
  /* ------------ */
  for (i=0; i<16; i++)
    rnd_picc_e[i] ^= vector[i];
  AES_Encrypt(&ctx->main_cipher, rnd_picc_e);

  /* Build the command buffer */
  /* ------------------------ */

  buffer[0] = MFP_CMD_AUTHENTICATE_PART_2;
  memcpy(&buffer[1],  rnd_pcd_e, 16);
  memcpy(&buffer[17], rnd_picc_e, 16);

  rc = SPROX_API_CALL(MifPlus_Command) (SPROX_PARAM_P  buffer, 33, buffer, sizeof(buffer), &rlen);
	if (rc != MFP_SUCCESS)
	  goto done;

  /* Check answer - shall be status + 16 bytes */
  rc = MifPlus_Result(buffer, rlen, 16);
	if (rc != MFP_SUCCESS)
    goto done;

  /* Verify the cryptogram */
  MifPlus_GetCbcVector_Response(SPROX_PARAM_P  vector);

  memcpy(rnd_pcd_e, &buffer[1], 16);
  AES_Decrypt(&ctx->main_cipher, rnd_pcd_e);
	for (i=0; i<16; i++)
	  rnd_pcd_e[i] ^= vector[i];

  /* Compare RndA' with RndA */
  /* ----------------------- */

  if (memcmp(&rnd_pcd_e[0], &rnd_pcd[1], 15) || (rnd_pcd_e[15] != rnd_pcd[0]))
  {
	  /* Authentication failed */
    rc = MFP_ERROR_CARD_AUTH;
    goto done;
  }
  
done:
  return rc;
}

/**f* MifPlusAPI/FollowingAuthenticate
 *
 * NAME
 *   FollowingAuthenticate
 *
 * DESCRIPTION
 *   Perform authentication using the specified AES key on the currently selected
 *   Mifare Plus card.
 *
 * NOTES
 *   In Level 3, FollowingAuthenticate could only be called within a session
 *   previously opened through FirstAuthenticate.
 *   In Level 1, FollowingAuthenticate could also be used outside a session
 *   (typically to verify the SL1 Card Authentication Key or to switch to
 *   an higher level).
 *
 * SYNOPSIS
 *
 *   [[sprox_mifplus.dll]]
 *   SWORD SPROX_MifPlus_FollowingAuthenticate(WORD key_address,
 *                                             const BYTE key_value[16]);
 *
 *   [[sprox_mifplus_ex.dll]]
 *   SWORD SPROXx_MifPlus_FollowingAuthenticate(SPROX_INSTANCE rInst,
 *                                              WORD key_address,
 *                                              const BYTE key_value[16]);
 *
 *   [[pcsc_mifplus.dll]]
 *   LONG  SCardMifPlus_FollowingAuthenticate(SCARDHANDLE hCard,
 *                                            WORD key_address,
 *                                            const BYTE key_value[16]);
 *
 * INPUTS
 *   WORD key_address               : the address of the key within the card
 *   const BYTE key_value[16]       : 16-byte key (AES)
 *
 * SEE ALSO
 *   FirstAuthenticate
 *
 **/
SPROX_API_FUNC(MifPlus_FollowingAuthenticate) (SPROX_PARAM  WORD key_address, const BYTE key_value[16])
{
  SPROX_RC rc;
	BYTE rnd_picc[16], rnd_pcd[16];
#ifdef _USE_PCSC
  BOOL resume_tracking = FALSE;
#endif
	SPROX_MIFPLUS_GET_CTX();

#ifdef _USE_PCSC
  if (!ctx->tcl)
  {
    /* When running at Mifare level (!T=CL) the tracking must be halted so that no */
    /* wake-up command could come in between the two steps of the authentication   */
    rc = SPROX_API_CALL(MifPlus_SuspendTracking) (SPROX_PARAM_PV);
    if (rc != MFP_SUCCESS)
      goto done;
    resume_tracking = TRUE;
  }
#endif

  AES_Init(&ctx->main_cipher, key_value);
  
	rc = SPROX_API_CALL(MifPlus_FollowingAuthenticate_Step1) (SPROX_PARAM_P  key_address, rnd_picc);
  if (rc != MFP_SUCCESS)
    goto done;

  GetRandomBytes(SPROX_PARAM_P  rnd_pcd, 16);

#ifdef _WITH_SELFTEST
  if (in_selftest)
    GetRandomBytes_Hook(SPROX_PARAM_P  rnd_pcd, 16);
#endif

	rc = SPROX_API_CALL(MifPlus_FollowingAuthenticate_Step2) (SPROX_PARAM_P  rnd_picc, rnd_pcd);
	if (rc != MFP_SUCCESS)
	  goto done;

	MifPlus_ComputeKey(SPROX_PARAM_P  rnd_pcd, rnd_picc, MFP_CONSTANT_KEY_ENC, ctx->session_enc_key);
	MifPlus_ComputeKey(SPROX_PARAM_P  rnd_pcd, rnd_picc, MFP_CONSTANT_KEY_MAC, ctx->session_mac_key);

	AES_Init(&ctx->main_cipher, ctx->session_enc_key);  
	MifPlus_InitCmac(SPROX_PARAM_P  ctx->session_mac_key);

  ctx->in_session = TRUE;

done:
#ifdef _USE_PCSC
  if (resume_tracking)
  {
    SPROX_API_CALL(MifPlus_ResumeTracking) (SPROX_PARAM_PV);
  }
#endif
  return rc;
}

/**f* MifPlusAPI/ResetAuthentication
 *
 * NAME
 *   ResetAuthentication
 *
 * DESCRIPTION
 *   Reset the authentication state of the Mifare Plus.
 *
 * SYNOPSIS
 *
 *   [[sprox_mifplus.dll]]
 *   SWORD SPROX_MifPlus_ResetAuthentication(void);
 *
 *   [[sprox_mifplus_ex.dll]]
 *   SWORD SPROXx_MifPlus_ResetAuthentication(SPROX_INSTANCE rInst)
 *
 *   [[pcsc_mifplus.dll]]
 *   LONG  SCardMifPlus_ResetAuthentication(SCARDHANDLE hCard)
 *
 * SEE ALSO
 *   FirstAuthenticate
 *   FollowingAuthenticate
 *
 **/
SPROX_API_FUNC(MifPlus_ResetAuthentication) (SPROX_PARAM_V)
{
  SPROX_RC rc;
  WORD rlen;
  BYTE buffer[64];
	SPROX_MIFPLUS_GET_CTX();

	ctx->in_session = FALSE;

  buffer[0] = MFP_CMD_RESET_AUTHENTICATION;

  rc = SPROX_API_CALL(MifPlus_Command) (SPROX_PARAM_P  buffer, 1, buffer, sizeof(buffer), &rlen);
	if (rc != MFP_SUCCESS)
	  goto done;

  rc = MifPlus_Result(buffer, rlen, 0);
 
done:
  return rc;
}



