using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace TopSecret.Helpers
{
    /// <summary>
    /// Simple test class to verify that dependency injection is working correctly
    /// </summary>
    public static class DependencyInjectionTest
    {
        /// <summary>
        /// Tests that all services can be resolved from the DI container
        /// </summary>
        public static bool ValidateDependencyInjection(IServiceProvider services)
        {
            try
            {
                // Test that all interfaces can be resolved
                var cryptoFactory = services.GetService<ICryptoHelperFactory>();
                var dataHelper = services.GetService<IDataHelper>();
                var storageHelper = services.GetService<IStorageHelper>();
                var passwordManager = services.GetService<IPasswordManager>();
                var loginHelper = services.GetService<ILoginHelper>();
                var killTimer = services.GetService<IKillTimer>();

                // Test that all pages can be resolved
                var loginPage = services.GetService<LoginPage>();
                var bigListPage = services.GetService<BigListPage>();
                var accountEditor = services.GetService<AccountEditor>();

                // Check that all services are not null
                return cryptoFactory != null &&
                       dataHelper != null &&
                       storageHelper != null &&
                       passwordManager != null &&
                       loginHelper != null &&
                       killTimer != null &&
                       loginPage != null &&
                       bigListPage != null &&
                       accountEditor != null;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}