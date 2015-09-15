using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MediCostAPI.Models
{
    public class Provider
    {
        public String Npi { get; set; }
        public String AddressID { get; set; }
        public String Street1 { get; set; }
        public String Street2 { get; set; }
        public String City { get; set; }
        public String Zip { get; set; }
        public String State { get; set; }
        public String Country { get; set; }
        public String Latitude { get; set; }
        public String Longitude { get; set; }
        public String LastName { get; set; }
        public String FirstName { get; set; }
        public String MiddleInitial { get; set; }
        public String Credentials { get; set; }
        public String Gender { get; set; }
        public String EntityCode { get; set; }
        public String Specialty { get; set; }

        // Key is HCPS code
        public Dictionary<string, Cost> Costs { get; set; }
    }
}