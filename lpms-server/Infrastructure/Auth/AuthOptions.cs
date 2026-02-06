namespace LegalCaseManagement.Infrastructure.Auth
{
    public class AuthOptions
    {
        public string Issuer { get; set; } = "lpms-server";
        public string Audience { get; set; } = "lpms-client";
        public string JwtKey { get; set; } = string.Empty;
        public bool DevBypassEnabled { get; set; } = false;
        public int DevBypassUserId { get; set; } = 1;
    }
}
