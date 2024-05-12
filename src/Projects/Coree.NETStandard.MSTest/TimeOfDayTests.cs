using Microsoft.VisualStudio.TestTools.UnitTesting;
using Coree.NETStandard.Classes.TimeOfDay;
using System;

#pragma warning disable

namespace Coree.NETStandard.MSTest
{
    [TestClass]
    public class TimeOfDayTests
    {
        [TestMethod]
        public void Constructor_ValidTime_SetsCorrectValues()
        {
            // Arrange & Act
            var timeOfDay = new TimeOfDay(13, 30, 15, 500);

            // Assert
            Assert.AreEqual(13, timeOfDay.Hour);
            Assert.AreEqual(30, timeOfDay.Minute);
            Assert.AreEqual(15, timeOfDay.Second);
            Assert.AreEqual(500, timeOfDay.Millisecond);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_InvalidHour_ThrowsArgumentOutOfRangeException()
        {
            // Arrange & Act & Assert
            var timeOfDay = new TimeOfDay(24, 0, 0, 0);
        }

        [TestMethod]
        public void ToTimeSpan_WhenCalled_ReturnsCorrectTimeSpan()
        {
            // Arrange
            var expectedTimeSpan = new TimeSpan(0, 13, 30, 15, 500);
            var timeOfDay = new TimeOfDay(13, 30, 15, 500);

            // Act
            var result = timeOfDay.ToTimeSpan();

            // Assert
            Assert.AreEqual(expectedTimeSpan, result);
        }

        [TestMethod]
        public void AddHours_AddValidHours_WrapsAroundMidnight()
        {
            // Arrange
            var timeOfDay = new TimeOfDay(23, 30, 0, 0);

            // Act
            var newTimeOfDay = timeOfDay.AddHours(2); // Should wrap around to 01:30:00

            // Assert
            Assert.AreEqual(1, newTimeOfDay.Hour);
            Assert.AreEqual(30, newTimeOfDay.Minute);
        }

        [TestMethod]
        public void FromTimeString_ValidFormat_ReturnsCorrectTimeOfDay()
        {
            // Arrange
            var expectedTime = new TimeOfDay(10, 20, 30);

            // Act
            var result = TimeOfDay.FromTimeString("10:20:30");

            // Assert
            Assert.AreEqual(expectedTime.Hour, result.Hour);
            Assert.AreEqual(expectedTime.Minute, result.Minute);
            Assert.AreEqual(expectedTime.Second, result.Second);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void FromTimeString_InvalidFormat_ThrowsFormatException()
        {
            // Arrange & Act & Assert
            var result = TimeOfDay.FromTimeString("invalid-time");
        }
    }
}

#pragma warning restore