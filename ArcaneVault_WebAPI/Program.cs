using ArcaneVault_WebAPI.Data;
using ArcaneVault_WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ArcaneVault_WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            //CHATGPT added line:
            builder.Services.AddDbContext<ArcaneVaultContext>(options =>
    options.UseSqlite("Data Source=ArcaneVault.db"));

            var app = builder.Build();

            //TEMPARORY 
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ArcaneVaultContext>();

                // Add Staff role if it doesn't exist
                if (!context.ArcaneVaultUserRoles.Any(r => r.RoleId == 1))
                {
                    context.ArcaneVaultUserRoles.Add(new ArcaneVaultUserRole
                    {
                        RoleId = 1,
                        RoleName = "Staff"
                    });
                }

                // Add User role if it doesn't exist
                if (!context.ArcaneVaultUserRoles.Any(r => r.RoleId == 2))
                {
                    context.ArcaneVaultUserRoles.Add(new ArcaneVaultUserRole
                    {
                        RoleId = 2,
                        RoleName = "User"
                    });
                }

                if (!context.ArcaneVaultUsers.Any())
                {
                    context.ArcaneVaultUsers.Add(new ArcaneVaultUser
                    {
                        UserName = "admin",
                        Email = "admin@nyp.edu.sg",
                        Password = "Admin123",
                        IsDeleted = false,
                        RoleId = 1
                    });
                }

                context.SaveChanges();
            }
            //TEMPARORY ^^

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            // Serve static files so uploaded images are accessible
            app.UseStaticFiles();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
