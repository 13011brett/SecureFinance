using System.ComponentModel.DataAnnotations;

namespace SecureFinance.Core.Models
{
    public class EncryptionKey
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        [Required]
        public string EncryptedAesKey { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime ExpiresAt { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(64)]
        public string KeyIdentifier { get; set; } = string.Empty;
    }
}