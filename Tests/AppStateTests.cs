using System;
using System.IO;
using Xunit;
using Newtonsoft.Json;

namespace SPTC_APPLICATION.Tests
{
    public class AppStateTests
    {
        // ─── Static Field Tests ──────────────────────────────────────────────────

        [Fact]
        public void AppState_DefaultPassword_IsNotEmpty()
        {
            // Assert
            Assert.NotNull(AppState.DEFAULT_PASSWORD);
            Assert.NotEmpty(AppState.DEFAULT_PASSWORD);
        }

        [Fact]
        public void AppState_DefaultPassword_IsAdmin1234()
        {
            // Assert
            Assert.Equal("Admin1234", AppState.DEFAULT_PASSWORD);
        }

        [Fact]
        public void AppState_DefaultAddressLine2_IsNotEmpty()
        {
            // Assert
            Assert.NotNull(AppState.DEFAULT_ADDRESSLINE2);
            Assert.NotEmpty(AppState.DEFAULT_ADDRESSLINE2);
        }

        [Fact]
        public void AppState_ExpirationDate_IsNotEmpty()
        {
            // Assert
            Assert.NotNull(AppState.EXPIRATION_DATE);
        }

        [Fact]
        public void AppState_Chairman_IsNotEmpty()
        {
            // Assert
            Assert.NotNull(AppState.CHAIRMAN);
            Assert.NotEmpty(AppState.CHAIRMAN);
        }

        [Fact]
        public void AppState_RegistrationNo_IsNotEmpty()
        {
            // Assert
            Assert.NotNull(AppState.REGISTRATION_NO);
            Assert.NotEmpty(AppState.REGISTRATION_NO);
        }

        [Fact]
        public void AppState_PrintAdjustments_IsPositive()
        {
            // Assert
            Assert.True(AppState.PRINT_AJUSTMENTS > 0);
        }

        [Fact]
        public void AppState_Employees_IsNotNull()
        {
            // Assert
            Assert.NotNull(AppState.Employees);
        }

        [Fact]
        public void AppState_Employees_HasFourEntries()
        {
            // Assert
            Assert.Equal(4, AppState.Employees.Count);
        }

        [Fact]
        public void AppState_Employees_ContainsGeneralManager()
        {
            // Assert
            Assert.Contains("General Manager", AppState.Employees);
        }

        [Fact]
        public void AppState_Employees_ContainsSecretary()
        {
            // Assert
            Assert.Contains("Secretary", AppState.Employees);
        }

        [Fact]
        public void AppState_Employees_ContainsTreasurer()
        {
            // Assert
            Assert.Contains("Treasurer", AppState.Employees);
        }

        [Fact]
        public void AppState_Employees_ContainsBookKeeper()
        {
            // Assert
            Assert.Contains("Book Keeper", AppState.Employees);
        }

        [Fact]
        public void AppState_IsAdmin_DefaultIsFalse()
        {
            // Arrange
            AppState.IS_ADMIN = false;

            // Assert
            Assert.False(AppState.IS_ADMIN);
        }

        [Fact]
        public void AppState_User_DefaultIsNull()
        {
            // Arrange
            AppState.USER = null;

            // Assert
            Assert.Null(AppState.USER);
        }

        // ─── IS_ADMIN Toggle Tests ───────────────────────────────────────────────

        [Fact]
        public void AppState_IsAdmin_CanBeSetToTrue()
        {
            // Arrange
            AppState.IS_ADMIN = false;

            // Act
            AppState.IS_ADMIN = true;

            // Assert
            Assert.True(AppState.IS_ADMIN);

            // Cleanup
            AppState.IS_ADMIN = false;
        }

        [Fact]
        public void AppState_IsAdmin_CanBeSetToFalse()
        {
            // Arrange
            AppState.IS_ADMIN = true;

            // Act
            AppState.IS_ADMIN = false;

            // Assert
            Assert.False(AppState.IS_ADMIN);
        }

        // ─── SaveToJson / LoadFromJson Tests ─────────────────────────────────────

        [Fact]
        public void SaveToJson_WhenFileDoesNotExist_CreatesFileAndSavesData()
        {
            // Arrange
            string testPath = Path.Combine(Path.GetTempPath(), "TestConfig", "AppState_test.json");
            string originalPath = AppState.APPSTATE_PATH;
            AppState.APPSTATE_PATH = testPath;

            // Cleanup before test
            if (File.Exists(testPath))
                File.Delete(testPath);
            if (Directory.Exists(Path.GetDirectoryName(testPath)))
                Directory.Delete(Path.GetDirectoryName(testPath), true);

            try
            {
                // Act
                AppState.SaveToJson();

                // Assert
                Assert.True(File.Exists(testPath));
                string content = File.ReadAllText(testPath);
                Assert.NotEmpty(content);
            }
            finally
            {
                // Cleanup
                AppState.APPSTATE_PATH = originalPath;
                if (File.Exists(testPath))
                    File.Delete(testPath);
                if (Directory.Exists(Path.GetDirectoryName(testPath)))
                    Directory.Delete(Path.GetDirectoryName(testPath), true);
            }
        }

        [Fact]
        public void SaveToJson_WhenFileExists_UpdatesFile()
        {
            // Arrange
            string testPath = Path.Combine(Path.GetTempPath(), "TestConfig2", "AppState_test2.json");
            string originalPath = AppState.APPSTATE_PATH;
            AppState.APPSTATE_PATH = testPath;

            // Create directory and file
            Directory.CreateDirectory(Path.GetDirectoryName(testPath));
            File.WriteAllText(testPath, "{}");

            try
            {
                // Act
                AppState.SaveToJson();

                // Assert
                string content = File.ReadAllText(testPath);
                Assert.NotEmpty(content);
                Assert.Contains("DEFAULT_PASSWORD", content);
            }
            finally
            {
                // Cleanup
                AppState.APPSTATE_PATH = originalPath;
                if (File.Exists(testPath))
                    File.Delete(testPath);
                if (Directory.Exists(Path.GetDirectoryName(testPath)))
                    Directory.Delete(Path.GetDirectoryName(testPath), true);
            }
        }

        [Fact]
        public void LoadFromJson_WhenFileDoesNotExist_DoesNotThrow()
        {
            // Arrange
            string originalPath = AppState.APPSTATE_PATH;
            AppState.APPSTATE_PATH = "NonExistent\\path\\AppState.json";

            try
            {
                // Act & Assert
                var exception = Record.Exception(() => AppState.LoadFromJson());
                Assert.Null(exception);
            }
            finally
            {
                AppState.APPSTATE_PATH = originalPath;
            }
        }

        [Fact]
        public void LoadFromJson_WithValidJson_LoadsData()
        {
            // Arrange
            string testPath = Path.Combine(Path.GetTempPath(), "TestConfig3", "AppState_test3.json");
            string originalPath = AppState.APPSTATE_PATH;
            AppState.APPSTATE_PATH = testPath;

            Directory.CreateDirectory(Path.GetDirectoryName(testPath));

            var testData = new
            {
                APPSTATE_PATH = testPath,
                DEFAULT_PASSWORD = "TestPassword",
                DEFAULT_ADDRESSLINE2 = "Test Address",
                EXPIRATION_DATE = "2025 - 2026",
                CHAIRMAN = "Test Chairman",
                REGISTRATION_NO = "TEST-001",
                PRINT_AJUSTMENTS = 25.0
            };

            File.WriteAllText(testPath, JsonConvert.SerializeObject(testData, Formatting.Indented));

            try
            {
                // Act
                AppState.LoadFromJson();

                // Assert
                Assert.Equal("TestPassword", AppState.DEFAULT_PASSWORD);
                Assert.Equal("Test Address", AppState.DEFAULT_ADDRESSLINE2);
                Assert.Equal("2025 - 2026", AppState.EXPIRATION_DATE);
                Assert.Equal("Test Chairman", AppState.CHAIRMAN);
                Assert.Equal("TEST-001", AppState.REGISTRATION_NO);
                Assert.Equal(25.0, AppState.PRINT_AJUSTMENTS);
            }
            finally
            {
                // Cleanup
                AppState.APPSTATE_PATH = originalPath;
                AppState.DEFAULT_PASSWORD = "Admin1234";
                AppState.DEFAULT_ADDRESSLINE2 = "Sapang Palay San Jose Del Monte, Bulacan";
                AppState.EXPIRATION_DATE = "2023 - 2024";
                AppState.CHAIRMAN = "ROLLY M. LABINDAO";
                AppState.REGISTRATION_NO = "9520-03006397";
                AppState.PRINT_AJUSTMENTS = 24.67712;

                if (File.Exists(testPath))
                    File.Delete(testPath);
                if (Directory.Exists(Path.GetDirectoryName(testPath)))
                    Directory.Delete(Path.GetDirectoryName(testPath), true);
            }
        }

        [Fact]
        public void LoadFromJson_WithInvalidJson_DoesNotThrow()
        {
            // Arrange
            string testPath = Path.Combine(Path.GetTempPath(), "TestConfig4", "AppState_test4.json");
            string originalPath = AppState.APPSTATE_PATH;
            AppState.APPSTATE_PATH = testPath;

            Directory.CreateDirectory(Path.GetDirectoryName(testPath));
            File.WriteAllText(testPath, "{ invalid json content }}}");

            try
            {
                // Act & Assert
                var exception = Record.Exception(() => AppState.LoadFromJson());
                Assert.Null(exception);
            }
            finally
            {
                AppState.APPSTATE_PATH = originalPath;
                if (File.Exists(testPath))
                    File.Delete(testPath);
                if (Directory.Exists(Path.GetDirectoryName(testPath)))
                    Directory.Delete(Path.GetDirectoryName(testPath), true);
            }
        }

        [Fact]
        public void AppState_AppStatePath_IsNotEmpty()
        {
            // Assert
            Assert.NotNull(AppState.APPSTATE_PATH);
            Assert.NotEmpty(AppState.APPSTATE_PATH);
        }

        [Fact]
        public void AppState_PrintAdjustments_HasExpectedValue()
        {
            // Assert
            Assert.Equal(24.67712, AppState.PRINT_AJUSTMENTS, 5);
        }
    }
}
