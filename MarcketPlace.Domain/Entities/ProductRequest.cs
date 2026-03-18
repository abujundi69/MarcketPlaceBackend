using MarcketPlace.Domain.Enums;

namespace MarcketPlace.Domain.Entities
{
    public class ProductRequest
    {
        public int Id { get; set; }

        public int VendorId { get; set; }
        public int StoreId { get; set; }
        public int CategoryId { get; set; }

        /// <summary>
        /// جديد:
        /// أضفناه حتى طلب المنتج يدعم نفس فكرة وحدة البيع
        /// مثل kg / piece / L قبل اعتماد المنتج من الأدمن.
        /// </summary>
        public int? UnitId { get; set; }

        public string NameAr { get; set; } = default!;
        public string NameEn { get; set; } = default!;
        public string? DescriptionAr { get; set; }
        public string? DescriptionEn { get; set; }

        /// <summary>
        /// معدل:
        /// أعدنا الصورة داخل طلب المنتج لتكون Blob بدل Url
        /// حتى تتوافق مع أسلوب التخزين الحالي عندك.
        /// </summary>
        public byte[]? Image { get; set; }

        /// <summary>
        /// جديد:
        /// يحدد هل الطلب لمنتج عادي أم منتج له متغيرات.
        /// </summary>
        public ProductType ProductType { get; set; } = ProductType.Simple;

        /// <summary>
        /// جديد:
        /// يحدد هل المنتج يقبل الشراء بالكمية أو بالمبلغ أو الاثنين.
        /// </summary>
        public ProductPurchaseInputMode PurchaseInputMode { get; set; } = ProductPurchaseInputMode.QuantityOnly;

        /// <summary>
        /// جديد:
        /// لدعم المنتجات الموزونة والحجمية.
        /// </summary>
        public bool AllowDecimalQuantity { get; set; }

        public decimal Price { get; set; }
        public decimal? SalePrice { get; set; }
        public decimal? CostPrice { get; set; }

        public decimal StockQuantity { get; set; }

        public decimal MinStockQuantity { get; set; }
        public decimal MinPurchaseQuantity { get; set; } = 1m;
        public decimal? MaxPurchaseQuantity { get; set; }
        public decimal QuantityStep { get; set; } = 1m;

        public ProductApprovalStatus Status { get; set; }
        public string? AdminNote { get; set; }

        public DateTime RequestedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public int? ReviewedByUserId { get; set; }

        public int? ProductId { get; set; }

        public Vendor Vendor { get; set; } = default!;
        public Store Store { get; set; } = default!;
        public Category Category { get; set; } = default!;
        public ProductUnit? Unit { get; set; }
        public Product? Product { get; set; }

    }
}