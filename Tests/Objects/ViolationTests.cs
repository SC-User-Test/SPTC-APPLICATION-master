using System;
using Xunit;

namespace SPTC_APPLICATION.Objects.Tests
{
    public class ViolationTests
    {
        // ─── Constructor Tests ───────────────────────────────────────────────────

        [Fact]
        public void DefaultConstructor_CreatesInstance()
        {
            // Arrange & Act
            var violation = new Violation();

            // Assert
            Assert.NotNull(violation);
        }

        [Fact]
        public void DefaultConstructor_HasDefaultValues()
        {
            // Arrange & Act
            var violation = new Violation();

            // Assert
            Assert.Equal(0, violation.franchiseId);
            Assert.Equal(0, violation.violationLevelCount);
            Assert.Equal(0, violation.violationTypeId);
            Assert.Equal(default(DateTime), violation.violationDate);
            Assert.Null(violation.suspensionStart);
            Assert.Null(violation.suspensionEnd);
            Assert.Null(violation.remarks);
            Assert.Equal(0, violation.nameId);
            Assert.False(violation.isDeleted);
        }

        // ─── Property Assignment Tests ───────────────────────────────────────────

        [Fact]
        public void Violation_CanSetFranchiseId()
        {
            // Arrange
            var violation = new Violation();

            // Act
            violation.franchiseId = 42;

            // Assert
            Assert.Equal(42, violation.franchiseId);
        }

        [Fact]
        public void Violation_CanSetViolationLevelCount()
        {
            // Arrange
            var violation = new Violation();

            // Act
            violation.violationLevelCount = 3;

            // Assert
            Assert.Equal(3, violation.violationLevelCount);
        }

        [Fact]
        public void Violation_CanSetViolationTypeId()
        {
            // Arrange
            var violation = new Violation();

            // Act
            violation.violationTypeId = 5;

            // Assert
            Assert.Equal(5, violation.violationTypeId);
        }

        [Fact]
        public void Violation_CanSetViolationDate()
        {
            // Arrange
            var violation = new Violation();
            var date = new DateTime(2024, 1, 15);

            // Act
            violation.violationDate = date;

            // Assert
            Assert.Equal(date, violation.violationDate);
        }

        [Fact]
        public void Violation_CanSetSuspensionStart()
        {
            // Arrange
            var violation = new Violation();
            var date = new DateTime(2024, 2, 1);

            // Act
            violation.suspensionStart = date;

            // Assert
            Assert.Equal(date, violation.suspensionStart);
        }

        [Fact]
        public void Violation_CanSetSuspensionEnd()
        {
            // Arrange
            var violation = new Violation();
            var date = new DateTime(2024, 3, 1);

            // Act
            violation.suspensionEnd = date;

            // Assert
            Assert.Equal(date, violation.suspensionEnd);
        }

        [Fact]
        public void Violation_CanSetNullSuspensionDates()
        {
            // Arrange
            var violation = new Violation();

            // Act
            violation.suspensionStart = null;
            violation.suspensionEnd = null;

            // Assert
            Assert.Null(violation.suspensionStart);
            Assert.Null(violation.suspensionEnd);
        }

        [Fact]
        public void Violation_CanSetRemarks()
        {
            // Arrange
            var violation = new Violation();

            // Act
            violation.remarks = "Test remark";

            // Assert
            Assert.Equal("Test remark", violation.remarks);
        }

        [Fact]
        public void Violation_CanSetNameId()
        {
            // Arrange
            var violation = new Violation();

            // Act
            violation.nameId = 10;

            // Assert
            Assert.Equal(10, violation.nameId);
        }

        [Fact]
        public void Violation_CanSetIsDeleted()
        {
            // Arrange
            var violation = new Violation();

            // Act
            violation.isDeleted = true;

            // Assert
            Assert.True(violation.isDeleted);
        }

        [Fact]
        public void Violation_IdIsReadOnly_DefaultIsZero()
        {
            // Arrange & Act
            var violation = new Violation();

            // Assert
            Assert.Equal(0, violation.id);
        }

        [Fact]
        public void Violation_CanSetAllPropertiesAtOnce()
        {
            // Arrange
            var violation = new Violation();
            var violationDate = new DateTime(2024, 6, 15);
            var suspStart = new DateTime(2024, 6, 20);
            var suspEnd = new DateTime(2024, 7, 20);

            // Act
            violation.franchiseId = 1;
            violation.violationLevelCount = 2;
            violation.violationTypeId = 3;
            violation.violationDate = violationDate;
            violation.suspensionStart = suspStart;
            violation.suspensionEnd = suspEnd;
            violation.remarks = "Speeding";
            violation.nameId = 5;
            violation.isDeleted = false;

            // Assert
            Assert.Equal(1, violation.franchiseId);
            Assert.Equal(2, violation.violationLevelCount);
            Assert.Equal(3, violation.violationTypeId);
            Assert.Equal(violationDate, violation.violationDate);
            Assert.Equal(suspStart, violation.suspensionStart);
            Assert.Equal(suspEnd, violation.suspensionEnd);
            Assert.Equal("Speeding", violation.remarks);
            Assert.Equal(5, violation.nameId);
            Assert.False(violation.isDeleted);
        }
    }
}
