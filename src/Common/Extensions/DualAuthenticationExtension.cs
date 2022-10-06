using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Common.Extensions;

public static class DualAuthenticationExtension
{
    public static void AddDualAuthentication(this IServiceCollection services, IConfigurationRoot configuration)
    {
        services.AddAuthentication()
            
           .AddJwtBearer("B2C", options =>
        {
            options.Authority = configuration["DualAuth:B2C:Instance"] + "/"  + configuration["DualAuth:B2C:Domain"] + "/" + configuration["DualAuth:B2C:SignUpSignInPolicyId"] + "/v2.0";
            options.Audience = configuration["DualAuth:B2C:ClientId"];

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context => Task.CompletedTask,
                OnChallenge = context => Task.CompletedTask,
                OnAuthenticationFailed = (context) =>
                {
                    Console.WriteLine("OnAuthenticationFailed: " + context.Exception.Message);
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    Console.WriteLine("Validated: " + context.SecurityToken);
                    return Task.CompletedTask;
                }
            };
        })
           .AddJwtBearer("AAD", options =>
           {
               options.MetadataAddress = configuration["DualAuth:AAD:Instance"] + "/" + configuration["DualAuth:AAD:TenantId"] + "/v2.0/.well-known/openid-configuration";
               options.Authority = configuration["DualAuth:AAD:Instance"] + "/" + configuration["DualAuth:AAD:TenantId"];
               options.Audience = configuration["DualAuth:AAD:Audience"];
               options.TokenValidationParameters =
                   new TokenValidationParameters
                   {
                       ValidIssuer = $"https://sts.windows.net/{configuration["DualAuth:AAD:TenantId"]}/",
                   };

               options.Events = new JwtBearerEvents
               {
                   OnMessageReceived = context => Task.CompletedTask,
                   OnChallenge = context => Task.CompletedTask,
                   OnAuthenticationFailed = (context) =>
                   {
                       Console.WriteLine("OnAuthenticationFailed: " + context.Exception.Message);
                       return Task.CompletedTask;
                   },
                   OnTokenValidated = context =>
                   {
                       Console.WriteLine("Validated: " + context.SecurityToken);
                       return Task.CompletedTask;
                   }
               };
           });
        services
            .AddAuthorization(options =>
            {
                options.AddPolicy("DualUsers", new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes("AAD", "B2C")
                    .Build());
                options.DefaultPolicy = options.GetPolicy("DualUsers");
            });
    }
}