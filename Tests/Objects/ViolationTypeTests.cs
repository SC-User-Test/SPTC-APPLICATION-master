using System;
using Xunit;

namespace SPTC_APPLICATION.Objects.Tests
{
    public class ViolationTypeTests
    {
        // ─── Constructor Tests ───────────────────────────────────────────────────

        [Fact]
        public void DefaultConstructor_CreatesInstance()
        {
            // Arrange & Act
            var vt = new ViolationType();

            // Assert
            Assert.NotNull(vt);
        }

        [Fact]
        public void ParameterizedConstructor_SetsAllFields()
        {
            // Arrange & Act
            var vt = new ViolationType("Speeding", "Exceeding speed limit", 7, true);

            // Assert
            Assert.Equal("Speeding", vt.title);
            Assert.Equal("Exceeding speed limit", vt.details);
            Assert.Equal(7, vt.numOfDays);
            Assert.True(vt.isForDriver);
        }

        [Fact]
        public void ParameterizedConstructor_WithFalseIsForDriver_SetsCorrectly()
        {
            // Arrange & Act
            var vt = new ViolationType("Overloading", "Exceeding passenger limit", 3, false);

            // Assert
            Assert.Equal("Overloading", vt.title);
            Assert.Equal("Exceeding passenger limit", vt.details);
            Assert.Equal(3, vt.numOfDays);
            Assert.False(vt.isForDriver);
        }

        [Fact]
        public void ParameterizedConstructor_WithZeroDays_SetsZero()
        {
            // Arrange & Act
            var vt = new ViolationType("Warning", "Minor infraction", 0, true);

            // Assert
            Assert.Equal(0, vt.numOfDays);
        }

        // ─── ToString Tests ──────────────────────────────────────────────────────

        [Fact]
        public void ToString_ReturnsTitle()
        {
            // Arrange
            var vt = new ViolationType("Speeding", "Exceeding speed limit", 7, true);

            // Act
            string result = vt.ToString();

            // Assert
            Assert.Equal("Speeding", result);
        }

        [Fact]
        public void ToString_WithNullTitle_ReturnsEmptyString()
        {
            // Arrange
            var vt = new ViolationType();
            vt.title = null;

            // Act
            string result = vt.ToString();

            // Assert
            Assert.Equal(string.Empty, result);
        }

        // ─── Property Tests ──────────────────────────────────────────────────────

        [Fact]
        public void ViolationType_CanModifyTitle()
        {
            // Arrange
            var vt = new ViolationType("Old Title", "Details", 5, true);

            // Act
            vt.title = "New Title";

            // Assert
            Assert.Equal("New Title", vt.title);
        }

        [Fact]
        public void ViolationType_CanModifyNumOfDays()
        {
            // Arrange
            var vt = new ViolationType("Title", "Details", 5, true);

            // Act
            vt.numOfDays = 10;

            // Assert
            Assert.Equal(10, vt.numOfDays);
        }

        [Fact]
        public void ViolationType_IdIsReadOnly_DefaultIsZero()
        {
            // Arrange & Act
            var vt = new ViolationType();

            // Assert
            Assert.Equal(0, vt.id);
        }
    }
}
