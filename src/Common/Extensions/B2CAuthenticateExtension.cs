using Common.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Extensions
{
    public static class B2CAuthenticationExtension
    {
        public static IServiceCollection AddB2CAuthenticationServices(this IServiceCollection services, IConfiguration config)
        {

            // ReSharper disable once InconsistentNaming
            var azureADB2CSettings = new AzureADB2CSettings();
            config.GetSection("AzureADB2CSettings").Bind(azureADB2CSettings);
            
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;

            }).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options => { options.AccessDeniedPath = azureADB2CSettings.UnauthorisedPath; })
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.Authority = azureADB2CSettings.AuthoritySignInUp;
                options.ClientId = azureADB2CSettings.ClientId;
                options.ResponseType = azureADB2CSettings.ResponseType;
                options.SaveTokens = true;
                options.Scope.Add(azureADB2CSettings.Scope);
                options.ClientSecret = azureADB2CSettings.ClientSecret;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = azureADB2CSettings.NameClaimType
                };
                options.Events = new OpenIdConnectEvents
                {
                    OnTokenValidated = opt =>
                    {
                        var role = opt.Principal.FindFirstValue(azureADB2CSettings.UserRoleName);
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Role, role),
                        };
                        opt.Principal?.AddIdentity(new ClaimsIdentity(claims));
                        return Task.CompletedTask;
                    },
                    OnMessageReceived = context =>
                    {
                        if (string.IsNullOrEmpty(context.ProtocolMessage.Error) ||
                            string.IsNullOrEmpty(context.ProtocolMessage.ErrorDescription))
                        {
                            return Task.FromResult(0);
                        }

                        if (!context.ProtocolMessage.Error.Contains("access_denied"))
                        {
                            return Task.FromResult(0);
                        }
                        
                        context.HandleResponse();
                        context.Response.Redirect("/");
                        return Task.FromResult(0);
                    }
                };
            }).AddOpenIdConnect(azureADB2CSettings.PolicyEditProfile, GetOpenIdConnectOptionsForPolicy(azureADB2CSettings.PolicyEditProfile, azureADB2CSettings));

           var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var directory = new DirectoryInfo(path + "/CommonKeyRingFolder");
            services.AddDataProtection()
                .PersistKeysToFileSystem(directory)
                .SetApplicationName("SharedCookieApp");

            services.ConfigureApplicationCookie(options => {
                options.Cookie.Name = ".AspNet.SharedCookie";
            });
            return services;

        }

        private static Action<OpenIdConnectOptions> GetOpenIdConnectOptionsForPolicy(string policy, AzureADB2CSettings azureADB2CSettings) => options =>
        {
            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.Authority = azureADB2CSettings.AuthorityEditProfile;
            options.ClientId = azureADB2CSettings.ClientId;
            options.ResponseType = azureADB2CSettings.ResponseType;
            options.SaveTokens = true;
            options.Scope.Add(azureADB2CSettings.Scope);
            options.ClientSecret = azureADB2CSettings.ClientSecret;
            options.CallbackPath = $"{azureADB2CSettings.SignInPath}-{policy}";
            options.TokenValidationParameters = new TokenValidationParameters
            {
                NameClaimType = azureADB2CSettings.NameClaimType
            };
            options.Events = new OpenIdConnectEvents
            {
                OnMessageReceived = context =>
                {
                    if (string.IsNullOrEmpty(context.ProtocolMessage.Error) ||
                        string.IsNullOrEmpty(context.ProtocolMessage.ErrorDescription)) return Task.FromResult(0);
                    if (!context.ProtocolMessage.Error.Contains("access_denied")) return Task.FromResult(0);
                    context.HandleResponse();
                    context.Response.Redirect("/");
                    return Task.FromResult(0);
                }
            };
        };
    }
}

