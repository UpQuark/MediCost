using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Script.Serialization;
using MediCostAPI.Models;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace MediCostAPI.Controllers
{
    public class HcpsCodeController : ApiController
    {
        public string GetHcpsCode()
        {
            SqlDataAdapter da = null;
            var specialtiesTable = new DataTable();

            // Connection string lives in web.config
            //string connString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            string connString = "Data Source=poppy.arvixe.com;Database=MediCost;User Id=sennis;Password=necratog5data;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False";

            try
            {
                using (SqlConnection con = new SqlConnection(connString))
                {
                    // StoreProcedure retrieves one line per unique provider NPI
                    using (SqlCommand cmd = new SqlCommand("spGetSpecialties", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        con.Open();
                        cmd.ExecuteNonQuery();
                        da = new SqlDataAdapter(cmd);
                        // Query DB and fill DataTable
                        da.Fill(specialtiesTable);
                        con.Close();
                    }
                }
            }
            catch (Exception exp)
            {
                System.Diagnostics.EventLog.WriteEntry("Application", string.Format("MediCost db error {0}", exp.Message));

                if (da != null)
                    da.Dispose();

                throw (exp);
            }

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
