using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System.Security.Cryptography;
using System.Text;

namespace WhatsAppBridge.Crypto.Flow
{
    public static class EncryptionUtils
    {
        const int TAG_LENGTH = 16;

        public static (dynamic decryptedBody, byte[] aesKeyBytes, byte[] initialVectorBytes)
        DecryptRequest(string encryptedAesKey, string encryptedFlowData, string initialVector, string privatePem, string passphrase)
        {
            using (var rsa = RSA.Create())
            {
                // Load the private key from PEM
                var pemReader = new PemReader(new StringReader(privatePem), new PasswordFinder(passphrase));
                if (pemReader.ReadObject() is AsymmetricCipherKeyPair keyPair)
                {
                    // Extract the private key parameters
                    var privateKey = keyPair.Private as RsaPrivateCrtKeyParameters;
                    if (privateKey == null)
                    {
                        throw new CryptographicException("The provided PEM does not contain a valid RSA private key.");
                    }

                    // Convert Bouncy Castle RSA key parameters to .NET-compatible RSA parameters
                    var rsaParams = DotNetUtilities.ToRSAParameters(privateKey);
                    // Import into .NET RSA
                    rsa.ImportParameters(rsaParams);
                }
                else
                {
                    throw new CryptographicException("The provided PEM is not a valid encrypted PKCS#1 RSA private key.");
                }

                // Decrypt the AES key created by the client
                byte[] encryptedAesKeyBytes = Convert.FromBase64String(encryptedAesKey);
                byte[] aesKeyBytes = rsa.Decrypt(encryptedAesKeyBytes, RSAEncryptionPadding.OaepSHA256);

                // Decrypt the Flow data
                byte[] initialVectorBytes = Convert.FromBase64String(initialVector);
                byte[] flowDataBytes = Convert.FromBase64String(encryptedFlowData);
                byte[] plainTextBytes = new byte[flowDataBytes.Length - TAG_LENGTH];

                var cipher = new GcmBlockCipher(new AesEngine());
                var parameters = new AeadParameters(new KeyParameter(aesKeyBytes), TAG_LENGTH * 8, initialVectorBytes);
                cipher.Init(false, parameters);
                var offset = cipher.ProcessBytes(flowDataBytes, 0, flowDataBytes.Length, plainTextBytes, 0);
                cipher.DoFinal(plainTextBytes, offset);

                string decryptedJsonString = Encoding.UTF8.GetString(plainTextBytes);
                dynamic decryptedBody = System.Text.Json.JsonSerializer.Deserialize<dynamic>(decryptedJsonString);
                return (decryptedBody: decryptedBody, aesKeyBytes: aesKeyBytes, initialVectorBytes: initialVectorBytes);
            }
        }

        public static string EncryptResponse(dynamic response, byte[] aesKeyBytes, byte[] initialVectorBytes)
        {
            // Flip the initialization vector
            byte[] flippedIV = initialVectorBytes.Select(b => (byte)~b).ToArray();

            // Encrypt the response data
            string jsonResponse = System.Text.Json.JsonSerializer.Serialize(response);
            byte[] dataToEncrypt = Encoding.UTF8.GetBytes(jsonResponse);

            var cipher = new GcmBlockCipher(new AesEngine());
            var cipherParameters = new AeadParameters(new KeyParameter(aesKeyBytes), TAG_LENGTH * 8, flippedIV);

            // Encrypt the data
            cipher.Init(true, cipherParameters);
            byte[] encryptedDataBytes = new byte[cipher.GetOutputSize(dataToEncrypt.Length)];
            var offset = cipher.ProcessBytes(dataToEncrypt, 0, dataToEncrypt.Length, encryptedDataBytes, 0);
            cipher.DoFinal(encryptedDataBytes, offset);

            // Get the authentication tag
            byte[] authTag = new byte[TAG_LENGTH];
            Array.Copy(encryptedDataBytes, encryptedDataBytes.Length - TAG_LENGTH, authTag, 0, TAG_LENGTH);

            // Concatenate encrypted data and auth tag, then return as base64
            byte[] encryptedResponse = new byte[encryptedDataBytes.Length - TAG_LENGTH + TAG_LENGTH];
            Array.Copy(encryptedDataBytes, 0, encryptedResponse, 0, encryptedDataBytes.Length - TAG_LENGTH);
            Array.Copy(authTag, 0, encryptedResponse, encryptedDataBytes.Length - TAG_LENGTH, TAG_LENGTH);
            return Convert.ToBase64String(encryptedResponse);
        }
    }
}
