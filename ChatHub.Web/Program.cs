using ChatHub.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);


/* =========================================================
   MVC
   ========================================================= */

builder.Services.AddControllersWithViews();


/* =========================================================
   HttpContext
   ========================================================= */

builder.Services.AddHttpContextAccessor();


/* =========================================================
   Token Handler
   ========================================================= */

builder.Services.AddTransient<UserTokenHandler>();


/* =========================================================
   API Client
   ========================================================= */

builder.Services.AddHttpClient<ChatHubApiClient>(client =>
{
    client.BaseAddress =
        new Uri("http://localhost:5070/");
})
.AddHttpMessageHandler<UserTokenHandler>();


/* =========================================================
   Authentication
   ========================================================= */

builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath =
            "/Auth/Account/Login";

        options.AccessDeniedPath =
            "/Auth/Account/AccessDenied";

        options.ExpireTimeSpan =
            TimeSpan.FromHours(8);

        options.SlidingExpiration =
            true;
    });


/* =========================================================
   Authorization
   ========================================================= */

builder.Services.AddAuthorization();


var app = builder.Build();


/* =========================================================
   Error Handling
   ========================================================= */

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(
        "/User/Home/Error");

    app.UseHsts();
}


/* =========================================================
   Middleware
   ========================================================= */

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();


/* =========================================================
   Routing
   ========================================================= */

app.MapControllerRoute(
    name: "areas",
    pattern:
        "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern:
        "{area=User}/{controller=Home}/{action=Index}/{id?}");


app.Run();