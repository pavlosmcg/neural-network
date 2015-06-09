using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NeuralNetworkWeb.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";
            return View();
        }

        [HttpPost]
        public string Upload(ImageData value)
        {
            return value.base64Image;
        }
    }

    public class ImageData
    {
        public string base64Image { get; set; }
    }
}
