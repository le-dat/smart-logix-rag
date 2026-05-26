namespace SmartLogix.WebApi.Models
{
    /// <summary>
    /// Represents a system user with hashed credentials for JWT authentication.
    /// </summary>
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "User"; // "Admin" | "User"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
