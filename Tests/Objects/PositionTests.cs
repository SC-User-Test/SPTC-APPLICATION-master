using System;
using Xunit;

namespace SPTC_APPLICATION.Objects.Tests
{
    public class PositionTests
    {
        // ─── Constructor Tests ───────────────────────────────────────────────────

        [Fact]
        public void DefaultConstructor_CreatesInstance()
        {
            // Arrange & Act
            var position = new Position();

            // Assert
            Assert.NotNull(position);
        }

        [Fact]
        public void ParameterizedConstructor_SetsAllFields()
        {
            // Arrange & Act
            var position = new Position("Manager", true, true, false);

            // Assert
            Assert.Equal("Manager", position.title);
            Assert.True(position.canCreate);
            Assert.True(position.canEdit);
            Assert.False(position.canDelete);
        }

        [Fact]
        public void ParameterizedConstructor_AllPermissionsTrue()
        {
            // Arrange & Act
            var position = new Position("Admin", true, true, true);

            // Assert
            Assert.True(position.canCreate);
            Assert.True(position.canEdit);
            Assert.True(position.canDelete);
        }

        [Fact]
        public void ParameterizedConstructor_AllPermissionsFalse()
        {
            // Arrange & Act
            var position = new Position("Viewer", false, false, false);

            // Assert
            Assert.False(position.canCreate);
            Assert.False(position.canEdit);
            Assert.False(position.canDelete);
        }

        // ─── ToString Tests ──────────────────────────────────────────────────────

        [Fact]
        public void ToString_ReturnsTitle()
        {
            // Arrange
            var position = new Position("Secretary", true, false, false);

            // Act
            string result = position.ToString();

            // Assert
            Assert.Equal("Secretary", result);
        }

        [Fact]
        public void ToString_WithNullTitle_ReturnsEmptyString()
        {
            // Arrange
            var position = new Position();
            position.title = null;

            // Act
            string result = position.ToString();

            // Assert
            Assert.Equal(string.Empty, result);
        }

        // ─── Property Tests ──────────────────────────────────────────────────────

        [Fact]
        public void Position_CanModifyTitle()
        {
            // Arrange
            var position = new Position("Old Title", true, true, true);

            // Act
            position.title = "New Title";

            // Assert
            Assert.Equal("New Title", position.title);
        }

        [Fact]
        public void Position_CanModifyPermissions()
        {
            // Arrange
            var position = new Position("Title", false, false, false);

            // Act
            position.canCreate = true;
            position.canEdit = true;
            position.canDelete = true;

            // Assert
            Assert.True(position.canCreate);
            Assert.True(position.canEdit);
            Assert.True(position.canDelete);
        }

        [Fact]
        public void Position_IdIsReadOnly_DefaultIsZero()
        {
            // Arrange & Act
            var position = new Position();

            // Assert
            Assert.Equal(0, position.id);
        }
    }
}
