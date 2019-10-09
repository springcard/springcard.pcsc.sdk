namespace SpringCard.PCSC.CardHelpers
{
    public partial class SamAV
    {
        public const byte CLA = 0x80;

        public enum INS : byte
        {
            PKIGenerateKeyPair = 0x15,
            PKIGenerateSignature = 0x16,
            PKIGenerateHash = 0x17,
            PKIExportPublicKey = 0x18,
            PKIImportKey = 0x19,
            PKISendSignature = 0x1A,
            PKIVerifySignature = 0x1B,
            PKIUpdateKeyEntry = 0x1D,
            PKIExportPrivateKey = 0x1F,
            GetKUCEntry = 0x6C,
            ChangeKeyEntry = 0xC1,
            ChangeKUCEntry = 0xCC,
            DumpSessionKey = 0xD5,
            DumpSecretKey = 0xD6
        }
    }
}
