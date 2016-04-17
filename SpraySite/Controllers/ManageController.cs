using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SpraySite.DBHelpers;
using SpraySite.Models;

namespace SpraySite.Controllers
{
    public class ManageController : BaseController
    {

        private static readonly int SPRAYS_PER_PAGE = 100;

        public ActionResult Index()
        {
            return RedirectToAction("Manage");
        }

        public ActionResult Manage(int? after)
        {
            /* CHANGEME - This is my Steam ID. If I logged in, I got extra admin options */
            if (!Request.IsAuthenticated || (User.Identity.Name != "76561197999489042"))
                return View("Error");

            ViewBag.CurrentPage = "manage";
            ViewBag.BrowseMode = "newest";

            // Reset to 0 if negative or null
            int afterSpray = after ?? 0;
            if (afterSpray < 0) afterSpray = 0;

            SprayListModel sprays = new SprayListModel();
            sprays.Start = afterSpray;
            sprays.Prev = afterSpray - SPRAYS_PER_PAGE < 0 ? 0 : afterSpray - SPRAYS_PER_PAGE;

            using (var db = new SprayContext())
            {
                sprays.Sprays = db.Sprays.Where(s => s.Status == Status.UNMODERATED).OrderBy(s => s.DateAdded).Skip(afterSpray).Take(SPRAYS_PER_PAGE).ToList();
            }

            sprays.Next = sprays.Sprays.Count < SPRAYS_PER_PAGE ? afterSpray : afterSpray + SPRAYS_PER_PAGE;

            return View(sprays);
        }

        public ActionResult Approve(string id)
        {
            if (Request.IsAuthenticated && (User.Identity.Name == "76561197999489042")) /* CHANGEME - This is my Steam ID. If I logged in, I got extra admin options */
            {
                Guid providedId;
                if (!Guid.TryParse(id, out providedId))
                {
                    return View("Error");
                }

                using (var db = new SprayContext())
                {
                    Spray spray = db.Sprays.Where(s => s.Id == providedId).FirstOrDefault();
                    if (spray != null)
                    {
                        spray.Status = Status.PUBLIC;
                        spray.Safeness = Safeness.SFW;
                        db.SaveChanges();
                    }

                    return Json(true);

                }
            }
            else
            {
                return View("Error");
            }
        }

        public ActionResult ApproveSketchy(string id)
        {
            if (Request.IsAuthenticated && (User.Identity.Name == "76561197999489042")) /* CHANGEME - This is my Steam ID. If I logged in, I got extra admin options */
            {
                Guid providedId;
                if (!Guid.TryParse(id, out providedId))
                {
                    return View("Error");
                }

                using (var db = new SprayContext())
                {
                    Spray spray = db.Sprays.Where(s => s.Id == providedId).FirstOrDefault();
                    if (spray != null)
                    {
                        spray.Status = Status.PUBLIC;
                        spray.Safeness = Safeness.SKETCHY;
                        db.SaveChanges();
                    }

                    return Json(true);

                }
            }
            else
            {
                return View("Error");
            }
        }

        public ActionResult ApproveNSFW(string id)
        {
            if (Request.IsAuthenticated && (User.Identity.Name == "76561197999489042")) /* CHANGEME - This is my Steam ID. If I logged in, I got extra admin options */
            {
                Guid providedId;
                if (!Guid.TryParse(id, out providedId))
                {
                    return View("Error");
                }

                using (var db = new SprayContext())
                {
                    Spray spray = db.Sprays.Where(s => s.Id == providedId).FirstOrDefault();
                    if (spray != null)
                    {
                        spray.Status = Status.PUBLIC;
                        spray.Safeness = Safeness.NSFW;
                        db.SaveChanges();
                    }

                    return Json(true);

                }
            }
            else
            {
                return View("Error");
            }
        }

        public ActionResult Deny(string id)
        {
            if (Request.IsAuthenticated && (User.Identity.Name == "76561197999489042")) /* CHANGEME - This is my Steam ID. If I logged in, I got extra admin options */
            {
                Guid providedId;
                if (!Guid.TryParse(id, out providedId))
                {
                    return View("Error");
                }

                using (var db = new SprayContext())
                {
                    Spray spray = db.Sprays.Where(s => s.Id == providedId).FirstOrDefault();
                    if (spray != null)
                    {
                        spray.Status = Status.UNLISTED;
                        db.SaveChanges();
                    }

                    return Json(true);

                }
            }
            else
            {
                return View("Error");
            }
        }
    }
}
