#ifndef __MIFPLUS_CARD_H__
#define __MIFPLUS_CARD_H__

/**d* MifPlusAPI/MFP_ATS_BEGIN
 *
 * NAME
 *   MFP_ATS_BEGIN
 *
 * DESCRIPTION
 *   Start of the ATS of a Mifare Plus
 *
 **/
#define MFP_ATS_BEGIN    { 0x75, 0x77, 0x80, 0x02, 0xC1, 0x05 }

/**d* MifPlusAPI/MFP_ATS_SAMPLE
 *
 * NAME
 *   MFP_ATS_SAMPLE
 *
 * DESCRIPTION
 *   ATS of Mifare Plus engineering sample cards (Historical Bytes are "MFPENG")
 *
 **/
#define MFP_ATS_SAMPLE  { 0x75, 0x77, 0x84, 0x02, 0x4D, 0x46, 0x50, 0x5F, 0x45, 0x4E, 0x47 }

/**d* MifPlusAPI/MFP_SAK_LEVEL_1
 *
 * NAME
 *   MFP_SAK_LEVEL_1
 *
 * DESCRIPTION
 *   SAK of a Mifare Plus at Level 1
 *
 **/
#define MFP_SAK_LEVEL_1                    0x18

/**d* MifPlusAPI/MFP_SAK_LEVEL_2
 *
 * NAME
 *   MFP_SAK_LEVEL_2
 *
 * DESCRIPTION
 *   SAK of a Mifare Plus at Level 2
 *
 **/
#define MFP_SAK_LEVEL_2                    0x11

/**d* MifPlusAPI/MFP_SAK_LEVEL_0_3
 *
 * NAME
 *   MFP_SAK_LEVEL_0_3
 *
 * DESCRIPTION
 *   SAK of a Mifare Plus at Level 0 or 3
 *
 **/
#define MFP_SAK_LEVEL_0_3                  0x20

/*
 * Mifare Plus commands (codes sent by the PCD)
 * --------------------------------------------
 */

#define MFP_CMD_READ                       0x30
#define MFP_CMD_READ_MACED                 0x31
#define MFP_CMD_READ_PLAIN                 0x32
#define MFP_CMD_READ_PLAIN_MACED           0x33
#define MFP_CMD_READ_UNMACED               0x34
#define MFP_CMD_READ_UNMACED_R_MACED       0x35
#define MFP_CMD_READ_PLAIN_UNMACED         0x36
#define MFP_CMD_READ_PLAIN_UNMACED_R_MACED 0x37

#define MFP_CMD_READ_SIGNATURE             0x3C

#define MFP_CMD_SELECT_VIRTUAL_CARD        0x40
#define MFP_CMD_VIRTUAL_CARD_SUPPORT       0x42
#define MFP_CMD_DESELECT_VIRTUAL_CARD      0x48
#define MFP_CMD_VIRTUAL_CARD_SUPPORT_LAST  0x4B

#define MFP_CMD_GET_VERSION                0x60

#define MFP_CMD_FIRST_AUTHENTICATE         0x70
#define MFP_CMD_AUTHENTICATE_PART_2        0x72
#define MFP_CMD_FOLLOWING_AUTHENTICATE     0x76
#define MFP_CMD_RESET_AUTHENTICATION       0x78

#define MFP_CMD_WRITE                      0xA0
#define MFP_CMD_WRITE_MACED                0xA1
#define MFP_CMD_WRITE_PLAIN                0xA2
#define MFP_CMD_WRITE_PLAIN_MACED          0xA3

#define MFP_CMD_WRITE_PERSO                0xA8
#define MFP_CMD_COMMIT_PERSO               0xAA

/*
 * Mifare Plus status (codes returned by the PICC)
 * -----------------------------------------------
 */


#define MFP_SUCCESS                        0
#define MFP_CONTINUE                       0xAF

#define MFP_ERROR                          -1000


#define MFP_ERR_SUCCESS                    0x90

/**d* MifPlusAPI/MFP_ERR_AUTHENTICATION
 *
 * NAME
 *   MFP_ERR_AUTHENTICATION
 *
 * DESCRIPTION
 *   Mifare Plus card error : authentication error (reader application doesn't have the right key,
 *                            access condition not fulfilled, ...)
 *
 * NOTES
 *   The library's actual return code is MFP_ERROR-MFP_ERR_AUTHENTICATION
 *
 **/
#define MFP_ERR_AUTHENTICATION             0x06

/**d* MifPlusAPI/MFP_ERR_COMMAND_OVERFLOW
 *
 * NAME
 *   MFP_ERR_COMMAND_OVERFLOW
 *
 * DESCRIPTION
 *   Mifare Plus card error : too many read or write commands in the session or in the transaction
 *
 * NOTES
 *   The library's actual return code is MFP_ERROR-MFP_ERR_COMMAND_OVERFLOW
 *
 **/
#define MFP_ERR_COMMAND_OVERFLOW           0x07

/**d* MifPlusAPI/MFP_ERR_INVALID_MAC
 *
 * NAME
 *   MFP_ERR_INVALID_MAC
 *
 * DESCRIPTION
 *   Mifare Plus card error : invalid MAC in command
 *
 * NOTES
 *   The library's actual return code is MFP_ERROR-MFP_ERR_INVALID_MAC
 *
 **/
#define MFP_ERR_INVALID_MAC                0x08

/**d* MifPlusAPI/MFP_ERR_INVALID_BLOCK_NUMBER
 *
 * NAME
 *   MFP_ERR_INVALID_BLOCK_NUMBER
 *
 * DESCRIPTION
 *   Mifare Plus card error : this address is not valid
 *
 * NOTES
 *   The library's actual return code is MFP_ERROR-MFP_ERR_INVALID_BLOCK_NUMBER
 *
 **/
#define MFP_ERR_INVALID_BLOCK_NUMBER       0x09

/**d* MifPlusAPI/MFP_ERR_NOT_EXISTING_BLOCK_NUMBER
 *
 * NAME
 *   MFP_ERR_NOT_EXISTING_BLOCK_NUMBER
 *
 * DESCRIPTION
 *   Mifare Plus card error : this address does not exist
 *
 * NOTES
 *   The library's actual return code is MFP_ERROR-MFP_ERR_NOT_EXISTING_BLOCK_NUMBER
 *
 **/
#define MFP_ERR_NOT_EXISTING_BLOCK_NUMBER  0x0A

/**d* MifPlusAPI/MFP_ERR_CONDITION_OF_USE
 *
 * NAME
 *   MFP_ERR_CONDITION_OF_USE
 *
 * DESCRIPTION
 *   Mifare Plus card error : this command is not available at current card state
 *
 * NOTES
 *   The library's actual return code is MFP_ERROR-MFP_ERR_CONDITION_OF_USE
 *
 **/
#define MFP_ERR_CONDITION_OF_USE           0x0B

/**d* MifPlusAPI/MFP_ERR_LENGTH_ERROR
 *
 * NAME
 *   MFP_ERR_LENGTH_ERROR
 *
 * DESCRIPTION
 *   Mifare Plus card error : invalid Length in command
 *
 * NOTES
 *   The library's actual return code is MFP_ERROR-MFP_ERR_LENGTH_ERROR
 *
 **/
#define MFP_ERR_LENGTH_ERROR               0x0C

/**d* MifPlusAPI/MFP_ERR_GENERAL_MANIPULATION_ERROR
 *
 * NAME
 *   MFP_ERR_GENERAL_MANIPULATION_ERROR
 *
 * DESCRIPTION
 *   Mifare Plus card error : failure in the operation of the PICC
 *
 * NOTES
 *   The library's actual return code is MFP_ERROR-MFP_ERR_GENERAL_MANIPULATION_ERROR
 *
 **/
#define MFP_ERR_GENERAL_MANIPULATION_ERROR 0x0F

/**d* MifPlusAPI/MFP_LIB_CALL_ERROR
 *
 * NAME
 *   MFP_LIB_CALL_ERROR
 *
 * DESCRIPTION
 *   Mifare Plus API error : invalid parameters in function call
 *
 **/
#define MFP_LIB_CALL_ERROR                 MFP_ERROR+1

/**d* MifPlusAPI/MFP_LIB_INTERNAL_ERROR
 *
 * NAME
 *   MFP_LIB_INTERNAL_ERROR
 *
 * DESCRIPTION
 *   Mifare Plus API error : internal error
 *
 **/
#define MFP_LIB_INTERNAL_ERROR             MFP_ERROR+2

/**d* MifPlusAPI/MFP_LIB_UNKNOWN_CARD_LEVEL
 *
 * NAME
 *   MFP_LIB_UNKNOWN_CARD_LEVEL
 *
 * DESCRIPTION
 *   Mifare Plus API error : unable to identify the Level of the card
 *
 **/
#define MFP_LIB_UNKNOWN_CARD_LEVEL         MFP_ERROR+3

/**d* MifPlusAPI/MFP_INVALID_CMD_CODE
 *
 * NAME
 *   MFP_INVALID_CMD_CODE
 *
 * DESCRIPTION
 *   Mifare Plus API error : opcode of command is invalid
 *
 **/
#define MFP_INVALID_CMD_CODE               MFP_ERROR+6

/**d* MifPlusAPI/MFP_TOO_MANY_BLOCKS
 *
 * NAME
 *   MFP_TOO_MANY_BLOCKS
 *
 * DESCRIPTION
 *   Mifare Plus API error : the count parameter is bigger than supported
 *
 **/
#define MFP_TOO_MANY_BLOCKS                MFP_ERROR+7

/**d* MifPlusAPI/MFP_EMPTY_CARD_ANSWER
 *
 * NAME
 *   MFP_EMPTY_CARD_ANSWER
 *
 * DESCRIPTION
 *   Mifare Plus API error : after decapsulation, the card's answer is empty
 *
 **/
#define MFP_EMPTY_CARD_ANSWER              MFP_ERROR+8

/**d* MifPlusAPI/MFP_WRONG_CARD_LENGTH
 *
 * NAME
 *   MFP_WRONG_CARD_LENGTH
 *
 * DESCRIPTION
 *   Mifare Plus API error : the card's answer has not the expected size
 *
 **/
#define MFP_WRONG_CARD_LENGTH              MFP_ERROR+9

/**d* MifPlusAPI/MFP_WRONG_CARD_MAC
 *
 * NAME
 *   MFP_WRONG_CARD_MAC
 *
 * DESCRIPTION
 *   Mifare Plus API error : the card's MAC is incorrect
 *
 **/
#define MFP_WRONG_CARD_MAC                 MFP_ERROR+10

/**d* MifPlusAPI/MFP_ERROR_CARD_AUTH
 *
 * NAME
 *   MFP_ERROR_CARD_AUTH
 *
 * DESCRIPTION
 *   Mifare Plus API error : card authentication failed (card is cheating)
 *
 **/
#define MFP_ERROR_CARD_AUTH                MFP_ERROR+11

/**d* MifPlusAPI/MFP_ERR_RESPONSE_OVERFLOW
 *
 * NAME
 *   MFP_ERR_RESPONSE_OVERFLOW
 *
 * DESCRIPTION
 *   Mifare Plus API error : the card's response does not fit in the supplied buffer
 *
 **/
#define MFP_ERR_RESPONSE_OVERFLOW          MFP_ERROR+12


#define MFP_CONSTANT_KEY_ENC               0x11
#define MFP_CONSTANT_KEY_MAC               0x22
#define MFP_CONSTANT_KEY_CRYPTO1           0x33


#endif
