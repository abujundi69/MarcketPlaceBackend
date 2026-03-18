namespace MarcketPlace.Application.Admin.Customers.Dtos
{
    public class UpdateCustomerDto
    {
        public string FullName { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public bool IsActive { get; set; }
    }
}