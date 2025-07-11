using Moq;
using TopSecret.Core;
using TopSecret.Core.Interfaces;
using Xunit;

namespace TopSecret.Tests
{
    public class LoginHelperTests
    {
        private readonly Mock<IPasswordManager> _mockPasswordManager;
        private readonly Mock<ICryptoHelperFactory> _mockCryptoHelperFactory;
        private readonly Mock<ICryptoHelper> _mockCryptoHelper;
        private readonly LoginHelper _loginHelper;

        public LoginHelperTests()
        {
            _mockPasswordManager = new Mock<IPasswordManager>();
            _mockCryptoHelperFactory = new Mock<ICryptoHelperFactory>();
            _mockCryptoHelper = new Mock<ICryptoHelper>();
            
            _mockCryptoHelperFactory.Setup(x => x.CreateCryptoHelper(It.IsAny<string>()))
                .Returns(_mockCryptoHelper.Object);

            _loginHelper = new LoginHelper(_mockPasswordManager.Object, _mockCryptoHelperFactory.Object);
        }

        [Fact]
        public async Task IsPasswordRightAsync_WithNullPassword_ReturnsFalse()
        {
            // Act
            var result = await _loginHelper.IsPasswordRightAsync(null!);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsPasswordRightAsync_WithEmptyPassword_ReturnsFalse()
        {
            // Act
            var result = await _loginHelper.IsPasswordRightAsync(string.Empty);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsPasswordRightAsync_WithNoStoredPassword_SetsNewPasswordAndReturnsTrue()
        {
            // Arrange
            var allegedPassword = "NewPassword123!";
            _mockPasswordManager.Setup(x => x.GetMasterPasswordAsync())
                .ReturnsAsync((string?)null);

            // Act
            var result = await _loginHelper.IsPasswordRightAsync(allegedPassword);

            // Assert
            Assert.True(result);
            _mockPasswordManager.Verify(x => x.GetMasterPasswordAsync(), Times.Once);
            _mockPasswordManager.Verify(x => x.ChangeMasterPasswordAsync(allegedPassword), Times.Once);
        }

        [Fact]
        public async Task IsPasswordRightAsync_WithEmptyStoredPassword_SetsNewPasswordAndReturnsTrue()
        {
            // Arrange
            var allegedPassword = "NewPassword123!";
            _mockPasswordManager.Setup(x => x.GetMasterPasswordAsync())
                .ReturnsAsync(string.Empty);

            // Act
            var result = await _loginHelper.IsPasswordRightAsync(allegedPassword);

            // Assert
            Assert.True(result);
            _mockPasswordManager.Verify(x => x.GetMasterPasswordAsync(), Times.Once);
            _mockPasswordManager.Verify(x => x.ChangeMasterPasswordAsync(allegedPassword), Times.Once);
        }

        [Fact]
        public async Task IsPasswordRightAsync_WithWhitespaceStoredPassword_SetsNewPasswordAndReturnsTrue()
        {
            // Arrange
            var allegedPassword = "NewPassword123!";
            _mockPasswordManager.Setup(x => x.GetMasterPasswordAsync())
                .ReturnsAsync("   ");

            // Act
            var result = await _loginHelper.IsPasswordRightAsync(allegedPassword);

            // Assert
            Assert.True(result);
            _mockPasswordManager.Verify(x => x.GetMasterPasswordAsync(), Times.Once);
            _mockPasswordManager.Verify(x => x.ChangeMasterPasswordAsync(allegedPassword), Times.Once);
        }

        [Fact]
        public async Task IsPasswordRightAsync_WithCorrectPassword_ReturnsTrue()
        {
            // Arrange
            var allegedPassword = "CorrectPassword123!";
            var storedPassword = "encrypted_stored_password";
            var encryptedAlleged = "encrypted_alleged_password";

            _mockPasswordManager.Setup(x => x.GetMasterPasswordAsync())
                .ReturnsAsync(storedPassword);
            _mockCryptoHelper.Setup(x => x.Encrypt(allegedPassword))
                .Returns(encryptedAlleged);

            // Mock the case where passwords match
            _mockPasswordManager.Setup(x => x.GetMasterPasswordAsync())
                .ReturnsAsync(encryptedAlleged);

            // Act
            var result = await _loginHelper.IsPasswordRightAsync(allegedPassword);

            // Assert
            Assert.True(result);
            _mockPasswordManager.Verify(x => x.GetMasterPasswordAsync(), Times.Once);
            _mockCryptoHelperFactory.Verify(x => x.CreateCryptoHelper(allegedPassword), Times.Once);
            _mockCryptoHelper.Verify(x => x.Encrypt(allegedPassword), Times.Once);
        }

        [Fact]
        public async Task IsPasswordRightAsync_WithIncorrectPassword_ReturnsFalse()
        {
            // Arrange
            var allegedPassword = "WrongPassword123!";
            var storedPassword = "encrypted_stored_password";
            var encryptedAlleged = "encrypted_alleged_password";

            _mockPasswordManager.Setup(x => x.GetMasterPasswordAsync())
                .ReturnsAsync(storedPassword);
            _mockCryptoHelper.Setup(x => x.Encrypt(allegedPassword))
                .Returns(encryptedAlleged);

            // Act
            var result = await _loginHelper.IsPasswordRightAsync(allegedPassword);

            // Assert
            Assert.False(result);
            _mockPasswordManager.Verify(x => x.GetMasterPasswordAsync(), Times.Once);
            _mockCryptoHelperFactory.Verify(x => x.CreateCryptoHelper(allegedPassword), Times.Once);
            _mockCryptoHelper.Verify(x => x.Encrypt(allegedPassword), Times.Once);
        }

        [Theory]
        [InlineData("Password123!")]
        [InlineData("AnotherPassword456@")]
        [InlineData("VeryLongPasswordWithSpecialCharacters789#")]
        public async Task IsPasswordRightAsync_WithFirstTimeSetup_WorksForVariousPasswords(string password)
        {
            // Arrange
            _mockPasswordManager.Setup(x => x.GetMasterPasswordAsync())
                .ReturnsAsync((string?)null);

            // Act
            var result = await _loginHelper.IsPasswordRightAsync(password);

            // Assert
            Assert.True(result);
            _mockPasswordManager.Verify(x => x.ChangeMasterPasswordAsync(password), Times.Once);
        }

        [Fact]
        public async Task IsPasswordRightAsync_CallsPasswordManagerOnce()
        {
            // Arrange
            var allegedPassword = "TestPassword123!";
            _mockPasswordManager.Setup(x => x.GetMasterPasswordAsync())
                .ReturnsAsync("stored_password");
            _mockCryptoHelper.Setup(x => x.Encrypt(allegedPassword))
                .Returns("encrypted_password");

            // Act
            await _loginHelper.IsPasswordRightAsync(allegedPassword);

            // Assert
            _mockPasswordManager.Verify(x => x.GetMasterPasswordAsync(), Times.Once);
        }

        [Fact]
        public async Task IsPasswordRightAsync_CreatesCryptoHelperOnlyWhenNeeded()
        {
            // Arrange
            var allegedPassword = "TestPassword123!";
            _mockPasswordManager.Setup(x => x.GetMasterPasswordAsync())
                .ReturnsAsync((string?)null);

            // Act
            await _loginHelper.IsPasswordRightAsync(allegedPassword);

            // Assert
            // Should not create CryptoHelper for first-time setup
            _mockCryptoHelperFactory.Verify(x => x.CreateCryptoHelper(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task IsPasswordRightAsync_CreatesCryptoHelperForPasswordValidation()
        {
            // Arrange
            var allegedPassword = "TestPassword123!";
            _mockPasswordManager.Setup(x => x.GetMasterPasswordAsync())
                .ReturnsAsync("stored_password");
            _mockCryptoHelper.Setup(x => x.Encrypt(allegedPassword))
                .Returns("encrypted_password");

            // Act
            await _loginHelper.IsPasswordRightAsync(allegedPassword);

            // Assert
            _mockCryptoHelperFactory.Verify(x => x.CreateCryptoHelper(allegedPassword), Times.Once);
        }
    }
}