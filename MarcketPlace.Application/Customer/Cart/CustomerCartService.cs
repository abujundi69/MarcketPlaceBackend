using MarcketPlace.Application.Customer.Cart.Dtos;
using MarcketPlace.Domain.Entities;
using MarcketPlace.Domain.Enums;
using MarcketPlace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarcketPlace.Application.Customer.Cart
{
    public class CustomerCartService : ICustomerCartService
    {
        private readonly AppDbContext _context;

        public CustomerCartService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CustomerCartDto> GetCartAsync(
            int customerUserId,
            CancellationToken cancellationToken = default)
        {
            var customerId = await GetCustomerIdAsync(customerUserId, cancellationToken);

            var cartItems = await _context.CartItems
                .AsNoTracking()
                .Include(x => x.Product)
                    .ThenInclude(x => x!.Store)
                .Where(x => x.CustomerId == customerId)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            return BuildCartDto(cartItems);
        }

        public async Task<CustomerCartDto> AddItemAsync(
            int customerUserId,
            AddCustomerCartItemDto dto,
            CancellationToken cancellationToken = default)
        {
            if (dto is null)
                throw new InvalidOperationException("البيانات المرسلة غير صالحة.");

            if (dto.ProductId <= 0)
                throw new InvalidOperationException("المنتج غير صالح.");

            if (dto.Quantity <= 0)
                throw new InvalidOperationException("الكمية يجب أن تكون أكبر من صفر.");

            var customerId = await GetCustomerIdAsync(customerUserId, cancellationToken);

            var product = await _context.Products
                .Include(x => x.Store)
                .FirstOrDefaultAsync(x => x.Id == dto.ProductId, cancellationToken);

            if (product is null)
                throw new KeyNotFoundException("المنتج غير موجود.");

            if (product.StoreId is null || product.Store is null)
                throw new InvalidOperationException("هذا المنتج غير مرتبط بمتجر.");

            if (!product.Store.IsActive)
                throw new InvalidOperationException("المتجر غير نشط.");

            if (product.ApprovalStatus != ProductApprovalStatus.Approved)
                throw new InvalidOperationException("المنتج غير معتمد.");

            if (product.StockQuantity <= 0)
                throw new InvalidOperationException("المنتج غير متوفر حاليًا.");

            var existingCartItem = await _context.CartItems
                .FirstOrDefaultAsync(
                    x => x.CustomerId == customerId && x.ProductId == dto.ProductId,
                    cancellationToken);

            var targetQuantity = dto.Quantity;

            if (existingCartItem is not null)
                targetQuantity += existingCartItem.Quantity;

            if (targetQuantity > product.StockQuantity)
                throw new InvalidOperationException("الكمية المطلوبة أكبر من المخزون المتاح.");

            var utcNow = DateTime.UtcNow;

            if (existingCartItem is null)
            {
                var cartItem = new CartItem
                {
                    CustomerId = customerId,
                    ProductId = product.Id,
                    Quantity = dto.Quantity,
                    CreatedAt = utcNow,
                    UpdatedAt = null
                };

                _context.CartItems.Add(cartItem);
            }
            else
            {
                existingCartItem.Quantity = targetQuantity;
                existingCartItem.UpdatedAt = utcNow;
            }

            await _context.SaveChangesAsync(cancellationToken);

            return await GetCartAsync(customerUserId, cancellationToken);
        }

        public async Task<CustomerCartDto> UpdateQuantityAsync(
            int customerUserId,
            int cartItemId,
            UpdateCustomerCartItemQuantityDto dto,
            CancellationToken cancellationToken = default)
        {
            if (dto is null)
                throw new InvalidOperationException("البيانات المرسلة غير صالحة.");

            if (dto.Quantity <= 0)
                throw new InvalidOperationException("الكمية يجب أن تكون أكبر من صفر.");

            var customerId = await GetCustomerIdAsync(customerUserId, cancellationToken);

            var cartItem = await _context.CartItems
                .Include(x => x.Product)
                    .ThenInclude(x => x!.Store)
                .FirstOrDefaultAsync(
                    x => x.Id == cartItemId && x.CustomerId == customerId,
                    cancellationToken);

            if (cartItem is null)
                throw new KeyNotFoundException("عنصر السلة غير موجود.");

            if (cartItem.Product is null)
                throw new InvalidOperationException("المنتج المرتبط بعنصر السلة غير صالح.");

            if (cartItem.Product.StoreId is null || cartItem.Product.Store is null)
                throw new InvalidOperationException("المنتج غير مرتبط بمتجر.");

            if (!cartItem.Product.Store.IsActive)
                throw new InvalidOperationException("المتجر غير نشط.");

            if (cartItem.Product.ApprovalStatus != ProductApprovalStatus.Approved)
                throw new InvalidOperationException("المنتج غير معتمد.");

            if (dto.Quantity > cartItem.Product.StockQuantity)
                throw new InvalidOperationException("الكمية المطلوبة أكبر من المخزون المتاح.");

            cartItem.Quantity = dto.Quantity;
            cartItem.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return await GetCartAsync(customerUserId, cancellationToken);
        }

        public async Task RemoveItemAsync(
            int customerUserId,
            int cartItemId,
            CancellationToken cancellationToken = default)
        {
            var customerId = await GetCustomerIdAsync(customerUserId, cancellationToken);

            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(
                    x => x.Id == cartItemId && x.CustomerId == customerId,
                    cancellationToken);

            if (cartItem is null)
                throw new KeyNotFoundException("عنصر السلة غير موجود.");

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task ClearCartAsync(
            int customerUserId,
            CancellationToken cancellationToken = default)
        {
            var customerId = await GetCustomerIdAsync(customerUserId, cancellationToken);

            var cartItems = await _context.CartItems
                .Where(x => x.CustomerId == customerId)
                .ToListAsync(cancellationToken);

            if (cartItems.Count == 0)
                return;

            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync(cancellationToken);
        }

        private async Task<int> GetCustomerIdAsync(
            int customerUserId,
            CancellationToken cancellationToken)
        {
            var customer = await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == customerUserId, cancellationToken);

            if (customer is null)
                throw new KeyNotFoundException("الزبون غير موجود.");

            return customer.Id;
        }

        private static CustomerCartDto BuildCartDto(List<CartItem> cartItems)
        {
            if (cartItems.Count == 0)
            {
                return new CustomerCartDto
                {
                    TotalItemsCount = 0,
                    StoresCount = 0,
                    Subtotal = 0,
                    Stores = new List<CustomerCartStoreDto>()
                };
            }

            var stores = cartItems
                .GroupBy(x => new
                {
                    StoreId = x.Product.StoreId ?? 0,
                    StoreNameAr = x.Product.Store?.NameAr ?? "متجر غير معروف",
                    StoreNameEn = x.Product.Store?.NameEn ?? "Unknown Store"
                })
                .Select(g => new CustomerCartStoreDto
                {
                    StoreId = g.Key.StoreId,
                    StoreNameAr = g.Key.StoreNameAr,
                    StoreNameEn = g.Key.StoreNameEn,
                    StoreSubtotal = g.Sum(i => i.Product.Price * i.Quantity),
                    Items = g.Select(i => new CustomerCartItemDto
                    {
                        CartItemId = i.Id,
                        ProductId = i.ProductId,
                        ProductNameAr = i.Product.NameAr,
                        ProductNameEn = i.Product.NameEn,
                        ProductImage = i.Product.Image,
                        UnitPrice = i.Product.Price,
                        Quantity = i.Quantity,
                        LineTotal = i.Product.Price * i.Quantity,
                        AvailableStock = i.Product.StockQuantity
                    }).ToList()
                })
                .OrderBy(x => x.StoreNameAr)
                .ToList();

            return new CustomerCartDto
            {
                TotalItemsCount = cartItems.Sum(x => x.Quantity),
                StoresCount = stores.Count,
                Subtotal = stores.Sum(x => x.StoreSubtotal),
                Stores = stores
            };
        }
    }
}