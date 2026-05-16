namespace El_Sim.Web.Services;

public static class PasswordHasher
{
    public static string HashPassword(string password, string salt)
    {
        var saltBytes = Encoding.UTF8.GetBytes(salt);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, 120000, HashAlgorithmName.SHA256, 32);

        return Convert.ToBase64String(hash);
    }

    public static bool VerifyPassword(string password, string hash, string salt)
    {
        var hashBytes = Convert.FromBase64String(hash);
        var saltBytes = Encoding.UTF8.GetBytes(salt);
        var candidate = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, 120000, HashAlgorithmName.SHA256, 32);

        if (CryptographicOperations.FixedTimeEquals(hashBytes, candidate))
        {
            return true;
        }

        try
        {
            var legacySaltBytes = Convert.FromBase64String(salt);
            var legacyCandidate = Rfc2898DeriveBytes.Pbkdf2(password, legacySaltBytes, 120000, HashAlgorithmName.SHA256, 32);

            return CryptographicOperations.FixedTimeEquals(hashBytes, legacyCandidate);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
