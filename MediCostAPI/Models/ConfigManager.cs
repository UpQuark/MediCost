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
        public string readConfig()
        {
            using (StreamReader r = new StreamReader("../../../DB/medicostconfig.json"))
            {
                string json = r.ReadToEnd();
                //object jsonRead = JsonConvert.DeserializeObject<object>(json);
                //return "";

                var serializer = new JavaScriptSerializer();
                //serializer.MaxJsonLength = Int32.MaxValue;
                Dictionary<string, string> JsonItem = serializer.Deserialize<Dictionary<string, string>>(json);
                return JsonItem["connectionString"];
            }
        }
    }
}