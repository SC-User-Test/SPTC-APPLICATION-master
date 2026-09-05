using System;
using Xunit;

namespace SPTC_APPLICATION.Objects.Tests
{
    public class GeneralEnumTests
    {
        [Fact]
        public void General_HasFranchise()
        {
            Assert.True(Enum.IsDefined(typeof(General), General.FRANCHISE));
        }

        [Fact]
        public void General_HasOperator()
        {
            Assert.True(Enum.IsDefined(typeof(General), General.OPERATOR));
        }

        [Fact]
        public void General_HasDriverDay()
        {
            Assert.True(Enum.IsDefined(typeof(General), General.DRIVER_DAY));
        }

        [Fact]
        public void General_HasDriverNight()
        {
            Assert.True(Enum.IsDefined(typeof(General), General.DRIVAR_NIGHT));
        }

        [Fact]
        public void General_HasNewDriver()
        {
            Assert.True(Enum.IsDefined(typeof(General), General.NEW_DRIVER));
        }

        [Fact]
        public void General_HasNewOperator()
        {
            Assert.True(Enum.IsDefined(typeof(General), General.NEW_OPERATOR));
        }

        [Fact]
        public void General_HasNewFranchise()
        {
            Assert.True(Enum.IsDefined(typeof(General), General.NEW_FRANCHISE));
        }

        [Fact]
        public void General_HasLoginEmployee()
        {
            Assert.True(Enum.IsDefined(typeof(General), General.LOGIN_EMPLOYEE));
        }

        [Fact]
        public void General_HasFetchDriverUsingId()
        {
            Assert.True(Enum.IsDefined(typeof(General), General.FETCH_DRIVER_USING_ID));
        }

        [Fact]
        public void General_HasFetchDriverUsingBodyNumber()
        {
            Assert.True(Enum.IsDefined(typeof(General), General.FETCH_DRIVER_USING_BODYNUMBER));
        }

        [Fact]
        public void General_HasFetchOperatorUsingId()
        {
            Assert.True(Enum.IsDefined(typeof(General), General.FETCH_OPERATOR_USING_ID));
        }

        [Fact]
        public void General_HasFetchOperatorUsingBodyNumber()
        {
            Assert.True(Enum.IsDefined(typeof(General), General.FETCH_OPERATOR_USING_BODYNUMBER));
        }

        [Fact]
        public void General_HasFetchFranchiseUsingId()
        {
            Assert.True(Enum.IsDefined(typeof(General), General.FETCH_FRANCHISE_USING_ID));
        }

        [Fact]
        public void General_HasFetchFranchiseUsingBodyNumber()
        {
            Assert.True(Enum.IsDefined(typeof(General), General.FETCH_FRANCHISE_USING_BODYNUMBER));
        }

        [Fact]
        public void General_HasFetchPaymentDetailsUsingId()
        {
            Assert.True(Enum.IsDefined(typeof(General), General.FETCH_PAYMENT_DETAILS_USING_ID));
        }

        [Fact]
        public void General_HasNewPaymentDetails()
        {
            Assert.True(Enum.IsDefined(typeof(General), General.NEW_PAYMENT_DETAILS));
        }

        [Fact]
        public void General_AllValuesAreDistinct()
        {
            var values = Enum.GetValues(typeof(General));
            var distinctValues = new System.Collections.Generic.HashSet<int>();
            foreach (var v in values)
            {
                distinctValues.Add((int)v);
            }
            Assert.Equal(values.Length, distinctValues.Count);
        }
    }

    public class ImageTests
    {
        // ─── Constructor Tests ───────────────────────────────────────────────────

        [Fact]
        public void DefaultConstructor_CreatesInstance()
        {
            // Arrange & Act
            var image = new Image();

            // Assert
            Assert.NotNull(image);
        }

        [Fact]
        public void DefaultConstructor_HasDefaultValues()
        {
            // Arrange & Act
            var image = new Image();

            // Assert
            Assert.Equal(0, image.id);
            Assert.Null(image.picture);
            Assert.Null(image.name);
        }

        [Fact]
        public void ByteArrayConstructor_SetsNameAndPicture()
        {
            // Arrange
            byte[] imageData = new byte[] { 1, 2, 3, 4, 5 };
            string name = "test_image";

            // Act
            var image = new Image(imageData, name);

            // Assert
            Assert.Equal(name, image.name);
            Assert.Equal(imageData, image.picture);
        }

        [Fact]
        public void ByteArrayConstructor_WithNullData_SetsNullPicture()
        {
            // Arrange & Act
            var image = new Image((byte[])null, "test");

            // Assert
            Assert.Null(image.picture);
            Assert.Equal("test", image.name);
        }

        [Fact]
        public void ByteArrayConstructor_WithEmptyArray_SetsEmptyPicture()
        {
            // Arrange
            byte[] emptyData = new byte[0];

            // Act
            var image = new Image(emptyData, "empty");

            // Assert
            Assert.NotNull(image.picture);
            Assert.Empty(image.picture);
        }

        // ─── GetSource Tests ─────────────────────────────────────────────────────

        [Fact]
        public void GetSource_WithNullPicture_ReturnsNull()
        {
            // Arrange
            var image = new Image();
            image.picture = null;

            // Act
            var result = image.GetSource();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetSource_WithEmptyPicture_ReturnsNull()
        {
            // Arrange
            var image = new Image();
            image.picture = new byte[0];

            // Act
            var result = image.GetSource();

            // Assert
            Assert.Null(result);
        }

        // ─── ToString Tests ──────────────────────────────────────────────────────

        [Fact]
        public void ToString_WithName_ReturnsName()
        {
            // Arrange
            var image = new Image(new byte[] { 1, 2, 3 }, "profile_photo");

            // Act
            string result = image.ToString();

            // Assert
            Assert.Equal("profile_photo", result);
        }

        [Fact]
        public void ToString_WithNullName_ReturnsEmptyString()
        {
            // Arrange
            var image = new Image();
            image.name = null;

            // Act
            string result = image.ToString();

            // Assert
            Assert.Equal(string.Empty, result);
        }

        // ─── Property Tests ──────────────────────────────────────────────────────

        [Fact]
        public void Image_CanSetName()
        {
            // Arrange
            var image = new Image();

            // Act
            image.name = "new_name";

            // Assert
            Assert.Equal("new_name", image.name);
        }

        [Fact]
        public void Image_CanSetPicture()
        {
            // Arrange
            var image = new Image();
            byte[] data = new byte[] { 10, 20, 30 };

            // Act
            image.picture = data;

            // Assert
            Assert.Equal(data, image.picture);
        }

        [Fact]
        public void Image_IdIsReadOnly_DefaultIsZero()
        {
            // Arrange & Act
            var image = new Image();

            // Assert
            Assert.Equal(0, image.id);
        }
    }
}
