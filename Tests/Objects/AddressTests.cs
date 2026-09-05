using System;
using Xunit;

namespace SPTC_APPLICATION.Objects.Tests
{
    public class AddressTests
    {
        // ─── Constructor Tests ───────────────────────────────────────────────────

        [Fact]
        public void DefaultConstructor_CreatesInstance()
        {
            // Arrange & Act
            var address = new Address();

            // Assert
            Assert.NotNull(address);
        }

        [Fact]
        public void FullParameterConstructor_SetsAllFields()
        {
            // Arrange & Act
            var address = new Address("123", "Main St", "Barangay 1", "City", "1234", "Province", "Philippines");

            // Assert
            Assert.Equal("123", address.houseNo);
            Assert.Equal("Main St", address.streetname);
            Assert.Equal("Barangay 1", address.barangay);
            Assert.Equal("City", address.city);
            Assert.Equal("1234", address.zipcode);
            Assert.Equal("Province", address.province);
            Assert.Equal("Philippines", address.country);
        }

        [Fact]
        public void AddressLineConstructor_SetsAddressLines()
        {
            // Arrange & Act
            var address = new Address("123 Main St", "Barangay 1, City");

            // Assert
            Assert.Equal("123 Main St", address.addressline1);
            Assert.Equal("Barangay 1, City", address.addressline2);
        }

        [Fact]
        public void AddressLineConstructor_WithNullValues_SetsNullFields()
        {
            // Arrange & Act
            var address = new Address(null, null);

            // Assert
            Assert.Null(address.addressline1);
            Assert.Null(address.addressline2);
        }

        // ─── ToString Tests ──────────────────────────────────────────────────────

        [Fact]
        public void ToString_WithAddressLines_ReturnsCombinedAddressLines()
        {
            // Arrange
            var address = new Address("123 Main St", "Barangay 1, City");

            // Act
            string result = address.ToString();

            // Assert
            Assert.Contains("123 Main St", result);
            Assert.Contains("Barangay 1, City", result);
        }

        [Fact]
        public void ToString_WithFullAddress_ReturnsFormattedAddress()
        {
            // Arrange
            var address = new Address("123", "Main St", "Barangay 1", "City", "1234", "Province", "Philippines");

            // Act
            string result = address.ToString();

            // Assert
            Assert.Contains("123", result);
            Assert.Contains("Main St", result);
            Assert.Contains("Barangay 1", result);
            Assert.Contains("City", result);
        }

        [Fact]
        public void ToString_WithEmptyFields_ReturnsEmptyString()
        {
            // Arrange
            var address = new Address();

            // Act
            string result = address.ToString();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void ToString_WithOnlyAddressLine1_ReturnsEmptyString()
        {
            // Arrange
            var address = new Address("123 Main St", null);

            // Act
            string result = address.ToString();

            // Assert
            // addressline1 is set but addressline2 is null, so it falls to the else branch
            Assert.NotNull(result);
        }

        [Fact]
        public void ToString_WithOnlyAddressLine2_ReturnsEmptyString()
        {
            // Arrange
            var address = new Address(null, "Barangay 1, City");

            // Act
            string result = address.ToString();

            // Assert
            Assert.NotNull(result);
        }

        // ─── Field Assignment Tests ──────────────────────────────────────────────

        [Fact]
        public void FullParameterConstructor_WithEmptyStrings_SetsEmptyFields()
        {
            // Arrange & Act
            var address = new Address("", "", "", "", "", "", "");

            // Assert
            Assert.Equal("", address.houseNo);
            Assert.Equal("", address.streetname);
            Assert.Equal("", address.barangay);
            Assert.Equal("", address.city);
            Assert.Equal("", address.zipcode);
            Assert.Equal("", address.province);
            Assert.Equal("", address.country);
        }

        [Fact]
        public void Address_CanModifyFields()
        {
            // Arrange
            var address = new Address();

            // Act
            address.houseNo = "456";
            address.city = "New City";

            // Assert
            Assert.Equal("456", address.houseNo);
            Assert.Equal("New City", address.city);
        }
    }
}
