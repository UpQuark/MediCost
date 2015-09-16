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
    public class MediCostController : ApiController
    {
        // GET city params
        public String GetMediCost([FromUri] MediCostApiQuery queryModel)
        {
            SqlDataAdapter da = null;
            var officeTable = new DataTable();
            var providerTable = new DataTable();
            var costTable = new DataTable();
            var config = new ConfigManager();

            string connString = config.getConnectionString();

            try
            {
                using (SqlConnection con = new SqlConnection(connString))
                {
                    // StoreProcedure retrieves one line per unique provider NPI
                    using (SqlCommand cmd = new SqlCommand("spGetOfficesByCity", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@city", SqlDbType.VarChar).Value = queryModel.City;
                        //cmd.Parameters.Add("@specialty", SqlDbType.VarChar).Value = queryModel.specialty ?? Convert.DBNull;

                        con.Open();
                        cmd.ExecuteNonQuery();
                        da = new SqlDataAdapter(cmd);

                        // Query DB and fill DataTable
                        da.Fill(officeTable);
                        con.Close();
                    }

                    // StoreProcedure retrieves one line per unique provider NPI
                    using (SqlCommand cmd = new SqlCommand("spGetProvidersByCity", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@city", SqlDbType.VarChar).Value = queryModel.City;
                        cmd.Parameters.Add("@specialty", SqlDbType.VarChar).Value = queryModel.Specialty ?? Convert.DBNull;

                        con.Open();
                        cmd.ExecuteNonQuery();
                        da = new SqlDataAdapter(cmd);

                        // Query DB and fill DataTable
                        da.Fill(providerTable);
                        con.Close();
                    }

                    using (SqlCommand cmd = new SqlCommand("spGetProviderCostsByCity", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@city", SqlDbType.VarChar).Value = queryModel.City;
                        cmd.Parameters.Add("@specialty", SqlDbType.VarChar).Value = queryModel.Specialty ?? Convert.DBNull;

                        con.Open();
                        cmd.ExecuteNonQuery();
                        da = new SqlDataAdapter(cmd);

                        // Query DB and fill DataTable
                        da.Fill(costTable);
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

            var officesList = new List<Office>();
            foreach (DataRow r in officeTable.Rows)
            {
                var office = new Office()
                {
                    AddressID = r.ItemArray[0].ToString(),
                    Street1 = r.ItemArray[1].ToString(),
                    Street2 = r.ItemArray[2].ToString(),
                    City = r.ItemArray[3].ToString(),
                    Zip = r.ItemArray[4].ToString(),
                    State = r.ItemArray[5].ToString(),
                    Country = r.ItemArray[6].ToString(),
                    Latitude = r.ItemArray[8].ToString(),
                    Longitude = r.ItemArray[7].ToString(),
                    Providers = new Dictionary<string, Provider>(),
                };
                officesList.Add(office);
            }

            var providersList = new List<Provider>();
            foreach (DataRow r in providerTable.Rows)
            {
                var provider = new Provider()
                {
                    Npi = r[0].ToString(),
                    AddressID = r[1].ToString(),
                    Street1 = r[2].ToString(),
                    Street2 = r[3].ToString(),
                    City = r[4].ToString(),
                    Zip = r[5].ToString(),
                    State = r[6].ToString(),
                    Country = r[7].ToString(),
                    Latitude = r[9].ToString(),
                    Longitude = r[8].ToString(),
                    LastName = r[10].ToString(),
                    FirstName = r[11].ToString(),
                    MiddleInitial = r[12].ToString(),
                    Credentials = r[13].ToString(),
                    Gender = r[14].ToString(),
                    EntityCode = r[15].ToString(),
                    Specialty = r[16].ToString(),
                    Costs = new Dictionary<string, Cost>(),
                };
                providersList.Add(provider);
            }

            var costsList = new List<Cost>();
            foreach (DataRow r in costTable.Rows)
            {
                var cost = new Cost()
                {
                    Npi = r[0].ToString(),
                    AddressID = r[1].ToString(),
                    MedicareParticipationIndicator = r[2].ToString(),
                    PlaceOfService = r[3].ToString(),
                    hcpcsCode = r[4].ToString(),
                    LineServiceCount = r[5].ToString(),
                    BenefitsUniqueCount = r[6].ToString(),
                    BenefitsDayServiceCount = r[7].ToString(),
                    AvgMedicareAllowedAmount = r[8].ToString(),
                    AvgMedicareAllowedAmountStDev = r[9].ToString(),
                    AvgSubmittedChargeAmount = r[10].ToString(),
                    AvgSubmittedChargeAmountStDev = r[11].ToString(),
                    AvgMedicarePaymentAmount = r[12].ToString(),
                    AvgMedicarePaymentAmountStDev = r[13].ToString(),
                    Street1 = r[14].ToString(),
                    Street2 = r[15].ToString(),
                    City = r[16].ToString(),
                    Zip = r[17].ToString(),
                    State = r[18].ToString(),
                    Country = r[19].ToString(),
                    Latitude = r[21].ToString(),
                    Longitude = r[20].ToString(),
                    hcpcsCode2 = r[22].ToString(),
                    HcpcsDescription = r[23].ToString(),
                    LastName = r[24].ToString(),
                    FirstName = r[25].ToString(),
                    MiddleInitial = r[26].ToString(),
                    Credentials = r[27].ToString(),
                    Gender = r[28].ToString(),
                    EntityCode = r[29].ToString(),
                    Specialty = r[30].ToString(),
                };
                costsList.Add(cost);
            }

            // Create dictionary of offices by ID
            var officeDictionary = new Dictionary<string, Office>();

            officeDictionary = officesList
            .GroupBy(p => p.AddressID, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            foreach (var provider in providersList)
            {
                if (officeDictionary[provider.AddressID] == null)
                    officeDictionary[provider.AddressID] = new Office();
                officeDictionary[provider.AddressID].Providers[provider.Npi] = provider;
            }

            foreach (var cost in costsList)
            {
                officeDictionary[cost.AddressID].Providers[cost.Npi].Costs[cost.hcpcsCode] = cost;
            }

            var keysToRemove = new List<string>();
            foreach (var o in officeDictionary)
            {
                if (o.Value.Providers.Count == 0)
                    keysToRemove.Add(o.Key);
            }

            foreach (var key in keysToRemove)
            {
                officeDictionary.Remove(key);
            }
            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = Int32.MaxValue;
            return serializer.Serialize(officeDictionary);
        }
    }
}
