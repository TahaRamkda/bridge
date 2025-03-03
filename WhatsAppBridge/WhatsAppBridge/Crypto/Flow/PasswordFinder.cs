using Org.BouncyCastle.OpenSsl;

namespace WhatsAppBridge.Crypto.Flow
{
    // Helper class for providing a password to the PemReader
    public class PasswordFinder : IPasswordFinder
    {
        private readonly char[] _password;

        public PasswordFinder(string password)
        {
            _password = password.ToCharArray();
        }

        public char[] GetPassword()
        {
            return _password;
        }
    }
}
