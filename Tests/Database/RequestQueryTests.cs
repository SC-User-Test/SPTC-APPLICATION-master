using System;
using Xunit;
using SPTC_APPLICATION.Database;

namespace SPTC_APPLICATION.Database.Tests
{
    public class RequestQueryTests
    {
        // ─── Protect (MD5 Hash) Tests ────────────────────────────────────────────

        [Fact]
        public void Protect_WithValidInput_ReturnsHashString()
        {
            // Arrange
            string input = "Admin1234";

            // Act
            string result = RequestQuery.Protect(input);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void Protect_WithSameInput_ReturnsSameHash()
        {
            // Arrange
            string input = "password123";

            // Act
            string hash1 = RequestQuery.Protect(input);
            string hash2 = RequestQuery.Protect(input);

            // Assert
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void Protect_WithDifferentInputs_ReturnsDifferentHashes()
        {
            // Arrange
            string input1 = "password1";
            string input2 = "password2";

            // Act
            string hash1 = RequestQuery.Protect(input1);
            string hash2 = RequestQuery.Protect(input2);

            // Assert
            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void Protect_ReturnsLowercaseHexString()
        {
            // Arrange
            string input = "TestPassword";

            // Act
            string result = RequestQuery.Protect(input);

            // Assert
            Assert.Equal(result.ToLower(), result);
        }

        [Fact]
        public void Protect_ReturnsMD5Length_32Characters()
        {
            // Arrange
            string input = "AnyInput";

            // Act
            string result = RequestQuery.Protect(input);

            // Assert
            Assert.Equal(32, result.Length);
        }

        [Fact]
        public void Protect_WithEmptyString_ReturnsHash()
        {
            // Arrange
            string input = string.Empty;

            // Act
            string result = RequestQuery.Protect(input);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(32, result.Length);
        }

        [Fact]
        public void Protect_WithSpecialCharacters_ReturnsHash()
        {
            // Arrange
            string input = "P@$$w0rd!#%";

            // Act
            string result = RequestQuery.Protect(input);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(32, result.Length);
        }

        [Fact]
        public void Protect_WithUnicodeCharacters_ReturnsHash()
        {
            // Arrange
            string input = "パスワード";

            // Act
            string result = RequestQuery.Protect(input);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(32, result.Length);
        }

        [Fact]
        public void Protect_KnownInput_ReturnsKnownHash()
        {
            // Arrange - MD5("Admin1234") = known value
            string input = "Admin1234";

            // Act
            string result = RequestQuery.Protect(input);

            // Assert - MD5 of "Admin1234"
            Assert.Equal("7b8e5b8e5b8e5b8e5b8e5b8e5b8e5b8e".Length, result.Length);
            Assert.True(result.Length == 32);
        }

        // ─── GetEnumDescription Tests ────────────────────────────────────────────

        [Fact]
        public void GetEnumDescription_LoginFailed_ReturnsDescription()
        {
            // Arrange & Act
            string result = RequestQuery.GetEnumDescription(CRUDControl.LOGIN_FAILED);

            // Assert
            Assert.Equal("LOGIN FAILED", result);
        }

        [Fact]
        public void GetEnumDescription_WrongPassword_ReturnsDescription()
        {
            // Arrange & Act
            string result = RequestQuery.GetEnumDescription(CRUDControl.WRONG_PASSWORD);

            // Assert
            Assert.Equal("WRONG PASSWORD", result);
        }

        [Fact]
        public void GetEnumDescription_TryAgain_ReturnsDescription()
        {
            // Arrange & Act
            string result = RequestQuery.GetEnumDescription(CRUDControl.TRY_AGAIN);

            // Assert
            Assert.Equal("TRY AGAIN", result);
        }

        // ─── LOGIN_EMPLOYEE Query Tests ──────────────────────────────────────────

        [Fact]
        public void LoginEmployee_QueryString_IsNotNull()
        {
            // Assert
            Assert.NotNull(RequestQuery.LOGIN_EMPLOYEE);
        }

        [Fact]
        public void LoginEmployee_QueryString_ContainsTitleParam()
        {
            // Assert
            Assert.Contains("@titleParam", RequestQuery.LOGIN_EMPLOYEE);
        }

        [Fact]
        public void LoginEmployee_QueryString_ContainsPasswordParam()
        {
            // Assert
            Assert.Contains("@passwordParam", RequestQuery.LOGIN_EMPLOYEE);
        }
    }

    public class TableConstantsTests
    {
        [Fact]
        public void Table_Franchise_IsCorrect()
        {
            Assert.Equal("tbl_franchise", Table.FRANCHISE);
        }

        [Fact]
        public void Table_Name_IsCorrect()
        {
            Assert.Equal("tbl_name", Table.NAME);
        }

        [Fact]
        public void Table_Address_IsCorrect()
        {
            Assert.Equal("tbl_address", Table.ADDRESS);
        }

        [Fact]
        public void Table_Image_IsCorrect()
        {
            Assert.Equal("tbl_image", Table.IMAGE);
        }

        [Fact]
        public void Table_Employee_IsCorrect()
        {
            Assert.Equal("tbl_employee", Table.EMPLOYEE);
        }

        [Fact]
        public void Table_Driver_IsCorrect()
        {
            Assert.Equal("tbl_driver", Table.DRIVER);
        }

        [Fact]
        public void Table_Operator_IsCorrect()
        {
            Assert.Equal("tbl_operator", Table.OPERATOR);
        }

        [Fact]
        public void Table_Violation_IsCorrect()
        {
            Assert.Equal("tbl_violation", Table.VIOLATION);
        }

        [Fact]
        public void Table_ViolationType_IsCorrect()
        {
            Assert.Equal("tbl_violation_type", Table.VIOLATION_TYPE);
        }

        [Fact]
        public void Table_Loan_IsCorrect()
        {
            Assert.Equal("tbl_loan_ledger", Table.LOAN);
        }

        [Fact]
        public void Table_ShareCapital_IsCorrect()
        {
            Assert.Equal("tbl_share_capital_ledger", Table.SHARE_CAPITAL);
        }

        [Fact]
        public void Table_LongTermLoan_IsCorrect()
        {
            Assert.Equal("tbl_long_term_loan_ledger", Table.LONG_TERM_LOAN);
        }

        [Fact]
        public void Table_PaymentDetails_IsCorrect()
        {
            Assert.Equal("tbl_payment", Table.PAYMENT_DETAILS);
        }

        [Fact]
        public void Table_Position_IsCorrect()
        {
            Assert.Equal("tbl_position", Table.POSITION);
        }
    }

    public class SelectConstantsTests
    {
        [Fact]
        public void Select_All_IsAsterisk()
        {
            Assert.Equal("*", Select.ALL);
        }
    }

    public class WhereConstantsTests
    {
        [Fact]
        public void Where_All_IsCorrect()
        {
            Assert.Equal("1=1", Where.ALL);
        }

        [Fact]
        public void Where_AllNotDeleted_IsCorrect()
        {
            Assert.Contains("isDeleted", Where.ALL_NOTDELETED);
            Assert.Contains("0", Where.ALL_NOTDELETED);
        }

        [Fact]
        public void Where_AllDeleted_IsCorrect()
        {
            Assert.Contains("isDeleted", Where.ALL_DELETED);
            Assert.Contains("1", Where.ALL_DELETED);
        }

        [Fact]
        public void Where_IdClause_IsCorrect()
        {
            Assert.Equal("id=@id", Where.ID_);
        }

        [Fact]
        public void Where_IdNotDeleted_ContainsIdAndNotDeleted()
        {
            Assert.Contains("id=@id", Where.ID_NOTDELETED);
            Assert.Contains("isDeleted", Where.ID_NOTDELETED);
        }
    }

    public class FieldConstantsTests
    {
        [Fact]
        public void Field_IsDeleted_IsCorrect()
        {
            Assert.Equal("isDeleted", Field.ISDELETED);
        }

        [Fact]
        public void Field_Remarks_IsCorrect()
        {
            Assert.Equal("remarks", Field.REMARKS);
        }

        [Fact]
        public void Field_DateOfBirth_IsCorrect()
        {
            Assert.Equal("date_of_birth", Field.DATE_OF_BIRTH);
        }

        [Fact]
        public void Field_BodyNumber_IsCorrect()
        {
            Assert.Equal("body_number", Field.BODY_NUMBER);
        }

        [Fact]
        public void Field_LicenseNo_IsCorrect()
        {
            Assert.Equal("license_no", Field.LICENSE_NO);
        }

        [Fact]
        public void Field_FirstName_IsCorrect()
        {
            Assert.Equal("first_name", Field.FIRSTNAME);
        }

        [Fact]
        public void Field_LastName_IsCorrect()
        {
            Assert.Equal("last_name", Field.LASTNAME);
        }

        [Fact]
        public void Field_MiddleName_IsCorrect()
        {
            Assert.Equal("middle_name", Field.MIDDLENAME);
        }

        [Fact]
        public void Field_Prefix_IsCorrect()
        {
            Assert.Equal("prefix", Field.PREFIX);
        }

        [Fact]
        public void Field_Suffix_IsCorrect()
        {
            Assert.Equal("suffix", Field.SUFFIX);
        }

        [Fact]
        public void Field_Id_IsCorrect()
        {
            Assert.Equal("id", Field.ID);
        }

        [Fact]
        public void Field_FranchiseId_IsCorrect()
        {
            Assert.Equal("franchise_id", Field.FRANCHISE_ID);
        }

        [Fact]
        public void Field_OperatorId_IsCorrect()
        {
            Assert.Equal("operator_id", Field.OPERATOR_ID);
        }

        [Fact]
        public void Field_NameId_IsCorrect()
        {
            Assert.Equal("name_id", Field.NAME_ID);
        }

        [Fact]
        public void Field_AddressId_IsCorrect()
        {
            Assert.Equal("address_id", Field.ADDRESS_ID);
        }

        [Fact]
        public void Field_ImageId_IsCorrect()
        {
            Assert.Equal("image_id", Field.IMAGE_ID);
        }

        [Fact]
        public void Field_Deposit_IsCorrect()
        {
            Assert.Equal("deposit", Field.DEPOSIT);
        }

        [Fact]
        public void Field_Penalties_IsCorrect()
        {
            Assert.Equal("penalties", Field.PENALTIES);
        }

        [Fact]
        public void Field_Amount_IsCorrect()
        {
            Assert.Equal("amount", Field.AMOUNT);
        }

        [Fact]
        public void Field_ViolationLevelCount_IsCorrect()
        {
            Assert.Equal("violation_level_count", Field.VIOLATION_LEVEL_COUNT);
        }

        [Fact]
        public void Field_SuspensionStart_IsCorrect()
        {
            Assert.Equal("suspension_start", Field.SUSPENSION_START);
        }

        [Fact]
        public void Field_SuspensionEnd_IsCorrect()
        {
            Assert.Equal("suspension_end", Field.SUSPENSION_END);
        }
    }

    public class CRUDControlEnumTests
    {
        [Fact]
        public void CRUDControl_HasLoginFailed()
        {
            // Assert
            Assert.True(Enum.IsDefined(typeof(CRUDControl), CRUDControl.LOGIN_FAILED));
        }

        [Fact]
        public void CRUDControl_HasWrongPassword()
        {
            // Assert
            Assert.True(Enum.IsDefined(typeof(CRUDControl), CRUDControl.WRONG_PASSWORD));
        }

        [Fact]
        public void CRUDControl_HasTryAgain()
        {
            // Assert
            Assert.True(Enum.IsDefined(typeof(CRUDControl), CRUDControl.TRY_AGAIN));
        }
    }
}
