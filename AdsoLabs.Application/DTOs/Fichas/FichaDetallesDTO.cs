namespace AdsoLabs.Application.DTOs.Fichas
{
    public class FichaDetallesDTO
    {
        public string NumeroFicha { get; set; } = string.Empty;

        /// <summary>
        /// Duración de la ficha en meses calendario (FechaInicio → FechaFin).
        /// Devuelve "En curso" cuando la ficha no tiene fecha fin definida.
        /// </summary>
        public string Meses
        {
            get
            {
                if (FechaFin == DateTime.MaxValue) return "En curso";

                var meses = (FechaFin.Year  - FechaInicio.Year)  * 12
                          + (FechaFin.Month - FechaInicio.Month);
                return $"{meses} meses";
            }
        }

        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin    { get; set; }

        public int TotalAprendices         { get; set; }
        public int TotalCompetencias       { get; set; }
        public int TotalCompetenciasVistas { get; set; }

        /// <summary>
        /// Total de horas asignadas a la ficha: suma de Competencia.HorasAsignadas
        /// de todas las FichaCompetencias (independientemente de su estado).
        /// </summary>
        public int TotalHoras       { get; set; }

        /// <summary>
        /// Horas correspondientes a competencias completadas (estado Vista):
        /// suma de Competencia.HorasAsignadas para las FichaCompetencias en Vista.
        /// </summary>
        public int TotalHorasVistas { get; set; }

        /// <summary>
        /// Progreso académico de la ficha.
        /// Fórmula canónica (CLAUDE.md §7.3):
        ///     (horas de competencias Vista / total horas asignadas) × 100
        ///
        /// Este es el indicador principal de avance real del programa de formación.
        /// No depende de fechas: refleja cuánto del currículo está completado.
        /// </summary>
        public int ProgresoGeneral => TotalHoras > 0
            ? (int)Math.Round((double)TotalHorasVistas / TotalHoras * 100)
            : 0;

        /// <summary>
        /// Progreso temporal: porcentaje del calendario transcurrido entre FechaInicio y FechaFin.
        /// Dato informativo secundario (ej. para mostrar si la ficha está adelantada o atrasada
        /// respecto al tiempo transcurrido). No debe confundirse con ProgresoGeneral.
        /// </summary>
        public int ProgresoTemporal
        {
            get
            {
                if (FechaFin == DateTime.MaxValue) return 0;
                var hoy = DateTime.Today;
                if (hoy <= FechaInicio) return 0;
                if (hoy >= FechaFin)    return 100;
                var total        = (FechaFin    - FechaInicio).TotalDays;
                var transcurrido = (hoy         - FechaInicio).TotalDays;
                return (int)Math.Round((transcurrido / total) * 100);
            }
        }

        /// <summary>
        /// Fase del proyecto formativo en la que está la ficha actualmente
        /// (Inducción|Análisis|Planeación|Ejecución|Evaluación), calculada a partir de la
        /// fase más avanzada entre sus competencias Vista/En Curso/Programada.
        /// </summary>
        public string FaseActual { get; set; } = "";

        /// <summary>
        /// Etapa Lectiva o Práctica, derivada de las fechas: la Etapa Práctica corresponde
        /// a los últimos 6 meses calendario de la ficha (FechaFin - 6 meses hasta FechaFin).
        /// </summary>
        public string EtapaFormativa
        {
            get
            {
                if (FechaFin == DateTime.MaxValue) return "Lectiva";
                var inicioEtapaPractica = FechaFin.AddMonths(-6);
                return DateTime.Today >= inicioEtapaPractica ? "Práctica" : "Lectiva";
            }
        }
    }
}
