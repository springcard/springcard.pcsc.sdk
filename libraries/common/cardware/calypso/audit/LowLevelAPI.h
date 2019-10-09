/*====================================================================================================
	\file	LowLevelAPI.h
	\brief	Calypso Portable Object Reader Interface for Example Audit Application

	Interface to communicate to a Calypso card through a contactless reader.
	This low level interface is used in the audit application example
	(AuditApplication.c)

	This file is given as an example, on an "as-is" basis, without any warranty.
	Using this file is under the responsibility of the party using it.

	Copyright Spirtech 2009. All rights reserved.
	

HISTORY:
	090903-chy	Created

=====================================================================================================
*/

#define CALYPSO_LOW_LEVEL_API

//====================================================================================================
//   CALYPSO APPLICATION STANDARD FILE SFI
//====================================================================================================

//---------------------------------
// SFI
#define kSfiFree					0x01
#define kSfiID						0x03
#define kSfiEnvironment				0x07
#define kSfiEvents					0x08
#define kSfiContract				0x09
#define kSfiCounters				0x19
#define kSfiSpecialEvent			0x1D
#define kSfiContractList			0x1E


//====================================================================================================
//   LLA CONSTANTS
//====================================================================================================

//---------------------------------
// Error codes

#define kSUCCESS					0		// no error
#define	kErrNoCard					1		// no card detected

#define	kErrNoReader				2		// no reader detected
#define kErrNotImplemented			3		// function not implemented
#define kErrNoSvGet					4		// SV Get must be done first
#define kErrWrongParameter			5		// incorrect parameter
#define kErrCommunication			6		// communication error

#define kErrOther			7

//---------------------------------
// Constants Signal State


#define kSignal_NOK						0		// 0: NOK => Red led is activated 2 s
#define kSignal_OK						1		// 1: OK  => Green led is activated 2 s
#define kSignal_OK_SHORT				2		// 2: OK_SHORT => Green led is activated 100 ms
#define kSignal_OK_BLINK				3		// 3: OK_BLINK => Green led blinks 4 seconds
#define kSignal_NOK_BLINK				4		// 4: NOK_BLINK => Red led blinks 2 seconds

//---------------------------------
// Close Mode

// 0: Close without ratification
#define kCLOSE_WITHOUT_RATIFICATION		0

// 1: Close with immediate ratification. Ratification 
//    in Close Secured Session
#define kCLOSE_IMMEDIATE_RATIFICATION	1

// 2: Close without immediate ratification.
//    Ratification in Close Secured Session, but with 
//    explicite ratification command (GetChallenge 
//    with Le inccorect)
#define kCLOSE_EXPLICIT_RATIFICATION	2

// 3: Close cancelling the modifications
#define kCLOSE_CANCEL					3



//====================================================================================================
//   LLA FUNCTIONS
//====================================================================================================

/* ----------------------------------------------------------------------------
	poDetect
		Searches a portable object and connects to it, specifying the AID.
           * Searches at least for ISO14443 Type B or
           * Searches at least for ISO14443 Type A and B
             if optional Type A audit is requested
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
CALYPSO_LOW_LEVEL_API
int   poDetect
	(
	unsigned char	  inAid[16],		// beginning of AID to select
	unsigned char	  inAidLength,		// length of AID
	unsigned char	  outAid[16],		// AID of the application selected
	unsigned char	* outAidLength,		// 0 if no app. selected, else length of outAid
	unsigned char	  outSerial[8],		// Calypso Serial number of the selected application
	unsigned char	  outInfos[7]		// Startup Infos as returned by Select App.
	);


/* ----------------------------------------------------------------------------
	poApplicationSelect
		Selects an application in the portable object by its AID.
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
CALYPSO_LOW_LEVEL_API
int   poApplicationSelect
	(
	unsigned char	inAid[16],			// beginning of AID to select
	unsigned char	inAidLength,	// length of AID
	unsigned char	outAid[16],		// AID of the application selected
	unsigned char * outAidLength,	// 0 if no app. selected, else length of outAid
	unsigned char 	outSerial[8],	// Calypso Serial number of the selected application
	unsigned char 	outInfos[7],	// Startup Infos as returned by Select App.
	unsigned char	outSW[ 2 ]		// Status Word from the PO
	);


/* ----------------------------------------------------------------------------
	poRelease
		Send ratification commands (GetChallenge with Le to 1 : 0084000001h)
        waiting 200 ms between each command  until no response is received.
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
CALYPSO_LOW_LEVEL_API
int   poRelease
	(
	);


/* ----------------------------------------------------------------------------
	poSessionOpen
		Opens a secure session with optional reading of a record.
		If the open session does not return the KIF and KVC of the key,
		the default values to use are:
		21h for key#1, 27h for key#2 and 30h for key#3
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
CALYPSO_LOW_LEVEL_API
int   poSessionOpen
	(
	unsigned char	inKey,			// key index (1, 2 or 3)
	unsigned char	inRecNum,		// record to read (or 0 not to read)
	unsigned char	inSFI,			// SFI of file to select (or 0 not to select)
	unsigned char *	outRatified,	// 0 if prev. session not ratified, 1 if ratified
	unsigned char 	outRecord[256],	// in: buffer for the data to read (or NULL if inRecNum is 0)
	unsigned char *	outRecordLength,// length of the record data read (or NULL if inRecNum is 0)
	unsigned char	outSW[ 2 ]		// Status Word from the PO
	);


/* ----------------------------------------------------------------------------
	poSessionClose
		Closes the secure session currently opened. If a ratification outside the
		session is requested, this command will be sent only if the session is
		successfully closed. The function return code is the Close Session one.
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
CALYPSO_LOW_LEVEL_API
int   poSessionClose
	(
	unsigned char	inMode,		// 0: Close without ratification, 
								// 1: Close with immediate ratification (ratification 
								//    in Close Secured Session)
								// 2: Close with ratification after Close Session,
								//    using an explicit ratification command 
								//    (GetChallenge with Le incorrect)
								// 3: Close cancelling the modifications
	unsigned char	outSW[ 2 ]	// Status Word from the PO
	);


/* ----------------------------------------------------------------------------
	poRecordAppend
		Appends a record to a cyclic file of the currently selected
		application.
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
CALYPSO_LOW_LEVEL_API
int   poRecordAppend
	(
	unsigned char	inSFI,			// SFI of file to select (or 0 not to select)
	unsigned char 	inRecord[128],	// data of the record to append
	unsigned char	inRecordLength,	// length of inRecord
	unsigned char	outSW[ 2 ]		// Status Word from the PO
	); 


/* ----------------------------------------------------------------------------
	poRecordRead
		Reads a record from a file of the currently selected application.
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
CALYPSO_LOW_LEVEL_API
int   poRecordRead
	(
	unsigned char	inRecNum,		// record number
	unsigned char	inSFI,			// SFI of file to select (or 0 not to select)
	unsigned char	outRecord[128],	// in: buffer for the record data to read
	unsigned char * ioRecordLength,	// input:  length of the data to be read
									// output: length of the data read
	unsigned char	outSW[ 2 ]		// Status Word from the PO
	);

	
/* ----------------------------------------------------------------------------
	poRecordUpdate
		Updates a record of a file of the currently selected application.
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
CALYPSO_LOW_LEVEL_API
int   poRecordUpdate
	(
	unsigned char	inRecNum,		// record number
	unsigned char	inSFI,			// SFI of file to select (or 0 not to select)
	unsigned char	inRecord[128],	// data of the record to update
	unsigned char	inRecordLength,	// length of inRecord
	unsigned char	outSW[ 2 ]		// Status Word from the PO
	);


/* ----------------------------------------------------------------------------
	poRecordWrite
		Writes into a record of a file of the currently selected application.
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
CALYPSO_LOW_LEVEL_API
int   poRecordWrite
	(
	unsigned char	inRecNum,		// record number
	unsigned char	inSFI,			// SFI of file to select (or 0 not to select)
	unsigned char	inRecord[128],	// data of the record to write
	unsigned char	inRecordLength,	// length of inRecord
	unsigned char	outSW[ 2 ]		// Status Word from the PO
	); 


/* ----------------------------------------------------------------------------
	poIncrease
		Increases one to seven counters of the currently selected application.
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
CALYPSO_LOW_LEVEL_API
int   poIncrease
	(
	unsigned char	inCounter,	// counter number
	unsigned char	inSFI,		// SFI of file to select (or 0 not to select)
	unsigned long	inValue,	// value to add to the counter (3 bytes)
	unsigned char	outSW[ 2 ]	// Status Word from the PO
	);


/* ----------------------------------------------------------------------------
	poDecrease
		Decreases one to seven counters of the currently selected application.
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
CALYPSO_LOW_LEVEL_API
int   poDecrease
	(
	unsigned char	inCounter,	// counter number
	unsigned char	inSFI,		// SFI of file to select (or 0 not to select)
	unsigned long	inValue,	// value to substract to the counter (3 bytes)
	unsigned char	outSW[ 2 ]	// Status Word from the PO
	);


/* ----------------------------------------------------------------------------
	poBinaryRead
		Reads data from a binary file of the currently selected application.
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
CALYPSO_LOW_LEVEL_API
int   poBinaryRead
	(
	unsigned char	inOffset,		// Offset of data in the file
	unsigned char	inSFI,			// SFI of file to select (or 0 not to select)
	unsigned char *	outData,		// in: buffer for the data to read
	unsigned char *	ioLength,		// input:  length of the data to be read
									// output: length of the data read
	unsigned char	outSW[ 2 ]		// Status Word from the PO
	);


/* ----------------------------------------------------------------------------
	poBinaryUpdate
		Updates data to a binary file of the currently selected application.
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
CALYPSO_LOW_LEVEL_API
int   poBinaryUpdate
	(
	unsigned char	inOffset,		// Offset of data in the file
	unsigned char	inSFI,			// SFI of file to select (or 0 not to select)
	unsigned char	inData[128],	// data to update
	unsigned char	inDataLength,	// length of inData
	unsigned char	outSW[ 2 ]		// Status Word from the PO
	);


/* ----------------------------------------------------------------------------
	poVerifyPIN
		Verify the PIN
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
CALYPSO_LOW_LEVEL_API
int   poVerifyPin
	(
	unsigned char	inMode,		// 0: Plain data, 1: Encrypted data
	unsigned char	inPIN[4],	// Plain PIN to verify
	unsigned char	outSW[ 2 ]	// Status Word from the PO
	);


/* ----------------------------------------------------------------------------
	poChangePIN
		Change the PIN
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
CALYPSO_LOW_LEVEL_API
int   poChangePin
	(
	unsigned char	inMode,		// 0: Plain data, 1: Encrypted data
	unsigned char	inPIN[4],	// plain PIN value to set
	unsigned char	outSW[ 2 ]	// Status Word from the PO
	);


/* ----------------------------------------------------------------------------
	poSVGet
		Get the Stored Value balance of the portable object
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
CALYPSO_LOW_LEVEL_API
int   poSVGet
	(
	unsigned char	inTransType,	// Transaction type. 0: reload, 1: Debit, 
									//                   2: DebitUndo
	long	*		outValue,		// Value of the Stored Value
	unsigned char	outSW[ 2 ]		// Status Word from the PO
	);


/* ----------------------------------------------------------------------------
	poSVDebit
		Debit the given value from the Stored Value of the portable object 
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
CALYPSO_LOW_LEVEL_API
int   poSVDebit
	(
	short			inValue,	// Value to debit from the Stored Value
	unsigned char	outSW[ 2 ]	// Status Word from the PO
	);


/* ----------------------------------------------------------------------------
	poSVLoad
		Load the given value into the Stored Value of the portable object
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
CALYPSO_LOW_LEVEL_API
int   poSVLoad
	(
	long			inValue,	// Value to load into the Stored Value (signed value on 3 bytes)
	unsigned char	outSW[ 2 ]	// Status Word from the PO
	);


/* ----------------------------------------------------------------------------
	poInvalidate
		Invalidate the current DF
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
CALYPSO_LOW_LEVEL_API
int   poInvalidate
	(
	unsigned char	outSW[ 2 ]	// Status Word from the PO
	);


/* ----------------------------------------------------------------------------
	poRehabilitate
		Rehabilitate the current DF
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
CALYPSO_LOW_LEVEL_API
int   poRehabilitate
	(
	unsigned char	outSW[ 2 ]	// Status Word from the PO
	);


/* ----------------------------------------------------------------------------
	poGetVersion
		Get the version of the coupler (firmware and/or hardware).
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
CALYPSO_LOW_LEVEL_API
int   poGetVersion
	(
	unsigned char	outVersion[16]	// Version of the coupler, encoded in Ascii format, the
									// ending bytes are set to 00h if unused
	);


/* ----------------------------------------------------------------------------
	poSamSendReceive
		Send an APDU to the reader SAM and receive the answer.
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
CALYPSO_LOW_LEVEL_API
int   poSamSendReceive
	(
	unsigned char *	inSendData,		// APDU to send to the reader SAM
	unsigned char	inSendLength,	// Length of the APDU to send to the reader SAM
	unsigned char *	outRecvData,	 // Answer received from the reader SAM
	unsigned char *	outRecvLength	// Length of the answer received from the reader SAM
	);


/* ----------------------------------------------------------------------------
	uiSignalState
		Signal the algorithm state to the user interface.
	Return
		0 if no OS occured
-------------------------------------------------------------------------------
*/
CALYPSO_LOW_LEVEL_API
int   poSignalState
	(
	unsigned char	inState		// state to signal to the user:
								// 0: NOK => Red led is activated 2 s
								// 1: OK  => Green led is activated 2 s
								// 2: OK_SHORT => Green led is activated 100 ms
								// 3: OK_BLINK => Green led blinks 4 seconds
	);



//////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////

void getCurrentTime(unsigned char *outDateTimeCompact // The 2 first bytes of outDateTimeCompact is the DateCompact
                    // the 2 last bytes of outDateTimeCompact is the Timecomapct
  );
