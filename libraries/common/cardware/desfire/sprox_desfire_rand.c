/**h* DesfireAPI/Random
 *
 * NAME
 *   DesfireAPI :: Pseudo-randon numbers generation module
 *
 * COPYRIGHT
 *   (c) 2009 SpringCard - www.springcard.com
 *
 * DESCRIPTION
 *   This is a basic PRNG, better than rand() but lacking
 *   some entropy to be really secure.
 *
 **/
#include "sprox_desfire_i.h"

#ifndef WIN32
#include <sys/time.h>
#include <unistd.h>
#endif

static void  Randomize(void);

/* CAUTION: We use a static structure to store our values in. */
static BOOL  random_inited = FALSE;
static BYTE  random_seed[8];
static TDES_CTX_ST random_cipher_ctx;

/*
ANSI X9.31 ALGORITHM:
Given

    * D, a 64-bit representation of the current date-time
    * S, a secret 64-bit seed that will be updated by this process
    * K, a secret (Triple DES) key

Step 1. Compute the 64-bit block X = G(S, K, D) as follows:

   1. I = E(D, K)
   2. X = E(I XOR S, K)
   3. S' = E(X XOR I, K)

where E(p, K) is the (Triple DES) encryption of the 64-bit block p using key K.

Step 2. Return X and set S = S' for the next cycle.
*/

void GetRandomBytes(SPROX_PARAM  BYTE rnd[], DWORD size)
{
	BYTE I[8], X[8];
  DWORD i;
#if (defined(WIN32) && defined(WINCE))
  SYSTEMTIME sys_time;
#endif
  //SPROX_DESFIRE_GET_CTX_V();

  if (rnd == NULL) return;

	/* Init context is still not done */
	if (!random_inited)
	{
		Randomize();
	  random_inited = TRUE;
	}

  /* Assign D */
#ifdef WIN32
  #ifdef WINCE
  GetSystemTime(&sys_time);
  SystemTimeToFileTime(&sys_time , (FILETIME *) I);
  #else
  GetSystemTimeAsFileTime((FILETIME *) I);
  #endif
#else
	gettimeofday((struct timeval *) I, NULL);
#endif

  while (size)
  {
	  /* I = E(D, K) */
    TDES_Encrypt(&random_cipher_ctx, I);

	  /* X = E(I XOR S, K) */
    for (i=0; i<8; i++)
      X[i] = I[i] ^ random_seed[i];
    TDES_Encrypt(&random_cipher_ctx, X);

    /* return X */
    if (size >= 8)
    {
      memcpy(rnd, X, 8);
      rnd  += 8;
      size -= 8;
    } else
    {
      memcpy(rnd, X, size);
      size = 0;
    }

	  /* S' = E(X XOR I, K) */
    for (i=0; i<8; i++)
      random_seed[i] = I[i] ^ X[i];

    TDES_Encrypt(&random_cipher_ctx, random_seed);
  }
}



#ifdef WIN32
  #include <wincrypt.h>
  #include <windows.h>
#else
  #include <unistd.h>
  #include <sys/time.h>
  #include <sys/types.h>
#endif


static void Randomize(void)
{
  BYTE key[16];

#ifdef WIN32
  HINSTANCE pid;
  #ifdef WINCE
  SYSTEMTIME sys_time;
  GetSystemTime(&sys_time);
  SystemTimeToFileTime(&sys_time , (FILETIME *) &key[0]);
  #else
  GetSystemTimeAsFileTime((FILETIME *) &key[0]);
  #endif
  pid = GetModuleHandle(NULL);
  memcpy(&key[8],  &pid, 4);
  memcpy(&key[12], &pid, 4);
#endif

#ifdef __linux__
  FILE *fp = fopen("/dev/urandom", "rb");
  if (fp != NULL)
  {
  	fread(key, sizeof(key), 1, fp);
  	fclose(fp);
  } else
  {
	  pid_t pid;
		gettimeofday((struct timeval *) &key[0], NULL);
	  pid = getpid();
	  memcpy(&key[8],  &pid, 4);
	  memcpy(&key[12], &pid, 4);
	}
#endif

  TDES_Init(&random_cipher_ctx, &key[0], &key[8], &key[0]);

  memcpy(random_seed, "SpringCard", 8);

  TDES_Encrypt(&random_cipher_ctx, random_seed);
}
