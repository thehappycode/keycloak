using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace MyWebApi.Authentication;
public static class ConfigureServiceAuthentificationExtensions
{
    public static void ConfigureJWT(this IServiceCollection services, bool IsDevelopment, string publicKeyJWT)
    {
        services.AddTransient<IClaimsTransformation, ClaimsTransformation>();

        var authenticationBuilder = services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        });

        authenticationBuilder.AddJwtBearer(options =>
        {
            // JWT Token Validation
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = true,
                ValidIssuers = new[] { "http://localhost:8080/realms/MyRealm" },
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = BuildRSAKey(publicKeyJWT),
                ValidateLifetime = true
            };

            // Event Authentication Handlers
            options.Events = new JwtBearerEvents()
            {
                OnTokenValidated = p =>
                {
                    Console.WriteLine("Use successfully authenticated");
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = p =>
                {
                    p.NoResult();

                    p.Response.StatusCode = 500;
                    p.Response.ContentType = "text/plain";

                    if (IsDevelopment)
                        return p.Response.WriteAsync(p.Exception.ToString());
                    return p.Response.WriteAsync("An error occured processing you authentication");
                }
            };
        });
    }

    private static RsaSecurityKey BuildRSAKey(string publicKeyJWT)
    {
        var rsa = RSA.Create();

        rsa.ImportSubjectPublicKeyInfo(
            source: Convert.FromBase64String(publicKeyJWT),
            bytesRead: out _
        );

        var issuerSigningKey = new RsaSecurityKey(rsa);
        return issuerSigningKey;
    }
}