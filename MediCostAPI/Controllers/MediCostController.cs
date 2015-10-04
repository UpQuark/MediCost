using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Script.Serialization;
using MediCostAPI.Models;
using MediCostAPI.Services;
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
            var dataSanitizer = new DataSanitizerAgent();
            var dbAgent = new DbSprocAgent();

            string connString = config.getConnectionString();

            try
            {
                officeTable = dbAgent.ExecuteSproc("spGetOfficesByCity", new Dictionary<string, string>
                {
                    { "@city", queryModel.City}
                });

                providerTable = dbAgent.ExecuteSproc("spGetProvidersByCity", new Dictionary<string, string>
                {
                    { "@city", queryModel.City},
                    { "@specialty", queryModel.Specialty}
                });

                costTable = dbAgent.ExecuteSproc("spGetProviderCostsByCity", new Dictionary<string, string>
                {
                    { "@city", queryModel.City},
                    { "@specialty", queryModel.Specialty}
                });
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
                    AddressID = r["AddressID"].ToString(),
                    Street1 = dataSanitizer.StringToTitleCase(r["nppes_provider_street1"].ToString()),
                    Street2 = dataSanitizer.StringToTitleCase(r["nppes_provider_street2"].ToString()),
                    City = dataSanitizer.StringToTitleCase(r["nppes_provider_city"].ToString()),
                    Zip = r["nppes_provider_zip"].ToString(),
                    State = r["nppes_provider_state"].ToString(),
                    Country = r["nppes_provider_country"].ToString(),
                    Latitude = r["lng"].ToString(), //lat and lng are switched at DB level. Temporary fix here.
                    Longitude = r["lat"].ToString(),
                    Providers = new Dictionary<string, Provider>(),
                };
                officesList.Add(office);
            }

            var providersList = new List<Provider>();
            foreach (DataRow r in providerTable.Rows)
            {
                var provider = new Provider()
                {
                    Npi = r["npi"].ToString(),
                    AddressID = r["AddressID"].ToString(),
                    Street1 = dataSanitizer.StringToTitleCase(r["nppes_provider_street1"].ToString()),
                    Street2 = dataSanitizer.StringToTitleCase(r["nppes_provider_street2"].ToString()),
                    City = dataSanitizer.StringToTitleCase(r["nppes_provider_city"].ToString()),
                    Zip = r["nppes_provider_zip"].ToString(),
                    State = r["nppes_provider_state"].ToString(),
                    Country = r["nppes_provider_country"].ToString(),
                    Latitude = r["lng"].ToString(),
                    Longitude = r["lat"].ToString(),
                    LastName = dataSanitizer.StringToTitleCase(r["nppes_provider_last_org_name"].ToString()),
                    FirstName = dataSanitizer.StringToTitleCase(r["nppes_provider_first_name"].ToString()),
                    MiddleInitial = r["nppes_provider_mi"].ToString(),
                    Credentials = r["nppes_credentials"].ToString(),
                    Gender = r["nppes_provider_gender"].ToString(),
                    EntityCode = r["nppes_entity_code"].ToString(),
                    Specialty = r["provider_type"].ToString(),
                    Costs = new Dictionary<string, Cost>(),
                };
                providersList.Add(provider);
            }

            var costsList = new List<Cost>();
            foreach (DataRow r in costTable.Rows)
            {
                var cost = new Cost()
                {
                    Npi = r["npi"].ToString(),
                    AddressID = r["AddressID"].ToString(),
                    MedicareParticipationIndicator = r["medicare_participation_indicator"].ToString(),
                    PlaceOfService = r["place_of_service"].ToString(),
                    hcpcsCode = r["hcpcs_code"].ToString(),
                    LineServiceCount = r["line_srvc_cnt"].ToString(),
                    BenefitsUniqueCount = r["bene_unique_cnt"].ToString(),
                    BenefitsDayServiceCount = r["bene_day_srvc_cnt"].ToString(),
                    AvgMedicareAllowedAmount = r["average_Medicare_allowed_amt"].ToString(),
                    AvgMedicareAllowedAmountStDev = r["stdev_Medicare_allowed_amt"].ToString(),
                    AvgSubmittedChargeAmount = r["average_submitted_chrg_amt"].ToString(),
                    AvgSubmittedChargeAmountStDev = r["stdev_submitted_chrg_amt"].ToString(),
                    AvgMedicarePaymentAmount = r["average_Medicare_payment_amt"].ToString(),
                    AvgMedicarePaymentAmountStDev = r["stdev_Medicare_payment_amt"].ToString(),
                    Street1 = dataSanitizer.StringToTitleCase(r["nppes_provider_street1"].ToString()),
                    Street2 = dataSanitizer.StringToTitleCase(r["nppes_provider_street2"].ToString()),
                    City = dataSanitizer.StringToTitleCase(r["nppes_provider_city"].ToString()),
                    Zip = r["nppes_provider_zip"].ToString(),
                    State = r["nppes_provider_state"].ToString(),
                    Country = r["nppes_provider_country"].ToString(),
                    Latitude = r["lng"].ToString(),
                    Longitude = r["lat"].ToString(),
                    hcpcsCode2 = r[22].ToString(), //TODO: redundant, test whether it can be removed
                    HcpcsDescription = r["hcpcs_description"].ToString(),
                    LastName = dataSanitizer.StringToTitleCase(r["nppes_provider_last_org_name"].ToString()),
                    FirstName = dataSanitizer.StringToTitleCase(r["nppes_provider_first_name"].ToString()),
                    MiddleInitial = r["nppes_provider_mi"].ToString(),
                    Credentials = r["nppes_credentials"].ToString(),
                    Gender = r["nppes_provider_gender"].ToString(),
                    EntityCode = r["nppes_entity_code"].ToString(),
                    Specialty = dataSanitizer.StringToTitleCase(r["provider_type"].ToString()),
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
