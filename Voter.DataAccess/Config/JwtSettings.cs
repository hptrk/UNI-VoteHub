namespace Voter.DataAccess.Config
{
    public class JwtSettings
    {
        public string Secret { get; set; } = string.Empty;
        public string ValidIssuer { get; set; } = string.Empty;
        public string ValidAudience { get; set; } = string.Empty;
        public int ExpiryMinutes { get; set; }
    }
}
