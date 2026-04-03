using BankApp.Core.Entities;
namespace BankApp.Infrastructure.DataAccess.Interfaces
{
    /// <summary>
    /// Defines data access operations for OAuth provider links.
    /// </summary>
    public interface IOAuthLinkDataAccess
    {
        /// <summary>
        /// Finds an OAuth link by its provider name and provider-specific user identifier.
        /// </summary>
        /// <param name="provider">The OAuth provider name.</param>
        /// <param name="providerUserId">The user identifier issued by the provider.</param>
        /// <returns>The matching <see cref="OAuthLink"/>, or <see langword="null"/> if not found.</returns>
        OAuthLink? FindByProvider(string provider, string providerUserId);

        /// <summary>
        /// Finds all OAuth links for the specified user.
        /// </summary>
        /// <param name="userId">The identifier of the user.</param>
        /// <returns>A list of OAuth links for the user.</returns>
        List<OAuthLink> FindByUserId(int userId);

        /// <summary>
        /// Creates a new OAuth link for the specified user.
        /// </summary>
        /// <param name="userId">The identifier of the user.</param>
        /// <param name="provider">The OAuth provider name.</param>
        /// <param name="providerUserId">The user identifier issued by the provider.</param>
        /// <param name="providerEmail">The email address from the provider, or <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the link was created successfully; otherwise, <see langword="false"/>.</returns>
        bool Create(int userId, string provider, string providerUserId, string? providerEmail);

        /// <summary>
        /// Deletes an OAuth link by its identifier.
        /// </summary>
        /// <param name="id">The OAuth link identifier.</param>
        void Delete(int id);
    }
}


