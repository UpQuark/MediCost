using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MediCostAPI.Models
{
    public class Office
    {
        public String Street1 { get; set; }
        public String Street2 { get; set; }
        public String City { get; set; }
        public String Zip { get; set; }
        public String State { get; set; }
        public String Country { get; set; }
        public String AddressID { get; set; }
        public String Latitude { get; set; }
        public String Longitude { get; set; }

        // Key is NPI
        public Dictionary<string, Provider> Providers { get; set; }
    }
}