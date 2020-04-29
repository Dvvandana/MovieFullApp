using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MovieFullApp.Models;

namespace MovieFullApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<IdentityUser> _signInMgr;
        private readonly UserManager<IdentityUser> _userMgr;
        public HomeController(ILogger<HomeController> logger, UserManager<IdentityUser> userMgr, SignInManager<IdentityUser> signInMgr)
        {
            _logger = logger;
            _userMgr = userMgr;
            _signInMgr = signInMgr;
        }

        public IActionResult Index()
        {
            if (!_signInMgr.IsSignedIn(User)) { 
                return View();
            }
            else
            {
                return RedirectToAction("Index", "NewMovies");
            }
        }

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
