using HealthCareManagementSystem.Repositories;
using static HealthCareManagementSystem.Repositories.IRepository;
using Microsoft.AspNetCore.Identity.UI.Services;


namespace HealthCareManagementSystem
{
    public static class AppConfigiration
    {
        public static void RegisterConfig(this IServiceCollection services)
        {
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddScoped<IRepository<ApplicationUserOtp>, Repository<ApplicationUserOtp>>();
            services.AddScoped<IDbInitializer, DbInitializer>();

        }
    }
}