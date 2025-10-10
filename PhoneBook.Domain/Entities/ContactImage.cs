namespace PhoneBook.Domain.Entities
{
    public class ContactImage
    {
        public int Id { get; set; }
        public int ContactId { get; set; }
        public byte[] ImageData { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = "image/jpeg";
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation Property
        public Contact Contact { get; set; } = null!;
    }
}