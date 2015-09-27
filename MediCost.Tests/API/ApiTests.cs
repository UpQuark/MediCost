using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MediCostAPI;
using MediCostAPI.Controllers;
using MediCostAPI.Models;

namespace MediCost.Tests
{
    [TestClass]
    public class ApiTests
    {
        [TestMethod]
        public void TestMediCostApiCall()
        {
            var medicostController = new MediCostController();
            var response = medicostController.GetMediCost(new MediCostApiQuery{City = "Boston", Specialty = "Cardiology"});
        }

        [TestMethod]
        public void TestSpecialtiesApiCall()
        {
            var specialtiesController = new SpecialtiesController();
            var response = specialtiesController.GetSpecialties();
            Assert.IsTrue(response.ToString().Contains("Cardiology"));
        }

        [TestMethod]
        public void TestHcpcsApiCall()
        {
            var hcpcsController = new HcpcsCodeController();
            var response = hcpcsController.GetHcpcsCode();
            Assert.IsTrue(response.ToString().Contains("Anesth salivary gland"));
        }
    }
}
