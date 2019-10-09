/**h* DesfireAPI/Transactions
 *
 * NAME
 *   DesfireAPI :: Transaction functions
 *
 * COPYRIGHT
 *   (c) 2009 SpringCard - www.springcard.com
 *
 * DESCRIPTION
 *   Implementation of DESFIRE transaction-related functions.
 *
 **/
#include "sprox_desfire_i.h"

/**f* DesfireAPI/CommitTransaction
 *
 * NAME
 *   CommitTransaction
 *
 * DESCRIPTION
 *   Validates all previous write access' on Backup Data Files, Value Files and Record Files within
 *   one application
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_CommitTransaction(void);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_CommitTransaction(SPROX_INSTANCE rInst);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_CommitTransaction(SCARDHANDLE hCard);
 *
 * RETURNS
 *   DF_OPERATION_OK    : operation succeeded
 *   Other code if internal or communication error has occured. 
 *
 * NOTES
 *   Validates all write access to files with integrated backup mechanisms
 *
 * SEE ALSO
 *   AbortTransaction
 *
 **/
SPROX_API_FUNC(Desfire_CommitTransaction) (SPROX_PARAM_V)
{
  SPROX_DESFIRE_GET_CTX();
  
  /* Create the info block containing the command code. */
  ctx->xfer_buffer[INF + 0] = DF_COMMIT_TRANSACTION;
  ctx->xfer_length = 1;

  /* Communicate the info block to the card and check the operation's return status. */
  return SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  0, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
}

/**f* DesfireAPI/AbortTransaction
 *
 * NAME
 *   AbortTransaction
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SWORD SPROX_Desfire_AbortTransaction(void);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SWORD SPROXx_Desfire_AbortTransaction(SPROX_INSTANCE rInst);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SSCardDesfire_AbortTransaction(SCARDHANDLE hCard);
 *
 * DESCRIPTION
 *   Invalidates all previous write access' on Backup Data Files, Value Files and Record Files
 *   within one application
 *
 * RETURNS
 *   DF_OPERATION_OK    : operation succeeded
 *   Other code if internal or communication error has occured. 
 *
 * NOTES
 *   Invalidates all write access to files with integrated backup mechanisms without changing the authentication status   
 *
 * SEE ALSO
 *   CommitTransaction
 *
 **/
SPROX_API_FUNC(Desfire_AbortTransaction) (SPROX_PARAM_V)
{
  SPROX_DESFIRE_GET_CTX();
  
  /* Create the info block containing the command code. */
  ctx->xfer_buffer[INF + 0] = DF_ABORT_TRANSACTION;
  ctx->xfer_length = 1;

  /* Communicate the info block to the card and check the operation's return status. */
  return SPROX_API_CALL(Desfire_Command) (SPROX_PARAM_P  0, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
}
