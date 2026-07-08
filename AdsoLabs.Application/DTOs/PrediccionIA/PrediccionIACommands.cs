namespace AdsoLabs.Application.DTOs.PrediccionIA;

public record EnviarSolicitudCommand(
    int IdInstructor,
    int IdFichaCompetencia,
    string Razonamiento,
    int PorcentajeCompatibilidad
);

public record ResponderSolicitudCommand(
    int IdSolicitud,
    string Estado,           // Aprobada | Rechazada
    string? ObservacionAdmin
);
