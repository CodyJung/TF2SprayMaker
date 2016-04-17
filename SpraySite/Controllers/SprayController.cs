using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SpraySite.DBHelpers;
using SpraySite.Models;

namespace SpraySite.Controllers
{
    public class SprayController : BaseController
    {

        public ActionResult Index()
        {
            return RedirectToAction("All", "Browse");
        }

        public ActionResult ViewSpray(string id)
        {
            ViewBag.CurrentPage = "browse";

            Guid providedId;
            if (!Guid.TryParse(id, out providedId))
            {
                return View("Error");
            }

            Spray spray;
            bool saved = false;
            bool mine = false;
            using (var db = new SprayContext())
            {
                spray = db.Sprays.Where(s => s.Id == providedId).FirstOrDefault();

                if (Request.IsAuthenticated)
                {
                    long steamId64 = long.Parse(User.Identity.Name);
                    if (spray.SavedBy.Where(u => u.SteamId == steamId64).Count() != 0)
                        saved = true;

                    if (spray.Creator != null && spray.Creator.SteamId == steamId64)
                        mine = true;
                }

                // Don't show unapproved sprays
                if (spray.Status == Status.ACTIVE && !mine)
                {
                    spray = null;
                }

            }

            if (spray == null)
                return View("Error");

            if(spray.Safeness == Safeness.NSFW)
                ViewBag.AdultAds = true;

            return View("View", new SprayViewModel { Spray = spray, SavedByCurrent = saved, IsCurrentUsersSpray = mine });
        }

        public ActionResult Save(string id)
        {
            if (Request.IsAuthenticated)
            {
                Guid providedId;
                if (!Guid.TryParse(id, out providedId))
                {
                    return View("Error");
                }

                using (var db = new SprayContext())
                {
                    Spray spray = db.Sprays.Where(s => s.Id == providedId).Where(s => s.Status != Status.DELETED).Where(s => s.DateExpires > DateTime.Now).ToList().FirstOrDefault();
                    if (spray != null)
                    {
                        long steamId64 = long.Parse(User.Identity.Name);
                        User u = db.Users.FirstOrDefault(x => x.SteamId == steamId64);

                        if (!u.Saved.Contains(spray))
                        {
                            u.Saved.Add(spray);
                            spray.Saves++;

                            db.SaveChanges();
                        }
                    }

                    return RedirectToRoute("View Spray");

                }
            }
            else
            {
                return View("Error");
            }
        }

        public ActionResult Unsave(string id)
        {
            if (Request.IsAuthenticated)
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
                        long steamId64 = long.Parse(User.Identity.Name);
                        User u = db.Users.FirstOrDefault(x => x.SteamId == steamId64);

                        if(u.Saved.Contains(spray)){
                            u.Saved.Remove(spray);
                            spray.Saves--;

                            db.SaveChanges();
                        }
                    }

                    return RedirectToRoute("View Spray");

                }
            }
            else
            {
                return View("Error");
            }
        }

        public ActionResult Privatize(string id)
        {
            if (Request.IsAuthenticated)
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
                        long steamId64 = long.Parse(User.Identity.Name);

                        if (spray.Creator.SteamId == steamId64 || User.Identity.Name == "76561197999489042") /* CHANGEME - This is my Steam ID. If I logged in, I got extra admin options */
                        {
                            spray.Status = Status.ACTIVE;
                            db.SaveChanges();
                        }
                    }

                    return RedirectToRoute("View Spray");

                }
            }
            else
            {
                return View("Error");
            }
        }

        public ActionResult Unlist(string id)
        {
            if (Request.IsAuthenticated)
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
                        long steamId64 = long.Parse(User.Identity.Name);

                        if (spray.Creator.SteamId == steamId64 || User.Identity.Name == "76561197999489042") /* CHANGEME - This is my Steam ID. If I logged in, I got extra admin options */
                        {
                            spray.Status = Status.UNLISTED;
                            db.SaveChanges();
                        }
                    }

                    return RedirectToRoute("View Spray");

                }
            }
            else
            {
                return View("Error");
            }
        }

        public ActionResult Publish(string id)
        {
            if (Request.IsAuthenticated)
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
                        long steamId64 = long.Parse(User.Identity.Name);

                        /* CHANGEME - This is my Steam ID. If I logged in, I got extra admin options */
                        if ((spray.Creator.SteamId == steamId64 || User.Identity.Name == "76561197999489042") && spray.Status != Status.PUBLIC) // So we don't have to re-moderate
                        {
                            spray.Status = Status.UNMODERATED;
                            db.SaveChanges();
                        }
                    }

                    return RedirectToRoute("View Spray");

                }
            }
            else
            {
                return View("Error");
            }
        }

        public ActionResult Delete(string id)
        {
            if (Request.IsAuthenticated)
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
                        long steamId64 = long.Parse(User.Identity.Name);

                        if (spray.Creator.SteamId == steamId64 || User.Identity.Name == "76561197999489042") /* CHANGEME - This is my Steam ID. If I logged in, I got extra admin options */
                        {
                            spray.Status = Status.DELETED;
                            db.SaveChanges();
                        }
                    }

                    return RedirectToAction("Mine", "Browse");

                }
            }
            else
            {
                return View("Error");
            }
        }

    }
}
