using HealthCareManagementSystem.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HealthCareManagementSystem
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var connectionString =
                    builder.Configuration.GetConnectionString("DefaultConnection")
                        ?? throw new InvalidOperationException("Connection string"
                        + "'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {

                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;

            }).AddEntityFrameworkStores<ApplicationDbContext>()
           .AddDefaultTokenProviders();


            // Transient , Scoped , Singleton

            builder.Services.RegisterConfig();


            builder.Services.ConfigureApplicationCookie(options =>
            {
                // Changing the default routes for Identity
                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";

                options.SlidingExpiration = true;
            });

            var app = builder.Build();


            using (var scope = app.Services.CreateScope())
            {
                var inializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
                await inializer.InitializeAsnc();
            }



            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                //pattern: "{controller=Home}/{action=Index}/{id?}")
                //pattern: "{area=Identity}/{controller=Account}/{action=Register}/{id?}")
                pattern: "{area=Admin}/{controller=Clinic}/{action=Create}/{id?}")
                //pattern: "{area=Admin}/{controller=Doctor}/{action=Create}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
