#ifndef __CALYPSO_ERRORS_H__
#define __CALYPSO_ERRORS_H__

/* Definition of Calypso error codes */
/* --------------------------------- */

#define CALYPSO_SUCCESS                 0x00

#define CALYPSO_ERR_INVALID_CONTEXT     0x01
#define CALYPSO_ERR_INVALID_PARAM       0x02
#define CALYPSO_ERR_BUFFER_TOO_SHORT    0x03

#define CALYPSO_ERR_INTERNAL_OVERFLOW   0x06
#define CALYPSO_ERR_INTERNAL_ERROR      0x08

//#define CALYPSO_ERR_COMMAND_OVERFLOW    0x04
//#define CALYPSO_ERR_RESPONSE_OVERFLOW   0x05
//#define CALYPSO_ERR_INTERNAL_NULL_PTR   0x07

#define CALYPSO_ERR_NOT_BINDED          0x09

#define CALYPSO_ERR_NOT_IMPLEMENTED     0x0E

#define CALYPSO_ERR_ATR_INVALID         0x10
#define CALYPSO_ERR_STATUS_WORD         0x11
#define CALYPSO_ERR_RESPONSE_MISSING    0x12
#define CALYPSO_ERR_RESPONSE_SIZE       0x13
#define CALYPSO_ERR_SW_WRONG_P1P2       0x14
#define CALYPSO_ERR_SW_WRONG_P3         0x15
//                                      0x16
//                                      0x17
//                                      0x18
//                                      0x19
//                                      0x1A
//                                      0x1B
//                                      0x1C
//                                      0x1D
//                                      0x1E
#define CALYPSO_DATA_SIZE_IS_UNKNOWN    0x1E
#define CALYPSO_KIF_IS_UNKNOWN_FOR_KEY  0x1F

#define CALYPSO_CARD_NOT_SUPPORTED      0x20
#define CALYPSO_CARD_FCI_INVALID        0x21
#define CALYPSO_CARD_FILE_INFO_INVALID  0x22
#define CALYPSO_CARD_DATA_MALFORMED     0x23

#define CALYPSO_CARD_COUNTER_LIMIT      0x25
#define CALYPSO_CARD_FILE_NOT_FOUND     0x29
#define CALYPSO_CARD_RECORD_NOT_FOUND   0x2A
#define CALYPSO_CARD_WRONG_FILE_TYPE    0x2B
#define CALYPSO_CARD_FILE_NOT_SELECTED  0x2C
#define CALYPSO_CARD_FILE_OVERFLOW      0x2D
//                                      0x2E
//                                      0x2F

#define CALYPSO_CARD_ACCESS_FORBIDDEN   0x30
#define CALYPSO_CARD_ACCESS_DENIED      0x31
//                                      0x32
#define CALYPSO_CARD_PIN_BLOCKED        0x33
//                                      0x34
//                                      0x35
//                                      0x36
//                                      0x37
#define CALYPSO_CARD_TRANSACTION_LIMIT  0x38
//                                      0x39
//                                      0x3A
#define CALYPSO_CARD_NOT_IN_SESSION     0x3B
#define CALYPSO_CARD_IN_SESSION         0x3C
#define CALYPSO_CARD_NO_SUCH_KEY        0x3D
//                                      0x3E
#define CALYPSO_CARD_DENIED_SAM_SIGN    0x3F

//                                      0x40
//                                      0x41
//                                      0x42
//                                      0x43
//                                      0x44
//                                      0x45
//                                      0x46
//                                      0x47
//                                      0x48
#define CALYPSO_SAM_IS_LOCKED           0x49
#define CALYPSO_SAM_COUNTER_LIMIT       0x4A
#define CALYPSO_SAM_NOT_IN_SESSION      0x4B
//                                      0x4C
#define CALYPSO_SAM_NO_SUCH_KEY         0x4D
#define CALYPSO_SAM_KEY_NOT_USABLE      0x4E
#define CALYPSO_SAM_DENIED_CARD_SIGN    0x4F



#define CALYPSO_ERR_FATAL_              0x80
#define CALYPSO_ERR_CARD_               0xA0
#define CALYPSO_ERR_SAM_                0xC0
#define CALYPSO_ERR_REMOVED_            0x01
#define CALYPSO_ERR_DIALOG_             0x02


#endif
