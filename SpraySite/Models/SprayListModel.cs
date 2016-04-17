using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpraySite.Models
{
    public class SprayListModel
    {
        public virtual ICollection<Spray> Sprays { get; set; }

        public int Start { get; set; }

        public int Next { get; set; }
        public int Prev { get; set; }
    }
}