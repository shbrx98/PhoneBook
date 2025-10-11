using Microsoft.AspNetCore.Http;

namespace PhoneBook.Application.DTOs
{
    public class ContactImageDto
    {
        public int Id { get; set; }
        public int ContactId { get; set; }
        public byte[] ImageData { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
    }
}
