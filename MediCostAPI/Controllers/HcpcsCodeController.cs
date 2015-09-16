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
    public class HcpcsCodeController : ApiController
    {
        public string GetHcpcsCode()
        {
            SqlDataAdapter da = null;
            var hcpcsTable = new DataTable();
            var config = new ConfigManager();
            string connString = config.getConnectionString();

            try
            {
                using (SqlConnection con = new SqlConnection(connString))
                {
                    // StoreProcedure retrieves one line per unique provider NPI
                    using (SqlCommand cmd = new SqlCommand("spGethcpcsCode", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        con.Open();
                        cmd.ExecuteNonQuery();
                        da = new SqlDataAdapter(cmd);
                        // Query DB and fill DataTable
                        da.Fill(hcpcsTable);
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

            var hcpcs = new Dictionary<string, HcpcsCode>();

            foreach (DataRow row in hcpcsTable.Rows)
            {
                var hcpcsCode = new HcpcsCode
                {
                    Code = row["hcpcs_code"].ToString(),
                    Description = row["hcpcs_description"].ToString()
                };
                hcpcs.Add(row["hcpcs_code"].ToString(), hcpcsCode);
            }

            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = Int32.MaxValue;
            return serializer.Serialize(hcpcs);
        }
    }
}
