using ArcaneVault_Web.DAL;

namespace ArcaneVault_Web
{
    public class Program
    {
        /// <summary>
        /// Base address of the backend API. Kept in one place so the typed
        /// clients below cannot drift apart.
        /// </summary>
        private const string ApiBaseUrl = "https://localhost:7297/";

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();

            builder.Services.AddDistributedMemoryCache();

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Typed HTTP clients for the backend API.
            AddApiClient<CategoryDAL>(builder);
            AddApiClient<CatalogItemApiClient>(builder);
            AddApiClient<WishlistApiClient>(builder);
            AddApiClient<NotificationApiClient>(builder);
            AddApiClient<ReviewApiClient>(builder);
            AddApiClient<SubmissionApiClient>(builder);
            AddApiClient<CartApiClient>(builder);
            AddApiClient<OrderApiClient>(builder);
            AddApiClient<TradeApiClient>(builder);
            AddApiClient<AnalyticsApiClient>(builder);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseSession();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapRazorPages()
               .WithStaticAssets();

            app.Run();
        }

        private static void AddApiClient<TClient>(WebApplicationBuilder builder)
            where TClient : class
        {
            builder.Services.AddHttpClient<TClient>(client =>
            {
                client.BaseAddress = new Uri(ApiBaseUrl);
            });
        }
    }
}
