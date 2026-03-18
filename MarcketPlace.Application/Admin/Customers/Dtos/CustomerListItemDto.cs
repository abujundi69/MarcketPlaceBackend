namespace MarcketPlace.Application.Admin.Customers.Dtos
{
    public class CustomerListItemDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        public string FullName { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;

        public bool IsActive { get; set; }
        public string StatusText { get; set; } = default!;

        public DateTime CreatedAt { get; set; }
        public string CreatedAtText { get; set; } = default!;

        public int OrdersCount { get; set; }
        public int DriverRatingsCount { get; set; }
    }
}