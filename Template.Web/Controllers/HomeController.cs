using Template.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBreadcrumbs.Attributes;
using System.Diagnostics;

namespace Template.Web.Controllers
{
	[Authorize]
    [DefaultBreadcrumb]
    public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger) => _logger = logger;

        [Authorize]
		public IActionResult Index()
		{			
			//var permissions = HttpContext.Session.GetString("permissions");
            //ViewData["permissions"] = permissions;
			return View();
        }
        public IActionResult TestPage()
        {
            return View();
        }

        [Breadcrumb("UI Kit", FromAction = nameof(Index))]
        public IActionResult UiKit()
        {
            return View();
        }

        [Breadcrumb("Privacy", FromAction = nameof(Index))]
        public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
