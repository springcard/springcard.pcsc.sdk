#ifndef __SPROX_DESFIRE_I_H__
#define __SPROX_DESFIRE_I_H__

#ifdef WIN32
  #include <windows.h>
  #include <tchar.h>

  #define SPROX_DESFIRE_LIB __declspec( dllexport )

  #ifndef UNDER_CE
    /* Under Desktop Windows we use the cdecl calling convention */
    #define SPROX_DESFIRE_API __cdecl
  #else
    /* Under Windows CE we use the stdcall calling convention */
    #define SPROX_DESFIRE_API __stdcall
  #endif

#else
  #define _T(x) x
#endif

#ifndef TRUE
  #define TRUE 1
#endif
#ifndef FALSE
  #define FALSE 0
#endif

#ifndef _USE_PCSC

  /*
   * SpringProx API compliant library
   * --------------------------------
   */

    #define SPROX_RC                     SWORD

  #ifndef SPROX_API_REENTRANT

    #define SPROX_API_FUNC_T(a, b)       SPROX_DESFIRE_LIB a     SPROX_DESFIRE_API SPROX_ ## b
    #define SPROX_API_FUNC(a)            SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROX_ ## a
    #define SPROX_DESFIRE_GET_CTX()      SPROX_DESFIRE_CTX_ST *ctx = &desfire_ctx; (void) ctx;
    #define SPROX_DESFIRE_GET_CTX_V()    SPROX_DESFIRE_CTX_ST *ctx = &desfire_ctx; (void) ctx;
    #define SPROX_API_CALL(a)            SPROX_ ## a
    #define SPROX_PARAM
    #define SPROX_PARAM_V                void
    #define SPROX_PARAM_P
    #define SPROX_PARAM_PV

    #include "products/springprox/api/springprox.h"
    #include "sprox_desfire.h"

    SPRINGPROX_LIB SWORD SPRINGPROX_API SPROX_TclA_ExchangeDF(BYTE cid, const BYTE send_buffer[], WORD send_len, BYTE recv_buffer[], WORD *recv_len);

  #else

    #define SPROX_API_FUNC_T(a, b)       SPROX_DESFIRE_LIB a     SPROX_DESFIRE_API SPROXx_ ## b
    #define SPROX_API_FUNC(a)            SPROX_DESFIRE_LIB SWORD SPROX_DESFIRE_API SPROXx_ ## a
    #define SPROX_DESFIRE_GET_CTX()      SPROX_DESFIRE_CTX_ST *ctx = desfire_get_ctx(rInst); if (ctx == NULL) return MI_INVALID_READER_CONTEXT;
    #define SPROX_DESFIRE_GET_CTX_V()    SPROX_DESFIRE_CTX_ST *ctx = desfire_get_ctx(rInst); if (ctx == NULL) return;
    #define SPROX_API_CALL(a)            SPROXx_ ## a
    #define SPROX_PARAM                  SPROX_INSTANCE rInst,
    #define SPROX_PARAM_V                SPROX_INSTANCE rInst
    #define SPROX_PARAM_P                rInst,
    #define SPROX_PARAM_PV               rInst

    #include "products/springprox/api/springprox_ex.h"
    #include "sprox_desfire_ex.h"

    SPRINGPROX_LIB SWORD SPRINGPROX_API SPROXx_TclA_ExchangeDF(SPROX_INSTANCE rInst, BYTE cid, const BYTE send_buffer[], WORD send_len, BYTE recv_buffer[], WORD *recv_len);

  #endif

#else

  /*
   * PC/SC API compliant library
   * ---------------------------
   */

    #define SPROX_RC                     LONG

    #define SPROX_API_FUNC_T(a, b)       SPROX_DESFIRE_LIB a     SPROX_DESFIRE_API SCard ## b
    #define SPROX_API_FUNC(a)            SPROX_DESFIRE_LIB LONG  SPROX_DESFIRE_API SCard ## a
    #define SPROX_DESFIRE_GET_CTX()      SPROX_DESFIRE_CTX_ST *ctx = desfire_get_ctx(hCard); if (ctx == NULL) return SCARD_E_INVALID_HANDLE;
    #define SPROX_DESFIRE_GET_CTX_V()    SPROX_DESFIRE_CTX_ST *ctx = desfire_get_ctx(hCard); if (ctx == NULL) return;
    #define SPROX_API_CALL(a)            SCard ## a
    #define SPROX_PARAM                  SCARDHANDLE hCard,
    #define SPROX_PARAM_V                SCARDHANDLE hCard
    #define SPROX_PARAM_P                hCard,
    #define SPROX_PARAM_PV               hCard

    #include "pcsc_desfire.h"

#endif


/* DES and 3-DES ciphering must be embedded in this library */
/* -------------------------------------------------------- */

typedef struct
{
  DWORD encrypt_subkeys[32];
  DWORD decrypt_subkeys[32];
} DES_CTX_ST;

typedef struct
{
  DES_CTX_ST key1_ctx;
  DES_CTX_ST key2_ctx;
  DES_CTX_ST key3_ctx;
} TDES_CTX_ST;

void DES_Init(DES_CTX_ST *context, const BYTE key[8]);

void DES_Encrypt(DES_CTX_ST *context, BYTE inoutbuf[8]);
void DES_Decrypt(DES_CTX_ST *context, BYTE inoutbuf[8]);

void DES_Encrypt2(DES_CTX_ST *context, BYTE outbuf[8], const BYTE inbuf[8]);
void DES_Decrypt2(DES_CTX_ST *context, BYTE outbuf[8], const BYTE inbuf[8]);

void TDES_Init(TDES_CTX_ST *context, const BYTE key1[8], const BYTE key2[8], const BYTE key3[8]);

void TDES_Encrypt(TDES_CTX_ST *context, BYTE inoutbuf[8]);
void TDES_Decrypt(TDES_CTX_ST *context, BYTE inoutbuf[8]);

void TDES_Encrypt2(TDES_CTX_ST *context, BYTE outbuf[8], const BYTE inbuf[8]);
void TDES_Decrypt2(TDES_CTX_ST *context, BYTE outbuf[8], const BYTE inbuf[8]);


typedef struct
{
  DWORD enc_schd[60];  /* Key schedule                          */
  DWORD dec_schd[60];  /* Key schedule                          */
  DWORD key_bits;      /* Size of the key (bits)                */
  DWORD rounds;        /* Key-length-dependent number of rounds */
} AES_CTX_ST;

void AES_Init(AES_CTX_ST *context, const BYTE key[16]);
void AES_Encrypt(AES_CTX_ST *context, BYTE data[16]);
void AES_Encrypt2(AES_CTX_ST *context, BYTE outbuf[16], BYTE inbuf[16]);
void AES_Decrypt(AES_CTX_ST *context, BYTE data[16]);
void AES_Decrypt2(AES_CTX_ST *context, BYTE outbuf[16], BYTE inbuf[16]);


BOOL BuildIsoApdu(const BYTE native_buffer[], DWORD native_buflen, BYTE iso_buffer[], DWORD *iso_buflen, BOOL use_reader_wrapping);


#include "desfire_card.h"



/*
 * Internal module functions
 */
SPROX_API_FUNC(Desfire_ReadDataEx)     (SPROX_PARAM  BYTE read_command, BYTE file_id, BYTE comm_mode, DWORD from_offset, DWORD item_count, DWORD item_size, BYTE * data, DWORD * done_size);
SPROX_API_FUNC(Desfire_WriteDataEx)    (SPROX_PARAM  BYTE write_command, BYTE file_id, BYTE comm_mode, DWORD from_offset, DWORD size, const BYTE data[]);
SPROX_API_FUNC(Desfire_ModifyValue)    (SPROX_PARAM  BYTE modify_command, BYTE file_id, BYTE comm_mode, LONG amount);

/*
 * Ciphering
 */
void     Desfire_CleanupAuthentication (SPROX_PARAM_V);
void     Desfire_InitCrypto3Des        (SPROX_PARAM  const BYTE des_key1[8], const BYTE des_key2[8], const BYTE des_key3[8]);
void     Desfire_InitCryptoAes         (SPROX_PARAM  const BYTE aes_key[16]);
void     Desfire_XferCipherSend        (SPROX_PARAM  DWORD start_offset);
void     Desfire_CipherSend            (SPROX_PARAM  BYTE data[], DWORD *length, DWORD max_length);
void     Desfire_CipherRecv            (SPROX_PARAM  BYTE data[], DWORD *length);

void     Desfire_CleanupInitVector(SPROX_PARAM_V);

/*
 * MAC stuff
 * ---------
 */
void     Desfire_ComputeMac            (SPROX_PARAM  const BYTE data[], DWORD length, BYTE mac[4]);
SPROX_RC Desfire_VerifyMacRecv         (SPROX_PARAM  BYTE recv_buffer[], DWORD *recv_length);

/*
 * CMAC stuff
 * ----------
 */
void     Desfire_InitCmac              (SPROX_PARAM_V);
void     Desfire_ComputeCmac           (SPROX_PARAM  const BYTE data[], DWORD length, BOOL move_status, BYTE cmac[8]);
SPROX_RC Desfire_VerifyCmacRecv        (SPROX_PARAM  BYTE buffer[], DWORD *length);
SPROX_RC Desfire_XferCmacSend          (SPROX_PARAM  BOOL append);
SPROX_RC Desfire_XferCmacRecv          (SPROX_PARAM_V);


/*
 * CRC stuff
 * ---------
 */
WORD     ComputeCrc16                  (const BYTE data[], DWORD length, BYTE crc[2]);
DWORD    ComputeCrc32                  (const BYTE data[], DWORD length, BYTE crc[4]);

void     Desfire_XferAppendCrc         (SPROX_PARAM  DWORD start_offset);

void     Desfire_ComputeCrc16          (SPROX_PARAM  const BYTE data[], DWORD length, BYTE crc[2]);
void     Desfire_ComputeCrc32          (SPROX_PARAM  const BYTE data[], DWORD length, BYTE crc[4]);

SPROX_RC Desfire_VerifyCrc16           (SPROX_PARAM  BYTE data[], DWORD  length, BYTE crc[2]);
SPROX_RC Desfire_VerifyCrc32           (SPROX_PARAM  BYTE data[], DWORD  length, BOOL move_status, BYTE crc[4]);

/*
 * PRNG
 * ----
 */
void GetRandomBytes            (SPROX_PARAM  BYTE rnd[], DWORD size);

/*
 * Communication with the card
 * ---------------------------
 */
SPROX_API_FUNC(Desfire_Command)  (SPROX_PARAM  WORD accept_len, BYTE flags);
SPROX_API_FUNC(Desfire_Exchange) (SPROX_PARAM_V);

#ifdef SPROX_DESFIRE_WITH_SAM
/*
 * Ciphering with the SAM
 * ----------------------
 */
LONG SAM_GenerateMAC(SCARDHANDLE hSam, BYTE *pbIn, DWORD dwInLength, BYTE *pbOut, DWORD * dwOutLength);
LONG SAM_VerifyMAC(SCARDHANDLE hSam, BYTE status, BYTE *pbIn, DWORD dwInLength, BYTE bMacLength, BOOL fKeepStatus);
LONG SAM_EncipherData(SCARDHANDLE hSam, BYTE *pbIn, DWORD dwInLength, BYTE offset, BYTE *pbOut, DWORD * dwOutLength);
LONG SAM_DecipherData(SCARDHANDLE hSam, BYTE bCardStatus, DWORD dwResultLength, BYTE *pbCipher, DWORD dwCipherLength, BOOL fSkpiStatus, BYTE *pbOut, DWORD * dwOutLength);
#endif

/* Flags for DesFireCard_Command */
#define INF 0
#define WANTS_OPERATION_OK      0x01
#define WANTS_ADDITIONAL_FRAME  0x02

#define FAST_CHAINING_ALLOWED   0x08

#define APPEND_COMMAND_CMAC     0x10
#define COMPUTE_COMMAND_CMAC    0x20
#define CHECK_RESPONSE_CMAC     0x40
#define LOOSE_RESPONSE_CMAC     0x80

#define KEY_EMPTY               0x00
//#define	KEY_UNSET               0xFF

#define KEY_LEGACY_DES          0x01
#define KEY_LEGACY_3DES         0x02

#define KEY_ISO_DES             0x81
#define KEY_ISO_3DES2K          0x82

#define KEY_ISO_3DES3K          0x83
#define KEY_ISO_AES             0x84

#define KEY_ISO_MODE            0x80

#undef D
#define D(x)

void dump_buffer(const BYTE * buffer, WORD len);

typedef struct
{
  BYTE  tcl_cid;
  BYTE  iso_wrapping;

  DWORD current_aid;

  BYTE  session_type;
  BYTE  session_key[24];
  BYTE  session_key_id;

  BOOL  xfer_fast;
  DWORD xfer_length;
  BYTE  xfer_buffer[64];

  BYTE  init_vector[16];

  BYTE  cmac_subkey_1[16];
  BYTE  cmac_subkey_2[16];

  union
  {
    DES_CTX_ST  des;
    TDES_CTX_ST tdes;
    AES_CTX_ST  aes;
  } cipher_context;

#ifdef SPROX_DESFIRE_WITH_SAM
  BOOL sam_session_active;
  struct
  {
    BOOL                  bInited;
    SCARDHANDLE           hSam;
  } sam_context;

#endif

} SPROX_DESFIRE_CTX_ST;

#ifndef _USE_PCSC
  #ifndef SPROX_API_REENTRANT
    extern SPROX_DESFIRE_CTX_ST desfire_ctx;
  #else
    SPROX_DESFIRE_CTX_ST *desfire_get_ctx(SPROX_INSTANCE rInst);
  #endif
#else
    SPROX_DESFIRE_CTX_ST *desfire_get_ctx(SCARDHANDLE hCard);
#endif

#ifdef _USE_PCSC
#include "sprox_desfire_pcsc_i.h"
#endif

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>

#endif
