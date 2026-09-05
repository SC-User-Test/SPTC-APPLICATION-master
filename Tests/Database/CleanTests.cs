using System;
using Xunit;
using SPTC_APPLICATION.Database;

namespace SPTC_APPLICATION.Database.Tests
{
    public class CleanTests
    {
        // ─── Constructor Tests ───────────────────────────────────────────────────

        [Fact]
        public void Constructor_WithTableName_CreatesInstance()
        {
            // Arrange & Act
            var clean = new Clean(Table.NAME);

            // Assert
            Assert.NotNull(clean);
        }

        [Fact]
        public void Constructor_WithFranchiseTable_CreatesInstance()
        {
            // Arrange & Act
            var clean = new Clean(Table.FRANCHISE);

            // Assert
            Assert.NotNull(clean);
        }

        [Fact]
        public void Constructor_WithDriverTable_CreatesInstance()
        {
            // Arrange & Act
            var clean = new Clean(Table.DRIVER);

            // Assert
            Assert.NotNull(clean);
        }

        [Fact]
        public void Constructor_WithOperatorTable_CreatesInstance()
        {
            // Arrange & Act
            var clean = new Clean(Table.OPERATOR);

            // Assert
            Assert.NotNull(clean);
        }

        [Fact]
        public void Constructor_WithViolationTable_CreatesInstance()
        {
            // Arrange & Act
            var clean = new Clean(Table.VIOLATION);

            // Assert
            Assert.NotNull(clean);
        }

        // ─── Start Tests ─────────────────────────────────────────────────────────

        [Fact]
        public void Start_WhenNotAdmin_ReturnsFalse()
        {
            // Arrange
            AppState.IS_ADMIN = false;
            var clean = new Clean(Table.NAME);

            // Act
            bool result = clean.Start();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Start_WhenNotAdmin_DoesNotThrow()
        {
            // Arrange
            AppState.IS_ADMIN = false;
            var clean = new Clean(Table.FRANCHISE);

            // Act & Assert
            var exception = Record.Exception(() => clean.Start());
            Assert.Null(exception);
        }

        [Fact]
        public void Start_WhenNotAdmin_ReturnsFalseForAllTables()
        {
            // Arrange
            AppState.IS_ADMIN = false;
            string[] tables = {
                Table.NAME, Table.ADDRESS, Table.FRANCHISE,
                Table.DRIVER, Table.OPERATOR, Table.EMPLOYEE,
                Table.VIOLATION, Table.VIOLATION_TYPE
            };

            foreach (var table in tables)
            {
                var clean = new Clean(table);

                // Act
                bool result = clean.Start();

                // Assert
                Assert.False(result);
            }
        }
    }
}
