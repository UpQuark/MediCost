using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Globalization;

namespace MediCostAPI.Services
{
    public class DataSanitizerAgent
    {
        public string StringToTitleCase(string input)
        {
            var cultureInfo = new CultureInfo("en-US");
            return cultureInfo.TextInfo.ToTitleCase(input.ToLower());
        }
    }
}