using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PhoneBook.Web.Models;

namespace PhoneBook.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
        {
            return RedirectToAction("Index", "Contacts");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(string? message)
        {
            var viewModel = new PhoneBook.Web.Models.ViewModels.ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                ErrorMessage = message ?? "Unexpected error occurred"
            };

            return View(viewModel);
        }
}
