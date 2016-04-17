using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNet.Highcharts;

namespace SpraySite.Models
{
    public class Stats
    {
        public DateTime LastGenerated { get; set; }
        public long TotalSprays { get; set; }
        public long PublishedSprays { get; set; }
        public long PendingSprays { get; set; }
        public long UnlistedSprays { get; set; }
        public long PrivateSprays { get; set; }
        public long NSFWSprays { get; set; }
        public long AnimatedSprays { get; set; }
        public long FadingSprays { get; set; }
        public long AnonymousSprays { get; set; }
        public long LoggedInSprays { get; set; }
        public long NumUsers { get; set; }

        public Highcharts PublishedChart { get; set; }
        public Highcharts AnonymousChart { get; set; }
        public Highcharts SprayTypeChart { get; set; }
        public Highcharts WeeklyChart { get; set; }
    }
}