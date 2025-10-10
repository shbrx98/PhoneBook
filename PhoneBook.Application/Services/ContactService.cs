using PhoneBook.Application.DTOs;
using PhoneBook.Domain.Entities;
using PhoneBook.Domain.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace PhoneBook.Application.Services
{
    public class ContactService : IContactService
    {
        private readonly IContactRepository _contactRepository;
        private readonly IContactImageRepository _imageRepository;
        private readonly IValidator<CreateContactDto> _createValidator;
        private readonly IValidator<UpdateContactDto> _updateValidator;
        private readonly ILogger<ContactService> _logger;
        private static readonly PersianCalendar _persianCalendar = new();

        public ContactService(
            IContactRepository contactRepository,
            IContactImageRepository imageRepository,
            IValidator<CreateContactDto> createValidator,
            IValidator<UpdateContactDto> updateValidator,
            ILogger<ContactService> logger)
        {
            _contactRepository = contactRepository;
            _imageRepository = imageRepository;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _logger = logger;
        }

        public async Task<IEnumerable<ContactDto>> GetAllContactsAsync()
        {
            try
            {
                var contacts = await _contactRepository.GetAllAsync();
                return contacts.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all contacts");
                return Enumerable.Empty<ContactDto>();
            }
        }

        public async Task<ContactDto?> GetContactByIdAsync(int id)
        {
            try
            {
                var contact = await _contactRepository.GetByIdWithImageAsync(id);
                return contact != null ? MapToDto(contact) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contact {ContactId}", id);
                return null;
            }
        }

        public async Task<ContactImage?> GetContactImageAsync(int contactId)
        {
            try
            {
                return await _imageRepository.GetByContactIdAsync(contactId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting image for contact {ContactId}", contactId);
                return null;
            }
        }

        public async Task<IEnumerable<ContactDto>> SearchContactsAsync(SearchContactDto search)
        {
            try
            {
                var contacts = await _contactRepository.SearchAsync(
                    search.FullName,
                    search.MobileNumber,
                    string.IsNullOrEmpty(search.BirthDateFrom) ? (DateTime?)null : DateTime.Parse(search.BirthDateFrom),
                    string.IsNullOrEmpty(search.BirthDateTo) ? (DateTime?)null : DateTime.Parse(search.BirthDateTo));
                
                return contacts.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching contacts");
                return Enumerable.Empty<ContactDto>();
            }
        }

        public async Task<ServiceResult<int>> CreateContactAsync(CreateContactDto dto)
        {
            try
            {
                var validationResult = await _createValidator.ValidateAsync(dto);
                if (!validationResult.IsValid)
                {
                    return ServiceResult<int>.Fail(validationResult.Errors.Select(e => e.ErrorMessage).ToList());
                }

                if (await _contactRepository.PhoneNumberExistsAsync(dto.MobileNumber))
                {
                    return ServiceResult<int>.Fail("شماره همراه تکراری است");
                }

                var contact = new Contact
                {
                    FullName = dto.FullName.Trim(),
                    MobileNumber = dto.MobileNumber.Trim(),
                    BirthDate = dto.BirthDate,
                    CreatedAt = DateTime.UtcNow
                };

                var createdContact = await _contactRepository.AddAsync(contact);

                if (dto.Image != null)
                {
                    await SaveImageAsync(createdContact.Id, dto.Image);
                }

                _logger.LogInformation("Contact created: {ContactId}", createdContact.Id);
                return ServiceResult<int>.Ok(createdContact.Id, "مخاطب با موفقیت ایجاد شد");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating contact");
                return ServiceResult<int>.Fail("خطا در ایجاد مخاطب");
            }
        }

        public async Task<ServiceResult> UpdateContactAsync(UpdateContactDto dto)
        {
            try
            {
                var validationResult = await _updateValidator.ValidateAsync(dto);
                if (!validationResult.IsValid)
                {
                    return ServiceResult.Fail(validationResult.Errors.Select(e => e.ErrorMessage).ToList());
                }

                var contact = await _contactRepository.GetByIdAsync(dto.Id);
                if (contact == null)
                {
                    return ServiceResult.Fail("مخاطب یافت نشد");
                }

                if (await _contactRepository.PhoneNumberExistsAsync(dto.MobileNumber, dto.Id))
                {
                    return ServiceResult.Fail("شماره همراه تکراری است");
                }

                contact.FullName = dto.FullName.Trim();
                contact.MobileNumber = dto.MobileNumber.Trim();
                contact.BirthDate = dto.BirthDate;
                contact.UpdatedAt = DateTime.UtcNow;

                await _contactRepository.UpdateAsync(contact);

                if (dto.RemoveImage)
                {
                    await _imageRepository.DeleteAsync(contact.Id);
                }
                else if (dto.Image != null)
                {
                    await SaveImageAsync(contact.Id, dto.Image);
                }

                _logger.LogInformation("Contact updated: {ContactId}", contact.Id);
                return ServiceResult.Ok("مخاطب با موفقیت ویرایش شد");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating contact {ContactId}", dto.Id);
                return ServiceResult.Fail("خطا در ویرایش مخاطب");
            }
        }

        public async Task<ServiceResult> DeleteContactAsync(int id)
        {
            try
            {
                var contact = await _contactRepository.GetByIdAsync(id);
                if (contact == null)
                {
                    return ServiceResult.Fail("مخاطب یافت نشد");
                }

                await _contactRepository.DeleteAsync(id);

                _logger.LogInformation("Contact deleted: {ContactId}", id);
                return ServiceResult.Ok("مخاطب با موفقیت حذف شد");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting contact {ContactId}", id);
                return ServiceResult.Fail("خطا در حذف مخاطب");
            }
        }

        private async Task SaveImageAsync(int contactId, IFormFile imageFile)
        {
            using var memoryStream = new MemoryStream();
            await imageFile.CopyToAsync(memoryStream);

            var existingImage = await _imageRepository.GetByContactIdAsync(contactId);

            if (existingImage != null)
            {
                existingImage.ImageData = memoryStream.ToArray();
                existingImage.ContentType = imageFile.ContentType;
                existingImage.FileName = imageFile.FileName;
                existingImage.FileSize = imageFile.Length;
                existingImage.UpdatedAt = DateTime.UtcNow;
                await _imageRepository.UpdateAsync(existingImage);
            }
            else
            {
                var newImage = new ContactImage
                {
                    ContactId = contactId,
                    ImageData = memoryStream.ToArray(),
                    ContentType = imageFile.ContentType,
                    FileName = imageFile.FileName,
                    FileSize = imageFile.Length,
                    CreatedAt = DateTime.UtcNow
                };
                await _imageRepository.AddAsync(newImage);
            }
        }

        private static ContactDto MapToDto(Contact contact)
        {
            return new ContactDto
            {
                Id = contact.Id,
                FullName = contact.FullName,
                MobileNumber = contact.MobileNumber,
                BirthDate = contact.BirthDate,
                BirthDatePersian = contact.BirthDate.HasValue
                    ? ToPersianDate(contact.BirthDate.Value)
                    : null,
                HasImage = contact.ContactImage != null
            };
        }

        private static string ToPersianDate(DateTime date)
        {
            return $"{_persianCalendar.GetYear(date)}/{_persianCalendar.GetMonth(date):D2}/{_persianCalendar.GetDayOfMonth(date):D2}";
        }

    Task<int> IContactService.CreateContactAsync(CreateContactDto dto)
    {
      throw new NotImplementedException();
    }

    Task IContactService.UpdateContactAsync(UpdateContactDto dto)
    {
      return UpdateContactAsync(dto);
    }

    Task IContactService.DeleteContactAsync(int id)
    {
      return DeleteContactAsync(id);
    }

    public Task<bool> IsMobileNumberExistsAsync(string mobileNumber, int? contactId = null)
    {
      throw new NotImplementedException();
    }
  }
}