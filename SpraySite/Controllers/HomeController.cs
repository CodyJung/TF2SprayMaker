using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SpraySite.DBHelpers;
using SpraySite.Models;

namespace SpraySite.Controllers
{
    public class HomeController : BaseController
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            SprayListModel sprays = new SprayListModel();
            //sprays.Start = 0;
            //sprays.Prev = 0;

            //using (var db = new SprayContext())
            //{
            //    sprays.Sprays = db.Sprays.Where(s => s.Safeness == Safeness.SFW).Where(s => s.Status == Status.PUBLIC).OrderByDescending(s => s.DateAdded).Take(3).ToList();
            //}

            //sprays.Next = 0;

            return View(sprays);
        }

    }
}
