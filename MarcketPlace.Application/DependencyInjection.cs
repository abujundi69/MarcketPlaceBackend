using MarcketPlace.Application.Admin.Categories;
using MarcketPlace.Application.Admin.Customers;
using MarcketPlace.Application.Admin.DeliveryZones;
using MarcketPlace.Application.Admin.Drivers;
using MarcketPlace.Application.Admin.Orders;
using MarcketPlace.Application.Admin.ProductRequests;
using MarcketPlace.Application.Admin.Products;
using MarcketPlace.Application.Admin.Stores;
using MarcketPlace.Application.Admin.SystemSettings;
using MarcketPlace.Application.Admin.Vendors;
using MarcketPlace.Application.Auth;
using MarcketPlace.Application.Customer.Orders;
using MarcketPlace.Application.Customer.Stores;
using MarcketPlace.Application.Orders;
using MarcketPlace.Application.Users;
using MarcketPlace.Application.Vendor.Orders;
using MarcketPlace.Application.Vendor.ProductRequests;
using MarcketPlace.Application.Vendor.Products;
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
            services.AddScoped<IVendorAdminService, VendorAdminService>();
            services.AddScoped<IStoreAdminService, StoreAdminService>();
            services.AddScoped<ICategoryAdminService, CategoryAdminService>();
            services.AddScoped<IProductAdminService, ProductAdminService>();
            services.AddScoped<IVendorProductRequestService, VendorProductRequestService>();
            services.AddScoped<IAdminProductRequestService, AdminProductRequestService>();
            services.AddScoped<IDeliveryZoneAdminService, DeliveryZoneAdminService>();
            services.AddScoped<ISystemSettingAdminService, SystemSettingAdminService>();
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IAdminOrderService, AdminOrderService>();
            services.AddScoped<IVendorProductService, VendorProductService>();
            services.AddScoped<IVendorStoreOrderService, VendorStoreOrderService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICustomerAdminService, CustomerAdminService>();
            services.AddScoped<ICustomerStoreCatalogService, CustomerStoreCatalogService>();
           services.AddScoped<ICustomerOrderService, CustomerOrderService>();

            return services;
        }
    }
}