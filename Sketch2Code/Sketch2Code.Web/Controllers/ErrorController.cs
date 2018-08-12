using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sketch2Code.Web.Controllers
{
    public class ErrorController : Controller
    {
        public ViewResult Index()
        {
            return View("Index");
        }
        //public ViewResult NotFound()
        //{
        //    Response.StatusCode = 404;  //you may want to set this to 200
        //    return View("NotFound");
        //}
    }
}