using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Script.Serialization;
using MediCostAPI.Models;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;


namespace MediCostAPI.Services
{
    public class DbSprocAgent
    {
        public DataTable SendSproc(string name)
        {
            SqlDataAdapter da = null;
            var table = new DataTable();
            var config = new ConfigManager();
            string connString = config.getConnectionString();

            try
            {
                using (SqlConnection con = new SqlConnection(connString))
                {
                    using (SqlCommand cmd = new SqlCommand(name, con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        con.Open();
                        cmd.ExecuteNonQuery();
                        da = new SqlDataAdapter(cmd);
                        // Query DB and fill DataTable
                        da.Fill(table);
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

            return table;
        }
    }
}