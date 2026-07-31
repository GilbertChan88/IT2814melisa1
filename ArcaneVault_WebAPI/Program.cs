using ArcaneVault_WebAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace ArcaneVault_WebAPI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            builder.Services.AddDbContext<ArcaneVaultContext>(options =>
                options.UseSqlite("Data Source=ArcaneVault.db"));

            // Allow the Razor Pages frontend to load images and call the API
            // from a different origin/port during development.
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<ArcaneVaultContext>();

                // Apply any pending migrations, then seed development data.
                await context.Database.MigrateAsync();

                try
                {
                    await DbSeeder.SeedAsync(context);
                }
                catch (Exception ex)
                {
                    // Seeding is a development convenience. If it fails (for
                    // example against a database with unexpected reference
                    // data) the API should still start and serve requests.
                    var logger = scope.ServiceProvider
                        .GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Database seeding failed; continuing startup.");
                }
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseCors();

            // Serve static files so uploaded images are accessible
            app.UseStaticFiles();

            app.UseAuthorization();

            app.MapControllers();

            await app.RunAsync();
        }
    }
}
