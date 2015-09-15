using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MediCostAPI.Models
{
    public class Specialty
    {
        public List<string> SpecialtyAlternateNames { get; set; }
        public int Id { get; set; }
        public string SpecialtyName { get; set; }
    }
}