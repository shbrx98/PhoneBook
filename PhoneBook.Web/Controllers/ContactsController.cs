using Microsoft.AspNetCore.Mvc;
using PhoneBook.Application.DTOs;
using PhoneBook.Application.Services;
using PhoneBook.Web.Models.ViewModels;
using PhoneBook.Web.Models.Constants;
using PhoneBook.Web.Models.Helpers;
using FluentValidation;

namespace PhoneBook.Web.Controllers
{
    public class ContactsController : Controller
    {
        private readonly IContactService _contactService;
        private readonly IValidator<CreateContactDto> _createValidator;
        private readonly IValidator<UpdateContactDto> _updateValidator;
        private readonly ILogger<ContactsController> _logger;

        public ContactsController(
            IContactService contactService,
            IValidator<CreateContactDto> createValidator,
            IValidator<UpdateContactDto> updateValidator,
            ILogger<ContactsController> logger)
        {
            _contactService = contactService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _logger = logger;
        }

        // GET: Contacts
        public async Task<IActionResult> Index(SearchContactDto searchDto)
        {
            try
            {
                IEnumerable<ContactDto> contacts;
                bool hasSearch = false;

                // ⭐ تصحیح: SearchTerm به جای FullName
                if (!string.IsNullOrWhiteSpace(searchDto.SearchTerm) ||
                    !string.IsNullOrWhiteSpace(searchDto.BirthDateFrom) ||
                    !string.IsNullOrWhiteSpace(searchDto.BirthDateTo))
                {
                    contacts = await _contactService.SearchContactsAsync(searchDto);
                    hasSearch = true;
                }
                else
                {
                    contacts = await _contactService.GetAllContactsAsync();
                }

                var viewModel = new ContactListViewModel
                {
                    Contacts = contacts,
                    SearchDto = searchDto,
                    TotalCount = contacts.Count(),
                    HasSearch = hasSearch
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در نمایش لیست مخاطبین");
                TempData["Error"] = "خطا در دریافت لیست مخاطبین";
                return View(new ContactListViewModel());
            }
        }

        // GET: Contacts/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var contact = await _contactService.GetContactByIdAsync(id);
                if (contact == null)
                {
                    TempData["Error"] = ErrorMessages.ContactNotFound;
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new ContactDetailsViewModel
                {
                    Contact = contact,
                    CanEdit = true,
                    CanDelete = true
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در نمایش جزئیات مخاطب {Id}", id);
                TempData["Error"] = ErrorMessages.UnexpectedError;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Contacts/Create
        public IActionResult Create()
        {
            return View(new CreateContactDto());
        }

        // POST: Contacts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateContactDto dto)
        {
            try
            {
                // Debug: چاپ داده‌های دریافتی
                _logger.LogInformation("Creating contact: Name={Name}, Mobile={Mobile}", 
                    dto.FullName, dto.MobileNumber);

                // Validation
                var validationResult = await _createValidator.ValidateAsync(dto);
                if (!validationResult.IsValid)
                {
                    foreach (var error in validationResult.Errors)
                    {
                        ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                        _logger.LogWarning("Validation error: {Field} = {Message}", 
                            error.PropertyName, error.ErrorMessage);
                    }
                    return View(dto);
                }

                // Create contact
                await _contactService.CreateContactAsync(dto);
                TempData["Success"] = "مخاطب با موفقیت ایجاد شد";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ایجاد مخاطب جدید");
                ModelState.AddModelError("", ex.Message);
                return View(dto);
            }
        }
       
        // GET: Contacts/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var contact = await _contactService.GetContactByIdAsync(id);
                if (contact == null)
                {
                    TempData["Error"] = "مخاطب یافت نشد";
                    return RedirectToAction(nameof(Index));
                }

            var hasImage = contact.HasImage;
            
            var viewModel = new ContactFormViewModel
            {
                UpdateDto = new UpdateContactDto
                {
                    Id = contact.Id,
                    FullName = contact.FullName,
                    MobileNumber = contact.MobileNumber,
                    BirthDate = contact.BirthDate
                }
            };

            ViewBag.HasImage = hasImage;
                return View(viewModel.UpdateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در نمایش فرم ویرایش");
                TempData["Error"] = "خطا در دریافت اطلاعات";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Contacts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateContactDto dto)
        {
            try
            {
                var validationResult = await _updateValidator.ValidateAsync(dto);
                if (!validationResult.IsValid)
                {
                    foreach (var error in validationResult.Errors)
                    {
                        ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                    }

                    var viewModel = await BuildEditViewModelAsync(dto);
                    return View(viewModel);
                }

                if (dto.Image != null)
                {
                    var (isValid, errorMessage, _) = await ImageHelper.ValidateAndProcessImageAsync(dto.Image);
                    if (!isValid)
                    {
                        ModelState.AddModelError(nameof(dto.Image), errorMessage!);

                        var viewModel = await BuildEditViewModelAsync(dto);
                        return View(viewModel);
                    }
                }

                await _contactService.UpdateContactAsync(dto);
                TempData["Success"] = SuccessMessages.ContactUpdated;

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ویرایش مخاطب {Id}", dto.Id);
                TempData["Error"] = ex.Message;

                var viewModel = await BuildEditViewModelAsync(dto);
                return View(viewModel);
            }
        }


        private async Task<ContactFormViewModel> BuildEditViewModelAsync(UpdateContactDto dto)
        {
            var contact = await _contactService.GetContactByIdAsync(dto.Id);

            return new ContactFormViewModel
            {
                UpdateDto = dto,
                IsEditMode = true,
                HasImage = contact?.HasImage ?? false,
                ImageUrl = contact?.HasImage == true ? Url.Action("GetImage", new { id = dto.Id }) : null
            };
        }

        // POST: Contacts/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _contactService.DeleteContactAsync(id);
                TempData["Success"] = SuccessMessages.ContactDeleted;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در حذف مخاطب {Id}", id);
                TempData["Error"] = ErrorMessages.UnexpectedError;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Contacts/GetImage/5
        public async Task<IActionResult> GetImage(int id)
        {
            try
            {
                var image = await _contactService.GetContactImageAsync(id);
                if (image == null)
                {
                    // بازگشت یک تصویر پیش‌فرض
                    return NotFound();
                }

                return File(image.ImageData, image.ContentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت تصویر مخاطب {Id}", id);
                return NotFound();
            }
        }

        // GET: Contacts/CheckMobileNumber
        [HttpGet]
        public async Task<IActionResult> CheckMobileNumber(string mobileNumber, int? contactId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(mobileNumber))
                {
                    return Json(new { available = true });
                }

                var exists = await _contactService.IsMobileNumberExistsAsync(mobileNumber, contactId);
                return Json(new { available = !exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در بررسی شماره موبایل");
                return Json(new { available = true, error = true });
            }
        }
    }
}