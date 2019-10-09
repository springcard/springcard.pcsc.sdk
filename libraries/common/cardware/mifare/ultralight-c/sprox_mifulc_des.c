/**h* MifUlCAPI/DES
 *
 * NAME
 *   MifUlCAPI :: DES module
 *
 * COPYRIGHT
 *   (c) 2009 SpringCard - www.springcard.com
 *
 * DESCRIPTION
 *   Implementation of DES and 3DES ciphering schemes.
 *
 **/
#include "sprox_mifulc_i.h"

static void DES_core(DES_CTX_ST *ctx, BYTE data[8], BOOL encrypt);
static void DES_schedule(const BYTE key[8], DWORD subkeys[32]);


/*
 ****************************************************************************
 *
 *  DES high level functions
 *
 ****************************************************************************
 */

/*
 * DES_Init
 * --------
 * Init a DES context with a 64-bit key
 * Parity bits are not checked
 */ 
void DES_Init(DES_CTX_ST *des_ctx, const BYTE key_data[8])
{
  BYTE i;

  if (des_ctx  == NULL) return;
  if (key_data == NULL) return;

  DES_schedule(key_data, des_ctx->encrypt_subkeys);

  for (i=0; i<32; i+=2)
  {
    des_ctx->decrypt_subkeys[i]   = des_ctx->encrypt_subkeys[30-i];
    des_ctx->decrypt_subkeys[i+1] = des_ctx->encrypt_subkeys[31-i];
  } 
}

void DES_Encrypt(DES_CTX_ST *des_ctx, BYTE data[8])
{
  DES_core(des_ctx, data, TRUE);
}
void DES_Encrypt2(DES_CTX_ST *des_ctx, BYTE outbuf[8], const BYTE inbuf[8])
{
  memcpy(outbuf, inbuf, 8);
  DES_Encrypt(des_ctx, outbuf);
}

void DES_Decrypt(DES_CTX_ST *des_ctx, BYTE data[8])
{
  DES_core(des_ctx, data, FALSE);
}
void DES_Decrypt2(DES_CTX_ST *des_ctx, BYTE outbuf[8], const BYTE inbuf[8])
{
  memcpy(outbuf, inbuf, 8);
  DES_Decrypt(des_ctx, outbuf);
}


/*
 *****************************************************************************
 *
 *  Triple-DES high level functions
 *
 *****************************************************************************
 */

/*
 * TDES_Init
 * ---------
 * Init a 3-DES context with 2 64-bit keys
 * Parity bits are not checked
 */
void TDES_Init(TDES_CTX_ST *tdes_ctx, const BYTE key1_data[8], const BYTE key2_data[8], const BYTE key3_data[8])
{
  DES_Init(&tdes_ctx->key1_ctx, key1_data);
  DES_Init(&tdes_ctx->key2_ctx, key2_data);

  if (key3_data != NULL)
  {
    DES_Init(&tdes_ctx->key3_ctx, key3_data);
  } else
  {
    memcpy(&tdes_ctx->key3_ctx, &tdes_ctx->key1_ctx, sizeof(DES_CTX_ST));
  }
}

/*
 * TDES_Encrypt
 * ------------
 * Standard 3-DES encryption (E1 -> D2 -> E3)
 */
void TDES_Encrypt(TDES_CTX_ST *tdes_ctx, BYTE data[8])
{
  DES_core(&tdes_ctx->key1_ctx, data, TRUE);
  DES_core(&tdes_ctx->key2_ctx, data, FALSE);
  DES_core(&tdes_ctx->key3_ctx, data, TRUE);
}
void TDES_Encrypt2(TDES_CTX_ST *tdes_ctx, BYTE outbuf[8], const BYTE inbuf[8])
{
  memcpy(outbuf, inbuf, 8);
  TDES_Encrypt(tdes_ctx, outbuf);
}

/*
 * TDES_Decrypt
 * ------------
 * Standard 3-DES decryption (D3 -> E2 -> D1)
 */
void TDES_Decrypt(TDES_CTX_ST *tdes_ctx, BYTE data[8])
{
  DES_core(&tdes_ctx->key3_ctx, data, FALSE);
  DES_core(&tdes_ctx->key2_ctx, data, TRUE);
  DES_core(&tdes_ctx->key1_ctx, data, FALSE);
}
void TDES_Decrypt2(TDES_CTX_ST *tdes_ctx, BYTE outbuf[8], const BYTE inbuf[8])
{
  memcpy(outbuf, inbuf, 8);
  TDES_Decrypt(tdes_ctx, outbuf);
}

/*
 ****************************************************************************
 *
 *  DES core
 *
 ****************************************************************************
 *
 * This is definitively not the fastest DES implementation, but the
 * interesting fact is that is compiles (and works !) on small MCUs
 * as well as on an i386 PC.
 *
 * This code has been writen with libgcrypt DES implementation as 
 * template (http://freshmeat.net/projects/libgcrypt/)
 */

/*
 * Pre-computed S-BOXes
 * --------------------
 */
static const DWORD DES_SBOX_1[64] =
{
  0x01010400, 0x00000000, 0x00010000, 0x01010404,
  0x01010004, 0x00010404, 0x00000004, 0x00010000,
  0x00000400, 0x01010400, 0x01010404, 0x00000400,
  0x01000404, 0x01010004, 0x01000000, 0x00000004,
  0x00000404, 0x01000400, 0x01000400, 0x00010400,
  0x00010400, 0x01010000, 0x01010000, 0x01000404,
  0x00010004, 0x01000004, 0x01000004, 0x00010004,
  0x00000000, 0x00000404, 0x00010404, 0x01000000,
  0x00010000, 0x01010404, 0x00000004, 0x01010000,
  0x01010400, 0x01000000, 0x01000000, 0x00000400,
  0x01010004, 0x00010000, 0x00010400, 0x01000004,
  0x00000400, 0x00000004, 0x01000404, 0x00010404,
  0x01010404, 0x00010004, 0x01010000, 0x01000404,
  0x01000004, 0x00000404, 0x00010404, 0x01010400,
  0x00000404, 0x01000400, 0x01000400, 0x00000000,
  0x00010004, 0x00010400, 0x00000000, 0x01010004
};

static const DWORD DES_SBOX_2[64] =
{
  0x80108020, 0x80008000, 0x00008000, 0x00108020,
  0x00100000, 0x00000020, 0x80100020, 0x80008020,
  0x80000020, 0x80108020, 0x80108000, 0x80000000,
  0x80008000, 0x00100000, 0x00000020, 0x80100020,
  0x00108000, 0x00100020, 0x80008020, 0x00000000,
  0x80000000, 0x00008000, 0x00108020, 0x80100000,
  0x00100020, 0x80000020, 0x00000000, 0x00108000,
  0x00008020, 0x80108000, 0x80100000, 0x00008020,
  0x00000000, 0x00108020, 0x80100020, 0x00100000,
  0x80008020, 0x80100000, 0x80108000, 0x00008000,
  0x80100000, 0x80008000, 0x00000020, 0x80108020,
  0x00108020, 0x00000020, 0x00008000, 0x80000000,
  0x00008020, 0x80108000, 0x00100000, 0x80000020,
  0x00100020, 0x80008020, 0x80000020, 0x00100020,
  0x00108000, 0x00000000, 0x80008000, 0x00008020,
  0x80000000, 0x80100020, 0x80108020, 0x00108000
};

static const DWORD DES_SBOX_3[64] =
{
  0x00000208, 0x08020200, 0x00000000, 0x08020008,
  0x08000200, 0x00000000, 0x00020208, 0x08000200,
  0x00020008, 0x08000008, 0x08000008, 0x00020000,
  0x08020208, 0x00020008, 0x08020000, 0x00000208,
  0x08000000, 0x00000008, 0x08020200, 0x00000200,
  0x00020200, 0x08020000, 0x08020008, 0x00020208,
  0x08000208, 0x00020200, 0x00020000, 0x08000208,
  0x00000008, 0x08020208, 0x00000200, 0x08000000,
  0x08020200, 0x08000000, 0x00020008, 0x00000208,
  0x00020000, 0x08020200, 0x08000200, 0x00000000,
  0x00000200, 0x00020008, 0x08020208, 0x08000200,
  0x08000008, 0x00000200, 0x00000000, 0x08020008,
  0x08000208, 0x00020000, 0x08000000, 0x08020208,
  0x00000008, 0x00020208, 0x00020200, 0x08000008,
  0x08020000, 0x08000208, 0x00000208, 0x08020000,
  0x00020208, 0x00000008, 0x08020008, 0x00020200
};

static const DWORD DES_SBOX_4[64] =
{
  0x00802001, 0x00002081, 0x00002081, 0x00000080,
  0x00802080, 0x00800081, 0x00800001, 0x00002001,
  0x00000000, 0x00802000, 0x00802000, 0x00802081,
  0x00000081, 0x00000000, 0x00800080, 0x00800001,
  0x00000001, 0x00002000, 0x00800000, 0x00802001,
  0x00000080, 0x00800000, 0x00002001, 0x00002080,
  0x00800081, 0x00000001, 0x00002080, 0x00800080,
  0x00002000, 0x00802080, 0x00802081, 0x00000081,
  0x00800080, 0x00800001, 0x00802000, 0x00802081,
  0x00000081, 0x00000000, 0x00000000, 0x00802000,
  0x00002080, 0x00800080, 0x00800081, 0x00000001,
  0x00802001, 0x00002081, 0x00002081, 0x00000080,
  0x00802081, 0x00000081, 0x00000001, 0x00002000,
  0x00800001, 0x00002001, 0x00802080, 0x00800081,
  0x00002001, 0x00002080, 0x00800000, 0x00802001,
  0x00000080, 0x00800000, 0x00002000, 0x00802080
};

static const DWORD DES_SBOX_5[64] =
{
  0x00000100, 0x02080100, 0x02080000, 0x42000100,
  0x00080000, 0x00000100, 0x40000000, 0x02080000,
  0x40080100, 0x00080000, 0x02000100, 0x40080100,
  0x42000100, 0x42080000, 0x00080100, 0x40000000,
  0x02000000, 0x40080000, 0x40080000, 0x00000000,
  0x40000100, 0x42080100, 0x42080100, 0x02000100,
  0x42080000, 0x40000100, 0x00000000, 0x42000000,
  0x02080100, 0x02000000, 0x42000000, 0x00080100,
  0x00080000, 0x42000100, 0x00000100, 0x02000000,
  0x40000000, 0x02080000, 0x42000100, 0x40080100,
  0x02000100, 0x40000000, 0x42080000, 0x02080100,
  0x40080100, 0x00000100, 0x02000000, 0x42080000,
  0x42080100, 0x00080100, 0x42000000, 0x42080100,
  0x02080000, 0x00000000, 0x40080000, 0x42000000,
  0x00080100, 0x02000100, 0x40000100, 0x00080000,
  0x00000000, 0x40080000, 0x02080100, 0x40000100
};

static const DWORD DES_SBOX_6[64] =
{
  0x20000010, 0x20400000, 0x00004000, 0x20404010,
  0x20400000, 0x00000010, 0x20404010, 0x00400000,
  0x20004000, 0x00404010, 0x00400000, 0x20000010,
  0x00400010, 0x20004000, 0x20000000, 0x00004010,
  0x00000000, 0x00400010, 0x20004010, 0x00004000,
  0x00404000, 0x20004010, 0x00000010, 0x20400010,
  0x20400010, 0x00000000, 0x00404010, 0x20404000,
  0x00004010, 0x00404000, 0x20404000, 0x20000000,
  0x20004000, 0x00000010, 0x20400010, 0x00404000,
  0x20404010, 0x00400000, 0x00004010, 0x20000010,
  0x00400000, 0x20004000, 0x20000000, 0x00004010,
  0x20000010, 0x20404010, 0x00404000, 0x20400000,
  0x00404010, 0x20404000, 0x00000000, 0x20400010,
  0x00000010, 0x00004000, 0x20400000, 0x00404010,
  0x00004000, 0x00400010, 0x20004010, 0x00000000,
  0x20404000, 0x20000000, 0x00400010, 0x20004010
};

static const DWORD DES_SBOX_7[64] =
{
  0x00200000, 0x04200002, 0x04000802, 0x00000000,
  0x00000800, 0x04000802, 0x00200802, 0x04200800,
  0x04200802, 0x00200000, 0x00000000, 0x04000002,
  0x00000002, 0x04000000, 0x04200002, 0x00000802,
  0x04000800, 0x00200802, 0x00200002, 0x04000800,
  0x04000002, 0x04200000, 0x04200800, 0x00200002,
  0x04200000, 0x00000800, 0x00000802, 0x04200802,
  0x00200800, 0x00000002, 0x04000000, 0x00200800,
  0x04000000, 0x00200800, 0x00200000, 0x04000802,
  0x04000802, 0x04200002, 0x04200002, 0x00000002,
  0x00200002, 0x04000000, 0x04000800, 0x00200000,
  0x04200800, 0x00000802, 0x00200802, 0x04200800,
  0x00000802, 0x04000002, 0x04200802, 0x04200000,
  0x00200800, 0x00000000, 0x00000002, 0x04200802,
  0x00000000, 0x00200802, 0x04200000, 0x00000800,
  0x04000002, 0x04000800, 0x00000800, 0x00200002
};

static const DWORD DES_SBOX_8[64] =
{
  0x10001040, 0x00001000, 0x00040000, 0x10041040,
  0x10000000, 0x10001040, 0x00000040, 0x10000000,
  0x00040040, 0x10040000, 0x10041040, 0x00041000,
  0x10041000, 0x00041040, 0x00001000, 0x00000040,
  0x10040000, 0x10000040, 0x10001000, 0x00001040,
  0x00041000, 0x00040040, 0x10040040, 0x10041000,
  0x00001040, 0x00000000, 0x00000000, 0x10040040,
  0x10000040, 0x10001000, 0x00041040, 0x00040000,
  0x00041040, 0x00040000, 0x10041000, 0x00001000, 
  0x00000040, 0x10040040, 0x00001000, 0x00041040,
  0x10001000, 0x00000040, 0x10000040, 0x10040000,
  0x10040040, 0x10000000, 0x00040000, 0x10001040,
  0x00000000, 0x10041040, 0x00040040, 0x10000040,
  0x10040000, 0x10001000, 0x10001040, 0x00000000,
  0x10041040, 0x00041000, 0x00041000, 0x00001040,
  0x00001040, 0x00040040, 0x10000000, 0x10041000
};

/*
 * 8 bytes to DWORD conversion
 * ---------------------------
 */
static void DES_arr2dw(const BYTE data[8], DWORD *left, DWORD *right)
{
  BYTE i;
  
  *left = 0;
  for (i=0; i<4; i++)
  {
    *left <<= 8;
    *left  += data[i];
  }
  
  *right = 0;
  for (i=4; i<8; i++)
  {
    *right <<= 8;
    *right  += data[i];
  }
}

static void DES_dw2arr(BYTE data[8], DWORD left, DWORD right)
{
  BYTE i;
  
  for (i=0; i<4; i++)
  {
    data[3 - i] = (BYTE) left;
    left >>= 8;
    data[7 - i] = (BYTE) right;
    right >>= 8;
  }  
}

/*
 * DES permutation (swap bits across two words)
 * --------------------------------------------
 */
#define DES_PERMUTATION(a,t,b,o,m) t=((a>>o)^b)&m; b^=t; a^=t<<o;

/*
 * A single DES round
 * ------------------
 */
#define DES_ROUND(src, dst, t, p_subkey) \
    t = src ^ *p_subkey++; \
    dst ^= DES_SBOX_8[ t      & 0x3F]; \
    dst ^= DES_SBOX_6[(t>>8)  & 0x3F]; \
    dst ^= DES_SBOX_4[(t>>16) & 0x3F]; \
    dst ^= DES_SBOX_2[(t>>24) & 0x3F]; \
    t = ((src << 28) | (src >> 4)) ^ *p_subkey++; \
    dst ^= DES_SBOX_7[ t      & 0x3F]; \
    dst ^= DES_SBOX_5[(t>>8)  & 0x3F]; \
    dst ^= DES_SBOX_3[(t>>16) & 0x3F]; \
    dst ^= DES_SBOX_1[(t>>24) & 0x3F];

/*
 * DES_core
 * --------
 * DES single block encryption/decryption
 */
static void DES_core(DES_CTX_ST *ctx, BYTE data[8], BOOL encrypt)
{
  DWORD left, right, t;
  DWORD *p_subkey;
  BYTE r;

  /* Translate data : 8 BYTEs -> 2 DWORDs */
  DES_arr2dw(data, &left, &right);
  
  /* Select subkeys according to encrypt/decrypt mode */
  if (encrypt)
    p_subkey = ctx->encrypt_subkeys;
  else
    p_subkey = ctx->decrypt_subkeys;

  /* Initial permutation */
  /* ------------------- */
  DES_PERMUTATION(left, t, right, 4, 0x0f0f0f0f);
  DES_PERMUTATION(left, t, right, 16, 0x0000ffff);
  DES_PERMUTATION(right, t, left, 2, 0x33333333);
  DES_PERMUTATION(right, t, left, 8, 0x00ff00ff);
  right =  (right << 1) | (right >> 31);
  t  =  (left ^ right) & 0xaaaaaaaa;
  right ^= t;
  left  ^= t;
  left  =  (left << 1) | (left >> 31);

  /* 16 rounds */
  /* --------- */
  for (r=0; r<16; r+= 2)
  {
    DES_ROUND (right, left, t, p_subkey);
    DES_ROUND (left, right, t, p_subkey);
  }

  /* Final permutation */
  /* ----------------- */
  right = (right << 31) | (right >> 1);
  t = (right ^ left) & 0xaaaaaaaa;
  right ^= t;
  left ^= t;
  left = (left << 31) | (left >> 1);
  DES_PERMUTATION(left, t, right, 8, 0x00FF00FF);
  DES_PERMUTATION(left, t, right, 2, 0x33333333);
  DES_PERMUTATION(right, t, left, 16, 0x0000FFFF);
  DES_PERMUTATION(right, t, left, 4, 0x0F0F0F0F);

  /* Replace data with result : 2 DWORDs -> 8 BYTEs */
  DES_dw2arr(data, right, left);
}

/*
 * DES key schedule
 * ----------------
 */

/* Permutation tables */
/* ------------------ */
static const DWORD DES_LEFT_PERM[16] =
{
  0x00000000, 0x00000001, 0x00000100, 0x00000101,
  0x00010000, 0x00010001, 0x00010100, 0x00010101,
  0x01000000, 0x01000001, 0x01000100, 0x01000101,
  0x01010000, 0x01010001, 0x01010100, 0x01010101
};

static const DWORD DES_RIGHT_PERM[16] =
{
  0x00000000, 0x01000000, 0x00010000, 0x01010000,
  0x00000100, 0x01000100, 0x00010100, 0x01010100,
  0x00000001, 0x01000001, 0x00010001, 0x01010001,
  0x00000101, 0x01000101, 0x00010101, 0x01010101
};

/* Rotation table */
/* -------------- */
static const BYTE DES_ROTATIONS[16] =
{
  1, 1, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 1
};

/*
 * DES key schedule
 * ----------------
 */
static void DES_schedule(const BYTE key[8], DWORD subkeys[32])
{
  DWORD left, right, t;
  BYTE r, i;

  DES_arr2dw(key, &left, &right);

  DES_PERMUTATION(right, t, left, 4, 0x0F0F0F0F);
  DES_PERMUTATION(right, t, left, 0, 0x10101010);

  left =
      (DES_LEFT_PERM[(left >> 0) & 0x0F] << 3)
    | (DES_LEFT_PERM[(left >> 8) & 0x0F] << 2)
    | (DES_LEFT_PERM[(left >> 16) & 0x0F] << 1)
    | (DES_LEFT_PERM[(left >> 24) & 0x0F])
    | (DES_LEFT_PERM[(left >> 5) & 0x0F] << 7)
    | (DES_LEFT_PERM[(left >> 13) & 0x0F] << 6)
    | (DES_LEFT_PERM[(left >> 21) & 0x0F] << 5)
    | (DES_LEFT_PERM[(left >> 29) & 0x0F] << 4);

  left &= 0x0FFFFFFF;

  right =
      (DES_RIGHT_PERM[(right >> 1) & 0x0F] << 3)
    | (DES_RIGHT_PERM[(right >> 9) & 0x0F] << 2)
    | (DES_RIGHT_PERM[(right >> 17) & 0x0F] << 1)
    | (DES_RIGHT_PERM[(right >> 25) & 0x0F])
    | (DES_RIGHT_PERM[(right >> 4) & 0x0F] << 7)
    | (DES_RIGHT_PERM[(right >> 12) & 0x0F] << 6)
    | (DES_RIGHT_PERM[(right >> 20) & 0x0F] << 5)
    | (DES_RIGHT_PERM[(right >> 28) & 0x0F] << 4);

  right &= 0x0FFFFFFF;

  i = 0;
  for (r = 0; r < 16; r++)
  {
    left  = 
      ((left << DES_ROTATIONS[r])
      | (left >> (28 - DES_ROTATIONS[r]))) & 0x0fffffff;
      
    right =
      ((right << DES_ROTATIONS[r])
      | (right >> (28 - DES_ROTATIONS[r]))) & 0x0fffffff;

    subkeys[i++] =
       ((left << 4) & 0x24000000)
      | ((left << 28) & 0x10000000)
      | ((left << 14) & 0x08000000)
      | ((left << 18) & 0x02080000)
      | ((left << 6) & 0x01000000)
      | ((left << 9) & 0x00200000)
      | ((left >> 1) & 0x00100000)
      | ((left << 10) & 0x00040000)
      | ((left << 2) & 0x00020000)
      | ((left >> 10) & 0x00010000)
      | ((right >> 13) & 0x00002000)
      | ((right >> 4) & 0x00001000)
      | ((right << 6) & 0x00000800)
      | ((right >> 1) & 0x00000400)
      | ((right >> 14) & 0x00000200)
      | (right & 0x00000100)
      | ((right >> 5) & 0x00000020)
      | ((right >> 10) & 0x00000010)
      | ((right >> 3) & 0x00000008)
      | ((right >> 18) & 0x00000004)
      | ((right >> 26) & 0x00000002)
      | ((right >> 24) & 0x00000001);

    subkeys[i++] =
       ((left << 15) & 0x20000000)
      | ((left << 17) & 0x10000000)
      | ((left << 10) & 0x08000000)
      | ((left << 22) & 0x04000000)
      | ((left >> 2) & 0x02000000)
      | ((left << 1) & 0x01000000)
      | ((left << 16) & 0x00200000)
      | ((left << 11) & 0x00100000)
      | ((left << 3) & 0x00080000)
      | ((left >> 6) & 0x00040000)
      | ((left << 15) & 0x00020000)
      | ((left >> 4) & 0x00010000)
      | ((right >> 2) & 0x00002000)
      | ((right << 8) & 0x00001000)
      | ((right >> 14) & 0x00000808)
      | ((right >> 9) & 0x00000400)
      | ((right) & 0x00000200)
      | ((right << 7) & 0x00000100)
      | ((right >> 7) & 0x00000020)
      | ((right >> 3) & 0x00000011)
      | ((right << 2) & 0x00000004)
      | ((right >> 21) & 0x00000002);
  }
}

