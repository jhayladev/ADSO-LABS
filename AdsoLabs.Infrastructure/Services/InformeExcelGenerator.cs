using AdsoLabs.Application.DTOs.Reportes;
using System.IO.Compression;
using System.Text;

namespace AdsoLabs.Infrastructure.Services;

/// <summary>
/// Genera archivos .xlsx para los reportes del módulo Reportes, usando
/// SpreadsheetML + ZipArchive sin dependencias externas (mismo patrón que
/// PlantillaAprendizGenerator y PlantillaFichasGenerator).
/// </summary>
public class InformeExcelGenerator
{
    // ─────────────────────────────────────────────────────────────────────────
    //  Reporte B — Asistencia por ficha
    // ─────────────────────────────────────────────────────────────────────────

    public byte[] GenerarAsistencia(ReporteAsistenciaFichaDTO datos)
    {
        var strings = new List<string>
        {
            // Encabezados (0-8)
            "Nombres y Apellidos", "Documento", "Estado",
            "Total Sesiones", "Presentes", "Justificados", "Ausentes",
            "% Asistencia", "En Riesgo"
        };

        var filas = new StringBuilder();
        int rowNum = 2;
        foreach (var a in datos.Aprendices)
        {
            int iNombre   = AddString(strings, a.NombreCompleto);
            int iDoc      = AddString(strings, a.NumeroDocumento);
            int iEstado   = AddString(strings, a.Estado);
            int iRiesgo   = AddString(strings, a.EnRiesgo ? "Sí" : "No");

            filas.Append($"<row r=\"{rowNum}\">");
            filas.Append($"<c r=\"A{rowNum}\" t=\"s\"><v>{iNombre}</v></c>");
            filas.Append($"<c r=\"B{rowNum}\" t=\"s\"><v>{iDoc}</v></c>");
            filas.Append($"<c r=\"C{rowNum}\" t=\"s\"><v>{iEstado}</v></c>");
            filas.Append($"<c r=\"D{rowNum}\"><v>{a.TotalSesiones}</v></c>");
            filas.Append($"<c r=\"E{rowNum}\"><v>{a.Presentes}</v></c>");
            filas.Append($"<c r=\"F{rowNum}\"><v>{a.Justificados}</v></c>");
            filas.Append($"<c r=\"G{rowNum}\"><v>{a.Ausentes}</v></c>");
            filas.Append($"<c r=\"H{rowNum}\"><v>{a.PorcentajeAsistencia}</v></c>");
            filas.Append($"<c r=\"I{rowNum}\" t=\"s\"><v>{iRiesgo}</v></c>");
            filas.Append("</row>");
            rowNum++;
        }

        string sheet = BuildSheet(
            $"<row r=\"1\">" +
            "<c r=\"A1\" t=\"s\"><v>0</v></c><c r=\"B1\" t=\"s\"><v>1</v></c>" +
            "<c r=\"C1\" t=\"s\"><v>2</v></c><c r=\"D1\" t=\"s\"><v>3</v></c>" +
            "<c r=\"E1\" t=\"s\"><v>4</v></c><c r=\"F1\" t=\"s\"><v>5</v></c>" +
            "<c r=\"G1\" t=\"s\"><v>6</v></c><c r=\"H1\" t=\"s\"><v>7</v></c>" +
            "<c r=\"I1\" t=\"s\"><v>8</v></c>" +
            "</row>" + filas);

        return BuildXlsx($"Asistencia {datos.NumeroFicha}", sheet, strings);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Reporte C — Progreso de competencias por ficha
    // ─────────────────────────────────────────────────────────────────────────

    public byte[] GenerarProgresoCompetencias(ReporteProgresoCompetenciasDTO datos)
    {
        var strings = new List<string>
        {
            // Encabezados (0-6)
            "Código", "Competencia", "Estado",
            "Instructor", "Horas Planeadas", "Horas Ejecutadas", "% Progreso"
        };

        var filas = new StringBuilder();
        int rowNum = 2;
        foreach (var c in datos.Competencias)
        {
            int iCod      = AddString(strings, c.CodigoCompetencia);
            int iNombre   = AddString(strings, c.NombreCompetencia);
            int iEstado   = AddString(strings, c.Estado);
            int iInstr    = AddString(strings, c.NombreInstructor ?? "--");

            filas.Append($"<row r=\"{rowNum}\">");
            filas.Append($"<c r=\"A{rowNum}\" t=\"s\"><v>{iCod}</v></c>");
            filas.Append($"<c r=\"B{rowNum}\" t=\"s\"><v>{iNombre}</v></c>");
            filas.Append($"<c r=\"C{rowNum}\" t=\"s\"><v>{iEstado}</v></c>");
            filas.Append($"<c r=\"D{rowNum}\" t=\"s\"><v>{iInstr}</v></c>");
            filas.Append($"<c r=\"E{rowNum}\"><v>{c.HorasPlaneadas}</v></c>");
            filas.Append($"<c r=\"F{rowNum}\"><v>{c.HorasEjecutadas}</v></c>");
            filas.Append($"<c r=\"G{rowNum}\"><v>{c.PorcentajeProgreso}</v></c>");
            filas.Append("</row>");
            rowNum++;
        }

        string sheet = BuildSheet(
            "<row r=\"1\">" +
            "<c r=\"A1\" t=\"s\"><v>0</v></c><c r=\"B1\" t=\"s\"><v>1</v></c>" +
            "<c r=\"C1\" t=\"s\"><v>2</v></c><c r=\"D1\" t=\"s\"><v>3</v></c>" +
            "<c r=\"E1\" t=\"s\"><v>4</v></c><c r=\"F1\" t=\"s\"><v>5</v></c>" +
            "<c r=\"G1\" t=\"s\"><v>6</v></c>" +
            "</row>" + filas);

        return BuildXlsx($"Competencias {datos.NumeroFicha}", sheet, strings);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Reporte E — Resumen global de fichas
    // ─────────────────────────────────────────────────────────────────────────

    public byte[] GenerarResumenGlobal(List<ReporteGlobalFichaDTO> datos)
    {
        var strings = new List<string>
        {
            // Encabezados (0-9)
            "Ficha", "Estado", "Total Aprendices", "Activos",
            "En Riesgo", "% Asistencia", "Progreso %",
            "Comp. Vista", "Comp. En Curso", "Comp. Pendientes"
        };

        var filas = new StringBuilder();
        int rowNum = 2;
        foreach (var f in datos)
        {
            int iFicha   = AddString(strings, f.NumeroFicha);
            int iEstado  = AddString(strings, f.Estado);

            filas.Append($"<row r=\"{rowNum}\">");
            filas.Append($"<c r=\"A{rowNum}\" t=\"s\"><v>{iFicha}</v></c>");
            filas.Append($"<c r=\"B{rowNum}\" t=\"s\"><v>{iEstado}</v></c>");
            filas.Append($"<c r=\"C{rowNum}\"><v>{f.TotalAprendices}</v></c>");
            filas.Append($"<c r=\"D{rowNum}\"><v>{f.AprendicesActivos}</v></c>");
            filas.Append($"<c r=\"E{rowNum}\"><v>{f.AprendicesEnRiesgo}</v></c>");
            filas.Append($"<c r=\"F{rowNum}\"><v>{f.PorcentajeAsistencia}</v></c>");
            filas.Append($"<c r=\"G{rowNum}\"><v>{f.ProgresoGeneral}</v></c>");
            filas.Append($"<c r=\"H{rowNum}\"><v>{f.CompetenciasVista}</v></c>");
            filas.Append($"<c r=\"I{rowNum}\"><v>{f.CompetenciasEnCurso}</v></c>");
            filas.Append($"<c r=\"J{rowNum}\"><v>{f.CompetenciasPendientes}</v></c>");
            filas.Append("</row>");
            rowNum++;
        }

        string sheet = BuildSheet(
            "<row r=\"1\">" +
            "<c r=\"A1\" t=\"s\"><v>0</v></c><c r=\"B1\" t=\"s\"><v>1</v></c>" +
            "<c r=\"C1\" t=\"s\"><v>2</v></c><c r=\"D1\" t=\"s\"><v>3</v></c>" +
            "<c r=\"E1\" t=\"s\"><v>4</v></c><c r=\"F1\" t=\"s\"><v>5</v></c>" +
            "<c r=\"G1\" t=\"s\"><v>6</v></c><c r=\"H1\" t=\"s\"><v>7</v></c>" +
            "<c r=\"I1\" t=\"s\"><v>8</v></c><c r=\"J1\" t=\"s\"><v>9</v></c>" +
            "</row>" + filas);

        return BuildXlsx("Resumen Global", sheet, strings);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Helpers internos
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Añade la cadena a la lista de shared strings y devuelve su índice.
    /// Si ya existe, devuelve el índice existente.
    /// </summary>
    private static int AddString(List<string> strings, string value)
    {
        int idx = strings.IndexOf(value);
        if (idx >= 0) return idx;
        strings.Add(value);
        return strings.Count - 1;
    }

    private static string BuildSheet(string rowsXml) =>
        $"""
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
          <sheetData>{rowsXml}</sheetData>
        </worksheet>
        """;

    private static byte[] BuildXlsx(string sheetName, string sheetXml, List<string> strings)
    {
        using var ms  = new MemoryStream();
        using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            AddEntry(zip, "[Content_Types].xml",        ContentTypesXml());
            AddEntry(zip, "_rels/.rels",                RelsXml());
            AddEntry(zip, "xl/workbook.xml",            WorkbookXml(sheetName));
            AddEntry(zip, "xl/_rels/workbook.xml.rels", WorkbookRelsXml());
            AddEntry(zip, "xl/worksheets/sheet1.xml",   sheetXml);
            AddEntry(zip, "xl/sharedStrings.xml",       SharedStringsXml(strings));
            AddEntry(zip, "xl/styles.xml",              StylesXml());
        }
        return ms.ToArray();
    }

    private static void AddEntry(ZipArchive zip, string path, string content)
    {
        var entry = zip.CreateEntry(path, CompressionLevel.Optimal);
        using var sw = new StreamWriter(
            entry.Open(),
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            leaveOpen: false);
        sw.Write(content);
    }

    private static string ContentTypesXml() =>
        """
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
          <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
          <Default Extension="xml" ContentType="application/xml"/>
          <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
          <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
          <Override PartName="/xl/sharedStrings.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sharedStrings+xml"/>
          <Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/>
        </Types>
        """;

    private static string RelsXml() =>
        """
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
          <Relationship Id="rId1"
            Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"
            Target="xl/workbook.xml"/>
        </Relationships>
        """;

    private static string WorkbookXml(string sheetName) =>
        $"""
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
                  xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
          <sheets>
            <sheet name="{EscapeXml(sheetName)}" sheetId="1" r:id="rId1"/>
          </sheets>
        </workbook>
        """;

    private static string WorkbookRelsXml() =>
        """
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
          <Relationship Id="rId1"
            Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet"
            Target="worksheets/sheet1.xml"/>
          <Relationship Id="rId2"
            Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/sharedStrings"
            Target="sharedStrings.xml"/>
          <Relationship Id="rId3"
            Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles"
            Target="styles.xml"/>
        </Relationships>
        """;

    private static string SharedStringsXml(List<string> strings)
    {
        var sb = new StringBuilder();
        sb.Append($"""<?xml version="1.0" encoding="UTF-8" standalone="yes"?><sst xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" count="{strings.Count}" uniqueCount="{strings.Count}">""");
        foreach (var s in strings)
            sb.Append($"<si><t>{EscapeXml(s)}</t></si>");
        sb.Append("</sst>");
        return sb.ToString();
    }

    private static string StylesXml() =>
        """
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
          <fonts count="1"><font><sz val="11"/><name val="Calibri"/></font></fonts>
          <fills count="2">
            <fill><patternFill patternType="none"/></fill>
            <fill><patternFill patternType="gray125"/></fill>
          </fills>
          <borders count="1">
            <border><left/><right/><top/><bottom/><diagonal/></border>
          </borders>
          <cellStyleXfs count="1">
            <xf numFmtId="0" fontId="0" fillId="0" borderId="0"/>
          </cellStyleXfs>
          <cellXfs count="1">
            <xf numFmtId="0" fontId="0" fillId="0" borderId="0" xfId="0"/>
          </cellXfs>
        </styleSheet>
        """;

    private static string EscapeXml(string s) => s
        .Replace("&",  "&amp;")
        .Replace("<",  "&lt;")
        .Replace(">",  "&gt;")
        .Replace("\"", "&quot;")
        .Replace("'",  "&apos;");
}
