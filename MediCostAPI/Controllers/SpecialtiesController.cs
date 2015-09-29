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

namespace MediCostAPI.Controllers
{
    public class SpecialtiesController : ApiController
    {
        public string GetSpecialties()
        {
            var dbAgent = new DbSprocAgent();
            var specialtiesTable = dbAgent.SendSproc("spGetSpecialties");

            var specialties = new Dictionary<string, Specialty>();

            foreach (DataRow row in specialtiesTable.Rows)
            {
                if (!specialties.ContainsKey(row["specialtyName"].ToString()))
                {
                    var specialty = new Specialty
                    {
                        Id = -1,//int.Parse(row["specialtyId"].ToString()),
                        SpecialtyName = row["specialtyName"].ToString(),
                        SpecialtyAlternateNames = new List<string>()
                        //{
                        //    row["specialtyAlternateName"].ToString()
                        //}
                    };
                    specialties.Add(row["specialtyName"].ToString(), specialty);
                }
                else
                {
                    specialties[row["specialtyName"].ToString()].SpecialtyAlternateNames.Add(row["specialtyAlternateName"].ToString());
                }
            }

            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = Int32.MaxValue;
            return serializer.Serialize(specialties);
        }
    }
}
