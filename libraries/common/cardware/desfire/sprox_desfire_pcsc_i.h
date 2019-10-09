#ifndef __SPROX_DESFIRE_PCSC_I_H__
#define __SPROX_DESFIRE_PCSC_I_H__

typedef struct
{
  BOOL                  bInited;
  SCARDHANDLE           hCard;
  SPROX_DESFIRE_CTX_ST *desfire_ctx;

} INSTANCE_ST;

#ifndef MAX_INSTANCES
#define MAX_INSTANCES 16
#endif

#ifndef MAX_DATA_SIZE
#define MAX_DATA_SIZE 255
#endif

#endif