using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace SpraySite.Models
{
    public class SprayViewModel
    {
        public Spray Spray { get; set; }
        public bool SavedByCurrent { get; set; }
        public bool IsCurrentUsersSpray { get; set; }
    }
}
