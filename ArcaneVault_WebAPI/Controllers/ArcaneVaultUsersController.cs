using ArcaneVault_WebAPI.Data;
using ArcaneVault_WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArcaneVault_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArcaneVaultUsersController : ControllerBase
    {
        private readonly ArcaneVaultContext _context;

        public ArcaneVaultUsersController(ArcaneVaultContext context)
        {
            _context = context;
        }

        //GET All Users
        [HttpGet]
        public async Task<ActionResult> GetArcaneVaultUsers()
        {
            var users =
                from user in _context.ArcaneVaultUsers
                select new
                {
                    user.UserName,
                    user.Email,
                    user.IsDeleted,
                    user.RoleId
                };

            return Ok(await users.ToListAsync());
        }

        // Check if email already exists
        [HttpGet("CheckEmail/{email}")]
        public async Task<ActionResult<bool>> CheckEmail(string email)
        {
            return await _context.ArcaneVaultUsers
                .AnyAsync(u => u.Email == email);
        }


        // Login
        [HttpPost("Login")]
        public async Task<ActionResult> Login(ArcaneVaultUser login)
        {
            var user = await _context.ArcaneVaultUsers
                .FirstOrDefaultAsync(u =>
                    u.UserName == login.UserName &&
                    u.Email == login.Email &&
                    u.Password == login.Password &&
                    !u.IsDeleted);

            if (user == null)
            {
                return Unauthorized();
            }

            return Ok(new
            {
                user.UserName,
                user.Email,
                user.IsDeleted,
                user.RoleId
            });
        }

        // Change Password
        [HttpPut("ChangePassword")]
        public async Task<ActionResult> ChangePassword(
            ChangePasswordModel changePassword)
        {
            var user = await _context.ArcaneVaultUsers
                .FirstOrDefaultAsync(u =>
                    u.UserName == changePassword.UserName &&
                    !u.IsDeleted);

            if (user == null)
            {
                return NotFound("User account not found.");
            }

            if (user.Password != changePassword.CurrentPassword)
            {
                return BadRequest("Current password is incorrect.");
            }

            if (changePassword.CurrentPassword == changePassword.NewPassword)
            {
                return BadRequest(
                    "New password must be different from the current password.");
            }

            user.Password = changePassword.NewPassword;

            await _context.SaveChangesAsync();

            return Ok("Password changed successfully.");
        }

        //Registration (POST)
        [HttpPost]
        public async Task<ActionResult<ArcaneVaultUser>> PostArcaneVaultUser(ArcaneVaultUser user)
        {
            // Check duplicate email
            bool emailExists = await _context.ArcaneVaultUsers
                .AnyAsync(u => u.Email == user.Email);

            if (emailExists)
            {
                return BadRequest("Email already exists.");
            }

            // Check duplicate username
            bool usernameExists = await _context.ArcaneVaultUsers
                .AnyAsync(u => u.UserName == user.UserName);

            if (usernameExists)
            {
                return BadRequest("Username already exists.");
            }

            _context.ArcaneVaultUsers.Add(user);

            await _context.SaveChangesAsync();

            return Ok("Registration successful.");
        }
    }
}