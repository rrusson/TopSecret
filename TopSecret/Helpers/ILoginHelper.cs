namespace TopSecret.Helpers
{
    public interface ILoginHelper
    {
        /// <summary>
        /// Validates if the provided password matches the stored master password
        /// </summary>
        /// <param name="allegedPw">The password to validate</param>
        /// <returns>True if password is correct, false otherwise</returns>
        Task<bool> IsPasswordRightAsync(string allegedPw);
    }
}