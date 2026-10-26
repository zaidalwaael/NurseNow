public class CreateAdminDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
}
