using System.ComponentModel.DataAnnotations;

namespace SecureFinance.Core.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(64)]
        public string ApiKey { get; set; } = string.Empty;

        public string? EncryptedSettings { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime LastActive { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public ICollection<ApiRequest> ApiRequests { get; set; } = new List<ApiRequest>();
        public ICollection<EncryptionKey> EncryptionKeys { get; set; } = new List<EncryptionKey>();
    }
}