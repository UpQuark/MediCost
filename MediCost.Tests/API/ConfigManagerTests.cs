using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MediCostAPI.Models;

namespace MediCost.Tests
{
    [TestClass]
    public class ConfigManagerTests
    {
        [TestMethod]
        public void TestDbConfigRetrieval()
        {
            var config = new ConfigManager();
            var connString = config.getConnectionString();
        }
    }
}
