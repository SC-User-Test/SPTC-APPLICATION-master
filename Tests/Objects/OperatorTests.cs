using System;
using Xunit;

namespace SPTC_APPLICATION.Objects.Tests
{
    public class OperatorTests
    {
        // ─── Constructor Tests ───────────────────────────────────────────────────

        [Fact]
        public void DefaultConstructor_CreatesInstance()
        {
            // Arrange & Act
            var op = new Operator();

            // Assert
            Assert.NotNull(op);
        }

        [Fact]
        public void DefaultConstructor_HasNullNavigationProperties()
        {
            // Arrange & Act
            var op = new Operator();

            // Assert
            Assert.Null(op.name);
            Assert.Null(op.address);
            Assert.Null(op.image);
            Assert.Null(op.signature);
        }

        [Fact]
        public void DefaultConstructor_HasDefaultValues()
        {
            // Arrange & Act
            var op = new Operator();

            // Assert
            Assert.Null(op.remarks);
            Assert.Equal(default(DateTime), op.birthday);
            Assert.Null(op.emergencyPerson);
            Assert.Null(op.emergencyContact);
        }

        // ─── WriteInto Tests ─────────────────────────────────────────────────────

        [Fact]
        public void WriteInto_SetsAllProperties_ReturnsTrue()
        {
            // Arrange
            var op = new Operator();
            var name = new Name("", "Jane", "", "Smith", "");
            var address = new Address("456 Oak Ave", "Town");
            var bday = new DateTime(1985, 7, 20);

            // Act
            bool result = op.WriteInto(name, address, null, null, "Some remarks", bday, "John Smith", "09111222333");

            // Assert
            Assert.True(result);
            Assert.Equal(name, op.name);
            Assert.Equal(address, op.address);
            Assert.Equal("Some remarks", op.remarks);
            Assert.Equal(bday, op.birthday);
            Assert.Equal("John Smith", op.emergencyPerson);
            Assert.Equal("09111222333", op.emergencyContact);
        }

        [Fact]
        public void WriteInto_WithNullValues_SetsNullProperties()
        {
            // Arrange
            var op = new Operator();
            var bday = new DateTime(1990, 1, 1);

            // Act
            bool result = op.WriteInto(null, null, null, null, null, bday, null, null);

            // Assert
            Assert.True(result);
            Assert.Null(op.name);
            Assert.Null(op.address);
            Assert.Null(op.remarks);
            Assert.Null(op.emergencyPerson);
            Assert.Null(op.emergencyContact);
        }

        // ─── ToString Tests ──────────────────────────────────────────────────────

        [Fact]
        public void ToString_WithName_ReturnsNameString()
        {
            // Arrange
            var op = new Operator();
            var name = new Name("", "Jane", "", "Smith", "");
            op.name = name;

            // Act
            string result = op.ToString();

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Smith", result);
        }

        [Fact]
        public void ToString_WithNullName_ReturnsEmptyString()
        {
            // Arrange
            var op = new Operator();

            // Act
            string result = op.ToString();

            // Assert
            Assert.Equal("", result);
        }

        // ─── Property Tests ──────────────────────────────────────────────────────

        [Fact]
        public void Operator_IdIsReadOnly_DefaultIsZero()
        {
            // Arrange & Act
            var op = new Operator();

            // Assert
            Assert.Equal(0, op.id);
        }

        [Fact]
        public void Operator_CanSetRemarks()
        {
            // Arrange
            var op = new Operator();

            // Act
            op.remarks = "Test remarks";

            // Assert
            Assert.Equal("Test remarks", op.remarks);
        }

        [Fact]
        public void Operator_CanSetBirthday()
        {
            // Arrange
            var op = new Operator();
            var bday = new DateTime(1980, 12, 31);

            // Act
            op.birthday = bday;

            // Assert
            Assert.Equal(bday, op.birthday);
        }

        [Fact]
        public void Operator_CanSetEmergencyInfo()
        {
            // Arrange
            var op = new Operator();

            // Act
            op.emergencyPerson = "Emergency Person";
            op.emergencyContact = "09876543210";

            // Assert
            Assert.Equal("Emergency Person", op.emergencyPerson);
            Assert.Equal("09876543210", op.emergencyContact);
        }
    }
}
