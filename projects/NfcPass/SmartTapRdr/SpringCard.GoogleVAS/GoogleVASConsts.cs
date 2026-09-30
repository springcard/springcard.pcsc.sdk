/**
 *
 * \ingroup GoogleVAS
 *
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCard.GoogleVas
{
    /**
	 * \brief Error codes returned by the Library
	 **/
    public enum GoogleVasError
    {
        Success,                        /*!< No error */
        Communication,                  /*!< The NFC communication channel with the mobile has been broken */
        WrongCommand,                   /*!< The mobile says that our command is erroneous */
        WrongResponseStatusWord,        /*!< The mobile's has reported an error in its status word */
        WrongResponseData,              /*!< The mobile's response is invalid */
        DataMissing,                    /*!< The mobile does not have any VAS data, or its configuration is broken */
        NoVasApplication,               /*!< The mobile does not run the VAS application */
        NoVasMerchant,                  /*!< The mobile has no available pass for this merchant */
        WaitingForIntervention,         /*!< The mobile is waiting for user intervention */
        WaitingForActivation,           /*!< The mobile is waiting for user activation */
        Unused11,
        Unused12,
        Unused13,
        MessageDecipherFailed,          /*!< A message has been received for the given merchant, but a decryption error has occured */
        WrongCommandData,               /*!< The mobile can't process our command due to invalid data */
        VersionNotSupported,            /*!< The mobile does not support our version of the VAS protocol */
        NoPaymentCard,                  /*!< The mobile does not have a payment card */
        NoVAS,                          /*!< The mobile does not support VAS */
        AuthenticationFailed,           /*!< The mobile has denied our signature */
        AuthenticationRequired,         /*!< The mobile expects that we use secure communication */
        InvalidUri,                     /*!< The mobile says our URI is invalid */
        TooManyRequests,                /*!< The mobile is giving up */
        DiscretionaryTemplateMissing    /*!< Discretionary template used for smart tap 2 is missing */
    }

    /**
	 * \brief See https://developers.google.com/pay/smart-tap/reference/ndef-records/session-records#status-byte
	 **/
    public enum GoogleVasStatus : byte
    {
        Unknown = 0x00,
        OK,
        NdefFormatError,
        UnsupportedVersion,
        InvalidSequenceNumber,
        UnknownMerchant,
        MerchantDataMissing,
        ServiceDataMissing,
        ResendRequest,
        DataNotAvailableYet
    }

    /**
	 * \brief See https://developers.google.com/pay/smart-tap/reference/ndef-records/service-object-records#service-type-byte
	 **/
    public enum GoogleVasServiceType : byte
    {
        AllServices = 0x00,
        AllServicesExceptPPSE = 0x01,
        PPSE = 0x02,
        Loyalty = 0x03,
        Offer = 0x04,
        GiftCard = 0x05,
        PrivateLabelCard = 0x06,
        EventTicket = 0x07,
        Flight = 0x08,
        Transit = 0x09,
        CloudBasedWallet = 0x10,
        MobileMarketingPlatform = 0x11,
        WalletCustomer = 0x40
    }

    /**
	 * \brief See https://developers.google.com/pay/smart-tap/reference/ndef-records/payload-formats#format-byte
	 **/
    public enum GoogleVasFormat : byte
    {
        Unspecified = 0x00,
        Binary = 0x04,
        BCD = 0x05
    }

    /**
	 * \brief See https://developers.google.com/pay/smart-tap/reference/ndef-records/service-object-records#service-issuer-byte
	 **/
    public enum GoogleVasServiceIssuer : byte
    {
        Unspecified = 0x00,
        Merchant = 0x01,
        Wallet = 0x02,
        Manufacturer = 0x03
    }
}
