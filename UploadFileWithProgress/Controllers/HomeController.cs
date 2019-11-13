using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace UploadFileWithProgress.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost] 
        [DisableRequestSizeLimit]
        public IActionResult UploadFile(IFormFile file)
        {
            return Json(new { state = 0, message = string.Empty });
        }

        public IActionResult UploadChunked()
        {
            return View();
        }
    }
}