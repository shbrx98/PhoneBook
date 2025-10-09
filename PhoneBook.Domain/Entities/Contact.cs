using System.ComponentModel.DataAnnotations;

namespace PhoneBook.Domain.Entities
{
    public class Contact : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(11, MinimumLength = 11)]
        public string MobileNumber { get; set; } = string.Empty;

        public DateTime? BirthDate { get; set; }

        // Navigation Property
        public ContactImage? ContactImage { get; set; }
    }
}
