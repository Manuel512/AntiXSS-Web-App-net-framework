using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XSSWebApp.Models;

namespace XSSWebApp.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var payload = new Payload
            {
                FirstName = "Manuel <a href='javascript:alert('script injection sample')'></a>",
                LastName = "Perez",
                Age = 31,
                CarsQuantity = new List<int>() { 1, 2, 3},
                ChildNames = new Child { FirstName = "Manuel", LastName = "Perez <script>alert('script injection sample')</script>" },
                Children = new List<Child>
                {
                    new Child { FirstName = "Cotufa <script>alert('script injection sample')</script>", LastName = "Perez"}
                },
                Description = "<div style=\"background-color: red; padding: 5px 10px;\"></div>",
                IsMarried = true
            };

            ViewBag.SampleScript = "Test script injection - <script>alert('script injection sample')</script>";
            ViewBag.SampleScript2 = "javascript:alert('I\'m a sample script')";

            return View(payload);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        public ActionResult TestPayload(Payload payload)
        {
            //payload.Description = @"\<a onmouseover=alert(document.cookie)\>xxs link\</a\>";
            return Json(payload);
        }

        public ActionResult TestRedirection(string returnUrl)
        {
            return new RedirectResult(returnUrl, true);
        }
    }
}