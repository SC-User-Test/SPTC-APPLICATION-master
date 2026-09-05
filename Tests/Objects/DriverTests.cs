using System;
using Xunit;

namespace SPTC_APPLICATION.Objects.Tests
{
    public class DriverTests
    {
        // ─── Constructor Tests ───────────────────────────────────────────────────

        [Fact]
        public void DefaultConstructor_CreatesInstance()
        {
            // Arrange & Act
            var driver = new Driver();

            // Assert
            Assert.NotNull(driver);
        }

        [Fact]
        public void DefaultConstructor_HasNullNavigationProperties()
        {
            // Arrange & Act
            var driver = new Driver();

            // Assert
            Assert.Null(driver.name);
            Assert.Null(driver.address);
            Assert.Null(driver.image);
            Assert.Null(driver.signature);
        }

        [Fact]
        public void DefaultConstructor_HasDefaultValues()
        {
            // Arrange & Act
            var driver = new Driver();

            // Assert
            Assert.Null(driver.remarks);
            Assert.Equal(default(DateTime), driver.birthday);
            Assert.Null(driver.emergencyPerson);
            Assert.Null(driver.emergencyContact);
            Assert.False(driver.isDayShift);
        }

        // ─── WriteInto Tests ─────────────────────────────────────────────────────

        [Fact]
        public void WriteInto_SetsAllProperties_ReturnsTrue()
        {
            // Arrange
            var driver = new Driver();
            var name = new Name("", "John", "", "Doe", "");
            var address = new Address("123 Main St", "City");
            var bday = new DateTime(1990, 5, 15);

            // Act
            bool result = driver.WriteInto(name, address, null, null, "No remarks", bday, "Jane Doe", "09123456789", true);

            // Assert
            Assert.True(result);
            Assert.Equal(name, driver.name);
            Assert.Equal(address, driver.address);
            Assert.Equal("No remarks", driver.remarks);
            Assert.Equal(bday, driver.birthday);
            Assert.Equal("Jane Doe", driver.emergencyPerson);
            Assert.Equal("09123456789", driver.emergencyContact);
            Assert.True(driver.isDayShift);
        }

        [Fact]
        public void WriteInto_WithNightShift_SetsDayShiftFalse()
        {
            // Arrange
            var driver = new Driver();
            var bday = new DateTime(1985, 3, 20);

            // Act
            bool result = driver.WriteInto(null, null, null, null, "", bday, "", "", false);

            // Assert
            Assert.True(result);
            Assert.False(driver.isDayShift);
        }

        [Fact]
        public void WriteInto_DefaultIsDay_IsTrue()
        {
            // Arrange
            var driver = new Driver();
            var bday = new DateTime(1985, 3, 20);

            // Act
            bool result = driver.WriteInto(null, null, null, null, "", bday, "", "");

            // Assert
            Assert.True(result);
            Assert.True(driver.isDayShift);
        }

        [Fact]
        public void WriteInto_WithNullName_SetsNullName()
        {
            // Arrange
            var driver = new Driver();
            var bday = new DateTime(1990, 1, 1);

            // Act
            driver.WriteInto(null, null, null, null, "remarks", bday, "person", "contact");

            // Assert
            Assert.Null(driver.name);
        }

        // ─── ToString Tests ──────────────────────────────────────────────────────

        [Fact]
        public void ToString_WithName_ReturnsNameString()
        {
            // Arrange
            var driver = new Driver();
            var name = new Name("", "John", "", "Doe", "");
            driver.name = name;

            // Act
            string result = driver.ToString();

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Doe", result);
        }

        [Fact]
        public void ToString_WithNullName_ReturnsEmptyString()
        {
            // Arrange
            var driver = new Driver();

            // Act
            string result = driver.ToString();

            // Assert
            Assert.Equal("", result);
        }

        // ─── Property Tests ──────────────────────────────────────────────────────

        [Fact]
        public void Driver_IdIsReadOnly_DefaultIsZero()
        {
            // Arrange & Act
            var driver = new Driver();

            // Assert
            Assert.Equal(0, driver.id);
        }

        [Fact]
        public void Driver_CanSetRemarks()
        {
            // Arrange
            var driver = new Driver();

            // Act
            driver.remarks = "Test remarks";

            // Assert
            Assert.Equal("Test remarks", driver.remarks);
        }

        [Fact]
        public void Driver_CanSetBirthday()
        {
            // Arrange
            var driver = new Driver();
            var bday = new DateTime(1995, 8, 25);

            // Act
            driver.birthday = bday;

            // Assert
            Assert.Equal(bday, driver.birthday);
        }

        [Fact]
        public void Driver_CanSetEmergencyInfo()
        {
            // Arrange
            var driver = new Driver();

            // Act
            driver.emergencyPerson = "Emergency Contact";
            driver.emergencyContact = "09987654321";

            // Assert
            Assert.Equal("Emergency Contact", driver.emergencyPerson);
            Assert.Equal("09987654321", driver.emergencyContact);
        }
    }
}
