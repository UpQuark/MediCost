using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Script.Serialization;
using MediCostAPI.Models;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using MediCostAPI.Services;
using System.Globalization;

namespace MediCostAPI.Controllers
{
    public class LocationsController : ApiController
    {
        public string GetLocations()
        {
            var dbAgent = new DbSprocAgent();
            var citiesTable = dbAgent.ExecuteSproc("spGetLocations");

            var locations = new List<City>();

            foreach (DataRow row in citiesTable.Rows)
            {
                var NameCaseFixed = row["City"].ToString().ToLower();
                NameCaseFixed = new CultureInfo("en-US").TextInfo.ToTitleCase(NameCaseFixed);
                locations.Add(new City
                {
                    Name = NameCaseFixed,
                    State = row["State"].ToString()
                });
            }

            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = Int32.MaxValue;
            return serializer.Serialize(locations);
        }
    }
}
