namespace PhoneBook.Domain.Entities
{
    public class Contact
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public DateTime? BirthDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation Property
        public ContactImage? ContactImage { get; set; }
    }
}