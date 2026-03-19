namespace MarcketPlace.Application.Auth.Dtos
{
    public class CustomerRegisterRequestDto
    {
        public string FullName { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string ConfirmPassword { get; set; } = default!;
    }
}