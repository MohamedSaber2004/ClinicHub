using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Options;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace ClinicHub.Infrastructure.Services
{
    public class IdProtectorService : IIdProtectorService
    {
        private const byte PayloadVersion = 1;
        private const int NonceSize = 12;
        private const int TagSize = 16;
        private const int MaxTokenLength = 512;

        private readonly byte[] _key;

        public IdProtectorService(IOptions<IdProtectionSettings> settings)
        {
            _key = Convert.FromBase64String(settings.Value.Key);
            if (_key.Length is not 16 and not 24 and not 32)
                throw new InvalidOperationException("IdProtectionSettings.Key must decode to a 16, 24 or 32 byte AES key.");
        }

        public string Protect(Guid id, string? purpose = null)
        {
            var plaintext = id.ToByteArray();
            var nonce = RandomNumberGenerator.GetBytes(NonceSize);
            var ciphertext = new byte[plaintext.Length];
            var tag = new byte[TagSize];
            var associatedData = Encoding.UTF8.GetBytes(purpose ?? string.Empty);

            using var aes = new AesGcm(_key, TagSize);
            aes.Encrypt(nonce, plaintext, ciphertext, tag, associatedData);

            var payload = new byte[1 + NonceSize + TagSize + ciphertext.Length];
            payload[0] = PayloadVersion;
            Buffer.BlockCopy(nonce, 0, payload, 1, NonceSize);
            Buffer.BlockCopy(tag, 0, payload, 1 + NonceSize, TagSize);
            Buffer.BlockCopy(ciphertext, 0, payload, 1 + NonceSize + TagSize, ciphertext.Length);

            return Base64UrlEncode(payload);
        }

        public Guid? Unprotect(string token, string? purpose = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token) || token.Length > MaxTokenLength)
                    return null;

                var payload = Base64UrlDecode(token);
                if (payload.Length < 1 + NonceSize + TagSize + 16 || payload[0] != PayloadVersion)
                    return null;

                var nonce = payload.AsSpan(1, NonceSize).ToArray();
                var tag = payload.AsSpan(1 + NonceSize, TagSize).ToArray();
                var ciphertext = payload.AsSpan(1 + NonceSize + TagSize).ToArray();
                var plaintext = new byte[ciphertext.Length];
                var associatedData = Encoding.UTF8.GetBytes(purpose ?? string.Empty);

                using var aes = new AesGcm(_key, TagSize);
                aes.Decrypt(nonce, ciphertext, tag, plaintext, associatedData);

                return new Guid(plaintext);
            }
            catch (CryptographicException)
            {
                return null;
            }
            catch (FormatException)
            {
                return null;
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        private static string Base64UrlEncode(byte[] data)
        {
            return Convert.ToBase64String(data)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static byte[] Base64UrlDecode(string token)
        {
            var s = token.Replace('-', '+').Replace('_', '/');
            switch (s.Length % 4)
            {
                case 2:
                    s += "==";
                    break;
                case 3:
                    s += "=";
                    break;
                case 1:
                    throw new FormatException("Invalid base64url string.");
            }

            return Convert.FromBase64String(s);
        }
    }
}
