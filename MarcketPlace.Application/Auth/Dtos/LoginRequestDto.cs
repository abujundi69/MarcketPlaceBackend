namespace MarcketPlace.Application.Auth.Dtos
{
    public class LoginRequestDto
    {
        public string PhoneNumber { get; set; } = default!;
        public string Password { get; set; } = default!;
    }
}