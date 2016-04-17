using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace SpraySite.Models
{
    public class Spray
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }
        public string Url { get; set; }
        public string PreviewImage { get; set; }
        public string PreviewImageNear { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime? DateExpires { get; set; }

        [Obsolete]
        public bool NSFW { get; set; }
        public bool Fading { get; set; }
        public bool Animated { get; set; }
        public int Saves { get; set; }

        // This property will be mapped to the DB...
        public int StatusValue { get; set; }

        // ...but we use this one in code
        public Status Status
        {
            get { return (Status)StatusValue; }
            set { StatusValue = (int)value; }
        }

        [InverseProperty("Sprays")]
        public virtual User Creator { get; set; }

        [InverseProperty("Saved")]
        public virtual ICollection<User> SavedBy { get; set; }

        // This property will be mapped to the DB...
        public int SafenessValue { get; set; }
        // ...but we use this one in code
        public Safeness Safeness
        {
            get { return (Safeness)SafenessValue; }
            set { SafenessValue = (int)value; }
        }
    }

    public enum Status
    {
        ACTIVE,
        UNLISTED,
        UNMODERATED,
        PUBLIC,
        DELETED
    }

    public enum Safeness
    {
        SFW,
        SKETCHY,
        NSFW
    }
}
