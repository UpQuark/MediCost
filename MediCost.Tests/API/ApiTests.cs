using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MediCostAPI;
using MediCostAPI.Controllers;

namespace MediCost.Tests
{
    [TestClass]
    public class ApiTests
    {
        [TestMethod]
        public void TestSpecialtiesApiCall()
        {
            var specialtiesController = new SpecialtiesController();
            var response = specialtiesController.GetSpecialties();
            Assert.IsTrue(response.ToString().Contains("Cardiology"));
        }

        [TestMethod]
        public void TesthcpcsApiCall()
        {
            var hcpcsController = new HcpcsCodeController();
            var response = hcpcsController.GetHcpcsCode();
            Assert.IsTrue(response.ToString().Contains("Anesth salivary gland"));
        }
    }
}
