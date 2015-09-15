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
    }
}
