using HealthCareManagementSystem.Repositories;
using Microsoft.AspNetCore.Identity.UI.Services;
using static HealthCareManagementSystem.Repositories.IRepository;


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
