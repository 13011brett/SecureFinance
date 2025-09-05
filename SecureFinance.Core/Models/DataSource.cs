using System.ComponentModel.DataAnnotations;

namespace SecureFinance.Core.Models
{
    public class DataSource
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string BaseUrl { get; set; } = string.Empty;

        public int RateLimit { get; set; } // requests per minute

        public bool IsActive { get; set; } = true;

        public DateTime LastHealthCheck { get; set; } = DateTime.UtcNow;

        public string? ApiKey { get; set; }

        // Navigation properties
        public ICollection<ApiRequest> ApiRequests { get; set; } = new List<ApiRequest>();
    }
}