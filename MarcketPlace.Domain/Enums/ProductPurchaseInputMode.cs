namespace MarcketPlace.Domain.Enums
{
    public enum ProductPurchaseInputMode
    {
        QuantityOnly = 1,     // الشراء بالكمية فقط
        AmountOnly = 2,       // الشراء بالمبلغ فقط
        QuantityOrAmount = 3  // الشراء بالكمية أو بالمبلغ
    }
}