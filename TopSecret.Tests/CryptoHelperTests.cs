using System.Security.Cryptography;
using TopSecret.Core.Helpers;
using Xunit;

namespace TopSecret.Tests
{
    public class CryptoHelperTests
    {
        private const string TestPassword = "TestMasterPassword123!";

        [Fact]
        public void Constructor_WithValidPassword_SetsPassword()
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
        public void Encrypt_WithValidPlainText_ReturnsEncryptedString()
        {
            // Arrange
            var cryptoHelper = new CryptoHelper(TestPassword);
            var plainText = "This is a test message";

            // Act
            var encryptedText = cryptoHelper.Encrypt(plainText);

            // Assert
            Assert.NotNull(encryptedText);
            Assert.NotEmpty(encryptedText);
            Assert.NotEqual(plainText, encryptedText);
            Assert.True(IsBase64String(encryptedText));
        }

        [Fact]
        public void Decrypt_WithValidCipherText_ReturnsOriginalPlainText()
        {
            // Arrange
            var cryptoHelper = new CryptoHelper(TestPassword);
            var originalText = "This is a test message";
            var encryptedText = cryptoHelper.Encrypt(originalText);

            // Act
            var decryptedText = cryptoHelper.Decrypt(encryptedText);

            // Assert
            Assert.Equal(originalText, decryptedText);
        }

        [Fact]
        public void Encrypt_Decrypt_RoundTrip_PreservesData()
        {
            // Arrange
            var cryptoHelper = new CryptoHelper(TestPassword);
            var testData = "Test data with special characters: áéíóú 123 !@#$%^&*()";

            // Act
            var encrypted = cryptoHelper.Encrypt(testData);
            var decrypted = cryptoHelper.Decrypt(encrypted);

            // Assert
            Assert.Equal(testData, decrypted);
        }

        [Fact]
        public void Encrypt_SameTextTwice_ProducesSameResult()
        {
            // Arrange
            var cryptoHelper = new CryptoHelper(TestPassword);
            var plainText = "Consistent encryption test";

            // Act
            var encrypted1 = cryptoHelper.Encrypt(plainText);
            var encrypted2 = cryptoHelper.Encrypt(plainText);

            // Assert
            Assert.Equal(encrypted1, encrypted2);
        }

        [Fact]
        public void Encrypt_WithDifferentPasswords_ProducesDifferentResults()
        {
            // Arrange
            var cryptoHelper1 = new CryptoHelper("Password1");
            var cryptoHelper2 = new CryptoHelper("Password2");
            var plainText = "Test message";

            // Act
            var encrypted1 = cryptoHelper1.Encrypt(plainText);
            var encrypted2 = cryptoHelper2.Encrypt(plainText);

            // Assert
            Assert.NotEqual(encrypted1, encrypted2);
        }

        [Fact]
        public void Decrypt_WithInvalidBase64_ThrowsCryptographicException()
        {
            // Arrange
            var cryptoHelper = new CryptoHelper(TestPassword);
            var invalidBase64 = "This is not valid base64!";

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
            var plainText = "Test message";
            var encryptedText = cryptoHelper1.Encrypt(plainText);

            // Act & Assert
            var exception = Assert.Throws<CryptographicException>(() => cryptoHelper2.Decrypt(encryptedText));
            Assert.Contains("Failed to decrypt data", exception.Message);
        }

        [Fact]
        public void Encrypt_WithEmptyString_HandlesEmptyString()
        {
            // Arrange
            var cryptoHelper = new CryptoHelper(TestPassword);
            var emptyString = "";

            // Act
            var encrypted = cryptoHelper.Encrypt(emptyString);
            var decrypted = cryptoHelper.Decrypt(encrypted);

            // Assert
            Assert.Equal(emptyString, decrypted);
        }

        [Theory]
        [InlineData("Simple text")]
        [InlineData("Text with numbers 123456")]
        [InlineData("Special chars: !@#$%^&*()")]
        [InlineData("Unicode: áéíóú ñ")]
        [InlineData("Very long text that should still work fine with the encryption algorithm and not cause any issues")]
        public void Encrypt_Decrypt_WithVariousInputs_WorksCorrectly(string input)
        {
            // Arrange
            var cryptoHelper = new CryptoHelper(TestPassword);

            // Act
            var encrypted = cryptoHelper.Encrypt(input);
            var decrypted = cryptoHelper.Decrypt(encrypted);

            // Assert
            Assert.Equal(input, decrypted);
        }

        [Fact]
        public void Encrypt_WithMultilineText_PreservesFormatting()
        {
            // Arrange
            var cryptoHelper = new CryptoHelper(TestPassword);
            var multilineText = "Line 1\nLine 2\rLine 3\r\nLine 4";

            // Act
            var encrypted = cryptoHelper.Encrypt(multilineText);
            var decrypted = cryptoHelper.Decrypt(encrypted);

            // Assert
            Assert.Equal(multilineText, decrypted);
        }

        private static bool IsBase64String(string base64)
        {
            try
            {
                Convert.FromBase64String(base64);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}