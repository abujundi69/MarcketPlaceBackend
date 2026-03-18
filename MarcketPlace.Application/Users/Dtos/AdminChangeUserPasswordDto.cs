namespace MarcketPlace.Application.Users.Dtos
{
    public class AdminChangeUserPasswordDto
    {
        public string NewPassword { get; set; } = default!;
        public string ConfirmPassword { get; set; } = default!;
    }
}