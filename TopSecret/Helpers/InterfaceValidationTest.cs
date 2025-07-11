using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TopSecret.Helpers
{
    /// <summary>
    /// Simple test class to verify that all interfaces are properly implemented
    /// </summary>
    public static class InterfaceValidationTest
    {
        /// <summary>
        /// Tests that all interfaces can be resolved and basic functionality works
        /// </summary>
        public static async Task<bool> ValidateInterfacesAsync()
        {
            try
            {
                // Test CryptoHelper interface
                var cryptoFactory = new CryptoHelperFactory();
                var cryptoHelper = cryptoFactory.CreateCryptoHelper("test123");
                var encrypted = cryptoHelper.Encrypt("Hello World");
                var decrypted = cryptoHelper.Decrypt(encrypted);
                
                if (decrypted != "Hello World")
                {
                    return false;
                }

                // Test DataHelper interface
                var dataHelper = new DataHelper();
                var testRecords = new List<AccountRecord>
                {
                    new AccountRecord("Test Account", "testuser", "testpass", "http://example.com")
                };
                
                var serialized = dataHelper.SerializeAccountRecords(testRecords);
                var deserialized = dataHelper.DeserializeAccountRecords(serialized).ToList();
                
                if (deserialized.Count != 1 || deserialized[0].AccountName != "Test Account")
                {
                    return false;
                }

                // Test KillTimer interface  
                var killTimer = new KillTimer();
                killTimer.Reset(); // Should not throw
                killTimer.Dispose();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}