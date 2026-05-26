using System;
using System.Security.Cryptography;

namespace SmartLogix.WebApi.Services
{
    public class PasswordHasher : IPasswordHasher
    {
        /// <summary>
        /// Hashes a plain-text password using PBKDF2 with SHA256, 100,000 iterations.
        /// Format: {base64_salt}:{base64_hash}
        /// </summary>
        public string HashPassword(string password)
        {
            byte[] salt = new byte[16];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);

            return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        }

        /// <summary>
        /// Verifies a plain-text password against a stored PBKDF2 hash string.
        /// </summary>
        public bool VerifyPassword(string password, string storedHash)
        {
            var parts = storedHash.Split(':');
            if (parts.Length != 2) return false;

            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] expectedHash = Convert.FromBase64String(parts[1]);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            byte[] actualHash = pbkdf2.GetBytes(32);

            return CryptographicOperations.FixedTimeEquals(expectedHash, actualHash);
        }
    }
}
