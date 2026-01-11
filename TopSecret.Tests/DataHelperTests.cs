using TopSecret.Core;

namespace TopSecret.Tests
{
    public class DataHelperTests
    {
        private readonly DataHelper _dataHelper;

        public DataHelperTests()
        {
            _dataHelper = new DataHelper();
        }

        [Fact]
        public void DeserializeAccountRecords_WithNullData_ReturnsEmptyCollection()
        {
            // Act
            var result = _dataHelper.DeserializeAccountRecords(null);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void DeserializeAccountRecords_WithEmptyData_ReturnsEmptyCollection()
        {
            // Act
            var result = _dataHelper.DeserializeAccountRecords(string.Empty);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void DeserializeAccountRecords_WithValidSingleRecord_ReturnsOneRecord()
        {
            // Arrange
            var id = Guid.NewGuid();
            var accountData = $"{id}\tTestAccount\tTestUser\tTestPassword\thttps://test.com{(char)13}";

            // Act
            var result = _dataHelper.DeserializeAccountRecords(accountData).ToList();

            // Assert
            Assert.Single(result);
            var record = result.First();
            Assert.Equal(id, record.Id);
            Assert.Equal("TestAccount", record.AccountName);
            Assert.Equal("TestUser", record.UserName);
            Assert.Equal("TestPassword", record.Password);
            Assert.Equal("https://test.com", record.Url);
        }

        [Fact]
        public void DeserializeAccountRecords_WithMultipleRecords_ReturnsAllRecords()
        {
            // Arrange
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var accountData = $"{id1}\tAccount1\tUser1\tPass1\tUrl1{(char)13}" +
                             $"{id2}\tAccount2\tUser2\tPass2\tUrl2{(char)13}";

            // Act
            var result = _dataHelper.DeserializeAccountRecords(accountData).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            
            var record1 = result[0];
            Assert.Equal(id1, record1.Id);
            Assert.Equal("Account1", record1.AccountName);
            Assert.Equal("User1", record1.UserName);
            Assert.Equal("Pass1", record1.Password);
            Assert.Equal("Url1", record1.Url);

            var record2 = result[1];
            Assert.Equal(id2, record2.Id);
            Assert.Equal("Account2", record2.AccountName);
            Assert.Equal("User2", record2.UserName);
            Assert.Equal("Pass2", record2.Password);
            Assert.Equal("Url2", record2.Url);
        }

        [Fact]
        public void DeserializeAccountRecords_WithEmptyLine_SkipsEmptyLines()
        {
            // Arrange
            var id = Guid.NewGuid();
            var accountData = $"{id}\tTestAccount\tTestUser\tTestPassword\thttps://test.com{(char)13}" +
                             $"{(char)13}" + // Empty line
                             $"{id}\tTestAccount2\tTestUser2\tTestPassword2\thttps://test2.com{(char)13}";

            // Act
            var result = _dataHelper.DeserializeAccountRecords(accountData).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("TestAccount", result[0].AccountName);
            Assert.Equal("TestAccount2", result[1].AccountName);
        }

        [Fact]
        public void DeserializeAccountRecords_WithSingleCharacterLine_SkipsShortLines()
        {
            // Arrange
            var id = Guid.NewGuid();
            var accountData = $"{id}\tTestAccount\tTestUser\tTestPassword\thttps://test.com{(char)13}" +
                             $"x{(char)13}"; // Single character line

            // Act
            var result = _dataHelper.DeserializeAccountRecords(accountData).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal("TestAccount", result[0].AccountName);
        }

        [Fact]
        public void SerializeAccountRecords_WithEmptyCollection_ReturnsEmptyString()
        {
            // Arrange
            var records = new List<AccountRecord>();

            // Act
            var result = _dataHelper.SerializeAccountRecords(records);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void SerializeAccountRecords_WithSingleRecord_ReturnsCorrectFormat()
        {
            // Arrange
            var id = Guid.NewGuid();
            var record = new AccountRecord("TestAccount", "TestUser", "TestPassword", "https://test.com");
            
            // Use reflection to set the ID for testing
            var idProperty = typeof(AccountRecord).GetProperty("Id");
            idProperty?.SetValue(record, id);

            var records = new List<AccountRecord> { record };

            // Act
            var result = _dataHelper.SerializeAccountRecords(records);

            // Assert
            var expected = $"{id}\tTestAccount\tTestUser\tTestPassword\thttps://test.com{(char)13}";
            Assert.Equal(expected, result);
        }

        [Fact]
        public void SerializeAccountRecords_WithMultipleRecords_ReturnsAllRecordsSerialized()
        {
            // Arrange
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            
            var record1 = new AccountRecord("Account1", "User1", "Pass1", "Url1");
            var record2 = new AccountRecord("Account2", "User2", "Pass2", "Url2");

            // Use reflection to set IDs for testing
            var idProperty = typeof(AccountRecord).GetProperty("Id");
            idProperty?.SetValue(record1, id1);
            idProperty?.SetValue(record2, id2);

            var records = new List<AccountRecord> { record1, record2 };

            // Act
            var result = _dataHelper.SerializeAccountRecords(records);

            // Assert
            var expected = $"{id1}\tAccount1\tUser1\tPass1\tUrl1{(char)13}" +
                          $"{id2}\tAccount2\tUser2\tPass2\tUrl2{(char)13}";
            Assert.Equal(expected, result);
        }

        [Fact]
        public void SerializeAccountRecords_WithRecordWithNullAccountName_SkipsRecord()
        {
            // Arrange
            var validRecord = new AccountRecord("ValidAccount", "ValidUser", "Pass", "Url");
            var invalidRecord = new AccountRecord(null, "InvalidUser", "Pass", "Url");
            var records = new List<AccountRecord> { validRecord, invalidRecord };

            // Act
            var result = _dataHelper.SerializeAccountRecords(records);

            // Assert
            Assert.DoesNotContain("InvalidUser", result); // Should not contain the user from the invalid record
            Assert.Contains("ValidAccount", result); // Should contain the valid record
        }

        [Fact]
        public void SerializeAccountRecords_WithRecordWithEmptyAccountName_SkipsRecord()
        {
            // Arrange
            var validRecord = new AccountRecord("ValidAccount", "ValidUser", "Pass", "Url");
            var invalidRecord = new AccountRecord("", "InvalidUser", "Pass", "Url");
            var records = new List<AccountRecord> { validRecord, invalidRecord };

            // Act
            var result = _dataHelper.SerializeAccountRecords(records);

            // Assert
            Assert.Single(result.Split((char)13, StringSplitOptions.RemoveEmptyEntries));
            Assert.Contains("ValidAccount", result);
        }

        [Fact]
        public void SerializeAccountRecords_WithRecordWithWhitespaceAccountName_SkipsRecord()
        {
            // Arrange
            var validRecord = new AccountRecord("ValidAccount", "ValidUser", "Pass", "Url");
            var invalidRecord = new AccountRecord("   ", "InvalidUser", "Pass", "Url");
            var records = new List<AccountRecord> { validRecord, invalidRecord };

            // Act
            var result = _dataHelper.SerializeAccountRecords(records);

            // Assert
            Assert.Single(result.Split((char)13, StringSplitOptions.RemoveEmptyEntries));
            Assert.Contains("ValidAccount", result);
        }

        [Fact]
        public void SerializeAccountRecords_WithNullValues_HandlesNullsCorrectly()
        {
            // Arrange
            var record = new AccountRecord("TestAccount", null, null, null);
            var records = new List<AccountRecord> { record };

            // Act
            var result = _dataHelper.SerializeAccountRecords(records);

            // Assert
            Assert.Contains("TestAccount", result);
            Assert.Contains("\t\t\t", result); // Should contain tabs for null values
        }

        [Fact]
        public void SerializeDeserialize_RoundTrip_PreservesData()
        {
            // Arrange
            var originalRecords = new List<AccountRecord>
            {
                new AccountRecord("Account1", "User1", "Pass1", "Url1"),
                new AccountRecord("Account2", "User2", "Pass2", "Url2"),
                new AccountRecord("Account3", "User3", "Pass3", "Url3")
            };

            // Act
            var serialized = _dataHelper.SerializeAccountRecords(originalRecords);
            var deserialized = _dataHelper.DeserializeAccountRecords(serialized).ToList();

            // Assert
            Assert.Equal(originalRecords.Count, deserialized.Count);
            
            for (int i = 0; i < originalRecords.Count; i++)
            {
                Assert.Equal(originalRecords[i].AccountName, deserialized[i].AccountName);
                Assert.Equal(originalRecords[i].UserName, deserialized[i].UserName);
                Assert.Equal(originalRecords[i].Password, deserialized[i].Password);
                Assert.Equal(originalRecords[i].Url, deserialized[i].Url);
            }
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public void SerializeAccountRecords_WithInvalidAccountNames_SkipsRecords(string accountName)
        {
            // Arrange
            var records = new List<AccountRecord>
            {
                new AccountRecord(accountName, "User", "Pass", "Url")
            };

            // Act
            var result = _dataHelper.SerializeAccountRecords(records);

            // Assert
            Assert.Equal(string.Empty, result);
        }
    }
}