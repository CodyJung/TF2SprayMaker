using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;
using System.Xml.Linq;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using ExplodingJelly.SprayGenerator;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json.Linq;
using SpraySite.DBHelpers;
using SpraySite.Hubs;
using SpraySite.Models;
using System.Collections.Generic;

namespace SpraySite.Controllers
{
    public class CreateController : BaseController
    {
        /* CHANGEME - You'll need to change these, since you don't have access to my buckets */
        private const string BUCKET_REGISTERED_VTF = "sprays.tfsprays.com";
        private const string BUCKET_REGISTERED_GIF = "thumbs.tfsprays.com";
        private const string BUCKET_ANONYMOUS_VTF = "anonsprays.tfsprays.com";
        private const string BUCKET_ANONYMOUS_GIF = "anonthumbs.tfsprays.com";

        private string _progressId;
        private bool _transparency;

        public ActionResult Index()
        {
            return RedirectToAction("Static");
        }

        #region static
        public ActionResult Static()
        {
            ViewBag.CurrentPage = "create";
            return View();

            //return View("Maintenance");
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult UploadStatic()
        {
            Guid sprayId = Guid.NewGuid();

            NameValueCollection nvc = Request.Form;
            XDocument doc = XDocument.Parse(nvc["s3Response"]);
            _transparency = bool.Parse(nvc["transparency"]);
            _progressId = nvc["progressId"];

            var location = doc.Element("PostResponse").Element("Location").Value;

            if (!location.StartsWith("https://tempsprays.s3-us-west-1.amazonaws.com")) /* CHANGEME - Change this to a temporary bucket. Mine deletes everything after a day */
                return new HttpStatusCodeResult(400, "Invalid image Uri. My S3 bucket only, please.");

            if (location.EndsWith(".tga"))
                return Json(".tga is no longer supported. Sorry! Please convert to .png or .gif");

            using (var wb = new WebClient())
            {
                wb.DownloadProgressChanged += wb_Static_DownloadProgressChanged;
                wb.OpenReadCompleted += wb_Static_OpenReadCompleted;
                wb.OpenReadAsync(new Uri(location), sprayId);
            }

            return Json(true);
        }

        private void wb_Static_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            MemoryStream outputStream = new MemoryStream();
            VTFGenerator g = new VTFGenerator(e.Result, outputStream);
            //g.Process(true);
            g.ProcessProgress += g_Static_ProcessProgress;
            g.ProcessingComplete += g_Static_ProcessingComplete;
            g.Process_Async(_transparency);
        }

        private void g_Static_ProcessingComplete(object sender, ProcessCompleteEventArgs e)
        {
            Guid g = Guid.NewGuid();

            var context = GlobalHost.ConnectionManager.GetHubContext<ProgressHub>();
            context.Clients.Client(_progressId).changePercent("100", "Processing");

            // UPLOAD NEW SPRAY
            AmazonS3Client client = new AmazonS3Client("KEY", "SECRET", RegionEndpoint.USWest1); /* CHANGEME - Amazon S3 Key/Secret */
            PutObjectRequest request = new PutObjectRequest();
            request.PutObjectProgressEvent += request_Static_PutObjectProgressEvent;
            request.BucketName = _isLoggedIn ? BUCKET_REGISTERED_VTF : BUCKET_ANONYMOUS_VTF;
            request.Key = string.Format("{0}.vtf", g);
            request.InputStream = e.outputStream;
            request.CannedACL = S3CannedACL.PublicRead;
            request.AutoCloseStream = true;
            client.PutObject(request);

            // UPLOAD NEW PREVIEW
            NameValueCollection nvc = Request.Form;
            XDocument doc = XDocument.Parse(nvc["s3Response"]);
            var location = UploadToBlitline(g, doc, false);

            // SAVE TO DATABASE
            using (var db = new SprayContext())
            {
                User u = null;
                DateTime expires = new DateTime(9999, 12, 31);

                if (_isLoggedIn)
                {
                    u = db.Users.FirstOrDefault(x => x.SteamId == _baseSteamId);
                }
                else
                {
                    expires = DateTime.Now.AddDays(7);
                }

                // Make a new spray object
                Spray spray = new Spray()
                {
                    Animated = e.Animated,
                    DateAdded = DateTime.Now,
                    DateExpires = expires,
                    Fading = e.Fading,
                    Id = g,
                    NSFW = false,
                    Safeness = Models.Safeness.SFW,
                    PreviewImage = string.Format("https://{0}/{1}{2}", _isLoggedIn ? BUCKET_REGISTERED_GIF : BUCKET_ANONYMOUS_GIF, g, location.ToString().EndsWith(".gif") ? ".gif" : ".png"),
                    Status = Status.ACTIVE,
                    Saves = 0,
                    Url = string.Format("https://{0}/{1}", request.BucketName, request.Key),
                    Creator = u
                };

                // Tell the client to show the spray
                context.Clients.Client(_progressId).showImage(spray.PreviewImage, spray.Url, "/Create/VMT/" + g.ToString(), spray.Animated, "/Spray/" + g.ToString());

                db.Sprays.Add(spray);
                db.SaveChanges();
            }
        }

        private void g_Static_ProcessProgress(object sender, ProcessProgressEventArgs e)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<ProgressHub>();
            context.Clients.Client(_progressId).changePercent((e.PercentComplete * 100).ToString(), "Processing");
        }

        private void request_Static_PutObjectProgressEvent(object sender, PutObjectProgressArgs e)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<ProgressHub>();
            context.Clients.Client(_progressId).changePercent(e.PercentDone.ToString(), "Upload2S3");
        }

        private void wb_Static_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<ProgressHub>();
            context.Clients.Client(_progressId).changePercent(e.ProgressPercentage.ToString(), "DownloadFromS3");
        }

        #endregion

        #region fading
        private Stream _nearStream;
        private Stream _farStream;

        public ActionResult Fading()
        {
            ViewBag.CurrentPage = "createFading";
            return View();
            //return View("Maintenance");
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult UploadFading()
        {
            Guid sprayId = Guid.NewGuid();

            NameValueCollection nvc = Request.Form;
            XDocument doc1 = XDocument.Parse(nvc["s3NearResponse"]);
            XDocument doc2 = XDocument.Parse(nvc["s3FarResponse"]);
            _transparency = bool.Parse(nvc["transparency"]);
            _progressId = nvc["progressId"];

            var nearLocation = doc1.Element("PostResponse").Element("Location").Value;
            var farLocation = doc2.Element("PostResponse").Element("Location").Value;

            if (!nearLocation.StartsWith("https://tempsprays.s3-us-west-1.amazonaws.com") || !farLocation.StartsWith("https://tempsprays.s3-us-west-1.amazonaws.com")) /* CHANGEME - Change this to a temporary bucket. Mine deletes everything after a day */
                return new HttpStatusCodeResult(400, "Invalid image Uri. My S3 bucket only, please.");

            if (nearLocation.EndsWith(".tga"))
                return Json(new object[] {0, ".tga is no longer supported. Sorry! Please convert to .png or .gif"});

            if (farLocation.EndsWith(".tga"))
                return Json(new object[] {1, ".tga is no longer supported. Sorry! Please convert to .png or .gif" });

            using (var wb = new WebClient())
            {
                wb.OpenReadCompleted += wb_Fading_OpenReadCompleted_Near;
                wb.OpenReadAsync(new Uri(nearLocation), sprayId);
            }

            using (var wb2 = new WebClient())
            {
                wb2.OpenReadCompleted += wb_Fading_OpenReadCompleted_Far;
                wb2.OpenReadAsync(new Uri(farLocation), sprayId);
            }

            return Json(new object[] {null, true});
        }

        //TODO oh god why
        private void wb_Fading_OpenReadCompleted_Near(object sender, OpenReadCompletedEventArgs e)
        {
            _nearStream = e.Result;
            CheckDownloadsComplete();
        }

        private void wb_Fading_OpenReadCompleted_Far(object sender, OpenReadCompletedEventArgs e)
        {
            _farStream = e.Result;
            CheckDownloadsComplete();
        }

        private void CheckDownloadsComplete()
        {
            if (_nearStream != null && _farStream != null)
            {
                MemoryStream outputStream = new MemoryStream();
                VTFGenerator g = new VTFGenerator(_farStream, _nearStream, outputStream);
                g.ProcessProgress += g_Static_ProcessProgress;
                g.ProcessingComplete += g_Fading_ProcessingComplete;
                g.Process_Async(_transparency);
            }
        }

        private void g_Fading_ProcessingComplete(object sender, ProcessCompleteEventArgs e)
        {
            Guid g = Guid.NewGuid();

            var context = GlobalHost.ConnectionManager.GetHubContext<ProgressHub>();
            context.Clients.Client(_progressId).changePercent("100", "Processing");

            // UPLOAD NEW SPRAY
            AmazonS3Client client = new AmazonS3Client("KEY", "SECRET", RegionEndpoint.USWest1); /* CHANGEME - Amazon S3 Key/Secret */
            PutObjectRequest request = new PutObjectRequest();
            request.PutObjectProgressEvent += request_Fading_PutObjectProgressEvent;
            request.BucketName = _isLoggedIn ? BUCKET_REGISTERED_VTF : BUCKET_ANONYMOUS_VTF;
            request.Key = string.Format("{0}.vtf", g);
            request.InputStream = e.outputStream;
            request.CannedACL = S3CannedACL.PublicRead;
            request.AutoCloseStream = true;
            client.PutObject(request);

            // UPLOAD NEW PREVIEW
            NameValueCollection nvc = Request.Form;
            XDocument doc1 = XDocument.Parse(nvc["s3NearResponse"]);
            XDocument doc2 = XDocument.Parse(nvc["s3FarResponse"]);
            var nearLocation = UploadToBlitline(g, doc1, true);
            var farLocation = UploadToBlitline(g, doc2, false);

            // SAVE TO DATABASE
            using (var db = new SprayContext())
            {
                User u = null;
                DateTime expires = new DateTime(9999, 12, 31);
                if (_isLoggedIn)
                {
                    u = db.Users.FirstOrDefault(x => x.SteamId == _baseSteamId);
                }
                else
                {
                    expires = DateTime.Now.AddDays(7);
                }

                // Make a new spray object
                Spray spray = new Spray()
                {
                    Animated = e.Animated,
                    DateAdded = DateTime.Now,
                    DateExpires = expires,
                    Fading = e.Fading,
                    Id = g,
                    NSFW = false,
                    Safeness = Models.Safeness.SFW,
                    PreviewImage = string.Format("https://{0}/{1}{2}", _isLoggedIn ? BUCKET_REGISTERED_GIF : BUCKET_ANONYMOUS_GIF, g, farLocation.ToString().EndsWith(".gif") ? ".gif" : ".png"),
                    PreviewImageNear = string.Format("https://{0}/{1}-n{2}", _isLoggedIn ? BUCKET_REGISTERED_GIF : BUCKET_ANONYMOUS_GIF, g, nearLocation.ToString().EndsWith(".gif") ? ".gif" : ".png"),
                    Status = Status.ACTIVE,
                    Saves = 0,
                    Url = string.Format("https://{0}/{1}", request.BucketName, request.Key),
                    Creator = u
                };

                db.Sprays.Add(spray);
                db.SaveChanges();

                // Tell the client to show the spray
                context.Clients.Client(_progressId).showImage(spray.PreviewImage, spray.PreviewImageNear, spray.Url, "/Create/VMT/" + g.ToString(), spray.Animated, "/Spray/" + g.ToString());
            }
        }

        private Uri UploadToBlitline(Guid g, XDocument doc, Boolean nearImage)
        {
            var blitlineJson = "";
            var location = new Uri(doc.Element("PostResponse").Element("Location").Value);
            if (location.ToString().EndsWith(".gif"))
            {
                blitlineJson = string.Format(@"{{
                ""application_id"":""{0}"",
                ""src"":""{1}"",
                ""pre_process"":{{
                    ""resize_gif_to_fit"":{{
                        ""params"":{{
                            ""width"":256,
                            ""height"":256
                        }},
                        ""s3_destination"":{{
                            ""bucket"": {{
                                ""name"": ""{2}"",
                                ""location"": ""us-west-1""
                            }},
                            ""key"":""{3}{4}.gif""
                        }}
                    }}
                }},
                ""functions"":[
                    {{
                        ""name"":""no_op"",
                        ""save"":{{
                            ""image_identifier"":""{3}{4}""
                        }}
                    }}
                ]
            }}", "BLITLINE KEY", location.ToString(), _isLoggedIn ? BUCKET_REGISTERED_GIF : BUCKET_ANONYMOUS_GIF, g.ToString(), nearImage ? "-n" : ""); /* CHANGEME - Blitline key */

            }
            else
            {
                blitlineJson = string.Format(@"{{
                ""application_id"":""{0}"",
                ""src"":""{1}"",
                ""functions"":[
                    {{
                        ""name"":""pad_resize_to_fit"",
                        ""params"":{{
                            ""width"":256,
                            ""height"":256,
                            ""color"":""transparent""
                        }},
                        ""save"":{{
                            ""image_identifier"":""{3}{4}"",
                            ""s3_destination"":{{
                                ""bucket"": {{
                                    ""name"": ""{2}"",
                                    ""location"": ""us-west-1""
                                }},
                                ""key"":""{3}{4}.png"",
                                ""extension"":"".png"",
                                ""png_quantize"":true
                            }}
                        }}
                    }}
                ]
            }}", "BLITLINE KEY", location.ToString(), _isLoggedIn ? BUCKET_REGISTERED_GIF : BUCKET_ANONYMOUS_GIF, g.ToString(), nearImage ? "-n" : ""); /* CHANGEME - Blitline key */

            }

            WebClient c = new WebClient();
            NameValueCollection values = new NameValueCollection();
            values.Add("json", blitlineJson);
            c.UploadValuesAsync(new Uri("http://api.blitline.com/job"), "POST", values);
            return location;
        }

        private void request_Fading_PutObjectProgressEvent(object sender, PutObjectProgressArgs e)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<ProgressHub>();
            context.Clients.Client(_progressId).changePercent(e.PercentDone.ToString(), "Upload2S3");
        }
        #endregion

        public ActionResult VMT(string id)
        {
            Guid providedId;
            if (!Guid.TryParse(id, out providedId))
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
