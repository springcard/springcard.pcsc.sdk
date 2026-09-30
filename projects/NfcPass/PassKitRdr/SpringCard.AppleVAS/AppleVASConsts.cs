/**
 *
 * \ingroup AppleVAS
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCard.AppleVas
{
    /**
     * \brief Error codes returned by the Library
     */
    public enum AppleVasError
    {
        Success,                        /*!< No error */
        Communication,                  /*!< The NFC communication channel with the mobile has been broken */
        LicenseMissing,                 /*!< The library requires a License */
        LicenseNotAllowed,              /*!< The conditions to accept the library's License are not met */
        LicenseNotActive,               /*!< The library's License is no more valid */
        WrongCommand,                   /*!< The mobile says that our command is erroneous */
        WrongResponseStatusWord,        /*!< The mobile's has reported an error in its status word */
        WrongResponseData,              /*!< The mobile's response is invalid */
        DataMissing,                    /*!< The mobile does not have any VAS data, or its configuration is broken */
        NoVasApplication,               /*!< The mobile does not run the VAS application */
        VasVersionConflict,             /*!< The mobile runs an unsupported version of the VAS application */
        VasDisabled,                    /*!< The mobile says we should skip the VAS protocol */
        NoVasMerchant,                  /*!< The mobile has no available pass for this merchant */
        NoVasMessage,                   /*!< The mobile's response does not contain a VAS message for our merchant */
        WaitingForIntervention,         /*!< The mobile is waiting for user intervention */
        WaitingForActivation,           /*!< The mobile is waiting for user activation */
        UnknownMerchantPublicKeyId,     /*!< The mobile announced a merchant public key id that the terminal does not know */
        NoMobilePublicKey,              /*!< The mobile did not provide its public key */
        KeyDerivationFailed,            /*!< The terminal has failed to compute the shared key */
        MessageDecipherFailed,          /*!< A message has been received for the given merchant, but a decryption error has occured */
        WrongParameter                  /*!< The user passed an erroneous parameter to the library */
    }
}
