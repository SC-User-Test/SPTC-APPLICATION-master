using System;
using Xunit;
using SPTC_APPLICATION.Database;

namespace SPTC_APPLICATION.Database.Tests
{
    public class UpsertTests
    {
        // ─── Constructor Tests ───────────────────────────────────────────────────

        [Fact]
        public void Constructor_WithNewRecord_CreatesInstance()
        {
            // Arrange & Act
            // id = -1 means new record (no DB call)
            var upsert = new Upsert(Table.NAME, -1);

            // Assert
            Assert.NotNull(upsert);
        }

        [Fact]
        public void Constructor_WithNewRecord_SetsIdToNegativeOne()
        {
            // Arrange & Act
            var upsert = new Upsert(Table.NAME, -1);

            // Assert
            Assert.Equal(-1, upsert.id);
        }

        // ─── Insert Tests ────────────────────────────────────────────────────────

        [Fact]
        public void Insert_WithStringValue_StoresValue()
        {
            // Arrange
            var upsert = new Upsert(Table.NAME, -1);

            // Act
            upsert.Insert("first_name", "John");

            // Assert
            var result = upsert.Access("first_name");
            Assert.Equal("John", result);
        }

        [Fact]
        public void Insert_WithIntValue_StoresValue()
        {
            // Arrange
            var upsert = new Upsert(Table.NAME, -1);

            // Act
            upsert.Insert("id", 42);

            // Assert
            var result = upsert.Access("id");
            Assert.Equal(42, result);
        }

        [Fact]
        public void Insert_WithBoolValue_StoresValue()
        {
            // Arrange
            var upsert = new Upsert(Table.NAME, -1);

            // Act
            upsert.Insert("isDeleted", true);

            // Assert
            var result = upsert.Access("isDeleted");
            Assert.Equal(true, result);
        }

        [Fact]
        public void Insert_WithDoubleValue_StoresValue()
        {
            // Arrange
            var upsert = new Upsert(Table.LOAN, -1);

            // Act
            upsert.Insert("amount", 50000.50);

            // Assert
            var result = upsert.Access("amount");
            Assert.Equal(50000.50, result);
        }

        [Fact]
        public void Insert_WithNullValue_StoresNull()
        {
            // Arrange
            var upsert = new Upsert(Table.NAME, -1);

            // Act
            upsert.Insert("remarks", null);

            // Assert
            var result = upsert.Access("remarks");
            Assert.Null(result);
        }

        [Fact]
        public void Insert_OverwritesExistingValue()
        {
            // Arrange
            var upsert = new Upsert(Table.NAME, -1);
            upsert.Insert("first_name", "John");

            // Act
            upsert.Insert("first_name", "Jane");

            // Assert
            var result = upsert.Access("first_name");
            Assert.Equal("Jane", result);
        }

        [Fact]
        public void Insert_MultipleFields_StoresAll()
        {
            // Arrange
            var upsert = new Upsert(Table.NAME, -1);

            // Act
            upsert.Insert("first_name", "John");
            upsert.Insert("last_name", "Doe");
            upsert.Insert("middle_name", "M");

            // Assert
            Assert.Equal("John", upsert.Access("first_name"));
            Assert.Equal("Doe", upsert.Access("last_name"));
            Assert.Equal("M", upsert.Access("middle_name"));
        }

        // ─── Access Tests ────────────────────────────────────────────────────────

        [Fact]
        public void Access_WithNonExistentKey_ReturnsNull()
        {
            // Arrange
            var upsert = new Upsert(Table.NAME, -1);

            // Act
            var result = upsert.Access("nonexistent_field");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Access_WithExistingKey_ReturnsValue()
        {
            // Arrange
            var upsert = new Upsert(Table.NAME, -1);
            upsert.Insert("prefix", "Mr.");

            // Act
            var result = upsert.Access("prefix");

            // Assert
            Assert.Equal("Mr.", result);
        }

        [Fact]
        public void Access_WithDateTimeValue_ReturnsDateTime()
        {
            // Arrange
            var upsert = new Upsert(Table.DRIVER, -1);
            var date = new DateTime(1990, 5, 15);
            upsert.Insert("date_of_birth", date);

            // Act
            var result = upsert.Access("date_of_birth");

            // Assert
            Assert.Equal(date, result);
        }

        [Fact]
        public void Access_WithByteArrayValue_ReturnsByteArray()
        {
            // Arrange
            var upsert = new Upsert(Table.IMAGE, -1);
            byte[] imageData = new byte[] { 1, 2, 3, 4, 5 };
            upsert.Insert("image_source_bin", imageData);

            // Act
            var result = upsert.Access("image_source_bin");

            // Assert
            Assert.Equal(imageData, result);
        }

        // ─── Id Property Tests ───────────────────────────────────────────────────

        [Fact]
        public void Id_CanBeSetDirectly()
        {
            // Arrange
            var upsert = new Upsert(Table.NAME, -1);

            // Act
            upsert.id = 100;

            // Assert
            Assert.Equal(100, upsert.id);
        }

        [Fact]
        public void Constructor_WithDifferentTableNames_CreatesInstances()
        {
            // Arrange & Act
            var upsert1 = new Upsert(Table.NAME, -1);
            var upsert2 = new Upsert(Table.ADDRESS, -1);
            var upsert3 = new Upsert(Table.FRANCHISE, -1);

            // Assert
            Assert.NotNull(upsert1);
            Assert.NotNull(upsert2);
            Assert.NotNull(upsert3);
        }
    }
}
