using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace SpraySite.Controllers
{
    public class ErrorController : Controller
    {
        //
        // GET: /Error/

        public ActionResult Error404()
        {
            return View("Error404");
        }

        [HttpPost, ValidateInput(false)]
        public JsonResult GetAmazonError()
        {
            NameValueCollection nvc = Request.Form;
            XDocument doc = XDocument.Parse(nvc["s3Response"]);

            var errorCode = doc.Element("Error").Element("Code").Value;
            var errorMessage = doc.Element("Error").Element("Message").Value;

            if (errorCode == "EntityTooLarge")
                return Json("Upload too large! Please resize your image.");
            else if (errorCode == "AccessDenied")
                if (errorMessage.Contains("$Content-Type"))
                    return Json("Upload not an image. Please convert to .png, .gif, or .jpg");
                else
                    return Json("An access error occurred during upload! Please file a bug report.");
            else
                return Json("An error occurred during upload! Please file a bug report.");

        }

    }
}
