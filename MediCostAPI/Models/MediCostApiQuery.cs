using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MediCostAPI.Models
{
    public class MediCostApiQuery
    {
        private string _city;
        private string _specialty;

        public string City
        {
            get
            {
                return System.Uri.UnescapeDataString(_city);
            }
            set
            {
                _city = value;
            }
        }

        public string Specialty 
        {
            get
            {
                return System.Uri.UnescapeDataString(_specialty);
            } 
            set
            {
                _specialty = value;
            }
        }
    }
}