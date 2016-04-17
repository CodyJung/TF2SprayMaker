using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using SpraySite.Models;

namespace SpraySite.DBHelpers
{
    public class SprayContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Spray> Sprays { get; set; }
    }
}