using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class Text
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public string EncryptCon { get; set; } = string.Empty;

        public bool IsEncrypted { get; set; } = false;

        public int UserId { get; set; }

        public User User { get; set; } = null!;
    }
}