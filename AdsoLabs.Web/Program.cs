using AdsoLabs.Web.Components;
using AdsoLabs.Web.Services;
using AdsoLabs.Web.Services.Api;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Server.Circuits;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddHubOptions(o =>
    {
        o.MaximumReceiveMessageSize = 50 * 1024 * 1024;
        o.EnableDetailedErrors = true;
        o.ClientTimeoutInterval = TimeSpan.FromMinutes(5);
        o.HandshakeTimeout = TimeSpan.FromMinutes(1);
        o.KeepAliveInterval = TimeSpan.FromSeconds(30);
    });

builder.Services.AddScoped<SesionService>();
builder.Services.AddScoped<FichaStateService>();
builder.Services.AddScoped<CircuitHandler, ErrorCircuitHandler>();
builder.Services.AddHttpContextAccessor();

// Firma un token interno corto por cada llamada a AdsoLabs.API a partir de
// la cookie ya validada del usuario actual (ver InternalTokenHandler.cs).
builder.Services.AddTransient<InternalTokenHandler>();

void ConfigurarCliente(HttpClient client)
{
    var apiUrl = builder.Configuration["ApiUrl"] ?? "http://localhost:5295/";
    if (!apiUrl.EndsWith('/')) apiUrl += "/";
    client.BaseAddress = new Uri(apiUrl);
}

builder.Services.AddHttpClient<AuthApiService>(ConfigurarCliente);
builder.Services.AddHttpClient<ActivacionCuentaApiService>(ConfigurarCliente);
builder.Services.AddHttpClient<AprendizApiService>(ConfigurarCliente).AddHttpMessageHandler<InternalTokenHandler>();
builder.Services.AddHttpClient<InstructorApiService>(ConfigurarCliente).AddHttpMessageHandler<InternalTokenHandler>();
builder.Services.AddHttpClient<FichaApiService>(ConfigurarCliente).AddHttpMessageHandler<InternalTokenHandler>();
builder.Services.AddHttpClient<CompetenciaApiService>(ConfigurarCliente).AddHttpMessageHandler<InternalTokenHandler>();
builder.Services.AddHttpClient<ProgramacionApiService>(ConfigurarCliente).AddHttpMessageHandler<InternalTokenHandler>();
builder.Services.AddHttpClient<InformeAprendizApiService>(ConfigurarCliente).AddHttpMessageHandler<InternalTokenHandler>();
builder.Services.AddHttpClient<ImportacionApiService>(ConfigurarCliente).AddHttpMessageHandler<InternalTokenHandler>();
builder.Services.AddHttpClient<AsistenciaApiService>(ConfigurarCliente).AddHttpMessageHandler<InternalTokenHandler>();
builder.Services.AddHttpClient<DashboardApiService>(ConfigurarCliente).AddHttpMessageHandler<InternalTokenHandler>();
builder.Services.AddHttpClient<RepositorioApiService>(ConfigurarCliente).AddHttpMessageHandler<InternalTokenHandler>();
builder.Services.AddHttpClient<ReportesApiService>(ConfigurarCliente).AddHttpMessageHandler<InternalTokenHandler>();
builder.Services.AddHttpClient<PatrocinioApiService>(ConfigurarCliente).AddHttpMessageHandler<InternalTokenHandler>();
builder.Services.AddHttpClient<MonitoriaApiService>(ConfigurarCliente).AddHttpMessageHandler<InternalTokenHandler>();
builder.Services.AddHttpClient<ProfileApiService>(ConfigurarCliente).AddHttpMessageHandler<InternalTokenHandler>();
builder.Services.AddHttpClient<NotificacionApiService>(ConfigurarCliente).AddHttpMessageHandler<InternalTokenHandler>();
builder.Services.AddHttpClient<PrediccionIAApiService>(ConfigurarCliente).AddHttpMessageHandler<InternalTokenHandler>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/";
        options.AccessDeniedPath = "/";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.Name = "AdsoLabs.Auth";
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapRazorPages();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();