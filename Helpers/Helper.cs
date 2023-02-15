using Microsoft.IdentityModel.Tokens;
using Minimal.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net.NetworkInformation;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Minimal.Helpers;


public class Helper
{
    public static string HashId(string email)
    {
        // We set our constants for the hashing.
        const int keySize = 32;
        const int iterations = 350000;
        const string saltString = "450d0b0db2bcf4adde5032eca1a7c416e560cf44"; // Provided in the problem.

        HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA1;

        // Creating the full hash
        var salt = Encoding.ASCII.GetBytes(saltString);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(email),
            salt,
            iterations,
            hashAlgorithm,
            keySize);
        return Convert.ToHexString(hash).ToLower();
    }

    public static string GetToken(User user, WebApplicationBuilder builder)
    {
        // We get the data we set for JWT from app settings.
        var issuer = builder.Configuration["Jwt:Issuer"];
        var audience = builder.Configuration["Jwt:Audience"];
        var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]);
        // Here we create a security token descriptor using our user Email and First Name
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("Id", Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.FirstName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString())
             }),
            Expires = DateTime.UtcNow.AddMinutes(5), // It expires in 5 minutes
            Issuer = issuer, // Issuer and Audience from app settings.
            Audience = audience,
            SigningCredentials = new SigningCredentials
            (new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha512Signature) // Security Algorithm
        };
        var tokenHandler = new JwtSecurityTokenHandler(); // We create a Token Handler to use it for Creating the Token
        var token = tokenHandler.CreateToken(tokenDescriptor); 
        var jwtToken = tokenHandler.WriteToken(token); 
        var stringToken = tokenHandler.WriteToken(token);

        return stringToken;

    }


}