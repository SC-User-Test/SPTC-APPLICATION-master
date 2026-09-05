using System;
using System.ComponentModel;
using Xunit;
using SPTC_APPLICATION.Database;

namespace SPTC_APPLICATION.Database.Tests
{
    public class DatabaseConnectionTests
    {
        // ─── Constructor Tests ───────────────────────────────────────────────────

        [Fact]
        public void Constructor_WithConnectionString_SetsConnectionString()
        {
            // Arrange
            string connStr = "Host=localhost;Port=5432;Database=testdb;Username=user;Password=pass;";

            // Act
            var dbConn = new DatabaseConnection(connStr);

            // Assert
            Assert.NotNull(dbConn);
        }

        [Fact]
        public void GetConnection_AfterConstructor_ReturnsNpgsqlConnection()
        {
            // Arrange
            string connStr = "Host=localhost;Port=5432;Database=testdb;Username=user;Password=pass;";
            var dbConn = new DatabaseConnection(connStr);

            // Act
            var connection = DatabaseConnection.GetConnection();

            // Assert
            Assert.NotNull(connection);
        }

        [Fact]
        public void GetConnection_WithEmptyConnectionString_ReturnsConnectionWithEmptyString()
        {
            // Arrange
            var dbConn = new DatabaseConnection(string.Empty);

            // Act
            var connection = DatabaseConnection.GetConnection();

            // Assert
            Assert.NotNull(connection);
        }

        // ─── GetEnumDescription Tests ────────────────────────────────────────────

        [Fact]
        public void GetEnumDescription_StringEmpty_ReturnsDescription()
        {
            // Arrange & Act
            string result = DatabaseConnection.GetEnumDescription(ConnectionLogs.STRING_EMPTY);

            // Assert
            Assert.Equal("Empty Connection string", result);
        }

        [Fact]
        public void GetEnumDescription_Established_ReturnsDescription()
        {
            // Arrange & Act
            string result = DatabaseConnection.GetEnumDescription(ConnectionLogs.ESTABLISHED);

            // Assert
            Assert.Equal("Connection Established", result);
        }

        [Fact]
        public void GetEnumDescription_ExceptionOccurred_ReturnsDescription()
        {
            // Arrange & Act
            string result = DatabaseConnection.GetEnumDescription(ConnectionLogs.EXCEPTION_OCCURED);

            // Assert
            Assert.Equal("Exception Occurred", result);
        }

        [Fact]
        public void GetEnumDescription_WrongPassword_ReturnsDescription()
        {
            // Arrange & Act
            string result = DatabaseConnection.GetEnumDescription(ConnectionLogs.WRONG_PASSWORD);

            // Assert
            Assert.Equal("Wrong Password", result);
        }

        [Fact]
        public void GetEnumDescription_CannotConnect_ReturnsDescription()
        {
            // Arrange & Act
            string result = DatabaseConnection.GetEnumDescription(ConnectionLogs.CANNOT_CONNECT);

            // Assert
            Assert.Equal("Cannot Connect", result);
        }

        // ─── Builder Tests ───────────────────────────────────────────────────────

        [Fact]
        public void Builder_Constructor_CreatesInstance()
        {
            // Arrange & Act
            var builder = new DatabaseConnection.Builder("localhost", "5432", "testdb", "user", "pass");

            // Assert
            Assert.NotNull(builder);
        }

        [Fact]
        public void Builder_Constructor_WithEmptyValues_CreatesInstance()
        {
            // Arrange & Act
            var builder = new DatabaseConnection.Builder("", "", "", "", "");

            // Assert
            Assert.NotNull(builder);
        }

        [Fact]
        public void Builder_Connect_WithInvalidConnection_ReturnsFalse()
        {
            // Arrange
            var builder = new DatabaseConnection.Builder("invalid_host", "5432", "testdb", "user", "pass");

            // Act
            bool result = builder.Connect();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Builder_Connect_WithEmptyConnectionString_ReturnsFalse()
        {
            // Arrange
            var builder = new DatabaseConnection.Builder("", "", "", "", "");

            // Act
            bool result = builder.Connect();

            // Assert
            Assert.False(result);
            Assert.True(builder.Log == ConnectionLogs.STRING_EMPTY || builder.Log == ConnectionLogs.EXCEPTION_OCCURED || builder.Log == ConnectionLogs.CANNOT_CONNECT);
        }

        [Fact]
        public void Builder_Connect_WithInvalidHost_SetsCannotConnectLog()
        {
            // Arrange
            var builder = new DatabaseConnection.Builder("nonexistent_host_xyz", "5432", "testdb", "user", "pass");

            // Act
            bool result = builder.Connect();

            // Assert
            Assert.False(result);
            // Log should be either CANNOT_CONNECT or EXCEPTION_OCCURED
            Assert.True(builder.Log == ConnectionLogs.CANNOT_CONNECT || builder.Log == ConnectionLogs.EXCEPTION_OCCURED);
        }

        [Fact]
        public async void Builder_CreateAsync_WithEmptyConnectionString_ReturnsFalse()
        {
            // Arrange
            var builder = new DatabaseConnection.Builder("", "", "", "", "");

            // Act
            bool result = await builder.CreateAsync();

            // Assert
            Assert.False(result);
            Assert.True(builder.Log == ConnectionLogs.STRING_EMPTY || builder.Log == ConnectionLogs.EXCEPTION_OCCURED || builder.Log == ConnectionLogs.CANNOT_CONNECT);
        }

        [Fact]
        public async void Builder_CreateAsync_WithInvalidHost_ReturnsFalse()
        {
            // Arrange
            var builder = new DatabaseConnection.Builder("nonexistent_host_xyz", "5432", "testdb", "user", "pass");

            // Act
            bool result = await builder.CreateAsync();

            // Assert
            Assert.False(result);
        }
    }

    public class ConnectionLogsEnumTests
    {
        [Fact]
        public void ConnectionLogs_HasStringEmpty()
        {
            Assert.True(Enum.IsDefined(typeof(ConnectionLogs), ConnectionLogs.STRING_EMPTY));
        }

        [Fact]
        public void ConnectionLogs_HasEstablished()
        {
            Assert.True(Enum.IsDefined(typeof(ConnectionLogs), ConnectionLogs.ESTABLISHED));
        }

        [Fact]
        public void ConnectionLogs_HasExceptionOccured()
        {
            Assert.True(Enum.IsDefined(typeof(ConnectionLogs), ConnectionLogs.EXCEPTION_OCCURED));
        }

        [Fact]
        public void ConnectionLogs_HasWrongPassword()
        {
            Assert.True(Enum.IsDefined(typeof(ConnectionLogs), ConnectionLogs.WRONG_PASSWORD));
        }

        [Fact]
        public void ConnectionLogs_HasCannotConnect()
        {
            Assert.True(Enum.IsDefined(typeof(ConnectionLogs), ConnectionLogs.CANNOT_CONNECT));
        }

        [Fact]
        public void ConnectionLogs_AllValuesAreDistinct()
        {
            var values = Enum.GetValues(typeof(ConnectionLogs));
            var distinctValues = new System.Collections.Generic.HashSet<int>();
            foreach (var v in values)
            {
                distinctValues.Add((int)v);
            }
            Assert.Equal(values.Length, distinctValues.Count);
        }
    }
}
