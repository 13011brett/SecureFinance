using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SecureFinance.Core.Models;

namespace SecureFinance.Core.Services
{
    public class HybridEncryptionService : IEncryptionService
    {
        private readonly RSA _rsaProvider;
        private readonly string _publicKeyPem;
        private readonly string _privateKeyPem;
        private readonly IConfiguration _configuration;

        public HybridEncryptionService(IConfiguration configuration)
        {
            _configuration = configuration;
            _rsaProvider = RSA.Create(2048);
            
            // Handle missing or empty keys gracefully
            var publicKey = _configuration["Encryption:PublicKey"];
            var privateKey = _configuration["Encryption:PrivateKey"];
            
            if (!string.IsNullOrEmpty(privateKey) && !string.IsNullOrEmpty(publicKey))
            {
                try
                {
                    _rsaProvider.ImportFromPem(privateKey);
                    _publicKeyPem = publicKey;
                    _privateKeyPem = privateKey;
                }
                catch
                {
                    // Generate new keys if import fails
                    var keyPair = GenerateKeyPair();
                    _publicKeyPem = keyPair.publicKey;
                    _privateKeyPem = keyPair.privateKey;
                }
            }
            else
            {
                // Generate new keys if none provided
                var keyPair = GenerateKeyPair();
                _publicKeyPem = keyPair.publicKey;
                _privateKeyPem = keyPair.privateKey;
            }
        }

        public async Task<string> EncryptDataAsync(string data, int userId)
        {
            try
            {
                // Generate AES key and IV for this session
                using var aes = Aes.Create();
                aes.GenerateKey();
                aes.GenerateIV();

                // Encrypt data with AES
                var encryptedData = await EncryptWithAesAsync(data, aes.Key, aes.IV);

                // Encrypt AES key with RSA
                var encryptedAesKey = _rsaProvider.Encrypt(aes.Key, RSAEncryptionPadding.OaepSHA256);

                // Create combined payload
                var payload = new EncryptedPayload
                {
                    EncryptedData = Convert.ToBase64String(encryptedData),
                    EncryptedAesKey = Convert.ToBase64String(encryptedAesKey),
                    IV = Convert.ToBase64String(aes.IV),
                    UserId = userId,
                    Timestamp = DateTime.UtcNow,
                    KeyIdentifier = GenerateKeyIdentifier()
                };

                return Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload)));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Encryption failed: {ex.Message}", ex);
            }
        }

        public async Task<string> DecryptDataAsync(string encryptedData, int userId)
        {
            try
            {
                // Decode the payload
                var payloadJson = Encoding.UTF8.GetString(Convert.FromBase64String(encryptedData));
                var payload = JsonSerializer.Deserialize<EncryptedPayload>(payloadJson);

                if (payload == null || payload.UserId != userId)
                {
                    throw new UnauthorizedAccessException("Invalid payload or user mismatch");
                }

                // Check if payload is not too old (prevent replay attacks)
                if (DateTime.UtcNow - payload.Timestamp > TimeSpan.FromHours(24))
                {
                    throw new InvalidOperationException("Encrypted payload has expired");
                }

                // Decrypt AES key with RSA
                var encryptedAesKey = Convert.FromBase64String(payload.EncryptedAesKey);
                var aesKey = _rsaProvider.Decrypt(encryptedAesKey, RSAEncryptionPadding.OaepSHA256);

                // Decrypt data with AES
                var encryptedDataBytes = Convert.FromBase64String(payload.EncryptedData);
                var iv = Convert.FromBase64String(payload.IV);

                return await DecryptWithAesAsync(encryptedDataBytes, aesKey, iv);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Decryption failed: {ex.Message}", ex);
            }
        }

        public async Task<string> GenerateApiKeyAsync()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);

            var apiKey = Convert.ToBase64String(bytes)
                .Replace("+", "")
                .Replace("/", "")
                .Replace("=", "")
                .Substring(0, 32);

            return await Task.FromResult($"sf_{apiKey}");
        }

        public async Task<bool> ValidateApiKeyAsync(string apiKey, int userId)
        {
            // Basic validation - in production this would check against database
            return await Task.FromResult(
                !string.IsNullOrEmpty(apiKey) &&
                apiKey.StartsWith("sf_") &&
                apiKey.Length == 35
            );
        }

        public string GenerateKeyIdentifier()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[16];
            rng.GetBytes(bytes);
            return Convert.ToHexString(bytes);
        }

        private async Task<byte[]> EncryptWithAesAsync(string data, byte[] key, byte[] iv)
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            using var memoryStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write);

            var dataBytes = Encoding.UTF8.GetBytes(data);
            await cryptoStream.WriteAsync(dataBytes);
            await cryptoStream.FlushFinalBlockAsync();

            return memoryStream.ToArray();
        }

        private async Task<string> DecryptWithAesAsync(byte[] encryptedData, byte[] key, byte[] iv)
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            using var memoryStream = new MemoryStream(encryptedData);
            using var cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var reader = new StreamReader(cryptoStream);

            return await reader.ReadToEndAsync();
        }

        private (string publicKey, string privateKey) GenerateKeyPair()
        {
            using var rsa = RSA.Create(2048);
            var publicKey = rsa.ExportRSAPublicKeyPem();
            var privateKey = rsa.ExportRSAPrivateKeyPem();
            return (publicKey, privateKey);
        }

        public void Dispose()
        {
            _rsaProvider?.Dispose();
        }
    }

    // Internal payload structure
    internal class EncryptedPayload
    {
        public string EncryptedData { get; set; } = string.Empty;
        public string EncryptedAesKey { get; set; } = string.Empty;
        public string IV { get; set; } = string.Empty;
        public int UserId { get; set; }
        public DateTime Timestamp { get; set; }
        public string KeyIdentifier { get; set; } = string.Empty;
    }
}
