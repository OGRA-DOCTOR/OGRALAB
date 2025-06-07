using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OGRALAB.Data;
using OGRALAB.Models;
using OGRALAB.Enums;
namespace OGRALAB.Services
{
    /// <summary>
    /// تنفيذ خدمة إدارة المستخدمين
    /// يتضمن جميع العمليات المتعلقة بإدارة حسابات المستخدمين والصلاحيات
    /// </summary>
    public class UserManagementService : IUserManagementService
    {
        private readonly OgralabDbContext _context;
        public UserManagementService(OgralabDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<List<User>> GetAllUsersAsync()
        {
            try
            {
                return await _context.Users
                    .OrderBy(u => u.Username)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "UserManagementService.GetAllUsersAsync");
                throw;
            }
        }
        public async Task<User?> GetUserByIdAsync(int id)
        {
            try
            {
                return await _context.Users.FindAsync(id);
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, $"UserManagementService.GetUserByIdAsync({id})");
                throw;
            }
        }
        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                    return null;
                return await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == username);
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, $"UserManagementService.GetUserByUsernameAsync({username})");
                throw;
            }
        }
        public async Task<bool> AddUserAsync(string username, string password, UserRole role, string? fullName = null, string? email = null)
        {
            try
            {
                // التحقق من صحة البيانات
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                    return false;
                // فحص توفر اسم المستخدم
                if (!await IsUsernameAvailableAsync(username))
                {
                    return false;
                }
                // فحص صحة كلمة المرور
                if (!ValidatePassword(password))
                {
                    return false;
                }
                var user = new User
                {
                    Username = username.Trim(),
                    PasswordHash = HashPassword(password),
                    Role = role,
                    FullName = string.IsNullOrWhiteSpace(fullName) ? null : fullName.Trim(),
                    Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim(),
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, $"UserManagementService.AddUserAsync({username}, {role})");
                return false;
            }
        }
        public async Task<bool> UpdateUserAsync(int id, string username, UserRole role, string? fullName = null, string? email = null)
        {
            try
            {
                var user = await GetUserByIdAsync(id);
                if (user == null)
                    return false;
                // التحقق من صحة البيانات
                if (string.IsNullOrWhiteSpace(username))
                    return false;
                // فحص توفر اسم المستخدم (باستثناء المستخدم الحالي)
                if (!await IsUsernameAvailableAsync(username, id))
                {
                    return false;
                }
                user.Username = username.Trim();
                user.Role = role;
                user.FullName = string.IsNullOrWhiteSpace(fullName) ? null : fullName.Trim();
                user.Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
                user.UpdateInfo(); // تحديث تاريخ التعديل
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, $"UserManagementService.UpdateUserAsync({id}, {username}, {role})");
                return false;
            }
        }
        public async Task<bool> ChangePasswordAsync(int id, string newPassword)
        {
            try
            {
                var user = await GetUserByIdAsync(id);
                if (user == null)
                    return false;
                if (!ValidatePassword(newPassword))
                    return false;
                user.PasswordHash = HashPassword(newPassword);
                user.UpdateInfo(); // تحديث تاريخ التعديل
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, $"UserManagementService.ChangePasswordAsync({id})");
                return false;
            }
        }
        public async Task<bool> DeleteUserAsync(int id)
        {
            try
            {
                var user = await GetUserByIdAsync(id);
                if (user == null)
                    return false;
                // فحص إمكانية الحذف
                if (!await CanDeleteUserAsync(id))
                {
                    return false;
                }
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, $"UserManagementService.DeleteUserAsync({id})");
                return false;
            }
        }
        public async Task<bool> ToggleUserActiveStatusAsync(int id)
        {
            try
            {
                var user = await GetUserByIdAsync(id);
                if (user == null)
                    return false;
                // فحص إمكانية إلغاء التفعيل
                if (user.IsActive && !await CanDeactivateUserAsync(id))
                {
                    return false;
                }
                user.IsActive = !user.IsActive;
                user.UpdateInfo(); // تحديث تاريخ التعديل
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, $"UserManagementService.ToggleUserActiveStatusAsync({id})");
                return false;
            }
        }
        public async Task<bool> IsUsernameAvailableAsync(string username, int? excludeUserId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                    return false;
                var query = _context.Users.Where(u => u.Username == username.Trim());

                if (excludeUserId.HasValue)
                {
                    query = query.Where(u => u.Id != excludeUserId.Value);
                }
                return !await query.AnyAsync();
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, $"UserManagementService.IsUsernameAvailableAsync({username}, {excludeUserId})");
                return false;
            }
        }
        public bool ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;
            // كلمة المرور يجب أن تكون على الأقل 6 أحرف
            // يمكن إضافة معايير أخرى حسب الحاجة
            return password.Length >= 6;
        }
        public string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        public bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hashedPassword))
                    return false;
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "UserManagementService.VerifyPassword");
                return false;
            }
        }
        public async Task<int> GetActiveAdminCountAsync()
        {
            try
            {
                return await _context.Users
                    .CountAsync(u => u.Role == UserRole.Admin && u.IsActive);
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "UserManagementService.GetActiveAdminCountAsync");
                return 0;
            }
        }
        public async Task<bool> CanDeleteUserAsync(int userId, int? currentUserId = null)
        {
            try
            {
                var user = await GetUserByIdAsync(userId);
                if (user == null)
                    return false;
                // لا يمكن حذف المستخدم المسجل دخوله حالياً
                if (currentUserId.HasValue && userId == currentUserId.Value)
                    return false;
                // إذا كان المستخدم مسؤولاً، تحقق من عدم كونه المسؤول الوحيد
                if (user.Role == UserRole.Admin && user.IsActive)
                {
                    var activeAdminCount = await GetActiveAdminCountAsync();
                    if (activeAdminCount <= 1)
                    {
                        return false; // لا يمكن حذف المسؤول الوحيد
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, $"UserManagementService.CanDeleteUserAsync({userId}, {currentUserId})");
                return false;
            }
        }
        public async Task<bool> CanDeactivateUserAsync(int userId)
        {
            try
            {
                var user = await GetUserByIdAsync(userId);
                if (user == null)
                    return false;
                // إذا كان المستخدم مسؤولاً نشطاً، تحقق من عدم كونه المسؤول الوحيد
                if (user.Role == UserRole.Admin && user.IsActive)
                {
                    var activeAdminCount = await GetActiveAdminCountAsync();
                    if (activeAdminCount <= 1)
                    {
                        return false; // لا يمكن إلغاء تفعيل المسؤول الوحيد
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, $"UserManagementService.CanDeactivateUserAsync({userId})");
                return false;
            }
        }
    }
}