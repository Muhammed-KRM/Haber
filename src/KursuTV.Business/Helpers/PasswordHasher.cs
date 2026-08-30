namespace KursuTV.Business.Helpers;

public static class PasswordHasher
{
    // BCrypt work factor: 12 (dÃ¶kÃ¼man Faz 7 - GÃ¼venlik)
    private const int WorkFactor = 12;

    public static string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public static bool Verify(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
