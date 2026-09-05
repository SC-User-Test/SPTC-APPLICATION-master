using System;
using Xunit;

namespace SPTC_APPLICATION.Objects.Tests
{
    public class StringExtensionsTests
    {
        // ─── CountLines Tests ────────────────────────────────────────────────────

        [Fact]
        public void CountLines_WithNullString_ReturnsZero()
        {
            // Arrange
            string? text = null;

            // Act
            int result = text.CountLines();

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void CountLines_WithEmptyString_ReturnsZero()
        {
            // Arrange
            string text = string.Empty;

            // Act
            int result = text.CountLines();

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void CountLines_WithSingleLine_ReturnsOne()
        {
            // Arrange
            string text = "Hello World";

            // Act
            int result = text.CountLines();

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public void CountLines_WithTwoLines_ReturnsTwo()
        {
            // Arrange
            string text = $"Line 1{Environment.NewLine}Line 2";

            // Act
            int result = text.CountLines();

            // Assert
            Assert.Equal(2, result);
        }

        [Fact]
        public void CountLines_WithThreeLines_ReturnsThree()
        {
            // Arrange
            string text = $"Line 1{Environment.NewLine}Line 2{Environment.NewLine}Line 3";

            // Act
            int result = text.CountLines();

            // Assert
            Assert.Equal(3, result);
        }

        [Fact]
        public void CountLines_WithMultipleNewlines_CountsCorrectly()
        {
            // Arrange
            string text = $"A{Environment.NewLine}B{Environment.NewLine}C{Environment.NewLine}D{Environment.NewLine}E";

            // Act
            int result = text.CountLines();

            // Assert
            Assert.Equal(5, result);
        }

        [Fact]
        public void CountLines_WithOnlyNewline_ReturnsTwo()
        {
            // Arrange
            string text = Environment.NewLine;

            // Act
            int result = text.CountLines();

            // Assert
            Assert.Equal(2, result);
        }

        [Fact]
        public void CountLines_WithWhitespaceOnly_ReturnsOne()
        {
            // Arrange
            string text = "   ";

            // Act
            int result = text.CountLines();

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public void CountLines_WithLargeText_CountsCorrectly()
        {
            // Arrange
            var lines = new System.Text.StringBuilder();
            for (int i = 0; i < 100; i++)
            {
                lines.Append($"Line {i}{Environment.NewLine}");
            }
            string text = lines.ToString().TrimEnd();

            // Act
            int result = text.CountLines();

            // Assert
            Assert.Equal(100, result);
        }
    }
}
