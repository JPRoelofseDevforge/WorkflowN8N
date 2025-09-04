namespace WorkflowApi.Models;

public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public UserDto User { get; set; } = null!;
}

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new List<string>();
    public List<string> Permissions { get; set; } = new List<string>();
    public UserPreferences Preferences { get; set; } = new UserPreferences();
}

public class UserPreferences
{
    public string Theme { get; set; } = "light";
    public bool Notifications { get; set; } = true;
    public string Language { get; set; } = "en";
}