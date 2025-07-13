using TopSecret.Core;
using Xunit;

namespace TopSecret.Tests
{
    public class AccountRecordTests
    {
        [Fact]
        public void Constructor_Default_SetsNewGuidForId()
        {
            // Arrange & Act
            var record = new AccountRecord();

            // Assert
            Assert.NotNull(record.Id);
            Assert.NotEqual(Guid.Empty, record.Id);
        }

        [Fact]
        public void Constructor_WithParameters_SetsAllPropertiesCorrectly()
        {
            // Arrange
            var accountName = "TestAccount";
            var userName = "TestUser";
            var password = "TestPassword";
            var url = "https://test.com";

            // Act
            var record = new AccountRecord(accountName, userName, password, url);

            // Assert
            Assert.Equal(accountName, record.AccountName);
            Assert.Equal(userName, record.UserName);
            Assert.Equal(password, record.Password);
            Assert.Equal(url, record.Url);
            Assert.NotNull(record.Id);
            Assert.NotEqual(Guid.Empty, record.Id);
        }

        [Fact]
        public void Constructor_WithParameters_AcceptsNullValues()
        {
            // Arrange & Act
            var record = new AccountRecord(null, null, null, null);

            // Assert
            Assert.Null(record.AccountName);
            Assert.Null(record.UserName);
            Assert.Null(record.Password);
            Assert.Null(record.Url);
            Assert.NotNull(record.Id);
        }

        [Fact]
        public void Constructor_WithValidAccountData_ParsesCorrectly()
        {
            // Arrange
            var id = Guid.NewGuid();
            var accountData = $"{id}\tTestAccount\tTestUser\tTestPassword\thttps://test.com";

            // Act
            var record = new AccountRecord(accountData);

            // Assert
            Assert.Equal(id, record.Id);
            Assert.Equal("TestAccount", record.AccountName);
            Assert.Equal("TestUser", record.UserName);
            Assert.Equal("TestPassword", record.Password);
            Assert.Equal("https://test.com", record.Url);
        }

        [Fact]
        public void Constructor_WithInvalidAccountData_ReturnsEarlyWithDefaults()
        {
            // Arrange
            var shortAccountData = "short\tdata";

            // Act
            var record = new AccountRecord(shortAccountData);

            // Assert
            Assert.Null(record.AccountName);
            Assert.Null(record.UserName);
            Assert.Null(record.Password);
            Assert.Null(record.Url);
            Assert.NotNull(record.Id);
        }

        [Fact]
        public void Constructor_WithInvalidGuidInAccountData_GeneratesNewGuid()
        {
            // Arrange
            var accountData = "invalid-guid\tTestAccount\tTestUser\tTestPassword\thttps://test.com";

            // Act
            var record = new AccountRecord(accountData);

            // Assert
            Assert.NotNull(record.Id);
            Assert.NotEqual(Guid.Empty, record.Id);
            Assert.Equal("TestAccount", record.AccountName);
            Assert.Equal("TestUser", record.UserName);
            Assert.Equal("TestPassword", record.Password);
            Assert.Equal("https://test.com", record.Url);
        }

        [Fact]
        public void ToString_ReturnsCorrectlyFormattedString()
        {
            // Arrange
            var id = Guid.NewGuid();
            var record = new AccountRecord("TestAccount", "TestUser", "TestPassword", "https://test.com");
            
            // Use reflection to set the private Id property for testing
            var idProperty = typeof(AccountRecord).GetProperty("Id");
            idProperty?.SetValue(record, id);

            // Act
            var result = record.ToString();

            // Assert
            var expected = $"{id}\tTestAccount\tTestUser\tTestPassword\thttps://test.com";
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ToString_WithNullValues_HandlesNullsCorrectly()
        {
            // Arrange
            var record = new AccountRecord(null, null, null, null);

            // Act
            var result = record.ToString();

            // Assert
            Assert.Contains(record.Id?.ToString() ?? string.Empty, result);
            Assert.Contains("\t\t\t\t", result); // Should have tabs for null values
        }

        [Theory]
        [InlineData("Account1", "User1", "Pass1", "https://example1.com")]
        [InlineData("Account2", "User2", "Pass2", "https://example2.com")]
        [InlineData("", "", "", "")]
        public void Constructor_WithParameters_TheoryTest(string accountName, string userName, string password, string url)
        {
            // Act
            var record = new AccountRecord(accountName, userName, password, url);

            // Assert
            Assert.Equal(accountName, record.AccountName);
            Assert.Equal(userName, record.UserName);
            Assert.Equal(password, record.Password);
            Assert.Equal(url, record.Url);
        }

        [Fact]
        public void Id_IsReadOnly_CannotBeSetDirectly()
        {
            // Arrange
            var record = new AccountRecord();
            var originalId = record.Id;

            // Act & Assert
            // This test verifies that Id has a private setter
            var idProperty = typeof(AccountRecord).GetProperty("Id");
            Assert.NotNull(idProperty);
            Assert.True(idProperty.CanRead);
            Assert.False(idProperty.SetMethod?.IsPublic ?? true);
        }
    }
}