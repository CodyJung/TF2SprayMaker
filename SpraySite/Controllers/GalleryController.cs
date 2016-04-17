using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace SpraySite.Controllers
{
    public class GalleryController : BaseController
    {
        //
        // GET: /Gallery/

        public ActionResult Index(long id)
        {
            ViewBag.NoAds = true;
            if (id <= 0)
                return View("Error");

            return View(id);
        }

        public ActionResult VMT(string id)
        {
            long providedId;
            if (!long.TryParse(id, out providedId))
            {
                return View("Error");
            }

            var cd = new System.Net.Mime.ContentDisposition
            {
                // for example foo.bak
                FileName = id + ".vmt",

                // always prompt the user for downloading, set to true if you want 
                // the browser to try to show the file inline
                Inline = false,
            };
            Response.AppendHeader("Content-Disposition", cd.ToString());

            string vmt_file = String.Format(@"""UnlitGeneric""
{{
    ""$basetexture""	""vgui\logos\{0}""
    ""$translucent"" ""1""
    ""$ignorez"" ""1""
    ""$vertexcolor"" ""1""
    ""$vertexalpha"" ""1""
}} ", id);


            return File(Encoding.UTF8.GetBytes(vmt_file), "application/vnd.valve.vmt");

        }
    }
}
