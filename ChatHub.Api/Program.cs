using System.Text;

using ChatHub.Api.Hubs;
using ChatHub.Api.Security;

using ChatHub.Application.Authentication;
using ChatHub.Application.Common.Interfaces;
using ChatHub.Application.Workspaces;
using ChatHub.Application.Channels;
using ChatHub.Application.Messages;

using ChatHub.Domain.Entities;

using ChatHub.Infrastructure.Persistence;
using ChatHub.Infrastructure.Persistence.Seed;
using ChatHub.Infrastructure.Repositories;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;

using Microsoft.EntityFrameworkCore;

using Microsoft.IdentityModel.Tokens;


/* =========================================================
   Builder
   ========================================================= */

var builder =
    WebApplication.CreateBuilder(args);


/* =========================================================
   Controllers
   ========================================================= */

builder.Services.AddControllers();


/* =========================================================
   CORS
   ========================================================= */

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "ChatHubWeb",
        policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:5163")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});


/* =========================================================
   SignalR
   ========================================================= */

builder.Services.AddSignalR();


/* =========================================================
   Database
   ========================================================= */

Console.WriteLine(
    $"ConnectionString: " +
    $"{builder.Configuration.GetConnectionString("DefaultConnection")}");


builder.Services.AddDbContext<AppDbContext>(
    options =>
    {
        options.UseSqlServer(
            builder.Configuration.GetConnectionString(
                "DefaultConnection"));
    });


/* =========================================================
   Application Services
   ========================================================= */

builder.Services.AddScoped<
    IWorkspaceRepository,
    WorkspaceRepository>();

builder.Services.AddScoped<
    IWorkspaceService,
    WorkspaceService>();


builder.Services.AddScoped<
    IChannelRepository,
    ChannelRepository>();

builder.Services.AddScoped<
    IChannelService,
    ChannelService>();


builder.Services.AddScoped<
    IMessageRepository,
    MessageRepository>();

builder.Services.AddScoped<
    IMessageService,
    MessageService>();


builder.Services.AddScoped<
    IUserRepository,
    UserRepository>();


builder.Services.AddScoped<
    IAuthenticationService,
    AuthenticationService>();


/* =========================================================
   Password Hashing
   ========================================================= */

builder.Services.AddScoped<
    IPasswordHasher<User>,
    PasswordHasher<User>>();


/* =========================================================
   JWT Options
   ========================================================= */

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(
        JwtOptions.SectionName));


/* =========================================================
   JWT Token Service
   ========================================================= */

builder.Services.AddSingleton<
    IJwtTokenService,
    JwtTokenService>();


/* =========================================================
   JWT Authentication
   ========================================================= */

var jwtOptions =
    builder.Configuration
        .GetSection(
            JwtOptions.SectionName)
        .Get<JwtOptions>()
    ?? throw new InvalidOperationException(
        "JWT configuration is missing.");


if (string.IsNullOrWhiteSpace(
        jwtOptions.SecretKey))
{
    throw new InvalidOperationException(
        "JWT SecretKey is not configured.");
}


if (jwtOptions.SecretKey.Length < 32)
{
    throw new InvalidOperationException(
        "JWT SecretKey must be at least 32 characters long.");
}


builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(
        options =>
        {
            options.TokenValidationParameters =
                new TokenValidationParameters
                {
                    ValidateIssuer = true,

                    ValidIssuer =
                        jwtOptions.Issuer,


                    ValidateAudience = true,

                    ValidAudience =
                        jwtOptions.Audience,


                    ValidateIssuerSigningKey =
                        true,

                    IssuerSigningKey =
                        new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(
                                jwtOptions.SecretKey)),


                    ValidateLifetime =
                        true,


                    ClockSkew =
                        TimeSpan.FromSeconds(30)
                };


            /*
             * SignalR Browser Authentication
             *
             * Browser WebSocket/SSE requests may send
             * the JWT through access_token.
             */
            options.Events =
                new JwtBearerEvents
                {
                    OnMessageReceived =
                        context =>
                        {
                            var accessToken =
                                context.Request.Query[
                                    "access_token"];


                            var path =
                                context.HttpContext
                                    .Request.Path;


                            if (
                                !string.IsNullOrEmpty(
                                    accessToken) &&
                                path.StartsWithSegments(
                                    "/hubs/chat"))
                            {
                                context.Token =
                                    accessToken;
                            }


                            return Task.CompletedTask;
                        }
                };
        });


/* =========================================================
   Authorization
   ========================================================= */

builder.Services.AddAuthorization();


/* =========================================================
   Build
   ========================================================= */

var app =
    builder.Build();


/* =========================================================
   Middleware
   ========================================================= */

app.UseCors("ChatHubWeb");

app.UseAuthentication();

app.UseAuthorization();


/* =========================================================
   Database Initialization
   ========================================================= */

using (var scope =
       app.Services.CreateScope())
{
    var db =
        scope.ServiceProvider
            .GetRequiredService<AppDbContext>();


    await DbInitializer.InitializeAsync(
        db);


    /* -----------------------------------------------------
       Development Password Seed
       ----------------------------------------------------- */

    var passwordHasher =
        scope.ServiceProvider
            .GetRequiredService<
                IPasswordHasher<User>>();


    var users =
        await db.Users
            .Where(x =>
                x.UserName == "ali" ||
                x.UserName == "reza" ||
                x.UserName == "sara" ||
                x.UserName == "mohammad")
            .ToListAsync();


    var developmentPasswords =
        new Dictionary<
            string,
            string>
        {
            ["ali"] =
                "Ali@12345",

            ["reza"] =
                "Reza@12345",

            ["sara"] =
                "Sara@12345",

            ["mohammad"] =
                "Mohammad@12345"
        };


    var passwordsUpdated =
        false;


    foreach (var user in users)
    {
        if (
            string.IsNullOrWhiteSpace(
                user.PasswordHash) &&
            developmentPasswords.TryGetValue(
                user.UserName,
                out var password))
        {
            user.PasswordHash =
                passwordHasher.HashPassword(
                    user,
                    password);


            passwordsUpdated =
                true;
        }
    }


    if (passwordsUpdated)
    {
        await db.SaveChangesAsync();
    }
}


/* =========================================================
   Endpoints
   ========================================================= */

app.MapControllers();


app.MapHub<ChatHubHub>(
    "/hubs/chat");


app.MapGet(
    "/",
    () => "ChatHub API OK");


app.MapGet(
    "/test",
    () => new
    {
        success = true,
        message = "ChatHub API is running."
    });


/* =========================================================
   Run
   ========================================================= */

app.Run();

