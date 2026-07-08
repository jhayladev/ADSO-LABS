using AdsoLabs.Infrastructure.Data.Configurations;
using AdsoLabs.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace AdsoLabs.Infrastructure.Data;

public class AdsoDbContext(DbContextOptions<AdsoDbContext> options) : DbContext(options)
{

    // Catálogos
    public DbSet<TipoDocumento> TipoDocumento => Set<TipoDocumento>();
    public DbSet<TipoArchivo> TipoArchivo => Set<TipoArchivo>();
    public DbSet<Rol> Rol => Set<Rol>();

    // Personas y seguridad
    public DbSet<Persona> Persona => Set<Persona>();
    public DbSet<Usuario> Usuario => Set<Usuario>();
    public DbSet<PasswordResetToken> PasswordResetToken => Set<PasswordResetToken>();

    // Perfiles
    public DbSet<AprendizPerfil> AprendizPerfil => Set<AprendizPerfil>();
    public DbSet<InstructorPerfil> InstructorPerfil => Set<InstructorPerfil>();
    public DbSet<FichaAprendiz> FichaAprendiz => Set<FichaAprendiz>();
    public DbSet<HistorialEstadoAprendiz> HistorialEstadoAprendiz => Set<HistorialEstadoAprendiz>();

    // Fichas y competencias
    public DbSet<Ficha> Ficha => Set<Ficha>();
    public DbSet<Competencia> Competencia => Set<Competencia>();
    public DbSet<FichaCompetencia> FichaCompetencia => Set<FichaCompetencia>();
    public DbSet<FichaCompetenciaResultado> FichaCompetenciaResultado => Set<FichaCompetenciaResultado>();
    public DbSet<Patrocinio> Patrocinio => Set<Patrocinio>();

    // Asistencia
    public DbSet<Sesion> Sesion => Set<Sesion>();
    public DbSet<Asistencia> Asistencia => Set<Asistencia>();
    public DbSet<AsistenciaPatrocinio> AsistenciaPatrocinio => Set<AsistenciaPatrocinio>();

    // Monitorías
    public DbSet<MonitorPerfil> MonitorPerfil => Set<MonitorPerfil>();
    public DbSet<SesionMonitoria> SesionMonitoria => Set<SesionMonitoria>();
    public DbSet<InscripcionMonitoria> InscripcionMonitoria => Set<InscripcionMonitoria>();
    public DbSet<AsistenciaMonitoria> AsistenciaMonitoria => Set<AsistenciaMonitoria>();
    public DbSet<InformeMonitoria> InformeMonitoria => Set<InformeMonitoria>();
    public DbSet<InformeMonitoriaAprendiz> InformeMonitoriaAprendiz => Set<InformeMonitoriaAprendiz>();

    // Repositorio
    public DbSet<Archivo> Archivo => Set<Archivo>();
    public DbSet<GuiaAprendizaje> GuiaAprendizaje => Set<GuiaAprendizaje>();
    public DbSet<InstrumentoEvaluacion> InstrumentoEvaluacion => Set<InstrumentoEvaluacion>();
    public DbSet<PlaneacionPedagogica> PlaneacionPedagogica => Set<PlaneacionPedagogica>();
    public DbSet<ProyectoFormativo> ProyectoFormativo => Set<ProyectoFormativo>();
    public DbSet<DesarrolloCurricular> DesarrolloCurricular => Set<DesarrolloCurricular>();

    // Juicios evaluativos
    public DbSet<PlanConcertado> PlanConcertado => Set<PlanConcertado>();
    public DbSet<ResultadoAprendizaje> ResultadoAprendizaje => Set<ResultadoAprendizaje>();
    public DbSet<JuicioResultado> JuicioResultado => Set<JuicioResultado>();

    // Importaciones
    public DbSet<ImportacionArchivo> ImportacionArchivo => Set<ImportacionArchivo>();
    public DbSet<ImportacionError> ImportacionError => Set<ImportacionError>();

    // Informe del aprendiz
    public DbSet<Acudiente> Acudiente => Set<Acudiente>();
    public DbSet<ObservacionAprendiz> ObservacionAprendiz => Set<ObservacionAprendiz>();

    // Notificaciones
    public DbSet<Notificacion> Notificacion => Set<Notificacion>();

    // Predicción IA
    public DbSet<SolicitudAsignacion> SolicitudAsignacion => Set<SolicitudAsignacion>();

    // Alertas y auditoría
    public DbSet<Auditoria> Auditoria => Set<Auditoria>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica todas las clases IEntityTypeConfiguration<T> del ensamblado
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AdsoDbContext).Assembly);
    }
}
