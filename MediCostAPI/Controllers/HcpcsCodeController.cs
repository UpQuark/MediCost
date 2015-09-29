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
    public class HcpcsCodeController : ApiController
    {
        public string GetHcpcsCode()
        {
            var dbAgent = new DbSprocAgent();
            var hcpcsTable = dbAgent.SendSproc("spGethcpcsCode");

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
