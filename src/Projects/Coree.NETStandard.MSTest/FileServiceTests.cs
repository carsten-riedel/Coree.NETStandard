using Microsoft.VisualStudio.TestTools.UnitTesting;
using Coree.NETStandard.Classes.TimeOfDay;
using System;
using Coree.NETStandard.Services.FileService;
using System.Runtime.Versioning;

namespace Coree.NETStandard.MSTest
{
    [TestClass]
    public class FileServiceTests
    {
        private FileService? fileservice;

        [TestInitialize]
        public void TestInitialize()
        {
            fileservice = new FileService();
        }


        [TestMethod]
        public void IsValidLocation_ReturnsTrue()
        {
            var result = fileservice?.IsValidLocation(@"C:\temp");
            Assert.AreEqual(result, true);
        }

        [TestMethod]
        public void IsValidLocationruntimes_ReturnsTrue()
        {
            var result = fileservice?.IsValidLocation(@"Runtimes");
            Assert.AreEqual(result, true);
        }

        [TestMethod]
        [SupportedOSPlatform("windows")]
        public void TryFixPathCaseing_Check()
        {
            // Act
            var actual = fileservice?.TryFixPathCaseing(@"c:\USERS\PuBLic");
            var expected = @"C:\Users\Public";

            Assert.AreEqual(expected, actual);
        }
    }
}