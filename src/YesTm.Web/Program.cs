using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Serilog;
using YesTm.Web.Common.Data;
using YesTm.Web.Common.Security;
using YesTm.Web.Features.Authentication;
using YesTm.Web.Features.Dashboard;
using YesTm.Web.Features.TimeBooking;
using YesTm.Web.Features.Administration;
using YesTm.Web.Features.Masters;

// ---------------------------------------------------------------------------
// Bootstrap Serilog as early as possible so even start-up errors are captured.
// ---------------------------------------------------------------------------
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/yes-tm-bootstrap-.log", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Yes Time Management web host");

    var builder = WebApplication.CreateBuilder(args);

    // Read full Serilog configuration from appsettings + enrich.
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("logs/yes-tm-.log", rollingInterval: RollingInterval.Day));

    // -----------------------------------------------------------------------
    // Vertical-slice Razor Pages: pages live under /Features, one folder per slice.
    // -----------------------------------------------------------------------
    builder.Services.AddRazorPages()
        .AddRazorPagesOptions(options =>
        {
            options.RootDirectory = "/Features";
            options.Conventions.AuthorizeFolder("/");                 // everything requires login...
            options.Conventions.AllowAnonymousToFolder("/Authentication"); // ...except the login slice
            options.Conventions.AuthorizeFolder("/Masters", Policies.AdminOrAbove); // masters: admin+
            options.Conventions.AuthorizeFolder("/JobCard", Policies.AdminOrAbove); // job cards: admin+
        });

    // Data access (Dapper over ODP.NET).
    builder.Services.AddSingleton<IDbConnectionFactory, OracleDbConnectionFactory>();

    // Feature repositories (one per slice).
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
    builder.Services.AddScoped<ITimeBookingRepository, TimeBookingRepository>();
    builder.Services.AddScoped<IAdministrationRepository, AdministrationRepository>();
    builder.Services.AddScoped<YesTm.Web.Features.JobCard.IJobCardRepository, YesTm.Web.Features.JobCard.JobCardRepository>();
    builder.Services.AddMasterRepositories();

    // -----------------------------------------------------------------------
    // Cookie authentication + role policies (USER / ADMIN / SITEADMIN).
    // -----------------------------------------------------------------------
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/Authentication/Login";
            options.LogoutPath = "/Authentication/Logout";
            options.AccessDeniedPath = "/Authentication/AccessDenied";
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;
            options.Cookie.Name = "YesTm.Auth";
        });

    builder.Services.AddAuthorizationBuilder()
        .AddPolicy(Policies.AdminOrAbove, p => p.RequireRole(Roles.Admin, Roles.SiteAdmin))
        .AddPolicy(Policies.SiteAdminOnly, p => p.RequireRole(Roles.SiteAdmin));

    builder.Services.AddHttpContextAccessor();

    // Persist data-protection keys to disk so auth cookies survive app-pool
    // recycles / restarts under IIS (otherwise users are logged out on recycle).
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(
            builder.Configuration["DataProtection:KeyPath"]
            ?? Path.Combine(builder.Environment.ContentRootPath, "keys")))
        .SetApplicationName("YesTime");

    var app = builder.Build();

    // Honour a path base first so static files, routing, auth and generated links
    // all account for the IIS sub-application virtual path (e.g. /YesTime).
    // Under IIS the ASP.NET Core Module normally sets this automatically; the
    // PathBase config value covers other reverse proxies (nginx, etc.).
    var pathBase = builder.Configuration["PathBase"];
    if (!string.IsNullOrWhiteSpace(pathBase))
    {
        // Normalise so a value like "YesTime" or "/YesTime/" can't crash startup.
        pathBase = "/" + pathBase.Trim().Trim('/');
        app.UsePathBase(pathBase);
    }

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Shared/Error");
        app.UseHsts();
    }

    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapRazorPages();

    // Root -> Dashboard, keeping any path base (so /YesTime -> /YesTime/Dashboard/Index).
    app.MapGet("/", (HttpContext ctx) => Results.Redirect($"{ctx.Request.PathBase}/Dashboard/Index"));

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Yes Time Management host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
