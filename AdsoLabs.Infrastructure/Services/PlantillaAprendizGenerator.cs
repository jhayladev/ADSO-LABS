using System.IO.Compression;
using System.Text;

namespace AdsoLabs.Infrastructure.Services;

// ═══════════════════════════════════════════════════════════════════════════════
//
//  PlantillaAprendizGenerator
//
//  Genera la plantilla Excel (.xlsx) para la importación masiva de aprendices.
//
//  El archivo se construye en memoria usando ZipArchive + XML SpreadsheetML,
//  sin dependencias externas — igual que PlantillaFichasGenerator.
//
//  El número de ficha NO va en el archivo:
//    Se toma del contexto de la pantalla (ficha actualmente visualizada).
//    Esto evita errores de digitación y acopla el archivo al flujo correcto.
//
//  Columnas:
//    A: TipoDocumento   → abreviatura del catálogo (CC / TI / CE / PPT / PA / RC)
//    B: NumeroDocumento → número único de identificación del aprendiz
//    C: Nombres         → nombres completos
//    D: Apellidos       → apellidos completos
//    E: Estado          → estado en la ficha (EN FORMACION / CANCELADO /
//                         RETIRO VOLUNTARIO / TRASLADADO)
//                         Si se deja vacío, el importador asume EN FORMACION.
//
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Genera la plantilla Excel para importación de aprendices sin dependencias externas.
/// </summary>
public static class PlantillaAprendizGenerator
{
    // ── Strings compartidos (índices 0..13) ──────────────────────────────────
    private static readonly string[] _strings =
    [
        // Encabezados (0–4)
        "TipoDocumento (CC / TI / CE / PPT / PA / RC)",
        "NumeroDocumento",
        "Nombres",
        "Apellidos",
        "Estado (EN FORMACION / CANCELADO / RETIRO VOLUNTARIO / TRASLADADO)",
        // Ejemplo fila 1 (5–9)
        "CC",
        "1234567890",
        "Juan David",
        "Pérez García",
        "EN FORMACION",
        // Ejemplo fila 2 (10–13)
        "TI",
        "987654321",
        "María Camila",
        "Rodríguez López",
    ];

    // ══════════════════════════════════════════════════════════════════════════
    //  Punto de entrada público
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Genera los bytes del archivo .xlsx de plantilla en memoria.
    /// El resultado puede servirse directamente como respuesta HTTP.
    /// </summary>
    public static byte[] Generar()
    {
        using var ms = new MemoryStream();

        using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            AddEntry(zip, "[Content_Types].xml",        ContentTypesXml());
            AddEntry(zip, "_rels/.rels",                RelsXml());
            AddEntry(zip, "xl/workbook.xml",            WorkbookXml());
            AddEntry(zip, "xl/_rels/workbook.xml.rels", WorkbookRelsXml());
            AddEntry(zip, "xl/worksheets/sheet1.xml",   Sheet1Xml());
            AddEntry(zip, "xl/sharedStrings.xml",       SharedStringsXml());
            AddEntry(zip, "xl/styles.xml",              StylesXml());
        }

        return ms.ToArray();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Helpers
    // ══════════════════════════════════════════════════════════════════════════

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

    private static string RelsXml() =>
        """
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
          <Relationship Id="rId1"
            Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"
            Target="xl/workbook.xml"/>
        </Relationships>
        """;

    private static string WorkbookXml() =>
        """
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
                  xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
          <sheets>
            <sheet name="PlantillaAprendices" sheetId="1" r:id="rId1"/>
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

    /// <summary>
    /// Hoja con encabezados (fila 1) y dos filas de ejemplo (filas 2-3).
    /// Índices de sharedStrings: encabezados 0-4, ejemplo-1 5-9, ejemplo-2 10-13 + 9.
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
              <c r="C3" t="s"><v>12</v></c>
              <c r="D3" t="s"><v>13</v></c>
              <c r="E3" t="s"><v>9</v></c>
            </row>
          </sheetData>
        </worksheet>
        """;

    private static string SharedStringsXml()
    {
        var sb = new StringBuilder();
        sb.Append($"""<?xml version="1.0" encoding="UTF-8" standalone="yes"?><sst xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" count="{_strings.Length}" uniqueCount="{_strings.Length}">""");
        foreach (var s in _strings)
            sb.Append($"<si><t>{EscapeXml(s)}</t></si>");
        sb.Append("</sst>");
        return sb.ToString();
    }

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

    private static string EscapeXml(string s) => s
        .Replace("&",  "&amp;")
        .Replace("<",  "&lt;")
        .Replace(">",  "&gt;")
        .Replace("\"", "&quot;")
        .Replace("'",  "&apos;");
}
