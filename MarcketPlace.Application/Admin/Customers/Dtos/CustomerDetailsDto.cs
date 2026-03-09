namespace MarcketPlace.Application.Admin.Customers.Dtos
{
    public class CustomerDetailsDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        public string FullName { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;

        public bool IsActive { get; set; }
        public string StatusText { get; set; } = default!;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string CreatedAtText { get; set; } = default!;
        public string? UpdatedAtText { get; set; }

        public int OrdersCount { get; set; }
        public int StoreRatingsCount { get; set; }
        public int DriverRatingsCount { get; set; }
    }
}