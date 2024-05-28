using Microsoft.VisualStudio.TestTools.UnitTesting;
using Coree.NETStandard.Classes.TimeOfDay;
using System;

using System.Runtime.Versioning;
using System.Runtime.InteropServices;
using Coree.NETStandard.Services.FileManagement;

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
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var result = fileservice?.IsValidLocation(@"C:\temp");
                Assert.AreEqual(result, true);
            }
            else
            {
                Assert.Inconclusive("Test not supported on this OS.");
            }

        }

        [TestMethod]
        public void IsValidLocationruntimes_ReturnsTrue()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var result = fileservice?.IsValidLocation(@"Runtimes");
                Assert.AreEqual(result, true);
            }
            else
            {
                Assert.Inconclusive("Test not supported on this OS.");
            }

        }

        [TestMethod]
        [SupportedOSPlatform("windows")]
        public void TryFixPathCaseing_Check()
        {
            // Act
            //var actual = fileservice?.TryFixPathCaseing(@"c:\USERS\PuBLic");
            //var expected = @"C:\Users\Public";

            //Assert.AreEqual(expected, actual);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Act
                var actual = fileservice?.TryFixPathCaseing(@"c:\USERS\PuBLic");
                var expected = @"C:\Users\Public";

                Assert.AreEqual(expected, actual);
            }
            else
            {
                Assert.Inconclusive("Test not supported on this OS.");
            }
        }
    }
}