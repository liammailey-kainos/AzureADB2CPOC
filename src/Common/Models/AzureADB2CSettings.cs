namespace Common.Models
{
    public class AzureADB2CSettings
    {
        public string Tenant { get; set; } = string.Empty;
        public string AzureADB2CHostname { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string PolicySignUpSignIn { get; set; } = string.Empty;
        public string PolicyEditProfile { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string ResponseType { get; set; } = string.Empty;
        public string UnauthorisedPath { get; set; } = string.Empty;
        public string UserRoleName { get; set; } = string.Empty;
        public string NameClaimType { get; set; } = string.Empty;
        public string SignInPath { get; set; } = string.Empty;

        public string AuthorityBase
        {
            get
            {
                return $"https://{AzureADB2CHostname}/{Tenant}/";
            }
        }

        public string AuthoritySignInUp
        {
            get
            {
                return $"https://{AzureADB2CHostname}/{Tenant}/{PolicySignUpSignIn}/v2.0";
            }
        }

        public string AuthorityEditProfile
        {
            get
            {
                return $"https://{AzureADB2CHostname}/{Tenant}/{PolicyEditProfile}/v2.0";
            }
        }
    }
}
