using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OGRALAB.Models;

namespace OGRALAB.Services
{
    public interface IAuthenticationService
    {
        // --- تمت إضافة هذه الخاصية ---
        User? CurrentUser { get; }

        Task<User?> AuthenticateAsync(string username, string password);
        Task<bool> ValidatePasswordAsync(string password, string hash);
        Task UpdateLastLoginAsync(string username);
        Task<List<string>> GetRecentUsernamesAsync();
        Task SaveUserSettingsAsync(string username, bool rememberMe);
        Task<UserSettings?> GetUserSettingsAsync(string username);
        string HashPassword(string password);

        // --- تمت إضافة هذه الدالة ---
        void Logout();
    }
}