using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Script.Serialization;

namespace MediCostAPI.Models
{
    public class ConfigManager
    {
        // Returns DB ConnectionString
        public string getConnectionString()
        {
            string path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            using (StreamReader r = new StreamReader(path + "/medicostconfig.json"))
            {
                string json = r.ReadToEnd();
                var serializer = new JavaScriptSerializer();
                Dictionary<string, string> JsonItem = serializer.Deserialize<Dictionary<string, string>>(json);
                return JsonItem["connectionString"];
            }
        }
    }
}