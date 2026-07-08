namespace AdsoLabs.Application.Common.Constants;

public static class EstadosCompetencia
{
    public const string Pendiente  = "Pendiente";   // RF-12: 100% POR EVALUAR — sin actividad
    public const string EnCurso    = "En Curso";    // RF-12: juicios mixtos (APROBADO + POR EVALUAR)
    public const string Programada = "Programada";  // RF-09: instructor/fechas/horas asignados
    public const string Vista      = "Vista";       // RF-09/12: 100% APROBADO — completada
}