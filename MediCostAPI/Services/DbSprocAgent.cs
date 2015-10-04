using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Script.Serialization;
using MediCostAPI.Models;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace MediCostAPI.Services
{
    public class DbSprocAgent
    {
        /// <summary>
        /// Executes a stored procedure on the application database
        /// </summary>
        /// <param name="name">Name of stored procedure to execute</param>
        /// <returns>DataTable of results from stored procedure</returns>
        public DataTable ExecuteSproc(string name)
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
                        da.Fill(table);
                        con.Close();
                    }
                }
            }
            catch (Exception exp)
            {
                var errorTemplate = "MediCost db error {0}. Application threw exception while executing StoredProcedure '{1}'";
                System.Diagnostics.EventLog.WriteEntry("Application", string.Format(errorTemplate, exp.Message, name));

                if (da != null)
                    da.Dispose();

                throw (exp);
            }

            return table;
        }

        /// <summary>
        /// Executes a stored procedure on the application database with SQL parameters
        /// </summary>
        /// <param name="name">Name of stored procedure to execute</param>
        /// <param name="args">Dictionary of parameter names / values</param>
        /// <returns>DataTable of results from stored procedure</returns>
        public DataTable ExecuteSproc(string name, Dictionary<string, string> args)
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

                        foreach (KeyValuePair<string, string> pair in args)
                        {
                            cmd.Parameters.Add(pair.Key, SqlDbType.VarChar).Value = pair.Value ?? Convert.DBNull;
                        }

                        con.Open();
                        cmd.ExecuteNonQuery();
                        da = new SqlDataAdapter(cmd);
                        da.Fill(table);
                        con.Close();
                    }
                }
            }
            catch (Exception exp)
            {
                var errorTemplate = "MediCost db error {0}. Application threw exception while executing StoredProcedure '{1}'";
                System.Diagnostics.EventLog.WriteEntry("Application", string.Format(errorTemplate, exp.Message, name));

                if (da != null)
                    da.Dispose();

                throw (exp);
            }
            return table;
        }
    }
}