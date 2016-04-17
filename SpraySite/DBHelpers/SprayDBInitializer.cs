using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace SpraySite.DBHelpers
{
    public class SprayDBInitializer : CreateDatabaseIfNotExists<SprayContext>
    {

    }
}