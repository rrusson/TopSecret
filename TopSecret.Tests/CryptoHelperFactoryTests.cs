using TopSecret.Core.Helpers;
using Xunit;

namespace TopSecret.Tests
{
    public class CryptoHelperFactoryTests
    {
        private readonly CryptoHelperFactory _factory;

        public CryptoHelperFactoryTests()
        {
            _factory = new CryptoHelperFactory();
        }

        [Fact]
        public void CreateCryptoHelper_WithValidPassword_ReturnsCryptoHelper()
        {
            // Arrange
            var password = "TestPassword123!";

            // Act
            var result = _factory.CreateCryptoHelper(password);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<CryptoHelper>(result);
        }

        [Fact]
        public void CreateCryptoHelper_WithNullPassword_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _factory.CreateCryptoHelper(null));
        }

        [Fact]
        public void CreateCryptoHelper_WithEmptyPassword_ReturnsCryptoHelper()
        {
            // Arrange
            var password = string.Empty;

            // Act
            var result = _factory.CreateCryptoHelper(password);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<CryptoHelper>(result);
        }

        [Fact]
        public void CreateCryptoHelper_CreatesTwoDifferentInstances()
        {
            // Arrange
            var password = "TestPassword123!";

            // Act
            var result1 = _factory.CreateCryptoHelper(password);
            var result2 = _factory.CreateCryptoHelper(password);

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotSame(result1, result2);
        }

        [Theory]
        [InlineData("Password1")]
        [InlineData("Password2")]
        [InlineData("VeryLongPasswordWithSpecialCharacters@123")]
        [InlineData("")]
        public void CreateCryptoHelper_WithVariousPasswords_ReturnsCryptoHelper(string password)
        {
            // Act
            var result = _factory.CreateCryptoHelper(password);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<CryptoHelper>(result);
        }
    }
}