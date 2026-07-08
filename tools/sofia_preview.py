#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
sofia_preview.py  —  Validador y previsualizador del Reporte de Juicios Evaluativos SOFIA Plus

USO:
    python sofia_preview.py <archivo.xls>
    python sofia_preview.py <archivo.xls> --csv salida.csv
    python sofia_preview.py <archivo.xls> --json

DESCRIPCIÓN:
    Replica la lógica de Fase 1 del ImportadorReporteJuicios (C#) en Python.
    No se conecta a la base de datos — solo valida el archivo Excel y muestra
    un resumen de lo que se importaría si se sube al sistema.

    Útil para:
      • Detectar problemas de formato antes de hacer el upload.
      • Ver un resumen de filas, aprendices, competencias y juicios.
      • Exportar un CSV limpio para inspección manual.

FORMATO ESPERADO:
    Archivo .xls exportado desde SOFIA Plus (Reporte de Juicios Evaluativos).
    Cabecera en filas 1–11. Datos desde fila 12 o 13 (se detecta automáticamente).
    Columnas clave: Tipo Doc, Nro Doc, Nombres, Apellidos, Estado, Competencia,
                    Resultado de Aprendizaje, Juicio Evaluativo, Fecha, Funcionario.

DEPENDENCIAS:
    pip install -r requirements.txt   (xlrd==1.2.0, colorama)
"""

import argparse
import csv
import json
import re
import sys
from collections import Counter, defaultdict
from datetime import datetime
from pathlib import Path

# ── Importación opcional de dependencias externas ────────────────────────────
try:
    import xlrd
except ImportError:
    print("ERROR: falta xlrd. Ejecuta:  pip install xlrd==1.2.0")
    sys.exit(1)

try:
    from colorama import Fore, Style, init as colorama_init
    colorama_init(autoreset=True)
    COLOR = True
except ImportError:
    COLOR = False

    class _NoColor:
        def __getattr__(self, _): return ""

    Fore = Style = _NoColor()


# ─────────────────────────────────────────────────────────────────────────────
#  CONSTANTES (deben coincidir con los valores del backend C#)
# ─────────────────────────────────────────────────────────────────────────────

ESTADOS_VALIDOS = {"EN FORMACION", "CANCELADO", "RETIRO VOLUNTARIO", "TRASLADADO"}
JUICIOS_VALIDOS = {"APROBADO", "POR EVALUAR"}

# Tipos de documento canónicos reconocidos por NormalizarTipoDoc en el importer
TIPOS_DOC_VALIDOS = {"CC", "TI", "CE", "PPT", "PA", "RC", "PASAPORTE"}


# ─────────────────────────────────────────────────────────────────────────────
#  HELPERS DE PARSEO
# ─────────────────────────────────────────────────────────────────────────────

def celda(hoja, fila, col):
    """Devuelve el valor de una celda como string limpio. '' si fuera de rango."""
    if col < 0 or col >= hoja.ncols or fila < 0 or fila >= hoja.nrows:
        return ""
    val = hoja.cell_value(fila, col)
    if isinstance(val, float):
        # xlrd lee las fechas como float; para texto numérico eliminar el .0
        return str(int(val)) if val == int(val) else str(val)
    return str(val).strip()


def limpiar(texto):
    """Elimina caracteres de control y espacios no estándar."""
    return re.sub(r"[\x00-\x1F\x7F\xa0]", "", texto or "").strip()


def separar_codigo_nombre(raw):
    """Separa 'CÓDIGO - Nombre' en (codigo, nombre). Retorna ('', raw) si no hay patrón."""
    m = re.match(r"^(\d+)\s*-\s*(.+)$", raw.strip(), re.DOTALL)
    if m:
        return m.group(1).strip(), m.group(2).strip()
    return "", raw.strip()


def extraer_digitos(raw):
    """Extrae la primera secuencia de 5+ dígitos del string (número de ficha)."""
    m = re.search(r"\d{5,}", raw or "")
    return m.group() if m else ""


def normalizar_tipo_doc(raw):
    """Normaliza el tipo de documento al código canónico."""
    s = raw.upper().strip()
    if s.startswith("CC") or "CIUDADAN" in s:
        return "CC"
    if s.startswith("TI") or "IDENTIDAD" in s:
        return "TI"
    if s.startswith("CE") or "EXTRAN" in s:
        return "CE"
    if "PASAPORTE" in s:
        return "Pasaporte"
    if s.startswith("PPT"):
        return "PPT"
    return s or "CC"


def normalizar_estado(raw):
    """Normaliza el estado del aprendiz al valor canónico."""
    s = limpiar(raw).upper()
    if "TRASLAD" in s:
        return "TRASLADADO"
    if "RETIRO VOLUNTARIO" in s:
        return "RETIRO VOLUNTARIO"
    if "CANCEL" in s:
        return "CANCELADO"
    return "EN FORMACION"


def normalizar_juicio(raw):
    """Normaliza el juicio al valor canónico."""
    s = limpiar(raw).upper()
    if "POR EVALUAR" in s:
        return "POR EVALUAR"
    if "APROBADO" in s:
        return "APROBADO"
    return ""


# ─────────────────────────────────────────────────────────────────────────────
#  EXTRACCIÓN DE CABECERA (filas 1–11 del reporte SOFIA Plus)
# ─────────────────────────────────────────────────────────────────────────────

def extraer_cabecera(hoja):
    """
    Lee la sección de cabecera del reporte SOFIA Plus y extrae los metadatos
    de la ficha: número, denominación, estado, fechas, regional, centro.
    Devuelve un diccionario con las claves encontradas.
    """
    numero_ficha = ""
    denominacion = ""
    estado_ficha = "EN EJECUCION"
    fecha_inicio = None
    fecha_fin    = None
    modalidad    = ""
    regional     = ""
    centro       = ""

    # Posición histórica fija del número de ficha en reportes SOFIA Plus
    numero_ficha = extraer_digitos(celda(hoja, 2, 2))

    limite = min(15, hoja.nrows)

    for r in range(limite):
        for c in range(max(0, hoja.ncols - 2)):
            clave = celda(hoja, r, c).upper()
            # Intentar tomar el valor de la celda de la derecha
            valor = celda(hoja, r, c + 1) or celda(hoja, r, c + 2)

            if any(k in clave for k in ("COGIGO", "CÓDIGO", "CODIGO", "FICHA")) \
                    and not numero_ficha:
                en_misma = extraer_digitos(clave)
                numero_ficha = en_misma or extraer_digitos(valor)

            elif any(k in clave for k in ("DENOMINACI", "NOMBRE DEL PROGRAMA")) \
                    and not denominacion:
                denominacion = valor

            elif "ESTADO DE LA FICHA" in clave:
                estado_ficha = valor

            elif "FECHA INICIO" in clave:
                fecha_inicio = _parsear_fecha(valor)

            elif "FECHA FIN" in clave:
                fecha_fin = _parsear_fecha(valor)

            elif "MODALIDAD" in clave and not modalidad:
                modalidad = valor

            elif "REGIONAL" in clave and not regional:
                regional = valor

            elif "CENTRO" in clave and not centro:
                centro = valor

    if not numero_ficha:
        numero_ficha = "SIN_NUMERO"
    if not denominacion:
        denominacion = "Análisis y Desarrollo de Software"

    return {
        "numero_ficha": numero_ficha,
        "denominacion": denominacion,
        "estado_ficha": estado_ficha,
        "fecha_inicio": fecha_inicio,
        "fecha_fin":    fecha_fin,
        "modalidad":    modalidad,
        "regional":     regional,
        "centro":       centro,
    }


def _parsear_fecha(raw):
    """Intenta parsear una fecha en varios formatos. Devuelve str 'DD/MM/YYYY' o None."""
    if not raw:
        return None
    raw = raw.split(" ")[0].strip()
    for fmt in ("%d/%m/%Y", "%d/%m/%y", "%Y-%m-%d"):
        try:
            return datetime.strptime(raw, fmt).strftime("%d/%m/%Y")
        except ValueError:
            continue
    return raw or None


# ─────────────────────────────────────────────────────────────────────────────
#  LOCALIZACIÓN DE FILA DE ENCABEZADOS DE COLUMNAS
# ─────────────────────────────────────────────────────────────────────────────

def localizar_fila_encabezado(hoja):
    """
    Busca la fila que contiene los títulos de columna del cuerpo de datos.
    Criterio: fila que tenga 'TIPO', 'DOCUMENTO' y 'NOMBRE' simultáneamente.
    Si no se encuentra, retorna 9 (posición histórica SOFIA Plus).
    """
    limite = min(20, hoja.nrows)
    for r in range(limite):
        fila_texto = " ".join(
            celda(hoja, r, c) for c in range(hoja.ncols)
        ).upper()
        if "TIPO" in fila_texto and "DOCUMENTO" in fila_texto and "NOMBRE" in fila_texto:
            return r
    return 9


def mapear_columnas(hoja, header_row):
    """
    Mapea los nombres de columna del encabezado a sus índices numéricos.
    Usa valores por defecto históricos para columnas no encontradas por nombre.
    """
    idx = {
        "tipo_doc":    0,
        "doc":         1,
        "nombres":     2,
        "apellidos":   3,
        "estado":      4,
        "competencia": 5,
        "resultado":   6,
        "juicio":      7,
        "fecha_j":     9,
        "funcionario": 10,
    }

    if header_row < 0 or header_row >= hoja.nrows:
        return idx

    reglas = {
        "TIPO":          "tipo_doc",
        "NÚMERO DE DOC": "doc",
        "NUMERO DE DOC": "doc",
        "NOMBRE":        "nombres",
        "APELLIDO":      "apellidos",
        "ESTADO":        "estado",
        "COMPETENCIA":   "competencia",
        "RESULTADO":     "resultado",
        "JUICIO":        "juicio",
        "FECHA":         "fecha_j",
        "FUNCIONARIO":   "funcionario",
    }

    usados = set(idx.values())
    for c in range(hoja.ncols):
        header = celda(hoja, header_row, c).upper()
        for patron, clave in reglas.items():
            if patron in header and c not in usados:
                idx[clave] = c
                usados.add(c)
                break

    return idx


# ─────────────────────────────────────────────────────────────────────────────
#  NORMALIZACIÓN DEL REPORTE (Fase 1 equivalente)
# ─────────────────────────────────────────────────────────────────────────────

def normalizar_reporte(ruta_xls):
    """
    Lee el archivo .xls de SOFIA Plus y devuelve:
        cabecera  : dict con metadatos de la ficha
        filas     : list[dict] con todas las filas de datos normalizadas
        advertencias : list[str] con problemas detectados (no fatales)
        errores   : list[str] con problemas graves (filas que el importer rechazaría)
    """
    libro   = xlrd.open_workbook(ruta_xls)
    hoja    = libro.sheet_by_index(0)

    cabecera      = extraer_cabecera(hoja)
    header_row    = localizar_fila_encabezado(hoja)
    col           = mapear_columnas(hoja, header_row)

    filas         = []
    advertencias  = []
    errores       = []

    for r in range(header_row + 1, hoja.nrows):
        doc  = celda(hoja, r, col["doc"]).strip()
        comp = celda(hoja, r, col["competencia"]).strip()

        # Ignorar filas completamente vacías
        if not doc and not comp:
            continue

        juicio_raw = limpiar(celda(hoja, r, col["juicio"])).upper()
        if not juicio_raw:
            continue   # fila sin juicio → descartada (igual que en C#)

        juicio = normalizar_juicio(juicio_raw)
        if juicio not in JUICIOS_VALIDOS:
            errores.append(
                f"Fila {r+1}: juicio desconocido '{juicio_raw}' "
                f"(doc={doc}, comp={comp})"
            )
            juicio = juicio_raw   # preservar para el CSV aunque sea inválido

        tipo_doc_raw    = celda(hoja, r, col["tipo_doc"])
        tipo_doc        = normalizar_tipo_doc(tipo_doc_raw)
        estado_raw      = celda(hoja, r, col["estado"])
        estado          = normalizar_estado(estado_raw)
        cod_comp, nom_comp   = separar_codigo_nombre(comp)
        cod_res, nom_res     = separar_codigo_nombre(
            celda(hoja, r, col["resultado"])
        )

        # Validaciones de advertencia (no detienen la importación pero pueden causar errores)
        if not doc:
            errores.append(f"Fila {r+1}: número de documento vacío (comp={comp})")

        if estado_raw and estado_raw.upper() not in {
            e.upper() for e in ESTADOS_VALIDOS
        }:
            advertencias.append(
                f"Fila {r+1}: estado '{estado_raw}' normalizado a '{estado}'"
            )

        if tipo_doc_raw and tipo_doc.upper() not in TIPOS_DOC_VALIDOS:
            advertencias.append(
                f"Fila {r+1}: tipo de documento '{tipo_doc_raw}' "
                f"no reconocido — se usará '{tipo_doc}'"
            )

        filas.append({
            "fila":             r + 1,
            "tipo_doc":         tipo_doc,
            "doc":              doc,
            "nombres":          celda(hoja, r, col["nombres"]).strip(),
            "apellidos":        celda(hoja, r, col["apellidos"]).strip(),
            "estado":           estado,
            "estado_original":  estado_raw,
            "cod_competencia":  cod_comp,
            "nom_competencia":  nom_comp,
            "cod_resultado":    cod_res,
            "nom_resultado":    nom_res,
            "juicio":           juicio,
            "fecha_juicio":     celda(hoja, r, col["fecha_j"]),
            "funcionario":      celda(hoja, r, col["funcionario"]).strip(),
        })

    return cabecera, filas, advertencias, errores


# ─────────────────────────────────────────────────────────────────────────────
#  RESUMEN ESTADÍSTICO
# ─────────────────────────────────────────────────────────────────────────────

def calcular_resumen(cabecera, filas):
    """Calcula métricas y estadísticas del contenido del reporte."""
    aprendices    = {}   # doc → {nombres, tipo_doc, estado}
    competencias  = set()
    resultados    = set()
    juicios_cnt   = Counter()
    estados_cnt   = Counter()

    for f in filas:
        doc = f["doc"]
        if doc:
            if doc not in aprendices:
                aprendices[doc] = {
                    "nombres":   f["nombres"] + " " + f["apellidos"],
                    "tipo_doc":  f["tipo_doc"],
                    "estado":    f["estado"],
                }
            # Si el mismo aprendiz aparece con estados diferentes, marcar inconsistencia
            elif aprendices[doc]["estado"] != f["estado"]:
                aprendices[doc]["estado"] = f"INCONSISTENTE ({aprendices[doc]['estado']} / {f['estado']})"

        if f["cod_competencia"]:
            competencias.add((f["cod_competencia"], f["nom_competencia"]))
        if f["cod_resultado"]:
            resultados.add((f["cod_resultado"], f["nom_resultado"]))

        juicios_cnt[f["juicio"]] += 1
        estados_cnt[f["estado"]] += 1

    return {
        "total_filas":       len(filas),
        "aprendices":        aprendices,
        "competencias":      sorted(competencias),
        "resultados":        resultados,
        "juicios":           dict(juicios_cnt),
        "estados":           dict(estados_cnt),
    }


# ─────────────────────────────────────────────────────────────────────────────
#  SALIDA EN CONSOLA
# ─────────────────────────────────────────────────────────────────────────────

def imprimir_separador(titulo=""):
    ancho = 70
    if titulo:
        pad = (ancho - len(titulo) - 2) // 2
        print(f"{Fore.CYAN}{'═'*pad} {titulo} {'═'*pad}{Style.RESET_ALL}")
    else:
        print(f"{Fore.CYAN}{'═'*ancho}{Style.RESET_ALL}")


def imprimir_resumen(cabecera, resumen, advertencias, errores):
    imprimir_separador("SOFIA PREVIEW — Reporte de Juicios Evaluativos")

    # ── Metadatos de la ficha ────────────────────────────────────────────────
    print(f"\n  {'Número de ficha:':<22} {Fore.YELLOW}{cabecera['numero_ficha']}{Style.RESET_ALL}")
    print(f"  {'Denominación:':<22} {cabecera['denominacion']}")
    print(f"  {'Estado de la ficha:':<22} {cabecera['estado_ficha']}")
    print(f"  {'Fecha inicio:':<22} {cabecera['fecha_inicio'] or '—'}")
    print(f"  {'Fecha fin:':<22} {cabecera['fecha_fin'] or '—'}")
    if cabecera["regional"]:
        print(f"  {'Regional:':<22} {cabecera['regional']}")
    if cabecera["centro"]:
        print(f"  {'Centro:':<22} {cabecera['centro']}")

    # ── Contadores principales ───────────────────────────────────────────────
    imprimir_separador("RESUMEN")
    print(f"\n  {'Total filas de datos:':<28} {resumen['total_filas']}")
    print(f"  {'Aprendices únicos:':<28} {len(resumen['aprendices'])}")
    print(f"  {'Competencias:':<28} {len(resumen['competencias'])}")
    print(f"  {'Resultados de aprendizaje:':<28} {len(resumen['resultados'])}")

    # ── Juicios ──────────────────────────────────────────────────────────────
    print()
    for juicio, cnt in resumen["juicios"].items():
        color = Fore.GREEN if juicio == "APROBADO" else Fore.YELLOW
        print(f"  {color}{juicio:<28}{Style.RESET_ALL} {cnt}")

    # ── Estados de aprendiz ──────────────────────────────────────────────────
    imprimir_separador("ESTADOS DE APRENDIZ")
    print()
    for estado, cnt in sorted(resumen["estados"].items()):
        color = Fore.GREEN if estado == "EN FORMACION" else Fore.RED
        print(f"  {color}{estado:<28}{Style.RESET_ALL} {cnt} filas")

    # ── Competencias ─────────────────────────────────────────────────────────
    imprimir_separador("COMPETENCIAS DETECTADAS")
    print()
    for cod, nom in resumen["competencias"]:
        print(f"  {Fore.CYAN}{cod:<12}{Style.RESET_ALL} {nom[:56]}")

    # ── Aprendices ───────────────────────────────────────────────────────────
    imprimir_separador("APRENDICES")
    print()
    print(f"  {'DOC':<15} {'TIPO':<6} {'NOMBRE':<35} ESTADO")
    print(f"  {'─'*15} {'─'*6} {'─'*35} {'─'*20}")
    for doc, info in sorted(resumen["aprendices"].items()):
        estado_col = Fore.RED if "INCONSISTENTE" in info["estado"] else (
            Fore.GREEN if info["estado"] == "EN FORMACION" else Fore.YELLOW
        )
        print(
            f"  {doc:<15} {info['tipo_doc']:<6} "
            f"{info['nombres'][:35]:<35} "
            f"{estado_col}{info['estado']}{Style.RESET_ALL}"
        )

    # ── Advertencias ─────────────────────────────────────────────────────────
    if advertencias:
        imprimir_separador("ADVERTENCIAS")
        print()
        for adv in advertencias:
            print(f"  {Fore.YELLOW}⚠  {adv}{Style.RESET_ALL}")

    # ── Errores ───────────────────────────────────────────────────────────────
    if errores:
        imprimir_separador("ERRORES — estas filas serían rechazadas por el importer")
        print()
        for err in errores:
            print(f"  {Fore.RED}✗  {err}{Style.RESET_ALL}")
    else:
        print(f"\n  {Fore.GREEN}✓  Sin errores detectados — el archivo debería importarse correctamente.{Style.RESET_ALL}")

    imprimir_separador()


# ─────────────────────────────────────────────────────────────────────────────
#  EXPORTACIÓN CSV
# ─────────────────────────────────────────────────────────────────────────────

COLUMNAS_CSV = [
    "fila", "tipo_doc", "doc", "nombres", "apellidos",
    "estado", "estado_original", "cod_competencia", "nom_competencia",
    "cod_resultado", "nom_resultado", "juicio", "fecha_juicio", "funcionario",
]

def exportar_csv(filas, ruta_salida):
    """Exporta las filas normalizadas a un archivo CSV con BOM UTF-8 para Excel."""
    with open(ruta_salida, "w", newline="", encoding="utf-8-sig") as f:
        writer = csv.DictWriter(f, fieldnames=COLUMNAS_CSV, extrasaction="ignore")
        writer.writeheader()
        writer.writerows(filas)
    print(f"\n  {Fore.GREEN}✓  CSV exportado → {ruta_salida}{Style.RESET_ALL}")


# ─────────────────────────────────────────────────────────────────────────────
#  SALIDA JSON
# ─────────────────────────────────────────────────────────────────────────────

def exportar_json(cabecera, filas, advertencias, errores):
    """Imprime la salida completa en formato JSON (para integración con pipelines)."""
    salida = {
        "cabecera":     cabecera,
        "total_filas":  len(filas),
        "advertencias": advertencias,
        "errores":      errores,
        "filas":        filas,
    }
    print(json.dumps(salida, ensure_ascii=False, indent=2))


# ─────────────────────────────────────────────────────────────────────────────
#  PUNTO DE ENTRADA
# ─────────────────────────────────────────────────────────────────────────────

def main():
    parser = argparse.ArgumentParser(
        description="Valida y previsualiza un Reporte de Juicios Evaluativos de SOFIA Plus.",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog=__doc__,
    )
    parser.add_argument(
        "archivo",
        help="Ruta al archivo .xls exportado desde SOFIA Plus.",
    )
    parser.add_argument(
        "--csv",
        metavar="SALIDA.csv",
        help="Exportar filas normalizadas a un archivo CSV.",
    )
    parser.add_argument(
        "--json",
        action="store_true",
        help="Imprimir la salida en formato JSON (desactiva la salida visual).",
    )

    args = parser.parse_args()
    ruta = Path(args.archivo)

    # ── Validaciones básicas del archivo ────────────────────────────────────
    if not ruta.exists():
        print(f"{Fore.RED}ERROR: El archivo '{ruta}' no existe.{Style.RESET_ALL}")
        sys.exit(1)

    if ruta.suffix.lower() != ".xls":
        print(f"{Fore.YELLOW}Advertencia: se esperaba un .xls; el archivo tiene extensión '{ruta.suffix}'.{Style.RESET_ALL}")

    # ── Parsear el archivo ───────────────────────────────────────────────────
    try:
        cabecera, filas, advertencias, errores = normalizar_reporte(str(ruta))
    except Exception as ex:
        print(f"{Fore.RED}ERROR al leer el archivo: {ex}{Style.RESET_ALL}")
        sys.exit(1)

    if not filas:
        print(f"{Fore.RED}No se encontraron filas de datos con juicio en el archivo.{Style.RESET_ALL}")
        sys.exit(1)

    # ── Salida ───────────────────────────────────────────────────────────────
    if args.json:
        exportar_json(cabecera, filas, advertencias, errores)
    else:
        resumen = calcular_resumen(cabecera, filas)
        imprimir_resumen(cabecera, resumen, advertencias, errores)

    if args.csv:
        exportar_csv(filas, args.csv)

    # Código de salida: 1 si hay errores (útil en scripts CI/CD)
    sys.exit(1 if errores else 0)


if __name__ == "__main__":
    main()
