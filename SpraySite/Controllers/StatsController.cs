using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Caching;
using System.Web.Mvc;
using SpraySite.DBHelpers;
using SpraySite.Models;
using DotNet.Highcharts;
using DotNet.Highcharts.Options;
using DotNet.Highcharts.Enums;
using System.Drawing;
using DotNet.Highcharts.Helpers;

namespace SpraySite.Controllers
{
    public class StatsController : BaseController
    {
        private ObjectCache cache;

        public StatsController()
        {
            cache = MemoryCache.Default;
        }

        private class ThingsOverTime
        {
            public DateTime Date { get; set; }
            public long SprayCount { get; set; }
        }

        public ActionResult Index()
        {
            DateTime lastGenerated;
            long totalSprays;
            long publishedSprays;
            long pendingSprays;
            long unlistedSprays;
            long privateSprays;

            long regularSprays;
            long nsfwSprays;
            long animatedSprays;
            long fadingSprays;

            long anonymousSprays;
            long loggedInSprays;

            List<ThingsOverTime> lastWeekSprays;

            long numUsers;

            if (cache.Contains("LastGenerated"))
            {
                lastGenerated = (DateTime)cache.Get("LastGenerated");

                totalSprays = (long)cache.Get("TotalSprays");
                publishedSprays = (long)cache.Get("PublishedSprays");
                pendingSprays = (long)cache.Get("PendingSprays");
                unlistedSprays = (long)cache.Get("UnlistedSprays");
                privateSprays = (long)cache.Get("PrivateSprays");

                regularSprays = (long)cache.Get("RegularSprays");
                nsfwSprays = (long)cache.Get("NSFWSprays");
                animatedSprays = (long)cache.Get("AnimatedSprays");
                fadingSprays = (long)cache.Get("FadingSprays");

                anonymousSprays = (long)cache.Get("AnonymousSprays");
                loggedInSprays = (long)cache.Get("LoggedInSprays");

                lastWeekSprays = (List<ThingsOverTime>)cache.Get("LastWeekSprays");

                numUsers = (long)cache.Get("NumUsers");
            }
            else
            {
                using (var db = new SprayContext())
                {
                    totalSprays = db.Sprays.LongCount(s => s.Status != Status.DELETED);

                    anonymousSprays = db.Sprays.LongCount(s => s.Creator == null);
                    loggedInSprays = totalSprays - anonymousSprays;

                    publishedSprays = db.Sprays.LongCount(s => s.Status == Status.PUBLIC);
                    pendingSprays = db.Sprays.LongCount(s => s.Status == Status.UNMODERATED);
                    unlistedSprays = db.Sprays.LongCount(s => s.Status == Status.UNLISTED);
                    privateSprays = totalSprays - publishedSprays - unlistedSprays - anonymousSprays;

                    regularSprays = db.Sprays.LongCount(s => !s.NSFW && !s.Animated && !s.Fading && s.Status != Status.DELETED && s.Creator != null);
                    nsfwSprays = db.Sprays.LongCount(s => s.NSFW && s.Status != Status.DELETED && s.Creator != null);
                    animatedSprays = db.Sprays.LongCount(s => s.Animated && s.Status != Status.DELETED && s.Creator != null);
                    fadingSprays = db.Sprays.LongCount(s => s.Fading && s.Status != Status.DELETED && s.Creator != null);

                    DateTime oldestDate = DateTime.Now.Date.Subtract(TimeSpan.FromDays(30));

                    var a = db.Sprays.Where(s => s.DateAdded >= oldestDate).OrderBy(s => s.DateAdded).ToList();
                    var b = a.GroupBy(x => x.DateAdded.Date);
                    var c = b.Select(y => new ThingsOverTime { Date = y.Key.Date, SprayCount = y.LongCount() });
                    lastWeekSprays = c.ToList();

                    numUsers = db.Users.LongCount();
                }

                lastGenerated = DateTime.Now;

                // Remake the cache
                CacheItemPolicy slidingExpires = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes(10) };

                cache.Add("LastGenerated", lastGenerated, slidingExpires);

                cache.Add("TotalSprays", totalSprays, slidingExpires);
                cache.Add("PublishedSprays", publishedSprays, slidingExpires);
                cache.Add("PendingSprays", pendingSprays, slidingExpires);
                cache.Add("UnlistedSprays", unlistedSprays, slidingExpires);
                cache.Add("PrivateSprays", privateSprays, slidingExpires);

                cache.Add("RegularSprays", regularSprays, slidingExpires);
                cache.Add("NSFWSprays", nsfwSprays, slidingExpires);
                cache.Add("AnimatedSprays", animatedSprays, slidingExpires);
                cache.Add("FadingSprays", fadingSprays, slidingExpires);

                cache.Add("AnonymousSprays", anonymousSprays, slidingExpires);
                cache.Add("LoggedInSprays", loggedInSprays, slidingExpires);

                cache.Add("LastWeekSprays", lastWeekSprays, slidingExpires);

                cache.Add("NumUsers", numUsers, slidingExpires);
            }


            /*
             * Charts
             */

            #region publishedChart
            Highcharts publishedChart = new Highcharts("publishedStatus")
            .InitChart(new Chart { PlotShadow = false })
            .SetTitle(new Title { Text = "Spray Status" })
            .SetSubtitle(new Subtitle { Text = "(Does not include anonymous sprays)" })
            .SetTooltip(new Tooltip { Formatter = "function() { return '<b>'+ this.point.name +'</b>: '+ this.y; }" })
            .SetPlotOptions(new PlotOptions
                {
                    Pie = new PlotOptionsPie
                            {
                                AllowPointSelect = true,
                                Cursor = Cursors.Pointer,
                                DataLabels = new PlotOptionsPieDataLabels
                                            {
                                                Color = ColorTranslator.FromHtml("#000000"),
                                                ConnectorColor = ColorTranslator.FromHtml("#000000"),
                                                Formatter = "function() { return '<b>'+ this.point.name +'</b>: '+ Math.round(this.percentage) +' %'; }"
                                            }
                            }
                })
            .SetCredits(new Credits { Enabled = false })
            .SetSeries(new Series
                {
                    Type = ChartTypes.Pie,
                    Name = "Percent of Each Status",
                    Data = new Data(new object[]
                            {
                                new object[] { "Published", publishedSprays },
                                new object[] { "Pending Moderation", pendingSprays },
                                new object[] { "Unlisted", unlistedSprays },
                                new object[] { "Private", privateSprays }
                            })
                });
            #endregion

            #region creatorChart
            Highcharts anonymousChart = new Highcharts("creatorStatus")
                .InitChart(new Chart { PlotShadow = false })
                .SetTitle(new Title { Text = "Spray Creators" })
                .SetTooltip(new Tooltip { Formatter = "function() { return '<b>'+ this.point.name +'</b>: '+ this.y; }" })
                .SetPlotOptions(new PlotOptions
                {
                    Pie = new PlotOptionsPie
                    {
                        AllowPointSelect = true,
                        Cursor = Cursors.Pointer,
                        DataLabels = new PlotOptionsPieDataLabels
                        {
                            Color = ColorTranslator.FromHtml("#000000"),
                            ConnectorColor = ColorTranslator.FromHtml("#000000"),
                            Formatter = "function() { return '<b>'+ this.point.name +'</b>: '+ Math.round(this.percentage) +' %'; }"
                        }
                    }
                })
                .SetCredits(new Credits { Enabled = false })
                .SetSeries(new Series
                    {
                        Type = ChartTypes.Pie,
                        Data = new Data(new object[]
                                            {
                                                new object[] { "Anonymous", anonymousSprays },
                                                new object[] { "Logged In", loggedInSprays },
                                            })
                    });
            #endregion

            #region sprayTypesChart
            Highcharts sprayTypes = new Highcharts("sprayTypes")
                .InitChart(new Chart { PlotShadow = false })
                .SetTitle(new Title { Text = "Spray Types" })
                .SetXAxis(new XAxis
                    {
                        Categories = new[] { "Animated", "Fading", "NSFW", "(Plain)" },
                        Title = new XAxisTitle { Text = String.Empty }
                    })
                .SetYAxis(new YAxis
                    {
                        Min = 0,
                        Title = new YAxisTitle
                        {
                            Text = "# of Sprays",
                            Align = AxisTitleAligns.High
                        }
                    })
                .SetTooltip(new Tooltip { Formatter = "function() { return '<b>'+ this.point.name +'</b>: '+ this.y; }" })
                .SetPlotOptions(new PlotOptions
                {
                    Bar = new PlotOptionsBar
                    {
                        DataLabels = new PlotOptionsBarDataLabels
                        {
                            Enabled = true,
                        }
                    }
                })
                .SetLegend(new Legend
                {
                    Enabled = false
                })
                .SetCredits(new Credits { Enabled = false })
                .SetSeries(new Series
                    {
                        Type = ChartTypes.Bar,
                        Data = new Data(new object[]
                        {
                            new object[] { "Animated", animatedSprays },
                            new object[] { "Fading", fadingSprays },
                            new object[] { "NSFW", nsfwSprays },
                            new object[] { "(Plain)", regularSprays }
                        })
                    });
            #endregion

            #region createdChart
            var cats = lastWeekSprays.Select(x => x.Date.ToShortDateString()).Take(30).ToList();
            cats.Add("Today (so far)");

            Highcharts weeklySprayChart = new Highcharts("weeklySprays")
                .InitChart(new Chart
                {
                    DefaultSeriesType = ChartTypes.Line,
                    ClassName = "chart"
                })
                .SetTitle(new Title
                {
                    Text = "Sprays Added Over the Last Month",
                    X = -20
                })
                .SetXAxis(new XAxis
                {
                    Categories = cats.ToArray(),
                    Title = new XAxisTitle { Text = "Date" },
                    Labels = new XAxisLabels
                    {
                        Rotation = -45,
                        Step = 7
                    },
                    Max = 31
                })  
                .SetYAxis(new YAxis
                {
                    Title = new YAxisTitle { Text = "# of Sprays Created" },
                    PlotLines = new[]
                                          {
                                              new XAxisPlotLines
                                              {
                                                  Value = 0,
                                                  Width = 1,
                                                  Color = ColorTranslator.FromHtml("#808080")
                                              }
                                          },
                    Min = 0
                })
                .SetTooltip(new Tooltip
                {
                    Formatter = @"function() {
                                        return '<b>'+ this.series.name +'</b><br/>'+
                                    this.x +': '+ this.y;
                                }"
                })
                .SetLegend(new Legend
                {
                    Enabled = false
                })
                .SetCredits(new Credits { Enabled = false })
                .SetSeries(new[]
                           {
                               new Series { Name = "Sprays Created", Data = new Data(lastWeekSprays.Select(x => x.SprayCount).Cast<object>().ToArray()) },
                           }
                );

            #endregion

            Stats stats = new Stats
            {
                AnimatedSprays = animatedSprays,
                AnonymousSprays = anonymousSprays,
                FadingSprays = fadingSprays,
                LastGenerated = lastGenerated,
                LoggedInSprays = loggedInSprays,
                NSFWSprays = nsfwSprays,
                NumUsers = numUsers,
                PendingSprays = pendingSprays,
                PublishedSprays = publishedSprays,
                PrivateSprays = privateSprays,
                TotalSprays = totalSprays,
                UnlistedSprays = unlistedSprays,

                PublishedChart = publishedChart,
                AnonymousChart = anonymousChart,
                SprayTypeChart = sprayTypes,
                WeeklyChart = weeklySprayChart
            };

            return View(stats);
        }
    }
}
