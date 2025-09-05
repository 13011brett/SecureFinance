using System.ComponentModel.DataAnnotations;

namespace SecureFinance.Core.Models
{
    public class ApiRequest
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int DataSourceId { get; set; }
        public DataSource DataSource { get; set; } = null!;

        [Required]
        [StringLength(200)]
        public string Endpoint { get; set; } = string.Empty;

        public int ResponseTimeMs { get; set; }

        public bool WasCached { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public string? ErrorMessage { get; set; }

        public bool IsSuccess { get; set; } = true;
    }
}