using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class RequestHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public string Endpoint { get; set; } = string.Empty;

        [Required]
        public string Method { get; set; } = string.Empty;

        public string RequestData { get; set; } = string.Empty;

        public string ResponseData { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
    }
}