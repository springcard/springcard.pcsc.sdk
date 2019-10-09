#ifdef SPROX_DESFIRE_WITH_SAM
/**h* DesfireAPI/SamUnlock
 *
 * NAME
 *   DesfireAPI :: Using the Desfire card together with a NXP SAM AV2 smartcard
 *
 * COPYRIGHT
 *   (c) 2014 SpringCard - www.springcard.com
 *
 * DESCRIPTION
 *   Implementation of authentication using the SAM.
 *
 **/

#include "sprox_desfire_i.h"

void AES_Cipher(const BYTE Key[16], BYTE IV[16], BYTE PlainBytes[16], BYTE out[16]);
void AES_Decipher(BYTE Key[16], BYTE IV[16], BYTE Encrypted_Bytes[16], BYTE out[16]);
void CipherSend_AES(BYTE data[], DWORD *length, DWORD max_length, BYTE Key[16], BYTE IV[16]);

static void rotate_left(BYTE in[16], BYTE out[16])
{
  int i;

  for (i = 0; i < 16 - 1; i++)
    out[i] = in[i+1];

  out[15] = in[0];

}


static void Calculate_CMAC(const BYTE Key[16], BYTE IV[16], BYTE PlainBytes[16], BYTE out[16])
{
	/* Generate subkeys */
  BYTE Zeros[16] = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
  BYTE L[16] = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
  BYTE Key1[16] = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
  BYTE Key2[16] = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
  BYTE tmp[16] = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
  BYTE M1[16] = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

  int i = 0;
  BYTE Rb = 0x87;
  BYTE MSB_L;
  BYTE MSB_K1;
  WORD decal;

  AES_Cipher(Key, IV, Zeros, L);
  MSB_L = L[0];

  /* Key 1  */
  for (i = 0; i < 16 - 1 ; i++)
  {
    decal = (WORD) (L[i] << 1);
    L[i] = (BYTE) (decal & 0x00FF);
    if ( (L[i+1] & 0x80) == 0x80 )
    {
      L[i] |= 0x01;
    } else
    {
      L[i] |= 0x00;
    }
  }
  decal = (WORD) (L[i] << 1);
  L[i] = (BYTE) (decal & 0x00FF);

  if ( MSB_L >= 0x80 )
    L[16 - 1] ^= Rb;

  for (i=0; i<16; i++)
    Key1[i] = L[i];

#ifdef _DEBUG_SAM_CMAC_CALC
  printf("Key1:");
  for (i=0; i< 16; i++)
    printf("%2.2x ", Key1[i]);
  printf("\n");
#endif


  for (i = 0; i < 16; i++)
    tmp[i] = Key1[i];

  /* Key 2  */
  MSB_K1 = Key1[0];
  for (i=0 ; i<15 ; i++)
  {
    decal = (WORD) (tmp[i] << 1);
    tmp[i] = (BYTE) (decal & 0x00FF);
    if ( (tmp[i+1] & 0x80) == 0x80 )
    {
      tmp[i] |= 0x01;
    } else
    {
      tmp[i] |= 0x00;
    }
  }
  decal = (WORD) (tmp[i] << 1);
  tmp[i] = (BYTE) (decal & 0x00FF);
  if (MSB_K1 >= 0x80)
    tmp[16 - 1] ^= Rb;

  for (i=0; i<16; i++)
    Key2[i] = tmp[i];

#ifdef _DEBUG_SAM_CMAC_CALC
  printf("Key2:");
  for (i=0; i< 16; i++)
    printf("%2.2x ", Key2[i]);
  printf("\n");
#endif

	/* Calculate CMAC by ciphering  */
  for (i=0; i< 16; i++)
    M1[i] = (BYTE) (PlainBytes[i] ^ Key1[i]);

  AES_Cipher(Key, IV, M1, out);

#ifdef _DEBUG_SAM_CMAC_CALC
  printf("LongCMAC:");
  for (i=0; i< 16; i++)
    printf("%2.2x ", out[i]);
  printf("\n");
#endif/

}

/**f* DesfireAPI/SAM_Unlock
 *
 * NAME
 *   SAM_Unlock
 *
 * DESCRIPTION
 *   Unlocks the SAM
 *
 * SYNOPSIS
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_SAM_Unlock(SCARDHANDLE hSam,
 *                                 BYTE bKeyNumberSam,
 *                                 BYTE bKeyVersion,
 *                                 const BYTE pbKeyValue[16])
 *
 * INPUTS
 *   BYTE bKeyNumberSam          : Key number, in the SAM, of the unlock key
 *   BYTE bKeyVersion            : Key version, in the SAM, of the unlock key
 *   const BYTE pbKeyValue[16]   : Value (16 bytes) of the unlock key
 *
 * RETURNS
 *   DF_OPERATION_OK    : application selected
 *   Other code if internal or communication error has occured.
 *
 **/
SPROX_API_FUNC(Desfire_SAM_Unlock) (SCARDHANDLE hSam, BYTE bKeyNumberSam, BYTE bKeyVersion, const BYTE pbKeyValue[16])
{
  LONG rc;
  int i, offset;
  int capdu_len = 0;
  int rapdu_len = DF_MAX_INFO_FRAME_SIZE;
  BYTE IV[16] = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
  BYTE capdu[DF_MAX_INFO_FRAME_SIZE], rapdu[DF_MAX_INFO_FRAME_SIZE], Rnd1[12], Rnd2[12], RndA[16], RndB[16], CMAC_load[12 + 1 + 3], // CMAC_load: +P1+MaxChainBlocks
        long_CMAC[16], CMAC[8], SV1[16], KXE[16], Ek_Kxe_RndB[16], CMAC_load_last_rapdu[16], second_capdu_data[20],
        RndBp[16], RndBpp[16], concat[32], RndAp[16], RndApp[16], EkRndapp[16], Recv_Rndapp[16];
  DWORD encrypt_concat_len = 32;

  if (pbKeyValue == NULL)
    return SCARD_E_INVALID_PARAMETER;

  /* 1. C-APDU, part 1 */
  /* ----------------- */
#ifdef _DEBUG_SAM_UNLOCK
  printf("Capdu, part 1\n");
#endif
  offset=0;
  capdu[offset++] = 0x80;
  capdu[offset++] = 0x10;
  capdu[offset++] = 0x00;
  capdu[offset++] = 0x00;
  capdu[offset++] = 0x02;
  capdu[offset++] = bKeyNumberSam;
  capdu[offset++] = bKeyVersion;
  capdu[offset++] = 0x00;

#ifdef _DEBUG_SAM_UNLOCK
  printf("- Capdu=");
  for (i=0; i<offset; i++)
    printf("%2.2x ", capdu[i]);
  printf("\n");
#endif

  /* 2. R-APDU */
  /* --------- */
#ifdef _DEBUG_SAM_UNLOCK
  printf("Rapdu, part 1\n");
#endif

  rc = SCardTransmit(hSam, SCARD_PCI_T1, capdu, offset, NULL, rapdu, &rapdu_len);
	if (rc != SCARD_S_SUCCESS)
	  return rc;

#ifdef _DEBUG_SAM_UNLOCK
  printf("- Rapdu=");
  for (i=0; i<rapdu_len; i++)
    printf("%2.2x ", rapdu[i]);
  printf("\n");
#endif

  if (rapdu_len != 14)
    return SCARD_E_INVALID_VALUE;

  /* 3. Rnd2 */
  /* ------- */
  for (i=0; i<rapdu_len-2; i++)
    Rnd2[i] = rapdu[i];

#ifdef _DEBUG_SAM_UNLOCK
  printf("- Rnd2=");
  for (i=0; i<rapdu_len-2; i++)
    printf("%2.2x ", Rnd2[i]);
  printf("\n");
#endif

  /* 4. CMAC_load */
  /* ------------ */
#ifdef _DEBUG_SAM_UNLOCK
  printf("CMAC load\n");
#endif
  offset = 0;
  for (i = 0; i<12; i++)
    CMAC_load[offset++] = Rnd2[i];

  CMAC_load[offset++] = 0x00;
  CMAC_load[offset++] = 0x00;
  CMAC_load[offset++] = 0x00;
  CMAC_load[offset++] = 0x00;

#ifdef _DEBUG_SAM_UNLOCK
  printf("- CMAC load=");
  for (i=0; i<offset; i++)
    printf("%2.2x ", CMAC_load[i]);
  printf("\n");
#endif


  /* 5. Calculate CMAC */
  /* ----------------- */
#ifdef _DEBUG_SAM_UNLOCK
  printf("Calculate CMAC\n");
#endif

  Calculate_CMAC(pbKeyValue, IV, CMAC_load, long_CMAC);
  offset = 0;
  for (i=1 ; i< sizeof(long_CMAC); )
  {
    CMAC[offset++] = long_CMAC[i];
    i+=2;
  }

#ifdef _DEBUG_SAM_UNLOCK
  printf("- long CMAC=");
  for (i=0; i<sizeof(long_CMAC); i++)
    printf("%2.2x ", long_CMAC[i]);
  printf("\n");

    printf("- CMAC=");
  for (i=0; i<sizeof(CMAC); i++)
    printf("%2.2x ", CMAC[i]);
  printf("\n");
#endif


  /* 6. Generate Random */
  /* ------------------ */
#ifdef _DEBUG_SAM_UNLOCK
  printf("Generate Random Rnd1\n");
#endif

  GetRandomBytes(0, Rnd1, 12);

#ifdef _DEBUG_SAM_UNLOCK
  printf("- Rnd1=");
  for (i=0; i<12; i++)
    printf("%2.2x ", Rnd1[i]);
  printf("\n");
#endif

  /* 7. Generate CAPDU */
  /* ----------------- */
#ifdef _DEBUG_SAM_UNLOCK
  printf("capdu, part 2\n");
#endif

  offset = 0;

  for (i = 0; i < sizeof(CMAC) ; i++)
    second_capdu_data[offset++] = CMAC[i];

  for (i = 0; i<sizeof(Rnd1) ; i++)
    second_capdu_data[offset++] = Rnd1[i];

#ifdef _DEBUG_SAM_UNLOCK
  printf("- capdu content=");
  for (i=0; i<offset; i++)
    printf("%2.2x ", second_capdu_data[i]);
  printf("\n");
#endif


  /* 8. SV1 */
  /* ------ */
#ifdef _DEBUG_SAM_UNLOCK
  printf("SV1\n");
#endif

  offset = 0;
  for (i = 7 ; i < sizeof(Rnd1) ; i++)
    SV1[offset++] = Rnd1[i];

  for (i = 7 ; i< sizeof(Rnd2) ; i++)
    SV1[offset++] = Rnd2[i];

  for (i = 0; i<5; i++)
    SV1[offset++] = (byte) (Rnd1[i] ^ Rnd2[i]);

  SV1[offset++] = 0x91;

#ifdef _DEBUG_SAM_UNLOCK
  printf("- SV1=");
  for (i=0; i<offset; i++)
    printf("%2.2x ", SV1[i]);
  printf("\n");
#endif

  /* 9. KXE */
  /* ------ */
#ifdef _DEBUG_SAM_UNLOCK
  printf("KXE\n");
#endif

  AES_Cipher(pbKeyValue, IV, SV1, KXE);

#ifdef _DEBUG_SAM_UNLOCK
  printf("- KXE=");
  for (i=0; i<16; i++)
    printf("%2.2x ", KXE[i]);
  printf("\n");
#endif


  /* 10. Get R-APDU */
  /* -------------- */
#ifdef _DEBUG_SAM_UNLOCK
  printf("rapdu, part 2\n");
#endif

  offset=0;
  capdu[offset++] = 0x80;
  capdu[offset++] = 0x10;
  capdu[offset++] = 0x00;
  capdu[offset++] = 0x00;
  capdu[offset++] = sizeof(second_capdu_data);
  for (i=0; i<sizeof(second_capdu_data); i++)
  {
    if (offset < sizeof(capdu))
    {
      capdu[offset++] = second_capdu_data[i];
    } else
    {
      return SCARD_F_INTERNAL_ERROR;
    }
  }
  capdu[offset++] = 0x00;

#ifdef _DEBUG_SAM_UNLOCK
  printf("- Capdu=");
  for (i=0; i<offset; i++)
    printf("%2.2x ", capdu[i]);
  printf("\n");
#endif

  rapdu_len = DF_MAX_INFO_FRAME_SIZE;

  rc = SCardTransmit(hSam, SCARD_PCI_T1, capdu, offset, NULL, rapdu, &rapdu_len);
	if (rc != SCARD_S_SUCCESS)
	  return rc;

#ifdef _DEBUG_SAM_UNLOCK
  printf("- Rapdu=");
  for (i=0; i<rapdu_len; i++)
    printf("%2.2x ", rapdu[i]);
  printf("\n");
#endif

  if (rapdu_len != 26)
    return SCARD_W_CARD_NOT_AUTHENTICATED;

  offset = 0;
  for (i = 8; i< rapdu_len - 2; i++)
    Ek_Kxe_RndB[offset++] = rapdu[i];

#ifdef _DEBUG_SAM_UNLOCK
  printf("- Ek(Kxe, RndB)=");
  for (i=0; i<offset; i++)
    printf("%2.2x ", Ek_Kxe_RndB[i]);
  printf("\n");
#endif

  /* 11. CMAC Load in last R-APDU */
  /* ---------------------------- */
#ifdef _DEBUG_SAM_UNLOCK
  printf("CMAC Load in last R-APDU");
#endif

  offset = 0;
  for (i = 0; i< sizeof(Rnd1); i++)
    CMAC_load_last_rapdu[offset ++] = Rnd1[i];

  CMAC_load_last_rapdu[offset ++] = 0x00;
  CMAC_load_last_rapdu[offset ++] = 0x00;
  CMAC_load_last_rapdu[offset ++] = 0x00;
  CMAC_load_last_rapdu[offset ++] = 0x00;

#ifdef _DEBUG_SAM_UNLOCK
  printf("- CMAC Load=");
  for (i=0; i<offset; i++)
    printf("%2.2x ", CMAC_load_last_rapdu[i]);
  printf("\n");
#endif

  /* 12. CMAC calculation */
  /* -------------------- */
#ifdef _DEBUG_SAM_UNLOCK
  printf("CMAC calculation \n");
#endif

  Calculate_CMAC(pbKeyValue, IV, CMAC_load_last_rapdu, long_CMAC);

  offset = 0;
  for ( i = 1; i<sizeof(long_CMAC); )
  {
    CMAC[offset++] = long_CMAC[i];
    i+=2;
  }

#ifdef _DEBUG_SAM_UNLOCK
  printf("- Long CMAC=");
  for (i=0; i<sizeof(long_CMAC); i++)
    printf("%2.2x ", long_CMAC[i]);
  printf("\n");

  printf("- CMAC=");
  for (i=0; i<sizeof(CMAC); i++)
    printf("%2.2x ", CMAC[i]);
  printf("\n");
#endif

  /* 13. RndB */
  /* -------- */
#ifdef _DEBUG_SAM_UNLOCK
  printf("RndB\n");
#endif

  AES_Decipher(KXE, IV, Ek_Kxe_RndB, RndB);

#ifdef _DEBUG_SAM_UNLOCK
  printf("- RndB=");
  for (i=0; i<sizeof(RndB); i++)
    printf("%2.2x ", RndB[i]);
  printf("\n");
#endif

  /* 14. RndA */
  /* -------- */
#ifdef _DEBUG_SAM_UNLOCK
  printf("RndA\n");
#endif

  GetRandomBytes(0, RndA, 16);

#ifdef _DEBUG_SAM_UNLOCK
  printf("- RndA=");
  for (i=0; i<sizeof(RndA); i++)
    printf("%2.2x ", RndA[i]);
  printf("\n");
#endif

  /* 15. RndBpp */
  /* ---------- */
#ifdef _DEBUG_SAM_UNLOCK
  printf("RndB''\n");
#endif

  rotate_left(RndB, RndBp);
  rotate_left(RndBp, RndBpp);

#ifdef _DEBUG_SAM_UNLOCK
  printf("- RndB''=");
  for (i=0; i<sizeof(RndBpp); i++)
    printf("%2.2x ", RndBpp[i]);
  printf("\n");
#endif

  /* 16.RndA+RndBpp */
#ifdef _DEBUG_SAM_UNLOCK
  printf("RndA + RndB''\n");
#endif

  offset = 0;

  for(i = 0; i< sizeof(RndA); i++)
    concat[offset++] = RndA[i];

  for(i = 0; i< sizeof(RndBpp); i++)
    concat[offset++] = RndBpp[i];

#ifdef _DEBUG_SAM_UNLOCK
  printf("- RndA + RndB''=");
  for (i=0; i<offset; i++)
    printf("%2.2x ", concat[i]);
  printf("\n");
#endif

  /* 17.Ek(Kxe, RndA+RndB'') */
  /* ----------------------- */
#ifdef _DEBUG_SAM_UNLOCK
  printf("Ek(Kxe, RndA+RndB'')\n");
#endif

  CipherSend_AES(concat, &encrypt_concat_len, DF_MAX_INFO_FRAME_SIZE, KXE, IV);

#ifdef _DEBUG_SAM_UNLOCK
  printf("- Ek(Kxe, RndA+RndB'')=");
  for (i=0; i<offset; i++)
    printf("%2.2x ", concat[i]);
  printf("\n");
#endif

  /* Re-init vector */
  for (i=0; i<sizeof(IV); i++)
    IV[i] = 0x00;

  /* 18.capdu part 3 */
  /* --------------- */
#ifdef _DEBUG_SAM_UNLOCK
  printf("Capdu, part 3\n");
#endif

  offset=0;
  capdu[offset++] = 0x80;
  capdu[offset++] = 0x10;
  capdu[offset++] = 0x00;
  capdu[offset++] = 0x00;
  capdu[offset++] = sizeof(concat);
  for (i=0; i<sizeof(concat); i++)
  {
    if (offset < sizeof(capdu))
    {
      capdu[offset++] = concat[i];
    } else
    {
      return SCARD_F_INTERNAL_ERROR;
    }
  }
  capdu[offset++] = 0x00;

#ifdef _DEBUG_SAM_UNLOCK
  printf("- capdu=");
  for (i=0; i<offset; i++)
    printf("%2.2x ", capdu[i]);
  printf("\n");
#endif

  /* 19. RndA'' */
  /* ---------- */
#ifdef _DEBUG_SAM_UNLOCK
  printf("RndA''\n");
#endif

  rotate_left(RndA, RndAp);
  rotate_left(RndAp, RndApp);

#ifdef _DEBUG_SAM_UNLOCK
  printf("- RndA''=");
  for (i=0; i<sizeof(RndApp); i++)
    printf("%2.2x ", RndApp[i]);
  printf("\n");
#endif

  /* 20. Get Rapdu */
  /* ------------- */
#ifdef _DEBUG_SAM_UNLOCK
  printf("Rapdu, part 3\n");
#endif

  rapdu_len = DF_MAX_INFO_FRAME_SIZE;
  rc = SCardTransmit(hSam, SCARD_PCI_T1, capdu, offset, NULL, rapdu, &rapdu_len);
	if (rc != SCARD_S_SUCCESS)
	  return rc;

#ifdef _DEBUG_SAM_UNLOCK
  printf("- Rapdu=");
  for (i=0; i<rapdu_len; i++)
    printf("%2.2x ", rapdu[i]);
  printf("\n");
#endif

  if (rapdu_len != 18)
    return SCARD_W_CARD_NOT_AUTHENTICATED;

  /* 21 Decrypt RAPDU */
  /* ---------------- */
#ifdef _DEBUG_SAM_UNLOCK
  printf("Decipher Rapdu\n");
#endif

  offset = 0;
  for (i = 0; i<sizeof(EkRndapp); i++ )
    EkRndapp[i] = rapdu[i];

#ifdef _DEBUG_SAM_UNLOCK
  printf("- Ek(Received RndA'')=");
  for (i=0; i<sizeof(EkRndapp); i++)
    printf("%2.2x ", EkRndapp[i]);
  printf("\n");
#endif

  AES_Decipher(KXE, IV, EkRndapp, Recv_Rndapp);

#ifdef _DEBUG_SAM_UNLOCK
  printf("- Received RndA''=");
  for (i=0; i<sizeof(Recv_Rndapp); i++)
    printf("%2.2x ", Recv_Rndapp[i]);
  printf("\n");
#endif

  /* 22. Comparaison */
  /* --------------- */
#ifdef _DEBUG_SAM_UNLOCK
  printf("Final test\n");
#endif

  for (i = 0; i< sizeof(Recv_Rndapp); i++)
    if (RndApp[i] != Recv_Rndapp[i])
      return SCARD_W_CARD_NOT_AUTHENTICATED;

  return SCARD_S_SUCCESS;

}
#endif
