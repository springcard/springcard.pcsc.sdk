#include "calypso_audit.h"

#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include "lib-c/utils/inifile.h"

CALYPSO_AUDIT_CFG_ST audit_config;

P_CALYPSO_CTX cal_ctx = NULL;

SWORD         rdr_rc;
CALYPSO_RC    cal_rc;

SCARDHANDLE   hCardPcsc;
SCARDHANDLE   hSamPcsc;

BYTE          SamAtr[32];
SIZE_T    SamAtrLen;

void CalypsoAuditCleanup(void)
{

}

void CalypsoAuditStartup(int argc, char **argv)
{
  int i;
  char *inifile = NULL;
  char buffer[MAX_PATH];

  atexit(CalypsoAuditCleanup);

  /* Instanciate PC/SC */
  if (!PcscStartup())
  {
    printf("Failed to create PC/SC context");
    exit(EXIT_FAILURE);
  }

  /* Instanciate Calypso context */
  cal_ctx = CalypsoCreateContext();
  if (cal_ctx == NULL)
  {
    printf("Failed to create Calypso context");
    exit(EXIT_FAILURE);
  }
  CalypsoCleanupContext(cal_ctx);

  memset(&audit_config, 0, sizeof(audit_config));
  strcpy(audit_config.LogFile, "stdout");

  /* Parse the command line */

  for (i=1; i<argc; i++)
  {
    if (!stricmp(argv[i], "-v"))
    {
      audit_config.Verbose = TRUE;
      audit_config.Quiet   = FALSE;
    } else
    if (!stricmp(argv[i], "-q"))
    {
      audit_config.Quiet   = TRUE;
      audit_config.Verbose = FALSE;
    } else
    if (!stricmp(argv[i], "-t"))
    {
      audit_config.Test = TRUE;
    } else
    if (!strnicmp(argv[i], "-d", 2))
    {      
      audit_config.LogLevel = atoi(&argv[i][2]);
      if (audit_config.LogLevel == 0x00) audit_config.LogLevel = 0xFF;
    } else
    if (!stricmp(argv[i], "-o"))
    {
      i++; if (i>= argc) break;
      strcpy(audit_config.LogFile, argv[i]);
    } else
    if (!stricmp(argv[i], "-c"))
    {
      i++; if (i>= argc) break;
      inifile = argv[i];
    }
  }

  if (inifile == NULL)
  {
    printf("Configuration not defined\n");
    exit(EXIT_FAILURE);
  }


  CalypsoSetTraceLevel(audit_config.LogLevel);
  CalypsoSetTraceFile(audit_config.LogFile);

  /* Configure the access to the card */
  /* -------------------------------- */

  INIFILE_ReadString(inifile, "card", "mode", buffer, "", sizeof(buffer));

  if (!stricmp(buffer, "legacy"))
  {
    INIFILE_ReadString(inifile, "card", "protos", buffer, "", sizeof(buffer));
    if (strlen(buffer))
      audit_config.CardLegacyProtos = atoi(buffer);
    if (audit_config.CardLegacyProtos == 0)
      audit_config.CardLegacyProtos = 0x0083;

    if (audit_config.Verbose)
    {
      printf("Card: Legacy reader will be used (protos=%04X)\n", audit_config.CardLegacyProtos);
    }

  } else
  if (!stricmp(buffer, "pcsc"))
  {
    audit_config.CardPcsc = TRUE;

    INIFILE_ReadString(inifile, "card", "reader", buffer, "", sizeof(buffer));

    if (!strlen(buffer))
    {
      printf("Card: 'reader' must be specified\n");
      exit(EXIT_FAILURE);
    }

    audit_config.CardPcscIndex = PcscReaderIndex(buffer);
    if (audit_config.CardPcscIndex == (DWORD) -1)
    {
      printf("Card: reader '%s' not found\n", buffer);
      exit(EXIT_FAILURE);
    }

    if (audit_config.Verbose)
    {
      printf("Card: PC/SC reader '%s' will be used\n", buffer);
    }

  } else
  {
    printf("Card: 'mode' must be either 'pcsc' or 'legacy'\n");
    exit(EXIT_FAILURE);
  }

  /* Configure the access to the SAM */
  /* ------------------------------- */

  INIFILE_ReadString(inifile, "sam", "mode", buffer, "", sizeof(buffer));

  if (!stricmp(buffer, "legacy"))
  {
    INIFILE_ReadString(inifile, "sam", "slot", buffer, "", sizeof(buffer));
    if (strlen(buffer))
      audit_config.SamLegacySlot = atoi(buffer);

    if (audit_config.Verbose)
    {
      printf("Sam: Legacy reader will be used (slot=%d)\n", audit_config.SamLegacySlot);
    }

  } else
  if (!stricmp(buffer, "pcsc"))
  {
    audit_config.SamPcsc = TRUE;

    INIFILE_ReadString(inifile, "sam", "reader", buffer, "", sizeof(buffer));

    if (!strlen(buffer))
    {
      printf("Sam: 'reader' must be specified\n");
      exit(EXIT_FAILURE);
    }

    audit_config.SamPcscIndex = PcscReaderIndex(buffer);
    if (audit_config.SamPcscIndex == (DWORD) -1)
    {
      printf("Sam: reader '%s' not found\n", buffer);
      exit(EXIT_FAILURE);
    }

    if (audit_config.Verbose)
    {
      printf("Sam: PC/SC reader '%s' will be used\n", buffer);
    }

  } else
  {
    printf("Sam: 'mode' must be either 'pcsc' or 'legacy'\n");
    exit(EXIT_FAILURE);
  }


  /* Connect to the Legacy reader is required */
  /* ---------------------------------------- */

  if (!audit_config.CardPcsc || !audit_config.SamPcsc)
  {
    SWORD rc;

    SPROX_GetLibraryA(buffer, sizeof(buffer));
    if (audit_config.Verbose)
      printf("Using library '%s'\n", buffer);

    INIFILE_ReadString(inifile, "legacy", "device", buffer, "", sizeof(buffer));

    if (strlen(buffer))
    {
      if (audit_config.Verbose)
        printf("Connecting to the reader on '%s'...\n", buffer);

      rc = SPROX_ReaderOpen(buffer);
    } else
    {
      if (audit_config.Verbose)
        printf("Connecting to the reader...\n");

      rc = SPROX_ReaderOpen(NULL);
    }

    if (rc != MI_OK)
    {
      if (strlen(buffer))
        printf("Failed to connect to legacy reader '%s' (rc=%d)\n", buffer, rc);
      else
        printf("Failed to connect to legacy reader (rc=%d)\n", rc);
      exit(EXIT_FAILURE);
    }

    SPROX_ReaderGetFirmwareA(buffer, sizeof(buffer));
    if (audit_config.Verbose)
      printf("Connected to '%s'\n", buffer);
  }

  /* Connect to the SAM (must be there on startup) */
  /* --------------------------------------------- */

  if (audit_config.Verbose)
    printf("Connecting to the SAM...\n");

  if (audit_config.SamPcsc)
  {
    DWORD l;

    if (!PcscCardPresent(audit_config.SamPcscIndex))
    {
      printf("No SAM in reader\n");
      exit(EXIT_FAILURE);
    }

    l = sizeof(SamAtr);
    if (!PcscCardAtr(audit_config.SamPcscIndex, SamAtr, &l))
    {
      printf("Failed to get the ATR of the SAM\n");
      exit(EXIT_FAILURE);
    }

    if (!PcscConnect(audit_config.SamPcscIndex, &hSamPcsc))
    {
      printf("Failed to connect to the SAM\n");
      exit(EXIT_FAILURE);
    }

    SamAtrLen = l;

  } else
  {
    WORD  l;
    SWORD rc;

    l = sizeof(SamAtr);
    rc = SPROX_Card_PowerUp_Auto(audit_config.SamLegacySlot, SamAtr, &l);
    if (rc != MI_OK)
    {
      l = sizeof(SamAtr);
      rc = SPROX_Card_PowerUp(audit_config.SamLegacySlot, 0, SamAtr, &l);
    }

    if (rc != MI_OK)
    {
      printf("Failed to connect to the SAM in slot %d (rc=%d)\n", audit_config.SamLegacySlot, rc);
      exit(EXIT_FAILURE);
    }

    SamAtrLen = l;
  }

  if (audit_config.Verbose)
  {
    printf("SAM found, ATR=");
    for (i=0; i<(int)SamAtrLen; i++)
      printf("%02X", SamAtr[i]);
    printf("\n");
  }

  /* Verify the SAM is OK */
  /* -------------------- */
 
  {
    CALYPSO_RC rc;
    rc = CalypsoParseSamAtr(cal_ctx, SamAtr, SamAtrLen);
    if (rc)
    {
      printf("SAM's ATR is invalid ? (rc=%04X)\n", rc);
      exit(EXIT_FAILURE);
    }
  }

  set_leds_t(0x0100, 500);
}

