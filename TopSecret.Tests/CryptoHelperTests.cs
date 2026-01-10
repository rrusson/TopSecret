using System.Security.Cryptography;

using TopSecret.Core.Helpers;

namespace TopSecret.Tests
{
	public class CryptoHelperTests
	{
		private const string TestPassword = "TestMasterPassword123!";
		private const string TestPlainText = "This is a secret message";

		[Fact]
		public void Constructor_WithValidPassword_CreatesInstance()
		{
			// Act
			var cryptoHelper = new CryptoHelper(TestPassword);

			// Assert
			Assert.NotNull(cryptoHelper);
		}

		[Fact]
		public void Constructor_WithNullPassword_ThrowsArgumentNullException()
		{
			// Act & Assert
			Assert.Throws<ArgumentNullException>(() => new CryptoHelper(null));
		}

		[Fact]
		public void Encrypt_WithValidPlainText_ReturnsBase64String()
		{
			// Arrange
			var cryptoHelper = new CryptoHelper(TestPassword);

			// Act
			var encrypted = cryptoHelper.Encrypt(TestPlainText);

			// Assert
			Assert.NotNull(encrypted);
			Assert.NotEmpty(encrypted);
			Assert.NotEqual(TestPlainText, encrypted);
			
			// Verify it's a valid Base64 string
			Assert.True(IsBase64String(encrypted));
		}

		[Fact]
		public void Encrypt_WithEmptyString_ReturnsEncryptedValue()
		{
			// Arrange
			var cryptoHelper = new CryptoHelper(TestPassword);

			// Act
			var encrypted = cryptoHelper.Encrypt(string.Empty);

			// Assert
			Assert.NotNull(encrypted);
			Assert.True(IsBase64String(encrypted));
		}

		[Fact]
		public void Encrypt_SamePlainTextTwice_ReturnsSameEncryptedValue()
		{
			// Arrange
			var cryptoHelper = new CryptoHelper(TestPassword);

			// Act
			var encrypted1 = cryptoHelper.Encrypt(TestPlainText);
			var encrypted2 = cryptoHelper.Encrypt(TestPlainText);

			// Assert
			Assert.Equal(encrypted1, encrypted2);
		}

		[Fact]
		public void Encrypt_DifferentPasswords_ReturnsDifferentEncryptedValues()
		{
			// Arrange
			var cryptoHelper1 = new CryptoHelper("Password1");
			var cryptoHelper2 = new CryptoHelper("Password2");

			// Act
			var encrypted1 = cryptoHelper1.Encrypt(TestPlainText);
			var encrypted2 = cryptoHelper2.Encrypt(TestPlainText);

			// Assert
			Assert.NotEqual(encrypted1, encrypted2);
		}

		[Fact]
		public void Decrypt_WithValidCipherText_ReturnsOriginalPlainText()
		{
			// Arrange
			var cryptoHelper = new CryptoHelper(TestPassword);
			var encrypted = cryptoHelper.Encrypt(TestPlainText);

			// Act
			var decrypted = cryptoHelper.Decrypt(encrypted);

			// Assert
			Assert.Equal(TestPlainText, decrypted);
		}

		[Fact]
		public void Decrypt_WithEmptyEncryptedString_ReturnsEmptyString()
		{
			// Arrange
			var cryptoHelper = new CryptoHelper(TestPassword);
			var encrypted = cryptoHelper.Encrypt(string.Empty);

			// Act
			var decrypted = cryptoHelper.Decrypt(encrypted);

			// Assert
			Assert.Equal(string.Empty, decrypted);
		}

		[Fact]
		public void Decrypt_WithInvalidBase64_ThrowsCryptographicException()
		{
			// Arrange
			var cryptoHelper = new CryptoHelper(TestPassword);
			var invalidBase64 = "This is not a valid Base64 string!@#";

			// Act & Assert
			var exception = Assert.Throws<CryptographicException>(() => cryptoHelper.Decrypt(invalidBase64));
			Assert.Contains("Invalid Base64 string", exception.Message);
		}

		[Fact]
		public void Decrypt_WithWrongPassword_ThrowsCryptographicException()
		{
			// Arrange
			var cryptoHelper1 = new CryptoHelper("CorrectPassword");
			var cryptoHelper2 = new CryptoHelper("WrongPassword");
			var encrypted = cryptoHelper1.Encrypt(TestPlainText);

			// Act & Assert
			var exception = Assert.Throws<CryptographicException>(() => cryptoHelper2.Decrypt(encrypted));
			Assert.Contains("Failed to decrypt data", exception.Message);
		}

		[Fact]
		public void Decrypt_WithCorruptedData_ThrowsCryptographicException()
		{
			// Arrange
			var cryptoHelper = new CryptoHelper(TestPassword);
			var encrypted = cryptoHelper.Encrypt(TestPlainText);
			
			// Corrupt the encrypted data by modifying a character
			var corruptedData = encrypted.Substring(0, encrypted.Length - 1) + "X";

			// Act & Assert
			Assert.Throws<CryptographicException>(() => cryptoHelper.Decrypt(corruptedData));
		}

		[Fact]
		public void EncryptDecrypt_RoundTrip_WithSpecialCharacters_PreservesData()
		{
			// Arrange
			var cryptoHelper = new CryptoHelper(TestPassword);
			var specialText = "Special chars: !@#$%^&*()_+-=[]{}|;':\",./<>?`~\n\r\t";

			// Act
			var encrypted = cryptoHelper.Encrypt(specialText);
			var decrypted = cryptoHelper.Decrypt(encrypted);

			// Assert
			Assert.Equal(specialText, decrypted);
		}

		[Fact]
		public void EncryptDecrypt_RoundTrip_WithUnicodeCharacters_PreservesData()
		{
			// Arrange
			var cryptoHelper = new CryptoHelper(TestPassword);
			var unicodeText = "Unicode: 你好世界 مرحبا العالم हैलो वर्ल्ड 🔐🔒🗝️";

			// Act
			var encrypted = cryptoHelper.Encrypt(unicodeText);
			var decrypted = cryptoHelper.Decrypt(encrypted);

			// Assert
			Assert.Equal(unicodeText, decrypted);
		}

		[Fact]
		public void EncryptDecrypt_RoundTrip_WithLargeText_PreservesData()
		{
			// Arrange
			var cryptoHelper = new CryptoHelper(TestPassword);
			var largeText = new string('A', 10000); // 10KB of text

			// Act
			var encrypted = cryptoHelper.Encrypt(largeText);
			var decrypted = cryptoHelper.Decrypt(encrypted);

			// Assert
			Assert.Equal(largeText, decrypted);
		}

		[Fact]
		public void EncryptDecrypt_MultipleInstances_WithSamePassword_WorksCorrectly()
		{
			// Arrange
			var cryptoHelper1 = new CryptoHelper(TestPassword);
			var cryptoHelper2 = new CryptoHelper(TestPassword);

			// Act
			var encrypted = cryptoHelper1.Encrypt(TestPlainText);
			var decrypted = cryptoHelper2.Decrypt(encrypted);

			// Assert
			Assert.Equal(TestPlainText, decrypted);
		}

		[Theory]
		[InlineData("")]
		[InlineData("a")]
		[InlineData("Short text")]
		[InlineData("Medium length text with some special characters !@#")]
		[InlineData("Very long text that spans multiple lines and contains various characters including numbers 123456789 and symbols !@#$%^&*()")]
		public void EncryptDecrypt_RoundTrip_WithVariousTextLengths_PreservesData(string plainText)
		{
			// Arrange
			var cryptoHelper = new CryptoHelper(TestPassword);

			// Act
			var encrypted = cryptoHelper.Encrypt(plainText);
			var decrypted = cryptoHelper.Decrypt(encrypted);

			// Assert
			Assert.Equal(plainText, decrypted);
		}

		[Theory]
		[InlineData("password")]
		[InlineData("P@ssw0rd!")]
		[InlineData("VeryLongPasswordWithManyCharacters123!@#")]
		[InlineData("短密码")]
		public void EncryptDecrypt_WithDifferentPasswordFormats_WorksCorrectly(string password)
		{
			// Arrange
			var cryptoHelper = new CryptoHelper(password);

			// Act
			var encrypted = cryptoHelper.Encrypt(TestPlainText);
			var decrypted = cryptoHelper.Decrypt(encrypted);

			// Assert
			Assert.Equal(TestPlainText, decrypted);
		}

		[Fact]
		public void Encrypt_WithMultilineText_PreservesNewlines()
		{
			// Arrange
			var cryptoHelper = new CryptoHelper(TestPassword);
			var multilineText = "Line 1\nLine 2\r\nLine 3\rLine 4";

			// Act
			var encrypted = cryptoHelper.Encrypt(multilineText);
			var decrypted = cryptoHelper.Decrypt(encrypted);

			// Assert
			Assert.Equal(multilineText, decrypted);
		}

		[Fact]
		public void Decrypt_WithEmptyString_ReturnsEmptyString()
		{
			// Arrange
			var cryptoHelper = new CryptoHelper(TestPassword);

			// Act
			string empty = cryptoHelper.Decrypt(string.Empty);

			// Act & Assert
			Assert.Equal(string.Empty, empty);
		}

		[Fact]
		public void Encrypt_CalledMultipleTimes_ProducesDeterministicResults()
		{
			// Arrange
			var cryptoHelper = new CryptoHelper(TestPassword);
			var results = new List<string>();

			// Act
			for (int i = 0; i < 5; i++)
			{
				results.Add(cryptoHelper.Encrypt(TestPlainText));
			}

			// Assert
			Assert.All(results, result => Assert.Equal(results[0], result));
		}

		private static bool IsBase64String(string value)
		{
			if (string.IsNullOrEmpty(value))
				return false;

			try
			{
				Convert.FromBase64String(value);
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}