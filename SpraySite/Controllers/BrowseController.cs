using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SpraySite.DBHelpers;
using SpraySite.Models;

namespace SpraySite.Controllers
{
    public class BrowseController : BaseController
    {

        private static readonly int SPRAYS_PER_PAGE = 15;

        public ActionResult Index()
        {
            return RedirectToAction("Newest");
        }

        public ActionResult Newest(int? after)
        {
            ViewBag.CurrentPage = "browse";
            return View("Gone");

            //ViewBag.BrowseMode = "newest";

            //// Reset to 0 if negative or null
            //int afterSpray = after ?? 0;
            //if (afterSpray < 0) afterSpray = 0;

            //SprayListModel sprays = new SprayListModel();
            //sprays.Start = afterSpray;
            //sprays.Prev = afterSpray - SPRAYS_PER_PAGE < 0 ? 0 : afterSpray - SPRAYS_PER_PAGE;

            //using (var db = new SprayContext())
            //{
            //    sprays.Sprays = db.Sprays.Where(s => s.Safeness == Safeness.SFW).Where(s => s.Status == Status.PUBLIC).OrderByDescending(s => s.DateAdded).Skip(afterSpray).Take(SPRAYS_PER_PAGE).ToList();
            //}

            //sprays.Next = sprays.Sprays.Count < SPRAYS_PER_PAGE ? afterSpray : afterSpray + SPRAYS_PER_PAGE;

            //return View(sprays);
        }

        public ActionResult Top(int? after)
        {
            ViewBag.CurrentPage = "browse";
            return View("Gone");

            //ViewBag.CurrentPage = "browse";
            //ViewBag.BrowseMode = "top";

            //// Reset to 0 if negative or null
            //int afterSpray = after ?? 0;
            //if (afterSpray < 0) afterSpray = 0;

            //SprayListModel sprays = new SprayListModel();
            //sprays.Start = afterSpray;
            //sprays.Prev = afterSpray - SPRAYS_PER_PAGE < 0 ? 0 : afterSpray - SPRAYS_PER_PAGE;

            //using (var db = new SprayContext())
            //{
            //    sprays.Sprays = db.Sprays.Where(s => s.Safeness == Safeness.SFW).Where(s => s.Status == Status.PUBLIC).OrderByDescending(s => s.Saves).Skip(afterSpray).Take(SPRAYS_PER_PAGE).ToList();
            //}

            //sprays.Next = sprays.Sprays.Count < SPRAYS_PER_PAGE ? afterSpray : afterSpray + SPRAYS_PER_PAGE;

            //return View(sprays);
        }

        public ActionResult NewestNSFW(int? after)
        {
            ViewBag.AdultAds = true;
            ViewBag.CurrentPage = "browse";
            return View("Gone");

            //ViewBag.CurrentPage = "browse";
            //ViewBag.BrowseMode = "newestnsfw";

            //// Reset to 0 if negative or null
            //int afterSpray = after ?? 0;
            //if (afterSpray < 0) afterSpray = 0;

            //SprayListModel sprays = new SprayListModel();
            //sprays.Start = afterSpray;
            //sprays.Prev = afterSpray - SPRAYS_PER_PAGE < 0 ? 0 : afterSpray - SPRAYS_PER_PAGE;

            //using (var db = new SprayContext())
            //{
            //    sprays.Sprays = db.Sprays.Where(s => s.Safeness == Safeness.NSFW).Where(s => s.Status == Status.PUBLIC).OrderByDescending(s => s.DateAdded).Skip(afterSpray).Take(SPRAYS_PER_PAGE).ToList();
            //}

            //sprays.Next = sprays.Sprays.Count < SPRAYS_PER_PAGE ? afterSpray : afterSpray + SPRAYS_PER_PAGE;

            //return View(sprays);
        }

        public ActionResult TopNSFW(int? after)
        {
            ViewBag.AdultAds = true;
            ViewBag.CurrentPage = "browse";
            return View("Gone");

            //ViewBag.CurrentPage = "browse";
            //ViewBag.BrowseMode = "topnsfw";

            //// Reset to 0 if negative or null
            //int afterSpray = after ?? 0;
            //if (afterSpray < 0) afterSpray = 0;

            //SprayListModel sprays = new SprayListModel();
            //sprays.Start = afterSpray;
            //sprays.Prev = afterSpray - SPRAYS_PER_PAGE < 0 ? 0 : afterSpray - SPRAYS_PER_PAGE;

            //using (var db = new SprayContext())
            //{
            //    sprays.Sprays = db.Sprays.Where(s => s.Safeness == Safeness.NSFW).Where(s => s.Status == Status.PUBLIC).OrderByDescending(s => s.Saves).Skip(afterSpray).Take(SPRAYS_PER_PAGE).ToList();
            //}

            //sprays.Next = sprays.Sprays.Count < SPRAYS_PER_PAGE ? afterSpray : afterSpray + SPRAYS_PER_PAGE;

            //return View(sprays);
        }

        public ActionResult NewestSketchy(int? after)
        {
            ViewBag.CurrentPage = "browse";
            return View("Gone");

            //ViewBag.CurrentPage = "browse";
            //ViewBag.BrowseMode = "newestsketchy";

            //// Reset to 0 if negative or null
            //int afterSpray = after ?? 0;
            //if (afterSpray < 0) afterSpray = 0;

            //SprayListModel sprays = new SprayListModel();
            //sprays.Start = afterSpray;
            //sprays.Prev = afterSpray - SPRAYS_PER_PAGE < 0 ? 0 : afterSpray - SPRAYS_PER_PAGE;

            //using (var db = new SprayContext())
            //{
            //    sprays.Sprays = db.Sprays.Where(s => s.Safeness == Safeness.SKETCHY).Where(s => s.Status == Status.PUBLIC).OrderByDescending(s => s.DateAdded).Skip(afterSpray).Take(SPRAYS_PER_PAGE).ToList();
            //}

            //sprays.Next = sprays.Sprays.Count < SPRAYS_PER_PAGE ? afterSpray : afterSpray + SPRAYS_PER_PAGE;

            //return View(sprays);
        }

        public ActionResult TopSketchy(int? after)
        {
            ViewBag.CurrentPage = "browse";
            return View("Gone");

            //ViewBag.CurrentPage = "browse";
            //ViewBag.BrowseMode = "topsketchy";

            //// Reset to 0 if negative or null
            //int afterSpray = after ?? 0;
            //if (afterSpray < 0) afterSpray = 0;

            //SprayListModel sprays = new SprayListModel();
            //sprays.Start = afterSpray;
            //sprays.Prev = afterSpray - SPRAYS_PER_PAGE < 0 ? 0 : afterSpray - SPRAYS_PER_PAGE;

            //using (var db = new SprayContext())
            //{
            //    sprays.Sprays = db.Sprays.Where(s => s.Safeness == Safeness.SKETCHY).Where(s => s.Status == Status.PUBLIC).OrderByDescending(s => s.Saves).Skip(afterSpray).Take(SPRAYS_PER_PAGE).ToList();
            //}

            //sprays.Next = sprays.Sprays.Count < SPRAYS_PER_PAGE ? afterSpray : afterSpray + SPRAYS_PER_PAGE;

            //return View(sprays);
        }

        public ActionResult Mine(int? after)
        {
            if (!Request.IsAuthenticated)
            {
                return View("Error");
            }

            ViewBag.CurrentPage = "mine";

            // Reset to 0 if negative or null
            int afterSpray = after ?? 0;
            if (afterSpray < 0) afterSpray = 0;

            SprayListModel sprays = new SprayListModel();
            sprays.Start = afterSpray;
            sprays.Prev = afterSpray - SPRAYS_PER_PAGE < 0 ? 0 : afterSpray - SPRAYS_PER_PAGE;

            using (var db = new SprayContext())
            {
                long steamId64 = long.Parse(User.Identity.Name);

                sprays.Sprays = db.Sprays.Where(s => s.Creator.SteamId == steamId64).Where(s => s.Status != Status.DELETED).OrderByDescending(s => s.DateAdded).Skip(afterSpray).Take(SPRAYS_PER_PAGE).ToList();
            }

            sprays.Next = sprays.Sprays.Count < SPRAYS_PER_PAGE ? afterSpray : afterSpray + SPRAYS_PER_PAGE;

            return View(sprays);
        }

        public ActionResult Saved(int? after)
        {
            if (!Request.IsAuthenticated)
            {
                return View("Error");
            }

            ViewBag.CurrentPage = "saved";

            // Reset to 0 if negative or null
            int afterSpray = after ?? 0;
            if (afterSpray < 0) afterSpray = 0;

            SprayListModel sprays = new SprayListModel();
            sprays.Start = afterSpray;
            sprays.Prev = afterSpray - SPRAYS_PER_PAGE < 0 ? 0 : afterSpray - SPRAYS_PER_PAGE;

            using (var db = new SprayContext())
            {
                long steamId64 = long.Parse(User.Identity.Name);

                sprays.Sprays = db.Users.Where(u => u.SteamId == steamId64).First().Saved.Where(s => s.Status != Status.DELETED).Where(s => s.Status != Status.ACTIVE).OrderByDescending(s => s.DateAdded).Skip(afterSpray).Take(SPRAYS_PER_PAGE).ToList();
            }

            sprays.Next = sprays.Sprays.Count < SPRAYS_PER_PAGE ? afterSpray : afterSpray + SPRAYS_PER_PAGE;

            return View(sprays);
        }

    }
}
