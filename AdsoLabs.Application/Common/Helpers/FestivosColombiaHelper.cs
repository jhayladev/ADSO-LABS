namespace AdsoLabs.Application.Common.Helpers;

/// <summary>
/// Calcula los festivos oficiales de Colombia (Ley 51 de 1983 y siguientes).
/// Incluye festivos fijos, festivos de "puente" y festivos móviles basados en Pascua.
/// </summary>
public static class FestivosColombiaHelper
{
    // ── Algoritmo anónimo gregoriano para calcular el Domingo de Pascua ─────────
    private static DateOnly Pascua(int year)
    {
        int a = year % 19;
        int b = year / 100;
        int c = year % 100;
        int d = b / 4;
        int e = b % 4;
        int f = (b + 8) / 25;
        int g = (b - f + 1) / 3;
        int h = (19 * a + b - d - g + 15) % 30;
        int i = c / 4;
        int k = c % 4;
        int l = (32 + 2 * e + 2 * i - h - k) % 7;
        int m = (a + 11 * h + 22 * l) / 451;
        int mes = (h + l - 7 * m + 114) / 31;
        int dia = ((h + l - 7 * m + 114) % 31) + 1;
        return new DateOnly(year, mes, dia);
    }

    /// <summary>
    /// Traslada la fecha al lunes más próximo (inclusive) si no cae ya en lunes.
    /// Aplica a los festivos de "puente" según la ley colombiana.
    /// </summary>
    private static DateOnly SiguienteLunes(DateOnly fecha)
    {
        int diasHastaLunes = ((int)DayOfWeek.Monday - (int)fecha.DayOfWeek + 7) % 7;
        return fecha.AddDays(diasHastaLunes);
    }

    /// <summary>
    /// Devuelve el conjunto de festivos para el año indicado.
    /// </summary>
    public static HashSet<DateOnly> ObtenerFestivos(int year)
    {
        var festivos = new HashSet<DateOnly>();

        // ── Festivos fijos ────────────────────────────────────────────────────────
        festivos.Add(new DateOnly(year,  1,  1));  // Año Nuevo
        festivos.Add(new DateOnly(year,  5,  1));  // Día del Trabajo
        festivos.Add(new DateOnly(year,  7, 20));  // Independencia
        festivos.Add(new DateOnly(year,  8,  7));  // Batalla de Boyacá
        festivos.Add(new DateOnly(year, 12,  8));  // Inmaculada Concepción
        festivos.Add(new DateOnly(year, 12, 25));  // Navidad

        // ── Festivos de puente (se trasladan al siguiente lunes) ─────────────────
        festivos.Add(SiguienteLunes(new DateOnly(year,  1,  6)));  // Reyes Magos
        festivos.Add(SiguienteLunes(new DateOnly(year,  3, 19)));  // San José
        festivos.Add(SiguienteLunes(new DateOnly(year,  6, 29)));  // San Pedro y San Pablo
        festivos.Add(SiguienteLunes(new DateOnly(year,  8, 15)));  // Asunción
        festivos.Add(SiguienteLunes(new DateOnly(year, 10, 12)));  // Día de la Raza
        festivos.Add(SiguienteLunes(new DateOnly(year, 11,  1)));  // Todos los Santos
        festivos.Add(SiguienteLunes(new DateOnly(year, 11, 11)));  // Independencia de Cartagena

        // ── Festivos móviles basados en Pascua ───────────────────────────────────
        var pascua = Pascua(year);
        festivos.Add(pascua.AddDays(-3));                         // Jueves Santo
        festivos.Add(pascua.AddDays(-2));                         // Viernes Santo
        festivos.Add(SiguienteLunes(pascua.AddDays(43)));         // Ascensión
        festivos.Add(SiguienteLunes(pascua.AddDays(64)));         // Corpus Christi
        festivos.Add(SiguienteLunes(pascua.AddDays(71)));         // Sagrado Corazón

        return festivos;
    }

    /// <summary>True si la fecha es un festivo colombiano oficial.</summary>
    public static bool EsFestivo(DateOnly fecha)
        => ObtenerFestivos(fecha.Year).Contains(fecha);

    /// <summary>True si la fecha es domingo o festivo colombiano.</summary>
    public static bool EsDomingoOFestivo(DateOnly fecha)
        => fecha.DayOfWeek == DayOfWeek.Sunday || EsFestivo(fecha);

    /// <summary>
    /// Nombre descriptivo del día si es domingo o festivo; null si es día hábil.
    /// Útil para construir mensajes de error.
    /// </summary>
    public static string? DescripcionDomingoOFestivo(DateOnly fecha)
    {
        if (fecha.DayOfWeek == DayOfWeek.Sunday) return "domingo";
        if (EsFestivo(fecha))                    return "festivo";
        return null;
    }
}
