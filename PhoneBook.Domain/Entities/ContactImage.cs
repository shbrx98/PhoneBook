using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhoneBook.Domain.Entities
{
    public class ContactImage : BaseEntity
    {
        [Required]
        public int ContactId { get; set; }

        [Required]
        public byte[] ImageData { get; set; } = Array.Empty<byte>();

        [Required]
        [MaxLength(100)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string ContentType { get; set; } = string.Empty;

        public long FileSize { get; set; }

        // Navigation Property
        [ForeignKey(nameof(ContactId))]
        public Contact Contact { get; set; } = null!;
    }
}