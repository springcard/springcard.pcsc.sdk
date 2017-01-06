//==============================================================;
//
//  Disable Smart card Plug and Play for specific cards
//
//  Abstract:
//      This is an example of how to create a new
//      Smart Card Database entry when a smart card is inserted
//      into the computer.
//
//  This source code is only intended as a supplement to existing Microsoft
//  documentation.
//
//  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED. THIS INCLUDES BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
//  PURPOSE.
//
//  Copyright (C) Microsoft Corporation.  All Rights Reserved.
//
//==============================================================;

// This code must be compiled with UNICODE support to work correctly
#ifndef UNICODE
#define UNICODE
#endif

#include <windows.h>
#include <winscard.h>
#include <stdio.h>
#include <strsafe.h>
#include <rpc.h>

// Change this prefix to specify what the beginning of the
// introduced card name in the registry will be. This is
// be prepended to a GUID value.
#define CARD_NAME_PREFIX    L"MyCustomCard"

// This is the name that will be provided as the CSP for 
// the card when introduced to the system. This is provided
// in order to disable Smart Card Plug and Play for this
// card.
#define CARD_CSP            L"$DisableSCPnP$"

// This special reader name is used to be notified when
// a reader is added to or removed from the system through
// SCardGetStatusChange.
#define PNP_READER_NAME     L"\\\\?PnP?\\Notification"

// Maximum ATR length plus alignment bytes. This value is
// used in the SCARD_READERSTATE structure
#define MAX_ATR_LEN         36

LONG GenerateCardName(
    __deref_out LPWSTR  *ppwszCardName)
{
    LONG        lReturn = NO_ERROR;
    HRESULT     hr = S_OK;
    DWORD       cchFinalString = 0;
    WCHAR       wszCardNamePrefix[] = CARD_NAME_PREFIX;
    LPWSTR      pwszFinalString = NULL;
    UUID        uuidCardGuid = {0};
    RPC_WSTR    pwszCardGuid = NULL;
    RPC_STATUS  rpcStatus = RPC_S_OK;

    // Parameter check
    if (NULL == ppwszCardName)
    {
        wprintf(L"Invalid parameter in GenerateCardName.\n");
        return ERROR_INVALID_PARAMETER;
    }

    // Generate GUID
    rpcStatus = UuidCreate(&uuidCardGuid);
    if (RPC_S_OK != rpcStatus)
    {
        wprintf(L"Failed to create new GUID with error 0x%x.\n");
        lReturn = (DWORD)rpcStatus;
    }
    else
    {
        // Convert GUID to string
        rpcStatus = UuidToString(&uuidCardGuid, &pwszCardGuid);
        if (RPC_S_OK != rpcStatus)
        {
            wprintf(L"Failed to convert new GUID to string with error 0x%x.\n", rpcStatus);
            lReturn = (DWORD)rpcStatus;
        }
        else
        {
            // Allocate memory for final string
            // Template is <prefix>-<guid>
            cchFinalString = (DWORD)(wcslen(wszCardNamePrefix) + 1 + wcslen((LPWSTR)pwszCardGuid) + 1);
            pwszFinalString = (LPWSTR)HeapAlloc(GetProcessHeap(), 0, cchFinalString * sizeof(WCHAR));
            if (NULL == pwszFinalString)
            {
                wprintf(L"Out of memory.\n");
                lReturn = ERROR_OUTOFMEMORY;
            }
            else
            {
                // Create final string
                hr = StringCchPrintf(
                            pwszFinalString,
                            cchFinalString,
                            L"%s-%s",
                            wszCardNamePrefix,
                            pwszCardGuid);
                if (FAILED(hr))
                {
                    wprintf(L"Failed to create card name with error 0x%x.\n", hr);
                    lReturn = (DWORD)hr;
                }
                else
                {
                    // Set output params
                    *ppwszCardName = pwszFinalString;
                    pwszFinalString = NULL;
                }
            }
        }
    }

    if (NULL != pwszCardGuid)
    {
        RpcStringFree(&pwszCardGuid);
    }

    if (NULL != pwszFinalString)
    {
        HeapFree(GetProcessHeap(), 0, pwszFinalString);
    }

    return lReturn;
}

LONG IntroduceCardATR(
    __in SCARDCONTEXT   hSC,
    __in LPBYTE         pbAtr,
    __in DWORD          cbAtr)
{
    LONG    lReturn = NO_ERROR;
    LPWSTR  pwszCardName = NULL;
    
    // Parameter checks
    if (NULL == hSC || NULL == pbAtr || 0 == cbAtr)
    {
        wprintf(L"Invalid parameter in IntroduceCardATR.\n");
        return ERROR_INVALID_PARAMETER;
    }

    // Generate a name for the card
    lReturn = GenerateCardName(&pwszCardName);
    if (NO_ERROR != lReturn)
    {
        wprintf(L"Failed to generate card name with error 0x%x.\n", lReturn);
    }
    else
    {
        // Introduce the card to the system
        lReturn = SCardIntroduceCardType(
                                hSC,
                                pwszCardName,
                                NULL,
                                NULL,
                                0,
                                pbAtr,
                                NULL,
                                cbAtr);
        if (SCARD_S_SUCCESS != lReturn)
        {
            wprintf(L"Failed to introduce card '%s' to system with error 0x%x.\n", pwszCardName, lReturn);
        }
        else
        {
            // Set the provider name
            lReturn = SCardSetCardTypeProviderName(
                                        hSC,
                                        pwszCardName,
                                        SCARD_PROVIDER_CSP,
                                        CARD_CSP);
            if (SCARD_S_SUCCESS != lReturn)
            {
                wprintf(L"Failed to set CSP for card '%s' with error 0x%x.\n", pwszCardName, lReturn);
            }
            else
            {
                wprintf(L"Card '%s' has been successfully introduced to the system and has had Plug and Play disabled.\n", pwszCardName);
            }
        }
    }

    if (NULL != pwszCardName)
    {
        HeapFree(GetProcessHeap(), 0, pwszCardName);
    }

    return lReturn;
}

LONG ProcessCard(
    __in SCARDCONTEXT           hSC,
    __in LPSCARD_READERSTATE    pRdr)
{
    LONG        lReturn = NO_ERROR;
    DWORD       dwActiveProtocol = 0;
    DWORD       cbAtr = MAX_ATR_LEN;
    DWORD       dwIndex = 0;
    DWORD       cchCards = SCARD_AUTOALLOCATE;
    LPWSTR      pmszCards = NULL;
    BYTE        rgbAtr[MAX_ATR_LEN] = {0};
    SCARDHANDLE hSCard = NULL;

    // Parameter checks
    if (NULL == hSC || NULL == pRdr)
    {
        wprintf(L"Invalid parameter in ProcessCard.\n");
        return ERROR_INVALID_PARAMETER;
    }

    // Connect to the card in the provided reader in shared mode
    lReturn = SCardConnect(
                    hSC,
                    pRdr->szReader,
                    SCARD_SHARE_SHARED,
                    SCARD_PROTOCOL_T0 | SCARD_PROTOCOL_T1,
                    &hSCard,
                    &dwActiveProtocol);
    if (SCARD_S_SUCCESS != lReturn)
    {
        wprintf(L"Failed to connect to card in reader '%s' with error 0x%x.\n", pRdr->szReader, lReturn);
    }
    else
    {
        wprintf(L"Connected to card in reader '%s'.\n", pRdr->szReader);

        /*
         * In this spot, put any necessary calls needed to identify that this
         * is the type of card you are looking for. Usually this is done via
         * SCardTransmit calls. For this example, we will grab the ATR of every
         * inserted card.
         */

        // Obtain the ATR of the inserted card
        lReturn = SCardGetAttrib(
                            hSCard,
                            SCARD_ATTR_ATR_STRING,
                            rgbAtr,
                            &cbAtr);
        if (SCARD_S_SUCCESS != lReturn)
        {
            wprintf(L"Failed to obtain ATR of card in reader '%s' with error 0x%x.\n", pRdr->szReader, lReturn);
        }
        else
        {
            // Output the ATR
            wprintf(L"ATR of card in reader '%s':", pRdr->szReader);
            for (dwIndex = 0; dwIndex < cbAtr; dwIndex++)
            {
                wprintf(L" %02x", rgbAtr[dwIndex]);
            }
            wprintf(L"\n");

            // Determine if the ATR is already in the Smart Card Database
            lReturn = SCardListCards(
                                hSC,
                                rgbAtr,
                                NULL,
                                0,
                                (LPWSTR)&pmszCards,
                                &cchCards);
            if (SCARD_S_SUCCESS != lReturn)
            {
                wprintf(L"Failed to determine if card in reader '%s' is currently recognized by the system with error 0x%x. Skipping.\n", pRdr->szReader, lReturn);
            }
            else if (NULL == pmszCards || 0 == *pmszCards)
            {
                // Card not found. We need to add it.
                wprintf(L"Card in reader '%s' is not currently recognized by the system. Adding ATR.\n", pRdr->szReader);
                lReturn = IntroduceCardATR(
                                    hSC, 
                                    rgbAtr, 
                                    cbAtr);

                // If an error occurs here, we will continue so we can try the next time
                // the card is inserted as well as examine other readers.
            }
            else
            {
                wprintf(L"Card in reader '%s' is already known by the system. Not adding ATR.\n", pRdr->szReader);
            }
        }
    }

    // Disconnect from the card. We do not need to reset it.
    if (NULL != hSCard)
    {
        SCardDisconnect(hSCard, SCARD_LEAVE_CARD);
    }

    // Free resources
    if (NULL != pmszCards)
    {
        SCardFreeMemory(hSC, pmszCards);
    }

    return lReturn;
}


LONG MonitorReaders(
        __in SCARDCONTEXT hSC)
{
    LPWSTR              pwszReaders = NULL;
    LPWSTR              pwszOldReaders = NULL;
    LPWSTR              pwszRdr = NULL;
    DWORD               dwRet = ERROR_SUCCESS;
    DWORD               cchReaders = SCARD_AUTOALLOCATE;
    DWORD               dwRdrCount = 0;
    DWORD               dwOldRdrCount = 0;
    DWORD               dwIndex = 0;
    LONG                lReturn = NO_ERROR;
    BOOL                fDone = FALSE;
    SCARD_READERSTATE   rgscState[MAXIMUM_SMARTCARD_READERS+1] = {0};
    SCARD_READERSTATE   rgscOldState[MAXIMUM_SMARTCARD_READERS+1] = {0};
    LPSCARD_READERSTATE pRdr = NULL;

    // Parameter check
    if (NULL == hSC)
    {
        wprintf(L"Invalid parameter in MonitorReaders.\n");
        return ERROR_INVALID_PARAMETER;
    }

    // One of the entries for monitoring will be to detect new readers
    // The first time through the loop will be to detect whether
    // the system has any readers.
    rgscState[0].szReader = PNP_READER_NAME;
    rgscState[0].dwCurrentState = SCARD_STATE_UNAWARE;
    dwRdrCount = 1;

    while (!fDone)
    {
        while (!fDone)
        {
            // Wait for status changes to occur
            wprintf(L"Monitoring for changes.\n");
            lReturn = SCardGetStatusChange(
                                    hSC,
                                    INFINITE,
                                    rgscState,
                                    dwRdrCount);
            switch (lReturn)
            {
                case SCARD_S_SUCCESS:
                    // Success
                    break;
                case SCARD_E_CANCELLED:
                    // Monitoring is being cancelled
                    wprintf(L"Monitoring cancelled. Exiting.\n");
                    fDone = TRUE;
                    break;
                default:
                    // Error occurred
                    wprintf(L"Error 0x%x occurred while monitoring reader states.\n", lReturn);
                    fDone = TRUE;
                    break;
            }

            if (!fDone)
            {
                // Examine the status change for each reader, skipping the PnP notification reader
                for (dwIndex = 1; dwIndex < dwRdrCount; dwIndex++)
                {
                    pRdr = &rgscState[dwIndex];

                    // Determine if a card is now present in the reader and
                    // it can be communicated with.
                    if ((pRdr->dwCurrentState & SCARD_STATE_EMPTY ||
                         SCARD_STATE_UNAWARE == pRdr->dwCurrentState) &&
                        pRdr->dwEventState & SCARD_STATE_PRESENT &&
                        !(pRdr->dwEventState & SCARD_STATE_MUTE))
                    {
                        // A card has been inserted and is available.
                        // Grab its ATR for addition to the database.
                        wprintf(L"A card has been inserted into reader '%s'. Grabbing its ATR.\n", pRdr->szReader);
                        lReturn = ProcessCard(hSC, pRdr);

                        // If an error occurs here, we will continue so we can try the next time
                        // the card is inserted as well as examine other readers.
                    }

                    // Save off the new state of the reader
                    pRdr->dwCurrentState = pRdr->dwEventState;
                }

                // Now see if the number of readers in the system has changed.
                // Save its new state as the current state for the next loop.
                pRdr = &rgscState[0];
                pRdr->dwCurrentState = pRdr->dwEventState;
                if (pRdr->dwEventState & SCARD_STATE_CHANGED)
                {
                    wprintf(L"Reader change detected.\n");
                    break;
                }
            }   
        }

        if (!fDone)
        {
            // Clean up previous loop
            if (NULL != pwszOldReaders)
            {
                SCardFreeMemory(hSC, pwszOldReaders);
                pwszOldReaders = NULL;
            }
            pwszReaders = NULL;
            cchReaders = SCARD_AUTOALLOCATE;
            
            // Save off PnP notification reader state and and list of readers previously found in the system
            memcpy_s(&rgscOldState[0], sizeof(SCARD_READERSTATE), &rgscState[0], sizeof(SCARD_READERSTATE));
            memset(rgscState, 0, sizeof(rgscState));
            dwOldRdrCount = dwRdrCount;
            pwszOldReaders = pwszReaders;
            
            // Obtain a list of all readers in the system
            wprintf(L"Building reader list.\n");
            lReturn = SCardListReaders(
                                hSC,
                                NULL,
                                (LPWSTR)&pwszReaders,
                                &cchReaders);
            switch (lReturn)
            {
                case SCARD_S_SUCCESS:
                    // Success
                    break;
                case SCARD_E_NO_READERS_AVAILABLE:
                    // No readers in the system. This is OK.
                    lReturn = SCARD_S_SUCCESS;
                    break;
                default:
                    // Error occurred
                    wprintf(L"Failed to obtain list of readers with error 0x%x.\n", lReturn);
                    fDone = TRUE;
                    break;
            }

            // Build the reader list for monitoring - NULL indicates end-of-list
            // First entry is the PnP Notification entry.
            pRdr = rgscState;
            memcpy_s(&rgscState[0], sizeof(SCARD_READERSTATE), &rgscOldState[0], sizeof(SCARD_READERSTATE));
            pRdr++;
            pwszRdr = pwszReaders;
            while ((NULL != pwszRdr) && (0 != *pwszRdr))
            {
                BOOL fFound = FALSE;
                dwRdrCount++;

                // Look for an existing reader state from a previous loop
                for (dwIndex = 1; dwIndex < dwOldRdrCount; dwIndex++)
                {
                    if ((lstrlen(pwszRdr) == lstrlen(rgscOldState[dwIndex].szReader)) &&
                        (0 == lstrcmpi(pwszRdr, rgscOldState[dwIndex].szReader)))
                    {
                        // Found a match. Copy it.
                        memcpy_s(pRdr, sizeof(SCARD_READERSTATE), &rgscOldState[dwIndex], sizeof(SCARD_READERSTATE));
                        fFound = TRUE;
                        break;
                    }
                }

                if (!fFound)
                {
                    // New reader
                    pRdr->szReader = pwszRdr;
                    pRdr->dwCurrentState = SCARD_STATE_UNAWARE;
                }

                // Increment reader indices
                pRdr++;
                pwszRdr += lstrlen(pwszRdr)+1;
            }
        }
    }

    // Clean up resources
    if (NULL != pwszReaders)
    {
        SCardFreeMemory(hSC, pwszReaders);
    }

    if (NULL != pwszOldReaders)
    {
        SCardFreeMemory(hSC, pwszOldReaders);
    }

    return lReturn;
}

LONG __cdecl main(
        VOID)
{
    DWORD               dwRet = ERROR_SUCCESS;
    SCARDCONTEXT        hSC = NULL;
    LONG                lReturn = NO_ERROR;
    HANDLE              hStartedEvent = NULL;

    // Get handle to event that will be signaled when the Smart Card Service is available
    hStartedEvent = SCardAccessStartedEvent();

    // Wait for the Smart Card Service to become available
    dwRet = WaitForSingleObject(hStartedEvent, INFINITE);
    if (WAIT_OBJECT_0 != dwRet)
    {
        wprintf(L"Wait for Smart Card Service failed with error 0x%x.\n", dwRet);
        lReturn = dwRet;
    }
    else
    {
        // Establish a system-level context with the Smart Card Service
        lReturn = SCardEstablishContext(
                                SCARD_SCOPE_SYSTEM,
                                NULL,
                                NULL,
                                &hSC);
        if (SCARD_S_SUCCESS != lReturn)
        {
            wprintf(L"Failed to establish context with the Smart Card Service with error 0x%x.\n", lReturn);
        }
        else
        {
            // Begin monitoring the readers in the system
            // This routine could be done in a separate thread so it can be cancelled via SCardCancel().
            lReturn = MonitorReaders(hSC);
        }
    }   

    // Cleanup resources
    if (NULL != hSC)
    {
        SCardReleaseContext(hSC);
    }

    if (NULL != hStartedEvent)
    {
        SCardReleaseStartedEvent();
    }

    wprintf(L"Done.\n");

    return lReturn;
}