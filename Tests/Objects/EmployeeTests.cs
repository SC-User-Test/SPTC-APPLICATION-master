using System;
using Xunit;

namespace SPTC_APPLICATION.Objects.Tests
{
    public class EmployeeTests
    {
        // ─── Constructor Tests ───────────────────────────────────────────────────

        [Fact]
        public void DefaultConstructor_CreatesInstance()
        {
            // Arrange & Act
            var employee = new Employee();

            // Assert
            Assert.NotNull(employee);
        }

        [Fact]
        public void DefaultConstructor_HasNullNavigationProperties()
        {
            // Arrange & Act
            var employee = new Employee();

            // Assert
            Assert.Null(employee.name);
            Assert.Null(employee.address);
            Assert.Null(employee.image);
            Assert.Null(employee.position);
        }

        [Fact]
        public void DefaultConstructor_HasDefaultValues()
        {
            // Arrange & Act
            var employee = new Employee();

            // Assert
            Assert.Equal(0, employee.id);
            Assert.Equal(default(DateTime), employee.startDate);
            Assert.Equal(default(DateTime), employee.endDate);
            Assert.Equal(default(DateTime), employee.birthday);
            Assert.Null(employee.contactNo);
        }

        // ─── Property Tests ──────────────────────────────────────────────────────

        [Fact]
        public void Employee_CanSetId()
        {
            // Arrange
            var employee = new Employee();

            // Act
            employee.id = 42;

            // Assert
            Assert.Equal(42, employee.id);
        }

        [Fact]
        public void Employee_CanSetStartDate()
        {
            // Arrange
            var employee = new Employee();
            var date = new DateTime(2020, 1, 1);

            // Act
            employee.startDate = date;

            // Assert
            Assert.Equal(date, employee.startDate);
        }

        [Fact]
        public void Employee_CanSetEndDate()
        {
            // Arrange
            var employee = new Employee();
            var date = new DateTime(2024, 12, 31);

            // Act
            employee.endDate = date;

            // Assert
            Assert.Equal(date, employee.endDate);
        }

        [Fact]
        public void Employee_CanSetBirthday()
        {
            // Arrange
            var employee = new Employee();
            var bday = new DateTime(1990, 6, 15);

            // Act
            employee.birthday = bday;

            // Assert
            Assert.Equal(bday, employee.birthday);
        }

        [Fact]
        public void Employee_CanSetContactNo()
        {
            // Arrange
            var employee = new Employee();

            // Act
            employee.contactNo = "09123456789";

            // Assert
            Assert.Equal("09123456789", employee.contactNo);
        }

        [Fact]
        public void Employee_CanSetName()
        {
            // Arrange
            var employee = new Employee();
            var name = new Name("", "Alice", "", "Johnson", "");

            // Act
            employee.name = name;

            // Assert
            Assert.Equal(name, employee.name);
        }

        [Fact]
        public void Employee_CanSetAddress()
        {
            // Arrange
            var employee = new Employee();
            var address = new Address("789 Pine St", "District");

            // Act
            employee.address = address;

            // Assert
            Assert.Equal(address, employee.address);
        }

        [Fact]
        public void Employee_CanSetPosition()
        {
            // Arrange
            var employee = new Employee();
            var position = new Position("Manager", true, true, false);

            // Act
            employee.position = position;

            // Assert
            Assert.Equal(position, employee.position);
        }

        [Fact]
        public void Employee_CanSetAllPropertiesAtOnce()
        {
            // Arrange
            var employee = new Employee();
            var name = new Name("", "Bob", "C", "Brown", "");
            var address = new Address("100 Elm St", "Village");
            var position = new Position("Treasurer", true, true, true);
            var startDate = new DateTime(2021, 3, 1);
            var endDate = new DateTime(2025, 3, 1);
            var bday = new DateTime(1988, 11, 20);

            // Act
            employee.id = 10;
            employee.name = name;
            employee.address = address;
            employee.position = position;
            employee.startDate = startDate;
            employee.endDate = endDate;
            employee.birthday = bday;
            employee.contactNo = "09555666777";

            // Assert
            Assert.Equal(10, employee.id);
            Assert.Equal(name, employee.name);
            Assert.Equal(address, employee.address);
            Assert.Equal(position, employee.position);
            Assert.Equal(startDate, employee.startDate);
            Assert.Equal(endDate, employee.endDate);
            Assert.Equal(bday, employee.birthday);
            Assert.Equal("09555666777", employee.contactNo);
        }
    }
}
