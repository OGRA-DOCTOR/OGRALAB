using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OGRALAB.Data;
using OGRALAB.Models;

namespace OGRALAB.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly OgralabDbContext _context;

        // --- تمت إضافة هذه الخاصية ---
        public User? CurrentUser { get; private set; }

        public AuthenticationService(OgralabDbContext context)
        {
            _context = context;
        }

        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return null;

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower() && u.IsActive);

            if (user == null)
                return null;

            bool isValidPassword = await ValidatePasswordAsync(password, user.PasswordHash);

            if (isValidPassword)
            {
                // إذا نجحت المصادقة، قم بتعيين المستخدم الحالي
                CurrentUser = user;
                await UpdateLastLoginAsync(username);
                return user;
            }

            return null;
        }

        public async Task<bool> ValidatePasswordAsync(string password, string hash)
        {
            return await Task.Run(() => BCrypt.Net.BCrypt.Verify(password, hash));
        }

        public async Task UpdateLastLoginAsync(string username)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());

            if (user != null)
            {
                user.LastLogin = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<string>> GetRecentUsernamesAsync()
        {
            return await _context.Users
                .Where(u => u.IsActive && u.LastLogin.HasValue)
                .OrderByDescending(u => u.LastLogin)
                .Take(10)
                .Select(u => u.Username)
                .ToListAsync();
        }

        public async Task SaveUserSettingsAsync(string username, bool rememberMe)
        {
            var settings = await _context.UserSettings
                .FirstOrDefaultAsync(s => s.Username.ToLower() == username.ToLower());

            if (settings == null)
            {
                settings = new UserSettings
                {
                    Username = username,
                    RememberMe = rememberMe,
                    RememberMeExpiry = rememberMe ? DateTime.UtcNow.AddDays(30) : null,
                    LastUpdated = DateTime.UtcNow
                };
                _context.UserSettings.Add(settings);
            }
            else
            {
                settings.RememberMe = rememberMe;
                settings.RememberMeExpiry = rememberMe ? DateTime.UtcNow.AddDays(30) : null;
                settings.LastUpdated = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<UserSettings?> GetUserSettingsAsync(string username)
        {
            return await _context.UserSettings
                .FirstOrDefaultAsync(s => s.Username.ToLower() == username.ToLower());
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // --- تمت إضافة هذه الدالة ---
        public void Logout()
        {
            CurrentUser = null;
        }
    }
}