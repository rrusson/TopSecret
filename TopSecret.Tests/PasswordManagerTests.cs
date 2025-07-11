using Moq;
using TopSecret.Core;
using TopSecret.Core.Helpers;
using Xunit;

namespace TopSecret.Tests
{
    public class PasswordManagerTests
    {
        private readonly Mock<IDataHelper> _mockDataHelper;
        private readonly Mock<IStorageHelper> _mockStorageHelper;
        private readonly Mock<ICryptoHelperFactory> _mockCryptoHelperFactory;
        private readonly Mock<ICryptoHelper> _mockCryptoHelper;
        private readonly PasswordManager _passwordManager;

        public PasswordManagerTests()
        {
            _mockDataHelper = new Mock<IDataHelper>();
            _mockStorageHelper = new Mock<IStorageHelper>();
            _mockCryptoHelperFactory = new Mock<ICryptoHelperFactory>();
            _mockCryptoHelper = new Mock<ICryptoHelper>();
            
            _mockCryptoHelperFactory.Setup(x => x.CreateCryptoHelper(It.IsAny<string>()))
                .Returns(_mockCryptoHelper.Object);

            _passwordManager = new PasswordManager(
                _mockDataHelper.Object,
                _mockStorageHelper.Object,
                _mockCryptoHelperFactory.Object);
        }

        [Fact]
        public void Constructor_InitializesEmptyRecordsList()
        {
            // Assert
            Assert.NotNull(_passwordManager.Records);
            Assert.Empty(_passwordManager.Records);
        }

        [Fact]
        public async Task GetMasterPasswordAsync_CallsStorageHelper()
        {
            // Arrange
            var expectedPassword = "encrypted_password";
            _mockStorageHelper.Setup(x => x.LoadAsync("MasterPassword"))
                .ReturnsAsync(expectedPassword);

            // Act
            var result = await _passwordManager.GetMasterPasswordAsync();

            // Assert
            Assert.Equal(expectedPassword, result);
            _mockStorageHelper.Verify(x => x.LoadAsync("MasterPassword"), Times.Once);
        }

        [Fact]
        public async Task UpdateRecord_WithValidRecord_AddsNewRecord()
        {
            // Arrange
            var record = new AccountRecord("TestAccount", "TestUser", "TestPass", "TestUrl");
            _mockDataHelper.Setup(x => x.SerializeAccountRecords(It.IsAny<IEnumerable<AccountRecord>>()))
                .Returns("serialized_data");
            _mockStorageHelper.Setup(x => x.IsBusy).Returns(false);

            // Act
            var result = await _passwordManager.UpdateRecord(record);

            // Assert
            Assert.True(result);
            Assert.Single(_passwordManager.Records);
            Assert.Equal(record, _passwordManager.Records[0]);
        }

        [Fact]
        public async Task UpdateRecord_WithExistingRecord_UpdatesRecord()
        {
            // Arrange
            var originalRecord = new AccountRecord("TestAccount", "TestUser", "TestPass", "TestUrl");
            _passwordManager.Records.Add(originalRecord);
            
            var updatedRecord = new AccountRecord("UpdatedAccount", "UpdatedUser", "UpdatedPass", "UpdatedUrl");
            // Set the same ID to simulate updating the same record
            var idProperty = typeof(AccountRecord).GetProperty("Id");
            idProperty?.SetValue(updatedRecord, originalRecord.Id);

            _mockDataHelper.Setup(x => x.SerializeAccountRecords(It.IsAny<IEnumerable<AccountRecord>>()))
                .Returns("serialized_data");
            _mockStorageHelper.Setup(x => x.IsBusy).Returns(false);

            // Act
            var result = await _passwordManager.UpdateRecord(updatedRecord);

            // Assert
            Assert.True(result);
            Assert.Single(_passwordManager.Records);
            Assert.Equal(updatedRecord, _passwordManager.Records[0]);
            Assert.Equal("UpdatedAccount", _passwordManager.Records[0].AccountName);
        }

        [Fact]
        public async Task UpdateRecord_WithNullRecord_ReturnsFalse()
        {
            // Act
            var result = await _passwordManager.UpdateRecord(null!);

            // Assert
            Assert.False(result);
            Assert.Empty(_passwordManager.Records);
        }

        [Fact]
        public async Task UpdateRecord_WithNullAccountName_ReturnsFalse()
        {
            // Arrange
            var record = new AccountRecord(null, "TestUser", "TestPass", "TestUrl");

            // Act
            var result = await _passwordManager.UpdateRecord(record);

            // Assert
            Assert.False(result);
            Assert.Empty(_passwordManager.Records);
        }

        [Fact]
        public async Task DeleteRecord_WithValidId_RemovesRecord()
        {
            // Arrange
            var record = new AccountRecord("TestAccount", "TestUser", "TestPass", "TestUrl");
            _passwordManager.Records.Add(record);
            
            _mockDataHelper.Setup(x => x.SerializeAccountRecords(It.IsAny<IEnumerable<AccountRecord>>()))
                .Returns("serialized_data");
            _mockStorageHelper.Setup(x => x.IsBusy).Returns(false);

            // Act
            var result = await _passwordManager.DeleteRecord(record.Id);

            // Assert
            Assert.True(result);
            Assert.Empty(_passwordManager.Records);
        }

        [Fact]
        public async Task DeleteRecord_WithNonExistentId_ReturnsFalse()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var record = new AccountRecord("TestAccount", "TestUser", "TestPass", "TestUrl");
            _passwordManager.Records.Add(record);

            // Act
            var result = await _passwordManager.DeleteRecord(nonExistentId);

            // Assert
            Assert.False(result);
            Assert.Single(_passwordManager.Records); // Record should still be there
        }

        [Fact]
        public async Task DeleteRecord_WithNullId_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _passwordManager.DeleteRecord(null));
        }

        [Fact]
        public async Task ChangeMasterPasswordAsync_WithValidPassword_UpdatesPassword()
        {
            // Arrange
            var newPassword = "NewPassword123!";
            var encryptedPassword = "encrypted_new_password";
            
            _mockCryptoHelper.Setup(x => x.Encrypt(newPassword))
                .Returns(encryptedPassword);
            _mockDataHelper.Setup(x => x.SerializeAccountRecords(It.IsAny<IEnumerable<AccountRecord>>()))
                .Returns("serialized_data");
            _mockStorageHelper.Setup(x => x.IsBusy).Returns(false);

            // Act
            await _passwordManager.ChangeMasterPasswordAsync(newPassword);

            // Assert
            _mockCryptoHelperFactory.Verify(x => x.CreateCryptoHelper(newPassword), Times.Once);
            _mockCryptoHelper.Verify(x => x.Encrypt(newPassword), Times.Once);
            _mockStorageHelper.Verify(x => x.RemoveAsync("MasterPassword"), Times.Once);
            _mockStorageHelper.Verify(x => x.SaveAsync("MasterPassword", encryptedPassword), Times.Once);
        }

        [Fact]
        public async Task ChangeMasterPasswordAsync_WithNullPassword_DoesNothing()
        {
            // Act
            await _passwordManager.ChangeMasterPasswordAsync(null!);

            // Assert
            _mockCryptoHelperFactory.Verify(x => x.CreateCryptoHelper(It.IsAny<string>()), Times.Never);
            _mockStorageHelper.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.Never);
            _mockStorageHelper.Verify(x => x.SaveAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ChangeMasterPasswordAsync_WithEmptyPassword_DoesNothing()
        {
            // Act
            await _passwordManager.ChangeMasterPasswordAsync("");

            // Assert
            _mockCryptoHelperFactory.Verify(x => x.CreateCryptoHelper(It.IsAny<string>()), Times.Never);
            _mockStorageHelper.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.Never);
            _mockStorageHelper.Verify(x => x.SaveAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ChangeMasterPasswordAsync_WithWhitespacePassword_DoesNothing()
        {
            // Act
            await _passwordManager.ChangeMasterPasswordAsync("   ");

            // Assert
            _mockCryptoHelperFactory.Verify(x => x.CreateCryptoHelper(It.IsAny<string>()), Times.Never);
            _mockStorageHelper.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.Never);
            _mockStorageHelper.Verify(x => x.SaveAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task PopulateRecordsAsync_WithValidData_PopulatesRecords()
        {
            // Arrange
            var serializedData = "serialized_account_data";
            var expectedRecords = new List<AccountRecord>
            {
                new AccountRecord("Account1", "User1", "Pass1", "Url1"),
                new AccountRecord("Account2", "User2", "Pass2", "Url2")
            };

            _mockStorageHelper.Setup(x => x.LoadAsync("AccountData"))
                .ReturnsAsync(serializedData);
            _mockDataHelper.Setup(x => x.DeserializeAccountRecords(serializedData))
                .Returns(expectedRecords);

            // Act
            await _passwordManager.PopulateRecordsAsync();

            // Assert
            Assert.Equal(2, _passwordManager.Records.Count);
            Assert.Equal(expectedRecords[0].AccountName, _passwordManager.Records[0].AccountName);
            Assert.Equal(expectedRecords[1].AccountName, _passwordManager.Records[1].AccountName);
        }

        [Fact]
        public async Task PopulateRecordsAsync_WithNoData_DoesNothing()
        {
            // Arrange
            _mockStorageHelper.Setup(x => x.LoadAsync("AccountData"))
                .ReturnsAsync((string?)null);

            // Act
            await _passwordManager.PopulateRecordsAsync();

            // Assert
            Assert.Empty(_passwordManager.Records);
            _mockDataHelper.Verify(x => x.DeserializeAccountRecords(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task PopulateRecordsAsync_WithEmptyData_DoesNothing()
        {
            // Arrange
            _mockStorageHelper.Setup(x => x.LoadAsync("AccountData"))
                .ReturnsAsync(string.Empty);

            // Act
            await _passwordManager.PopulateRecordsAsync();

            // Assert
            Assert.Empty(_passwordManager.Records);
            _mockDataHelper.Verify(x => x.DeserializeAccountRecords(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task UpdateRecord_WaitsForStorageHelperNotBusy()
        {
            // Arrange
            var record = new AccountRecord("TestAccount", "TestUser", "TestPass", "TestUrl");
            _mockDataHelper.Setup(x => x.SerializeAccountRecords(It.IsAny<IEnumerable<AccountRecord>>()))
                .Returns("serialized_data");
            
            var busySequence = new Queue<bool>(new[] { true, true, false });
            _mockStorageHelper.Setup(x => x.IsBusy).Returns(() => busySequence.Dequeue());

            // Act
            var result = await _passwordManager.UpdateRecord(record);

            // Assert
            Assert.True(result);
            _mockStorageHelper.Verify(x => x.SaveEncryptedAsync("AccountData", "serialized_data"), Times.Once);
        }

        [Fact]
        public async Task UpdateRecord_WithEmptySerializedData_DoesNotSave()
        {
            // Arrange
            var record = new AccountRecord("TestAccount", "TestUser", "TestPass", "TestUrl");
            _mockDataHelper.Setup(x => x.SerializeAccountRecords(It.IsAny<IEnumerable<AccountRecord>>()))
                .Returns(string.Empty);

            // Act
            var result = await _passwordManager.UpdateRecord(record);

            // Assert
            Assert.True(result);
            _mockStorageHelper.Verify(x => x.SaveEncryptedAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}