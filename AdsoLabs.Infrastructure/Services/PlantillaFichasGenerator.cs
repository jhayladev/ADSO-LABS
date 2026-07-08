using System.IO.Compression;
using System.Text;

namespace AdsoLabs.Infrastructure.Services;

// ═══════════════════════════════════════════════════════════════════════════════
//
//  PlantillaFichasGenerator
//
//  Genera en memoria el archivo Excel (.xlsx) que sirve como plantilla para
//  la importación masiva de fichas.
//
//  Tecnología:
//    • Sin dependencias externas — usa únicamente System.IO.Compression.ZipArchive
//      y XML manual (OOXML / SpreadsheetML).
//    • Un archivo .xlsx es un ZIP con partes XML bien definidas.
//
//  Estructura generada:
//    [Content_Types].xml          → MIME types de cada parte
//    _rels/.rels                  → Relación raíz → libro
//    xl/workbook.xml              → Libro con una hoja
//    xl/_rels/workbook.xml.rels   → Relaciones del libro (hoja, estilos, cadenas)
//    xl/worksheets/sheet1.xml     → Datos: encabezados + 2 filas de ejemplo
//    xl/sharedStrings.xml         → Strings compartidos (texto de celdas)
//    xl/styles.xml                → Estilos mínimos requeridos por OOXML
//
//  Columnas de la plantilla:
//    A: NumeroFicha               (obligatorio)
//    B: Jornada                   (obligatorio — Mañana | Tarde)
//    C: FechaInicio               (obligatorio, formato DD/MM/AAAA)
//    D: FechaFin                  (opcional, DD/MM/AAAA; vacío → En Curso)
//
//  El Nombre de la ficha NO va en el archivo: el importador lo completa
//  automáticamente con "ANALISIS Y DESARROLLO DE SOFTWARE".
//
//  Las competencias tampoco van en el archivo: el importador las vincula
//  automáticamente (todas las activas del catálogo del programa).
//
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Genera la plantilla Excel para importación de fichas sin dependencias externas.
/// </summary>
public static class PlantillaFichasGenerator
{
    // ── Strings compartidos (índices 0..9) ───────────────────────────────────
    // Orden exacto: las celdas del sheet referencian por índice de este array.
    private static readonly string[] _strings =
    [
        // Encabezados (0–4)
        "NumeroFicha",
        "Jornada (Mañana / Tarde)",
        "FechaInicio (DD/MM/AAAA)",
        "FechaFin (DD/MM/AAAA — vacío si en curso)",
        "Modalidad (Presencial / Virtual)",
        // Ejemplo fila 1 (5–9)
        "2753647",
        "Mañana",
        "01/02/2024",
        "31/01/2026",
        "Presencial",
        // Ejemplo fila 2 (10–11)
        "2753648",
        "Tarde",
        // Modalidad fila 2 (12)
        "Virtual",
    ];

    // ══════════════════════════════════════════════════════════════════════════
    //  Punto de entrada público
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Genera los bytes del archivo .xlsx y los devuelve como array en memoria.
    /// El resultado puede servirse directamente como respuesta HTTP (Content-Type xlsx).
    /// </summary>
    public static byte[] Generar()
    {
        using var ms = new MemoryStream();

        // ZipArchive construye el paquete OOXML en memoria
        using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            AddEntry(zip, "[Content_Types].xml",         ContentTypesXml());
            AddEntry(zip, "_rels/.rels",                 RelsXml());
            AddEntry(zip, "xl/workbook.xml",             WorkbookXml());
            AddEntry(zip, "xl/_rels/workbook.xml.rels",  WorkbookRelsXml());
            AddEntry(zip, "xl/worksheets/sheet1.xml",    Sheet1Xml());
            AddEntry(zip, "xl/sharedStrings.xml",        SharedStringsXml());
            AddEntry(zip, "xl/styles.xml",               StylesXml());
        }

        return ms.ToArray();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Helpers
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Añade una entrada de texto UTF-8 (sin BOM) al ZipArchive.</summary>
    private static void AddEntry(ZipArchive zip, string path, string content)
    {
        var entry = zip.CreateEntry(path, CompressionLevel.Optimal);
        using var sw = new StreamWriter(
            entry.Open(),
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            leaveOpen: false);
        sw.Write(content);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Partes OOXML
    // ══════════════════════════════════════════════════════════════════════════

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

    /// <summary>Relación raíz → libro de trabajo.</summary>
    private static string RelsXml() =>
        """
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
          <Relationship Id="rId1"
            Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"
            Target="xl/workbook.xml"/>
        </Relationships>
        """;

    /// <summary>Libro con una sola hoja llamada "PlantillaFichas".</summary>
    private static string WorkbookXml() =>
        """
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
                  xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
          <sheets>
            <sheet name="PlantillaFichas" sheetId="1" r:id="rId1"/>
          </sheets>
        </workbook>
        """;

    /// <summary>Relaciones del libro → hoja, strings compartidos, estilos.</summary>
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

    /// <summary>
    /// Hoja de datos con la fila de encabezados y dos filas de ejemplo.
    /// Cada fila representa una ficha. El nombre se auto-completa en el importador.
    /// </summary>
    private static string Sheet1Xml() =>
        """
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
          <sheetData>
            <row r="1">
              <c r="A1" t="s"><v>0</v></c>
              <c r="B1" t="s"><v>1</v></c>
              <c r="C1" t="s"><v>2</v></c>
              <c r="D1" t="s"><v>3</v></c>
              <c r="E1" t="s"><v>4</v></c>
            </row>
            <row r="2">
              <c r="A2" t="s"><v>5</v></c>
              <c r="B2" t="s"><v>6</v></c>
              <c r="C2" t="s"><v>7</v></c>
              <c r="D2" t="s"><v>8</v></c>
              <c r="E2" t="s"><v>9</v></c>
            </row>
            <row r="3">
              <c r="A3" t="s"><v>10</v></c>
              <c r="B3" t="s"><v>11</v></c>
              <c r="C3" t="s"><v>7</v></c>
              <c r="E3" t="s"><v>12</v></c>
            </row>
          </sheetData>
        </worksheet>
        """;

    /// <summary>
    /// Tabla de strings compartidos. Cada &lt;si&gt; es un string indexado
    /// desde 0; las celdas del sheet lo referencian con <c t="s">&lt;v&gt;N&lt;/v&gt;</c>.
    /// </summary>
    private static string SharedStringsXml()
    {
        var sb = new StringBuilder();
        sb.Append($"""<?xml version="1.0" encoding="UTF-8" standalone="yes"?><sst xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" count="{_strings.Length}" uniqueCount="{_strings.Length}">""");

        foreach (var s in _strings)
            sb.Append($"<si><t>{EscapeXml(s)}</t></si>");

        sb.Append("</sst>");
        return sb.ToString();
    }

    /// <summary>
    /// Estilos mínimos requeridos por la especificación OOXML:
    /// fonts, fills (al menos 2), borders, cellStyleXfs, cellXfs.
    /// </summary>
    private static string StylesXml() =>
        """
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
          <fonts count="1">
            <font><sz val="11"/><name val="Calibri"/></font>
          </fonts>
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

    /// <summary>Escapa los cinco caracteres XML especiales en valores de celda.</summary>
    private static string EscapeXml(string s) => s
        .Replace("&",  "&amp;")
        .Replace("<",  "&lt;")
        .Replace(">",  "&gt;")
        .Replace("\"", "&quot;")
        .Replace("'",  "&apos;");
}
