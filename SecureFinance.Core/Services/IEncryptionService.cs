namespace SecureFinance.Core.Services
{
    public interface IEncryptionService
    {
        Task<string> EncryptDataAsync(string data, int userId);
        Task<string> DecryptDataAsync(string encryptedData, int userId);
        Task<string> GenerateApiKeyAsync();
        Task<bool> ValidateApiKeyAsync(string apiKey, int userId);
        string GenerateKeyIdentifier();
    }
}