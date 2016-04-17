using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SpraySite.DBHelpers;

namespace SpraySite.Controllers
{
    public class BaseController : Controller
    {
        protected long _baseSteamId = -1;
        protected bool _isLoggedIn = false;

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);

            ViewBag.CurrentPage = "default";
            ViewBag.UserName = "Anonymous";
            ViewBag.AvatarUrl = ""; // TODO Default avatar URI

            if (Request.IsAuthenticated)
            {
                using (var db = new SprayContext())
                {
                    _baseSteamId = long.Parse(User.Identity.Name);
                    _isLoggedIn = true;
                    var user = db.Users.FirstOrDefault(x => x.SteamId == _baseSteamId);

                    if (user != null)
                    {
                        ViewBag.UserName = user.NickName;
                        ViewBag.AvatarUrl = user.AvatarURI;
                    }
                }
            }
        }
    }
}
