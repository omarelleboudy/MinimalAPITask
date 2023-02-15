using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Minimal.Data;
using Minimal.Helpers;
using Minimal.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure Services
// Configuring our connection to the Database.
builder.Services.AddDbContext<ApplicationDbContext>(opt => opt.UseSqlServer(builder.Configuration["ConnectionString"]));
    
// Configuring the Authentication Schema
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    // Configuring the Bearer Token
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey
        (Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true
    };
});


builder.Services.AddAuthorization();

var app = builder.Build();

// Configure
app.UseAuthentication();
app.UseAuthorization();


app.MapPost("/users", async (UserModel userModel, ApplicationDbContext db) =>
{
    // Since the ID is created using the Hash of the email, let's create it and validate first if user exists before creating a new one.
    string userId = Helper.HashId(userModel.Email);

    var existingUser = await db.Users.FindAsync(userId);

    if (existingUser != null) // User exists, no creation occurs. 
        return Results.Conflict("Email already exists. Please use a different email when adding a user.");

    // Create a new user
    var user = new User();

    user.Id = userId;
    user.FirstName = userModel.FirstName;
    user.LastName = userModel.LastName;
    user.Email = userModel.Email;
    user.MarketingConsent = userModel.MarketingConsent;

    // Add User to Database
    db.Users.Add(user);
    await db.SaveChangesAsync();

    // Get the User Access Token
    var stringToken = Helper.GetToken(user, builder);

    // return user Id and Access Token
    return Results.Created($"/users/{user.Id}", new {Id= user.Id, AccessToken = stringToken });
});


app.MapGet("/users/{id}", async (string id, ApplicationDbContext db) =>
    {
        // Check if user exists.
        var user = await db.Users.FindAsync(id);

        if (user == null) return Results.NotFound();

        // create New Model to return user in
        var model = new UserModel();

        model.Id = user.Id;
        model.FirstName = user.FirstName;
        model.LastName = user.LastName;
        model.Email = user.Email;
        model.MarketingConsent = user.MarketingConsent;

        // if User consents for marketing, return full model.
        if (model.MarketingConsent)
            return Results.Ok(model);
        else // User did not consent, return everything minus email.
            return Results.Ok(new {Id = model.Id,
                FirstName = model.FirstName,
                LastName = model.LastName,
                MarketingConsent = model.MarketingConsent });

    }).RequireAuthorization(); // This endpoint requires Authorization.













app.Run();