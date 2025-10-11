using Microsoft.Extensions.Logging;
using PhoneBook.Application.DTOs;
using PhoneBook.Application.Exceptions;
using PhoneBook.Domain.Entities;
using PhoneBook.Domain.Interfaces;
using MD.PersianDateTime;

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
                var contact = await _unitOfWork.Contacts.GetByIdWithImageAsync(id);
                return contact != null ? MapToDto(contact) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت مخاطب با شناسه {Id}", id);
                throw;
            }
        }

        public async Task<ContactImage?> GetContactImageAsync(int contactId)
        {
            try
            {
                var image = await _unitOfWork.ContactImages.GetByContactIdAsync(contactId);
                if (image == null) return null;

                return new ContactImage
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

        public async Task<IEnumerable<ContactDto>> SearchContactsAsync(SearchContactDto searchDto)
        {
            try
            {
                DateTime? birthDateFrom = null;
                DateTime? birthDateTo = null;

                if (!string.IsNullOrWhiteSpace(searchDto.BirthDateFrom))
                {
                    try
                    {
                        birthDateFrom = PersianDateTime.Parse(searchDto.BirthDateFrom).ToDateTime();
                    }
                    catch
                    {
                        _logger.LogWarning("تاریخ از نامعتبر: {Date}", searchDto.BirthDateFrom);
                    }
                }

                if (!string.IsNullOrWhiteSpace(searchDto.BirthDateTo))
                {
                    try
                    {
                        birthDateTo = PersianDateTime.Parse(searchDto.BirthDateTo).ToDateTime();
                    }
                    catch
                    {
                        _logger.LogWarning("تاریخ تا نامعتبر: {Date}", searchDto.BirthDateTo);
                    }
                }

                var contacts = await _unitOfWork.Contacts.SearchAsync(
                    searchDto.SearchTerm,
                    searchDto.FullName,
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
                if (await _unitOfWork.Contacts.PhoneNumberExistsAsync(dto.MobileNumber))
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
                    await _unitOfWork.SaveChangesAsync();
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
                _logger.LogInformation("=== شروع Update مخاطب {Id} ===", dto.Id);
                
                await _unitOfWork.BeginTransactionAsync();
                
                // 1. Get contact
                _logger.LogInformation("دریافت مخاطب {Id}", dto.Id);
                var contact = await _unitOfWork.Contacts.GetByIdAsync(dto.Id);
                if (contact == null)
                {
                    throw new NotFoundException($"مخاطب با شناسه {dto.Id} یافت نشد");
                }
                
                _logger.LogInformation("مخاطب فعلی: Name={Name}, Mobile={Mobile}", 
                    contact.FullName, contact.MobileNumber);
                
                // 2. Check duplicate mobile
                if (!string.IsNullOrWhiteSpace(dto.MobileNumber) &&
                    await _unitOfWork.Contacts.PhoneNumberExistsAsync(dto.MobileNumber, dto.Id))
                {
                    throw new BusinessException("این شماره موبایل قبلاً ثبت شده است");
                }
                
                // 3. Update fields - بدون if!
                _logger.LogInformation("به‌روزرسانی فیلدها: Name={Name}, Mobile={Mobile}", 
                    dto.FullName, dto.MobileNumber);
                
                contact.FullName = dto.FullName.Trim();
                contact.MobileNumber = dto.MobileNumber.Trim();
                contact.BirthDate = dto.BirthDate;
                contact.UpdatedAt = DateTime.Now;
                
                
                _unitOfWork.Contacts.Update(contact);
                
                _logger.LogInformation("صدا زدن SaveChanges...");
                var savedCount = await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("تعداد Entity های ذخیره شده: {Count}", savedCount);
                
                // 4. Handle image
                var existingImage = await _unitOfWork.ContactImages.GetByContactIdAsync(dto.Id);
                
                if (dto.RemoveImage && existingImage != null)
                {
                    _logger.LogInformation("حذف تصویر");
                    await _unitOfWork.ContactImages.DeleteAsync(existingImage.ContactId);
                    await _unitOfWork.SaveChangesAsync();
                }
                else if (dto.Image != null && dto.Image.Length > 0)
                {
                    if (existingImage != null)
                    {
                        _logger.LogInformation("حذف تصویر قبلی");
                        await _unitOfWork.ContactImages.DeleteAsync(existingImage.ContactId);
                        await _unitOfWork.SaveChangesAsync();
                    }
                    
                    _logger.LogInformation("ذخیره تصویر جدید");
                    await SaveContactImageAsync(dto.Id, dto.Image);
                    await _unitOfWork.SaveChangesAsync();
                }
                
                await _unitOfWork.CommitTransactionAsync();
                _logger.LogInformation("=== Update موفق - مخاطب {Id} ===", dto.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در Update: {Message}", ex.Message);
                await _unitOfWork.RollbackTransactionAsync();
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

               
                var image = await _unitOfWork.ContactImages.GetByContactIdAsync(id);
                if (image != null)
                {
                    await _unitOfWork.ContactImages.DeleteAsync(image.ContactId);
                }

               
                await _unitOfWork.Contacts.DeleteAsync(contact.Id);
                
                
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

        public async Task<bool> IsMobileNumberExistsAsync(string mobileNumber, int? contactId = null)
        {
            try
            {
                return await _unitOfWork.Contacts.PhoneNumberExistsAsync(mobileNumber, contactId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در بررسی وجود شماره موبایل {MobileNumber}", mobileNumber);
                throw;
            }
        }

        private async Task SaveContactImageAsync(int contactId, Microsoft.AspNetCore.Http.IFormFile imageFile)
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
                try
                {
                    var persianDate = new PersianDateTime(contact.BirthDate.Value);
                    dto.BirthDatePersian = persianDate.ToString("yyyy/MM/dd");
                }
                catch
                {
                    dto.BirthDatePersian = contact.BirthDate.Value.ToString("yyyy/MM/dd");
                }
            }

            return dto;
        }
    }
}