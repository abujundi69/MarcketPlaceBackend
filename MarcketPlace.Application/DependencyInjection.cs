using MarcketPlace.Application.Account;
using MarcketPlace.Application.Admin.Categories;
using MarcketPlace.Application.Vendor.ProductRequests;
using MarcketPlace.Application.Vendor.Products;
using MarcketPlace.Application.Admin.Customers;
using MarcketPlace.Application.Admin.Dashboard;
using MarcketPlace.Application.Admin.DeliveryZones;
using MarcketPlace.Application.Admin.Drivers;
using MarcketPlace.Application.Admin.Notifications;
using MarcketPlace.Application.Admin.Orders;
using MarcketPlace.Application.Admin.ProductRequests;
using MarcketPlace.Application.Admin.Vendors;
using MarcketPlace.Application.Admin.Products;
using MarcketPlace.Application.Admin.SystemSettings;
using MarcketPlace.Application.Auth;
using MarcketPlace.Application.Customer.Cart;
using MarcketPlace.Application.Customer.Catalog;
using MarcketPlace.Application.Customer.DriverRatings;
using MarcketPlace.Application.Customer.Favorites;
using MarcketPlace.Application.Customer.Locations;
using MarcketPlace.Application.Customer.Orders;
using MarcketPlace.Application.Driver.Orders;
using MarcketPlace.Application.Driver.Ratings;
using MarcketPlace.Application.Users;
using MarcketPlace.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace MarcketPlace.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IDriverAdminService, DriverAdminService>();
            services.AddScoped<IAdminProductRequestService, AdminProductRequestService>();
            services.AddScoped<IVendorAdminService, VendorAdminService>();
            services.AddScoped<IAdminProductService, AdminProductService>();
            services.AddScoped<IAdminProductDiscountService, AdminProductDiscountService>();
            services.AddScoped<IAdminNotificationService, AdminNotificationService>();
            services.AddScoped<IDeliveryZoneAdminService, DeliveryZoneAdminService>();
            services.AddScoped<ISystemSettingAdminService, SystemSettingAdminService>();
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            services.AddScoped<IAdminOrderService, AdminOrderService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICustomerAdminService, CustomerAdminService>();
            services.AddScoped<IMyAccountService, MyAccountService>();
            services.AddScoped<IAdminDashboardService, AdminDashboardService>();
            services.AddScoped<ICustomerLocationService, CustomerLocationService>();
            services.AddScoped<IAdminCategoryService, AdminCategoryService>();
            services.AddScoped<ICustomerCatalogService, CustomerCatalogService>();
            services.AddScoped<ICustomerCartService, CustomerCartService>();
            services.AddScoped<ICustomerOrderService, CustomerOrderService>();
            services.AddScoped<IDriverOrderService, DriverOrderService>();
            services.AddScoped<IDriverRatingService, DriverRatingService>();
            services.AddScoped<ICustomerFavoriteService, CustomerFavoriteService>();
            services.AddScoped<ICustomerDriverRatingService, CustomerDriverRatingService>();
            services.AddScoped<IVendorProductRequestService, VendorProductRequestService>();
            services.AddScoped<IVendorProductService, VendorProductService>();

            return services;
        }
    }
}