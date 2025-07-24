using System.Text.RegularExpressions;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


// Register DbContext and IDbConnectionFactory
builder.Services.AddScoped<SqlUserService>();
builder.Services.AddHttpsRedirection(options =>
{
    options.HttpsPort = 5111;
});
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Required for session to work
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.Use(async (context, next) =>
{
    Console.WriteLine($"Request Path: {context.Request.Path}");
    await next();
});



app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.Use(async (ctx, next) =>
{
    var path = ctx.Request.Path;
    var role = ctx.Session.GetString("role");

    if (path.StartsWithSegments("/admin") && role != "admin")
    {
        ctx.Response.StatusCode = 403;
        await ctx.Response.WriteAsync("Acceso no autorizado.");
        return;
    }

    await next();
});


app.UseAuthorization();
////Add test users
//using var scope = app.Services.CreateScope();
//var seededService = scope.ServiceProvider.GetRequiredService<SqlUserService>();
//seededService.AddUser("admin", BCrypt.Net.BCrypt.HashPassword("admin123"), "admin");
//seededService.AddUser("user", BCrypt.Net.BCrypt.HashPassword("user123"), "user");
//seededService.AddUser("guest", BCrypt.Net.BCrypt.HashPassword("guest123"), "user");


// Route for form submission
app.MapPost("/submit", async (HttpContext context, SqlUserService userService) =>
{

    var form = await context.Request.ReadFormAsync();
    var username = form["username"];
    var password = form["password"];
    
    if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_]{3,20}$"))
    {
        return Results.BadRequest("Invalid username format.");
    }
    if (Regex.IsMatch(username, @"(?i)<script\b|--|;|'", RegexOptions.Compiled) || Regex.IsMatch(password, @"(?i)<script\b|--|;|'", RegexOptions.Compiled))
    {
        return Results.BadRequest("Invalid input detected.");
    }

    var user = userService.GetByUsername(username);
    if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)){
        return Results.Unauthorized();
    }

    // Login succesful → store cookie
    context.Session.SetString("user", user.Username);
    context.Session.SetString("role", user.Role);
;
    
    return Results.Json(new
    {
        success = true,
        user = user.Username,
        role = user.Role
    });
});

app.MapGet("/admin/dashboard", (HttpContext context) =>
{
    var user = context.Session.GetString("user");
    return Results.Ok(new
    {
        message = "Welcome to the admin dashboard!"
    });
});

app.MapGet("/", () => "Welcome to SafeVault!");

app.Run();




