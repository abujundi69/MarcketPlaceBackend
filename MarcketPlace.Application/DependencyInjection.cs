using MarcketPlace.Application.Admin.Drivers;
using MarcketPlace.Application.Admin.Stores;
using MarcketPlace.Application.Admin.Vendors;
using MarcketPlace.Application.Users;
using Microsoft.Extensions.DependencyInjection;

namespace MarcketPlace.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IDriverAdminService, DriverAdminService>();
            services.AddScoped<IVendorAdminService, VendorAdminService>();
            services.AddScoped<IStoreAdminService, StoreAdminService>();

            return services;
        }
    }
}