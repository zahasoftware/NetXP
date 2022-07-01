namespace NetXP.Cryptography
{
    public interface ISymetricCrypt
    {
        SymetricKey Generate();
        byte[] Encrypt(byte[] decryptedBytes, SymetricKey symetricKey);
        byte[] Decrypt(byte[] encryptedBytes, SymetricKey symetricKey);
    }
}
