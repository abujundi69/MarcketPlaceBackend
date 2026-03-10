namespace MarcketPlace.Application.Admin.Orders.Dtos
{
    public class AdminOrderListItemDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = default!;
        public string CustomerName { get; set; } = default!;
        public string StoresText { get; set; } = default!;
        public string StatusText { get; set; } = default!;
        public string? DriverName { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedAtText { get; set; } = default!;
    }
}