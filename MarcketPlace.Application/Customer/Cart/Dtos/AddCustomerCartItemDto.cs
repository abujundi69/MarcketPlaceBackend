using MarcketPlace.Domain.Enums;

namespace MarcketPlace.Application.Customer.Cart.Dtos
{
    public class AddCustomerCartItemDto
    {
        public int ProductId { get; set; }
        public int? ProductVariantId { get; set; }

        public List<int>? SelectedOptionValueIds { get; set; }

        public ProductPurchaseInputMode PurchaseEntryMode { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? RequestedAmount { get; set; }
    }
}