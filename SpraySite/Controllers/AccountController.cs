using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.RelyingParty;
using SpraySite.DBHelpers;
using SpraySite.Models;

namespace SpraySite.Controllers
{
    public class AccountController : BaseController
    {
        static private readonly OpenIdRelyingParty Openid = new OpenIdRelyingParty();
        static private readonly string STEAM_OPENID_URI = "http://steamcommunity.com/openid";

        //TODO: Redirect to a different action, or give some sort of error message.
        public ActionResult OpenId()
        {
            var response = Openid.GetResponse();
            if (response == null)
            {
                //User submitting Identifier
                Identifier id;
                if (Identifier.TryParse(STEAM_OPENID_URI, out id))
                {
                    try
                    {
                        Realm realm = new Realm(Request.Url.Scheme + "://" + Request.Url.GetComponents(UriComponents.HostAndPort, UriFormat.UriEscaped));
                        Uri uri = new Uri(Request.Url.AbsoluteUri, UriKind.Absolute);
                        var request = Openid.CreateRequest(STEAM_OPENID_URI, realm, uri);
                        request.RedirectToProvider();
                        return new EmptyResult();
                    }
                    catch (ProtocolException)
                    {
                        return RedirectToAction("Index");
                    }
                }
                return RedirectToAction("Index");
            }

            switch (response.Status)
            {
                case AuthenticationStatus.Authenticated:
                    return LogOn(response.ClaimedIdentifier);
                case AuthenticationStatus.Canceled:
                    return RedirectToAction("Index");
                case AuthenticationStatus.Failed:
                    return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }

        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        private ActionResult LogOn(Identifier identifier)
        {
            //Remove the unnecessary portion of the identifier
            string steamIDString = identifier.ToString().Replace("http://steamcommunity.com/openid/id/", "");
            long steamId64 = long.Parse(steamIDString);

            using (var db = new SprayContext())
            {
                var user = db.Users.FirstOrDefault(x => x.SteamId == steamId64);

                SteamWebAPI.SteamAPISession session = new SteamWebAPI.SteamAPISession();
                session.accessToken = ""; /* CHANGEME - Steam Web API access token */
                var userInfo = session.GetUserInfo(steamId64.ToString());

                if (user == null)
                {
                    //Add the user if they're new
                    user = CreateUser(steamId64, db, userInfo);
                }
                else
                {
                    // Or update the relevant information
                    user.AvatarURI = userInfo.avatarUrl;
                    user.NickName = userInfo.nickname;
                    user.LastUpdated = DateTime.Now;
                }

                int recordsAffected = db.SaveChanges();
                FormsAuthentication.SetAuthCookie(steamId64.ToString(), true);
            }

            return RedirectToAction("Index", "Home");
        }

        private User CreateUser(long steamID64, SprayContext db, SteamWebAPI.SteamAPISession.User player)
        {
            //Add the user if they're new
            User user = new User
            {
                SteamId = steamID64,
                NickName = player.nickname,
                AvatarURI = player.avatarUrl,
                LastUpdated = DateTime.Now
            };
            db.Users.Add(user);
            return user;
        }
    }
}
