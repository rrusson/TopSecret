using Moq;

using TopSecret.Core;
using TopSecret.Core.Interfaces;

namespace TopSecret.Tests
{
	public class LoginHelperTests
	{
		private readonly Mock<ICryptoHelperFactory> _mockCryptoHelperFactory;
		private readonly Mock<ICryptoHelper> _mockCryptoHelper;
		private readonly Mock<IMasterPasswordProvider> _mockMasterPasswordProvider;
		private readonly Mock<IPasswordManager> _mockPasswordManager;
		private readonly LoginHelper _loginHelper;

		public LoginHelperTests()
		{
			_mockCryptoHelperFactory = new Mock<ICryptoHelperFactory>();
			_mockCryptoHelper = new Mock<ICryptoHelper>();
			_mockMasterPasswordProvider = new Mock<IMasterPasswordProvider>();
			_mockPasswordManager = new Mock<IPasswordManager>();
			_mockPasswordManager.As<IMasterPasswordProvider>()
				.Setup(x => x.GetMasterPasswordAsync())
				.Returns(() => _mockMasterPasswordProvider.Object.GetMasterPasswordAsync());
			_mockPasswordManager.As<IMasterPasswordProvider>()
				.Setup(x => x.ChangeMasterPasswordAsync(It.IsAny<string>()))
				.Returns((string pw) => _mockMasterPasswordProvider.Object.ChangeMasterPasswordAsync(pw));
			_mockPasswordManager.As<IMasterPasswordProvider>()
				.SetupGet(x => x.MasterPassword)
				.Returns(() => _mockMasterPasswordProvider.Object.MasterPassword);

			_mockCryptoHelperFactory.Setup(f => f.CreateCryptoHelper(It.IsAny<string>()))
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
			_mockMasterPasswordProvider.Setup(x => x.GetMasterPasswordAsync())
				.ReturnsAsync((string?)null);

			// Act
			var result = await _loginHelper.IsPasswordRightAsync(allegedPassword);

			// Assert
			Assert.True(result);
			_mockMasterPasswordProvider.Verify(x => x.GetMasterPasswordAsync(), Times.Once);
			_mockMasterPasswordProvider.Verify(x => x.ChangeMasterPasswordAsync(allegedPassword), Times.Once);
		}

		[Fact]
		public async Task IsPasswordRightAsync_WithEmptyStoredPassword_SetsNewPasswordAndReturnsTrue()
		{
			// Arrange
			var allegedPassword = "NewPassword123!";
			_mockMasterPasswordProvider.Setup(x => x.GetMasterPasswordAsync())
				.ReturnsAsync(string.Empty);

			// Act
			var result = await _loginHelper.IsPasswordRightAsync(allegedPassword);

			// Assert
			Assert.True(result);
			_mockMasterPasswordProvider.Verify(x => x.GetMasterPasswordAsync(), Times.Once);
			_mockMasterPasswordProvider.Verify(x => x.ChangeMasterPasswordAsync(allegedPassword), Times.Once);
		}

		[Fact]
		public async Task IsPasswordRightAsync_WithWhitespaceStoredPassword_SetsNewPasswordAndReturnsTrue()
		{
			// Arrange
			var allegedPassword = "NewPassword123!";
			_mockMasterPasswordProvider.Setup(x => x.GetMasterPasswordAsync())
				.ReturnsAsync("   ");

			// Act
			var result = await _loginHelper.IsPasswordRightAsync(allegedPassword);

			// Assert
			Assert.True(result);
			_mockMasterPasswordProvider.Verify(x => x.GetMasterPasswordAsync(), Times.Once);
			_mockMasterPasswordProvider.Verify(x => x.ChangeMasterPasswordAsync(allegedPassword), Times.Once);
		}

		[Fact]
		public async Task IsPasswordRightAsync_WithCorrectPassword_ReturnsTrue()
		{
			// Arrange
			var allegedPassword = "CorrectPassword123!";
			var storedPassword = "encrypted_stored_password";
			var encryptedAlleged = "encrypted_alleged_password";

			_mockMasterPasswordProvider.Setup(x => x.GetMasterPasswordAsync())
				.ReturnsAsync(storedPassword);
			_mockCryptoHelper.Setup(x => x.Encrypt(allegedPassword))
				.Returns(encryptedAlleged);

			// Mock the case where passwords match
			_mockMasterPasswordProvider.Setup(x => x.GetMasterPasswordAsync())
				.ReturnsAsync(encryptedAlleged);

			// Act
			var result = await _loginHelper.IsPasswordRightAsync(allegedPassword);

			// Assert
			Assert.True(result);
			_mockMasterPasswordProvider.Verify(x => x.GetMasterPasswordAsync(), Times.Once);
			_mockCryptoHelper.Verify(x => x.Encrypt(allegedPassword), Times.Once);
		}

		[Fact]
		public async Task IsPasswordRightAsync_WithIncorrectPassword_ReturnsFalse()
		{
			// Arrange
			var allegedPassword = "WrongPassword123!";
			var storedPassword = "encrypted_stored_password";
			var encryptedAlleged = "encrypted_alleged_password";

			_mockMasterPasswordProvider.Setup(x => x.GetMasterPasswordAsync())
				.ReturnsAsync(storedPassword);
			_mockCryptoHelper.Setup(x => x.Encrypt(allegedPassword))
				.Returns(encryptedAlleged);

			// Act
			var result = await _loginHelper.IsPasswordRightAsync(allegedPassword);

			// Assert
			Assert.False(result);
			_mockMasterPasswordProvider.Verify(x => x.GetMasterPasswordAsync(), Times.Once);
			_mockCryptoHelper.Verify(x => x.Encrypt(allegedPassword), Times.Once);
		}

		[Theory]
		[InlineData("Password123!")]
		[InlineData("AnotherPassword456@")]
		[InlineData("VeryLongPasswordWithSpecialCharacters789#")]
		public async Task IsPasswordRightAsync_WithFirstTimeSetup_WorksForVariousPasswords(string password)
		{
			// Arrange
			_mockMasterPasswordProvider.Setup(x => x.GetMasterPasswordAsync())
				.ReturnsAsync((string?)null);

			// Act
			var result = await _loginHelper.IsPasswordRightAsync(password);

			// Assert
			Assert.True(result);
			_mockMasterPasswordProvider.Verify(x => x.ChangeMasterPasswordAsync(password), Times.Once);
		}

		[Fact]
		public async Task IsPasswordRightAsync_CallsMasterPasswordProviderOnce()
		{
			// Arrange
			var allegedPassword = "TestPassword123!";
			_mockMasterPasswordProvider.Setup(x => x.GetMasterPasswordAsync())
				.ReturnsAsync("stored_password");
			_mockCryptoHelper.Setup(x => x.Encrypt(allegedPassword))
				.Returns("encrypted_password");

			// Act
			await _loginHelper.IsPasswordRightAsync(allegedPassword);

			// Assert
			_mockMasterPasswordProvider.Verify(x => x.GetMasterPasswordAsync(), Times.Once);
		}
	}
}