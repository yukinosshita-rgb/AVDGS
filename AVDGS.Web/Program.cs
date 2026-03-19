using Microsoft.AspNetCore.Authentication.Cookies;
using AVDGS_DAL;
using AVDGS_BLL;
using AVDGS.Web.Services;
using AVDGS.Web.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

// Weather
builder.Services.Configure<WeatherOptions>(builder.Configuration.GetSection("Weather"));
builder.Services.AddHttpClient<IWeatherService, OpenMeteoWeatherService>();

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Auth
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// Connection string
string cs = builder.Configuration.GetConnectionString("AvdgsDb")
    ?? throw new Exception("Missing ConnectionStrings:AvdgsDb in appsettings.json");

// DAL + BLL core
builder.Services.AddSingleton(new DbConfig(cs));
builder.Services.AddScoped<UserDal>();
builder.Services.AddScoped<AuthService>();

// Operations DAL/BLL
builder.Services.AddScoped<OperationsDal>();
builder.Services.AddScoped<OperationsService>();

// ✅ Reports DAL/BLL (NEW)
builder.Services.AddScoped<ReportsDal>();
builder.Services.AddScoped<ReportsService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();