using AdsoLabs.API.Hubs;
using AdsoLabs.Application.AppService.PrediccionIA;
using AdsoLabs.Application.AppService.Perfil;
using AdsoLabs.Application.AppService.Repositorio;
using AdsoLabs.Application.AppService.ActivacionCuenta;
using AdsoLabs.Application.AppService.Aprendiz;
using AdsoLabs.Application.AppService.Asistencia;
using AdsoLabs.Application.AppService.Auth;
using AdsoLabs.Application.AppService.Competencia;
using AdsoLabs.Application.AppService.Dashboard;
using AdsoLabs.Application.AppService.Fichas;
using AdsoLabs.Application.AppService.Patrocinio;
using AdsoLabs.Application.AppService.Reportes;
using AdsoLabs.Application.AppService.Importacion;
using AdsoLabs.Application.AppService.Informe;
using AdsoLabs.Application.AppService.Instructor;
using AdsoLabs.Application.AppService.Monitoria;
using AdsoLabs.Application.AppService.Programacion;
using AdsoLabs.Application.AppService.RecuperacionPassword;
using AdsoLabs.Application.Importacion;
using AdsoLabs.Application.Interfaces.Queries;
using AdsoLabs.Application.Interfaces.Repositories;
using AdsoLabs.Application.Interfaces.Services;
using AdsoLabs.Infrastructure.Data;
using AdsoLabs.Infrastructure.Data.Interceptors;
using AdsoLabs.Infrastructure.Queries;
using AdsoLabs.Infrastructure.Repositories;
using AdsoLabs.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Autenticacion interna servidor-a-servidor: AdsoLabs.Web mintea un JWT corto
// a partir de la cookie ya validada del usuario y lo manda como Bearer en
// cada llamada a esta API. El navegador nunca ve este token.
var internalAuthKey = builder.Configuration["InternalAuth:Key"]
    ?? throw new InvalidOperationException("Falta configurar InternalAuth:Key (ver appsettings.Development.json).");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["InternalAuth:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["InternalAuth:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(internalAuthKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

// Todo controller exige estar autenticado por defecto; los endpoints
// publicos (login, activacion, recuperacion de password) se marcan
// explicitamente con [AllowAnonymous].
builder.Services.AddControllers(options =>
    options.Filters.Add(new AuthorizeFilter()));
builder.Services.AddSignalR();

builder.Services.AddScoped<IHashService, HashService>();
builder.Services.AddScoped<IEmailServices, EmailServices>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAprendizService, AprendizService>();
builder.Services.AddScoped<ImportadorReporteJuicios>();
builder.Services.AddScoped<ImportadorFichas>();
builder.Services.AddScoped<ImportadorAprendices>();
builder.Services.AddScoped<IRecuperacionPasswordService, RecuperacionPasswordService>();
builder.Services.AddScoped<IActivacionCuentaService, ActivacionCuentaService>();
builder.Services.AddScoped<IFichaRepository, FichaRepository>();
builder.Services.AddScoped<FichaAppService>();

// AppServices
builder.Services.AddScoped<FichaAppService>();
builder.Services.AddScoped<AuthAppService>();
builder.Services.AddScoped<AprendizAppService>();
builder.Services.AddScoped<InstructorAppService>();
builder.Services.AddScoped<CompetenciaAppService>();
builder.Services.AddScoped<ActivacionCuentaAppService>();
builder.Services.AddScoped<RecuperacionPasswordAppService>();

builder.Services.AddScoped<IFichaRepository, FichaRepository>();
builder.Services.AddScoped<IFichaQueryService, FichaQueryService>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IPersonaRepository, PersonaRepository>();
builder.Services.AddScoped<IRolRepository, RolRepository>();
builder.Services.AddScoped<ITipoDocumentoRepository, TipoDocumentoRepository>();
builder.Services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
builder.Services.AddScoped<IAprendizRepository, AprendizRepository>();
builder.Services.AddScoped<IInstructorRepository, InstructorRepository>();
builder.Services.AddScoped<IAprendizQueryService, AprendizQueryService>();
builder.Services.AddScoped<IInstructorQueryService, InstructorQueryService>();
builder.Services.AddScoped<ICompetenciaQueryService, CompetenciaQueryService>();
builder.Services.AddScoped<IProgramacionQueryService, ProgramacionQueryService>();
builder.Services.AddScoped<IProgramacionRepository, ProgramacionRepository>();
builder.Services.AddScoped<ProgramacionAppService>();
builder.Services.AddScoped<IAsistenciaQueryService, AsistenciaQueryService>();
builder.Services.AddScoped<IAsistenciaRepository, AsistenciaRepository>();
builder.Services.AddScoped<AsistenciaAppService>();
builder.Services.AddScoped<IInformeRepository, InformeRepository>();
builder.Services.AddScoped<InformeAppService>();
builder.Services.AddScoped<IImportacionRepository, ImportacionRepository>();
builder.Services.AddScoped<ImportacionAppService>();
builder.Services.AddScoped<IDashboardQueryService, DashboardQueryService>();
builder.Services.AddScoped<DashboardAppService>();
builder.Services.AddScoped<IReportesQueryService, ReportesQueryService>();
builder.Services.AddScoped<ReportesAppService>();
builder.Services.AddScoped<InformeExcelGenerator>();
builder.Services.AddScoped<IPatrocinioQueryService, PatrocinioQueryService>();
builder.Services.AddScoped<IPatrocinioService, PatrocinioService>();
builder.Services.AddScoped<PatrocinioAppService>();
builder.Services.AddScoped<IMonitoriaQueryService, MonitoriaQueryService>();
builder.Services.AddScoped<IMonitoriaService, MonitoriaService>();
builder.Services.AddScoped<MonitoriaAppService>();

builder.Services.AddScoped<IPerfilService, PerfilService>();
builder.Services.AddScoped<PerfilAppService>();

builder.Services.AddScoped<INotificacionRepository, NotificacionRepository>();
builder.Services.AddScoped<INotificacionService, NotificacionService>();
builder.Services.AddScoped<INotificacionPusher, SignalRNotificacionPusher>();

// Predicción IA — Groq API
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<IAiAnalysisService, AiAnalysisService>();
builder.Services.AddScoped<IPrediccionIAService, PrediccionIAService>();
builder.Services.AddScoped<PrediccionIAAppService>();

// Repositorio documental — almacenamiento local
var storagePath = Path.Combine(builder.Environment.ContentRootPath,
    builder.Configuration["Storage:BasePath"] ?? "storage");
builder.Services.AddSingleton<IArchivoStorageService>(new LocalArchivoStorageService(storagePath));
builder.Services.AddScoped<IRepositorioRepository, RepositorioRepository>();
builder.Services.AddScoped<IRepositorioQueryService, RepositorioQueryService>();
builder.Services.AddScoped<RepositorioAppService>();

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
builder.Services.AddScoped<AuditoriaInterceptor>();
builder.Services.AddDbContext<AdsoDbContext>((sp, options) =>
    options.UseSqlServer(connectionString)
        .AddInterceptors(sp.GetRequiredService<AuditoriaInterceptor>()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificacionHub>("/hubs/notificaciones");


// Migraciones y seed automático al arrancar
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AdsoDbContext>();
    await context.Database.MigrateAsync();
    await AdsoDbContextSeeder.SeedAsync(context);

}


app.Run();
