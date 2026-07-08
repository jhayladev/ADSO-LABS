namespace AdsoLabs.Application.AppService.Importacion
{
    public class ResultadoImportacion
    {
        public int IdImportacion    { get; init; }
        public int TotalFilas       { get; init; }
        public int FilasOk          { get; set; }
        public int FilasError       { get; set; }

        // Contadores de trazabilidad
        public int AprendicesCreados     { get; set; }
        public int AprendicesActualizados { get; set; }
        public int JuiciosInsertados     { get; set; }
        public int JuiciosActualizados   { get; set; }
        public HashSet<string> FichasAfectadas { get; } = [];

        public List<string> Errores { get; } = [];
    }
}
