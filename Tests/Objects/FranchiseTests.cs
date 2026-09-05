using System;
using Xunit;

namespace SPTC_APPLICATION.Objects.Tests
{
    public class FranchiseTests
    {
        // ─── Constructor Tests ───────────────────────────────────────────────────

        [Fact]
        public void DefaultConstructor_CreatesInstance()
        {
            // Arrange & Act
            var franchise = new Franchise();

            // Assert
            Assert.NotNull(franchise);
        }

        [Fact]
        public void DefaultConstructor_HasNullNavigationProperties()
        {
            // Arrange & Act
            var franchise = new Franchise();

            // Assert
            Assert.Null(franchise.Operator);
            Assert.Null(franchise.Driver_day);
            Assert.Null(franchise.Driver_night);
            Assert.Null(franchise.owner);
            Assert.Null(franchise.lastFranchiseId);
        }

        [Fact]
        public void DefaultConstructor_HasNullStringProperties()
        {
            // Arrange & Act
            var franchise = new Franchise();

            // Assert
            Assert.Null(franchise.bodynumber);
            Assert.Null(franchise.licenceNO);
        }

        // ─── WriteInto Tests ─────────────────────────────────────────────────────

        [Fact]
        public void WriteInto_SetsAllProperties_ReturnsTrue()
        {
            // Arrange
            var franchise = new Franchise();
            var op = new Operator();
            var driverDay = new Driver();
            var driverNight = new Driver();

            // Act
            bool result = franchise.WriteInto("BN-001", op, driverDay, driverNight, "LIC-12345");

            // Assert
            Assert.True(result);
            Assert.Equal("BN-001", franchise.bodynumber);
            Assert.Equal(op, franchise.Operator);
            Assert.Equal(driverDay, franchise.Driver_day);
            Assert.Equal(driverNight, franchise.Driver_night);
            Assert.Equal("LIC-12345", franchise.licenceNO);
        }

        [Fact]
        public void WriteInto_WithNullDrivers_SetsNullDrivers()
        {
            // Arrange
            var franchise = new Franchise();

            // Act
            bool result = franchise.WriteInto("BN-002", null, null, null, "LIC-99999");

            // Assert
            Assert.True(result);
            Assert.Null(franchise.Operator);
            Assert.Null(franchise.Driver_day);
            Assert.Null(franchise.Driver_night);
        }

        [Fact]
        public void WriteInto_WithEmptyBodyNumber_SetsEmptyBodyNumber()
        {
            // Arrange
            var franchise = new Franchise();

            // Act
            bool result = franchise.WriteInto("", null, null, null, "");

            // Assert
            Assert.True(result);
            Assert.Equal("", franchise.bodynumber);
        }

        // ─── ToString Tests ──────────────────────────────────────────────────────

        [Fact]
        public void ToString_WithBodyNumber_ReturnsBodyNumber()
        {
            // Arrange
            var franchise = new Franchise();
            franchise.bodynumber = "BN-001";

            // Act
            string result = franchise.ToString();

            // Assert
            Assert.Equal("BN-001", result);
        }

        [Fact]
        public void ToString_WithNullBodyNumber_ReturnsEmptyString()
        {
            // Arrange
            var franchise = new Franchise();

            // Act
            string result = franchise.ToString();

            // Assert
            Assert.Equal("", result);
        }

        // ─── Property Tests ──────────────────────────────────────────────────────

        [Fact]
        public void Franchise_IdIsReadOnly_DefaultIsZero()
        {
            // Arrange & Act
            var franchise = new Franchise();

            // Assert
            Assert.Equal(0, franchise.id);
        }

        [Fact]
        public void Franchise_CanSetBodyNumber()
        {
            // Arrange
            var franchise = new Franchise();

            // Act
            franchise.bodynumber = "BN-100";

            // Assert
            Assert.Equal("BN-100", franchise.bodynumber);
        }

        [Fact]
        public void Franchise_CanSetLicenceNo()
        {
            // Arrange
            var franchise = new Franchise();

            // Act
            franchise.licenceNO = "LIC-2024-001";

            // Assert
            Assert.Equal("LIC-2024-001", franchise.licenceNO);
        }

        [Fact]
        public void Franchise_CanSetOperator()
        {
            // Arrange
            var franchise = new Franchise();
            var op = new Operator();

            // Act
            franchise.Operator = op;

            // Assert
            Assert.Equal(op, franchise.Operator);
        }

        [Fact]
        public void Franchise_CanSetDriverDay()
        {
            // Arrange
            var franchise = new Franchise();
            var driver = new Driver();

            // Act
            franchise.Driver_day = driver;

            // Assert
            Assert.Equal(driver, franchise.Driver_day);
        }

        [Fact]
        public void Franchise_CanSetDriverNight()
        {
            // Arrange
            var franchise = new Franchise();
            var driver = new Driver();

            // Act
            franchise.Driver_night = driver;

            // Assert
            Assert.Equal(driver, franchise.Driver_night);
        }
    }
}
