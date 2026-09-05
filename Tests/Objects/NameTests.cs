using System;
using System.Collections.Generic;
using Xunit;

namespace SPTC_APPLICATION.Objects.Tests
{
    public class NameTests
    {
        // ─── Constructor Tests ───────────────────────────────────────────────────

        [Fact]
        public void DefaultConstructor_CreatesInstance()
        {
            // Arrange & Act
            var name = new Name();

            // Assert
            Assert.NotNull(name);
        }

        [Fact]
        public void ParameterizedConstructor_SetsAllFields()
        {
            // Arrange & Act
            var name = new Name("Mr.", "John", "Michael", "Doe", "Jr.");

            // Assert
            Assert.Equal("Mr.", name.prefix);
            Assert.Equal("John", name.firstname);
            Assert.Equal("Michael", name.middlename);
            Assert.Equal("Doe", name.lastname);
            Assert.Equal("Jr.", name.suffix);
        }

        [Fact]
        public void ParameterizedConstructor_WithEmptyStrings_SetsEmptyFields()
        {
            // Arrange & Act
            var name = new Name("", "", "", "", "");

            // Assert
            Assert.Equal("", name.prefix);
            Assert.Equal("", name.firstname);
            Assert.Equal("", name.middlename);
            Assert.Equal("", name.lastname);
            Assert.Equal("", name.suffix);
        }

        // ─── WholeName Property Tests ────────────────────────────────────────────

        [Fact]
        public void WholeName_WithMiddleName_ReturnsFormattedNameWithInitials()
        {
            // Arrange
            var name = new Name("Mr.", "John", "Michael", "Doe", "Jr.");

            // Act
            string result = name.wholename;

            // Assert
            Assert.Contains("Doe", result);
            Assert.Contains("John", result);
            Assert.Contains("M.", result);
        }

        [Fact]
        public void WholeName_WithoutMiddleName_ReturnsFormattedNameWithoutInitials()
        {
            // Arrange
            var name = new Name("Mr.", "John", "", "Doe", "Jr.");

            // Act
            string result = name.wholename;

            // Assert
            Assert.Contains("Doe", result);
            Assert.Contains("John", result);
            Assert.DoesNotContain(".", result.Replace("Jr.", ""));
        }

        [Fact]
        public void WholeName_WithMultipleMiddleNameParts_ReturnsMultipleInitials()
        {
            // Arrange
            var name = new Name("", "Jane", "Anne Marie", "Smith", "");

            // Act
            string result = name.wholename;

            // Assert
            Assert.Contains("Smith", result);
            Assert.Contains("Jane", result);
            Assert.Contains("A", result);
            Assert.Contains("M", result);
        }

        [Fact]
        public void WholeName_WithNullMiddleName_ReturnsNameWithoutInitials()
        {
            // Arrange
            var name = new Name("", "John", null, "Doe", "");

            // Act
            string result = name.wholename;

            // Assert
            Assert.Contains("Doe", result);
            Assert.Contains("John", result);
        }

        [Fact]
        public void WholeName_WithSuffix_IncludesSuffix()
        {
            // Arrange
            var name = new Name("", "John", "", "Doe", "III");

            // Act
            string result = name.wholename;

            // Assert
            Assert.Contains("III", result);
        }

        // ─── ToString Tests ──────────────────────────────────────────────────────

        [Fact]
        public void ToString_ReturnsWholeName()
        {
            // Arrange
            var name = new Name("", "John", "M", "Doe", "");

            // Act
            string result = name.ToString();

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Doe", result);
        }

        [Fact]
        public void ToString_WithNullFields_ReturnsEmptyOrTrimmedString()
        {
            // Arrange
            var name = new Name("", "", "", "", "");

            // Act
            string result = name.ToString();

            // Assert
            Assert.NotNull(result);
        }
    }
}
