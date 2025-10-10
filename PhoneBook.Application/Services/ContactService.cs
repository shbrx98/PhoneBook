using Microsoft.Extensions.Logging;
using PhoneBook.Application.DTOs;
using PhoneBook.Application.Exceptions;
using PhoneBook.Domain.Entities;
using PhoneBook.Domain.Interfaces;
using MD.PersianDateTime.Standard;

namespace PhoneBook.Application.Services
{
    public class ContactService : IContactService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ContactService> _logger;

        public ContactService(IUnitOfWork unitOfWork, ILogger<ContactService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<ContactDto>> GetAllContactsAsync()
        {
            try
            {
                var contacts = await _unitOfWork.Contacts.GetAllAsync();
                return contacts.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت لیست مخاطبین");
                throw;
            }
        }

        public async Task<ContactDto?> GetContactByIdAsync(int id)
        {
            try
            {
                var contact = await _unitOfWork.Contacts.GetContactWithImageAsync(id);
                return contact != null ? MapToDto(contact) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت مخاطب با شناسه {Id}", id);
                throw;
            }
        }

        public async Task<ContactImageDto?> GetContactImageAsync(int contactId)
        {
            try
            {
                var image = await _unitOfWork.ContactImages.GetByContactIdAsync(contactId);
                if (image == null) return null;

                return new ContactImageDto
                {
                    ImageData = image.ImageData,
                    ContentType = image.ContentType,
                    FileName = image.FileName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت تصویر مخاطب {ContactId}", contactId);
                throw;
            }
        }

        public async Task<IEnumerable<ContactDto>> SearchContactsAsync(ContactSearchDto searchDto)
        {
            try
            {
                DateTime? birthDateFrom = null;
                DateTime? birthDateTo = null;

                if (!string.IsNullOrWhiteSpace(searchDto.BirthDateFrom))
                {
                    birthDateFrom = PersianDateTime.Parse(searchDto.BirthDateFrom).ToDateTime();
                }

                if (!string.IsNullOrWhiteSpace(searchDto.BirthDateTo))
                {
                    birthDateTo = PersianDateTime.Parse(searchDto.BirthDateTo).ToDateTime();
                }

                var contacts = await _unitOfWork.Contacts.SearchContactsAsync(
                    searchDto.SearchTerm,
                    birthDateFrom,
                    birthDateTo);

                return contacts.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در جستجوی مخاطبین");
                throw;
            }
        }

        public async Task<int> CreateContactAsync(CreateContactDto dto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Check duplicate mobile number
                if (await _unitOfWork.Contacts.IsMobileNumberExistsAsync(dto.MobileNumber))
                {
                    throw new BusinessException("این شماره موبایل قبلاً ثبت شده است");
                }

                var contact = new Contact
                {
                    FullName = dto.FullName.Trim(),
                    MobileNumber = dto.MobileNumber.Trim(),
                    BirthDate = dto.BirthDate,
                    CreatedAt = DateTime.Now
                };

                await _unitOfWork.Contacts.AddAsync(contact);
                await _unitOfWork.SaveChangesAsync();

                // Handle image if provided
                if (dto.Image != null && dto.Image.Length > 0)
                {
                    await SaveContactImageAsync(contact.Id, dto.Image);
                }

                await _unitOfWork.CommitTransactionAsync();
                _logger.LogInformation("مخاطب جدید با شناسه {Id} ایجاد شد", contact.Id);

                return contact.Id;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "خطا در ایجاد مخاطب جدید");
                throw;
            }
        }

        public async Task UpdateContactAsync(UpdateContactDto dto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var contact = await _unitOfWork.Contacts.GetByIdAsync(dto.Id);
                if (contact == null)
                {
                    throw new NotFoundException($"مخاطب با شناسه {dto.Id} یافت نشد");
                }

                // Check duplicate mobile number
                if (await _unitOfWork.Contacts.IsMobileNumberExistsAsync(dto.MobileNumber, dto.Id))
                {
                    throw new BusinessException("این شماره موبایل قبلاً ثبت شده است");
                }

                contact.FullName = dto.FullName.Trim();
                contact.MobileNumber = dto.MobileNumber.Trim();
                contact.BirthDate = dto.BirthDate;
                contact.UpdatedAt = DateTime.Now;

                _unitOfWork.Contacts.Update(contact);

                // Handle image
                if (dto.RemoveImage)
                {
                    await _unitOfWork.ContactImages.DeleteByContactIdAsync(dto.Id);
                }
                else if (dto.Image != null && dto.Image.Length > 0)
                {
                    await _unitOfWork.ContactImages.DeleteByContactIdAsync(dto.Id);
                    await SaveContactImageAsync(dto.Id, dto.Image);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                
                _logger.LogInformation("مخاطب با شناسه {Id} به‌روزرسانی شد", dto.Id);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "خطا در به‌روزرسانی مخاطب {Id}", dto.Id);
                throw;
            }
        }

        public async Task DeleteContactAsync(int id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var contact = await _unitOfWork.Contacts.GetByIdAsync(id);
                if (contact == null)
                {
                    throw new NotFoundException($"مخاطب با شناسه {id} یافت نشد");
                }

                await _unitOfWork.ContactImages.DeleteByContactIdAsync(id);
                _unitOfWork.Contacts.Delete(contact);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("مخاطب با شناسه {Id} حذف شد", id);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "خطا در حذف مخاطب {Id}", id);
                throw;
            }
        }

        private async Task SaveContactImageAsync(int contactId, IFormFile imageFile)
        {
            using var memoryStream = new MemoryStream();
            await imageFile.CopyToAsync(memoryStream);

            var contactImage = new ContactImage
            {
                ContactId = contactId,
                ImageData = memoryStream.ToArray(),
                FileName = imageFile.FileName,
                ContentType = imageFile.ContentType,
                FileSize = imageFile.Length,
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.ContactImages.AddAsync(contactImage);
        }

        private ContactDto MapToDto(Contact contact)
        {
            var dto = new ContactDto
            {
                Id = contact.Id,
                FullName = contact.FullName,
                MobileNumber = contact.MobileNumber,
                BirthDate = contact.BirthDate,
                HasImage = contact.ContactImage != null
            };

            if (contact.BirthDate.HasValue)
            {
                var persianDate = new PersianDateTime(contact.BirthDate.Value);
                dto.BirthDatePersian = persianDate.ToString("yyyy/MM/dd");
            }

            return dto;
        }
        public async Task<bool> IsMobileNumberExistsAsync(string mobileNumber, int? contactId = null)
        {
            try
            {
                return await _unitOfWork.Contacts.IsMobileNumberExistsAsync(mobileNumber, contactId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در بررسی شماره موبایل {MobileNumber}", mobileNumber);
                throw;
            }
        }
    }
}
