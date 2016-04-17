using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;

namespace SpraySite.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long SteamId { get; set; }
        public string NickName { get; set; }
        public string AvatarURI { get; set; }
        [InverseProperty("Creator")]
        public virtual ICollection<Spray> Sprays { get; set; }
        [InverseProperty("SavedBy")]
        public virtual ICollection<Spray> Saved { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}