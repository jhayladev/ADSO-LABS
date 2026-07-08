using AdsoLabs.Infrastructure.Data;
using AdsoLabs.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace AdsoLabs.Infrastructure.Data;

public static class AdsoDbContextSeeder
{
    public static async Task SeedAsync(AdsoDbContext context)
    {
        // Cada método verifica si ya existen datos antes de insertar,
        // así el seed es idempotente: se puede llamar múltiples veces sin duplicar.
        await SeedTiposDocumentoAsync(context);
        await SeedRolesAsync(context);
        await SeedTiposArchivoAsync(context);
        await SeedAdminAsync(context);
        await SeedCompetenciasAsync(context);
        await SeedResultadosAprendizajeAsync(context);
        await SeedInstructoresAsync(context);
        await SeedAdminInstructorPerfilAsync(context);
        await SeedFichaCompetenciaAsync(context);
        await SeedAsistenciasAsync(context);

    }

    // ─────────────────────────────────────────────
    // 1. TIPOS DE DOCUMENTO
    // ─────────────────────────────────────────────
    private static async Task SeedTiposDocumentoAsync(AdsoDbContext context)
    {

        if (await context.Rol.AnyAsync()) return;

        context.TipoDocumento.AddRange(
            new TipoDocumento { Nombre = "CC" },
            new TipoDocumento { Nombre = "TI" },
            new TipoDocumento { Nombre = "CE" },
            new TipoDocumento { Nombre = "PPT" },
            new TipoDocumento { Nombre = "PA" },
            new TipoDocumento { Nombre = "RC" }

        );
        await context.SaveChangesAsync();
    }

    // ─────────────────────────────────────────────
    // 2. ROLES
    // ─────────────────────────────────────────────
    private static async Task SeedRolesAsync(AdsoDbContext context)
    {
        if (await context.Rol.AnyAsync()) return;

        context.Rol.AddRange(
            new Rol { Nombre = "Administrador", Descripcion = "Acceso total al sistema", Activo = true },
            new Rol { Nombre = "Instructor", Descripcion = "Gestion academica", Activo = true },
            new Rol { Nombre = "Aprendiz", Descripcion = "Acceso limitado para aprendices", Activo = true }
        );
        await context.SaveChangesAsync();
    }

    // ─────────────────────────────────────────────
    // 3. TIPOS DE ARCHIVO
    // ─────────────────────────────────────────────
    private static async Task SeedTiposArchivoAsync(AdsoDbContext context)
    {
        if (await context.TipoArchivo.AnyAsync()) return;

        context.TipoArchivo.AddRange(
            new TipoArchivo { Nombre = "Guia de Aprendizaje" },
            new TipoArchivo { Nombre = "Instrumento de Evaluacion" },
            new TipoArchivo { Nombre = "Plan Concertado" },
            new TipoArchivo { Nombre = "Planeacion Pedagogica" },
            new TipoArchivo { Nombre = "Proyecto Formativo" },
            new TipoArchivo { Nombre = "Desarrollo Curricular" },
            new TipoArchivo { Nombre = "Importacion Masiva" }
        );
        await context.SaveChangesAsync();
    }

    // ─────────────────────────────────────────────
    // 4. USUARIO ADMINISTRADOR
    // ─────────────────────────────────────────────
    private static async Task SeedAdminAsync(AdsoDbContext context)
    {
        if (await context.Usuario.AnyAsync(u => u.Correo == "ypushaina@sena.edu.co")) return;

        var tipoCC = await context.TipoDocumento.FirstAsync(t => t.Nombre == "CC");
        var rolAdmin = await context.Rol.FirstAsync(r => r.Nombre == "Administrador");

        var persona = new Persona
        {
            IdTipoDocumento = tipoCC.IdTipoDocumento,
            NumeroDocumento = "5165372",
            Nombres = "Yasser",
            Apellidos = "Pushaina Rojas",
            FechaCreacion = DateTime.Now
        };
        context.Persona.Add(persona);
        await context.SaveChangesAsync();

        var usuario = new Usuario
        {
            IdPersona = persona.IdPersona,
            IdRol = rolAdmin.IdRol,
            Correo = "ypushaina@sena.edu.co",
            PasswordHash = [0x00],
            PasswordSalt = [0x00],
            Activo = true,
            PrimerLogin = true,
            IntentosFallidos = 0,
            FechaCreacion = DateTime.Now
        };
        context.Usuario.Add(usuario);
        await context.SaveChangesAsync();
    }

    // ─────────────────────────────────────────────
    // 5. COMPETENCIAS REALES ADSO
    // ─────────────────────────────────────────────
    private static async Task SeedCompetenciasAsync(AdsoDbContext context)
    {
        if (await context.Competencia.AnyAsync()) return;

        context.Competencia.AddRange(
            new Competencia
            {
                Codigo = "36182",
                HorasAsignadas = 48,
                Tipo = "Induccion",
                Estado = "Activa",
                FaseFormativa = "Induccion",
                Nombre = "Resultado de Aprendizaje de la Inducción",
                Descripcion = "01 Identificar la dinámica organizacional del SENA y el rol de la formación profesional integral de acuerdo con su proyecto de vida y el desarrollo profesional."
            },
            new Competencia
            {
                Codigo = "38561",
                HorasAsignadas = 48,
                Tipo = "Transversal",
                Estado = "Activa",
                FaseFormativa = "Evaluacion",
                Nombre = "Gestionar procesos propios de la cultura emprendedora y empresarial de acuerdo con el perfil personal y los requerimientos de los contextos productivo y social",
                Descripcion = "01 Integrar elementos de la cultura emprendedora teniendo en cuenta el perfil personal y el contexto de desarrollo social. " +
                    "02 Caracterizar la idea de negocio teniendo en cuenta las oportunidades y necesidades del sector productivo y social. " +
                    "03 Estructurar el plan de negocio de acuerdo con las características empresariales y tendencias de mercado. " +
                    "04 Valorar la propuesta de negocio conforme con su estructura y necesidades del sector productivo y social."
            },
            new Competencia
            {
                Codigo = "38362",
                HorasAsignadas = 336,
                Tipo = "Tecnica",
                Estado = "Activa",
                FaseFormativa = "Planeacion",
                Nombre = "Diseñar la solución de software de acuerdo con procedimientos y requisitos técnicos",
                Descripcion = "01 Elaborar los artefactos de diseño del software siguiendo las prácticas de la metodología seleccionada. " +
                    "02 Estructurar el modelo de datos del software de acuerdo con las especificaciones del análisis. " +
                    "03 Determinar las características técnicas de la interfaz gráfica del software adoptando estándares. " +
                    "04 Verificar los entregables de la fase de diseño del software de acuerdo con lo establecido en el informe de análisis."
            },
            new Competencia
            {
                Codigo = "38368",
                HorasAsignadas = 1008,
                Tipo = "Tecnica",
                Estado = "Activa",
                FaseFormativa = "Ejecucion",
                Nombre = "Desarrollar la solución de software de acuerdo con el diseño y metodologías de desarrollo",
                Descripcion = "01 Planear actividades de construcción del software de acuerdo con el diseño establecido. " +
                    "02 Construir la base de datos para el software a partir del modelo de datos. " +
                    "03 Crear componentes front-end del software de acuerdo con el diseño. " +
                    "04 Codificar el software de acuerdo con el diseño establecido. " +
                    "05 Realizar pruebas al software para verificar su funcionalidad."
            },
            new Competencia
            {
                Codigo = "37799",
                HorasAsignadas = 48,
                Tipo = "Transversal",
                Estado = "Activa",
                FaseFormativa = "Planeacion",
                Nombre = "Aplicar prácticas de protección ambiental, seguridad y salud en el trabajo de acuerdo con las políticas organizacionales y la normatividad vigente",
                Descripcion = "01 Analizar las estrategias para la prevención y control de los impactos ambientales y de los accidentes y enfermedades laborales (ATEL) de acuerdo con las políticas organizacionales. " +
                    "02 Implementar estrategias para el control de los impactos ambientales y de los accidentes y enfermedades de acuerdo con los planes y programas establecidos por la organización. " +
                    "03 Realizar seguimiento y acompañamiento al desarrollo de los planes y programas ambientales y SST, según el área de desempeño. " +
                    "04 Proponer acciones de mejora para el manejo ambiental y el control de la SST, de acuerdo con estrategias de trabajo colaborativo, cooperativo y coordinado en el contexto productivo."
            },
            new Competencia
            {
                Codigo = "37371",
                HorasAsignadas = 48,
                Tipo = "Clave",
                Estado = "Activa",
                FaseFormativa = "Analisis",
                Nombre = "Utilizar herramientas informáticas de acuerdo con las necesidades de manejo de información",
                Descripcion = "01 Alistar herramientas de tecnologías de la información y la comunicación (TIC), de acuerdo con las necesidades de procesamiento de información y comunicación. " +
                    "02 Aplicar funcionalidades de herramientas y servicios TIC, de acuerdo con manuales de uso, procedimientos establecidos y buenas prácticas. " +
                    "03 Evaluar los resultados, de acuerdo con los requerimientos. " +
                    "04 Optimizar los resultados, de acuerdo con la verificación."
            },
            new Competencia
            {
                Codigo = "38392",
                HorasAsignadas = 144,
                Tipo = "Tecnica",
                Estado = "Activa",
                FaseFormativa = "Analisis",
                Nombre = "Establecer requisitos de la solución de software de acuerdo con estándares y procedimiento técnico",
                Descripcion = "01 Caracterizar los procesos de la organización de acuerdo con el software a construir. " +
                    "02 Recolectar información del software a construir de acuerdo con las necesidades del cliente. " +
                    "03 Establecer los requisitos del software de acuerdo con la información recolectada. " +
                    "04 Validar el informe de requisitos de acuerdo con las necesidades del cliente."
            },
            new Competencia
            {
                Codigo = "36180",
                HorasAsignadas = 48,
                Tipo = "Transversal",
                Estado = "Activa",
                FaseFormativa = "Planeacion",
                Nombre = "Interactuar en el contexto productivo y social de acuerdo con principios éticos para la construcción de una cultura de paz",
                Descripcion = "01 Promover mi dignidad y la del otro a partir de los principios y valores éticos como aporte en la instauración de una cultura de paz. " +
                    "02 Establecer relaciones de crecimiento personal y comunitario a partir del bien común como aporte para el desarrollo social. " +
                    "03 Promover el uso racional de los recursos naturales a partir de criterios de sostenibilidad y sustentabilidad ética y normativa vigente. " +
                    "04 Contribuir con el fortalecimiento de la cultura de paz a partir de la dignidad humana y las estrategias para la transformación de conflictos."
            },
            new Competencia
            {
                Codigo = "38199",
                HorasAsignadas = 48,
                Tipo = "Transversal",
                Estado = "Activa",
                FaseFormativa = "Planeacion",
                Nombre = "Orientar investigación formativa según referentes técnicos",
                Descripcion = "01 Analizar el contexto productivo según sus características y necesidades. " +
                    "02 Estructurar el proyecto de acuerdo a criterios de la investigación. " +
                    "03 Argumentar aspectos teóricos del proyecto según referentes nacionales e internacionales. " +
                    "04 Proponer soluciones a las necesidades del contexto según resultados de la investigación."
            },
            new Competencia
            {
                Codigo = "38560",
                HorasAsignadas = 48,
                Tipo = "Clave",
                Estado = "Activa",
                FaseFormativa = "Planeacion",
                Nombre = "Razonar cuantitativamente frente a situaciones susceptibles de ser abordadas de manera matemática en contextos laborales, sociales y personales",
                Descripcion = "01 Identificar modelos matemáticos de acuerdo con los requerimientos del problema planteado en contextos sociales y productivos. " +
                    "02 Plantear problemas matemáticos a partir de situaciones generadas en el contexto social y productivo. " +
                    "03 Resolver problemas matemáticos a partir de situaciones generadas en el contexto social y productivo. " +
                    "04 Proponer acciones de mejora frente a los resultados de los procedimientos matemáticos de acuerdo con el problema planteado."
            },
            new Competencia
            {
                Codigo = "37801",
                HorasAsignadas = 48,
                Tipo = "Clave",
                Estado = "Activa",
                FaseFormativa = "Planeacion",
                Nombre = "Aplicación de conocimientos de las ciencias naturales de acuerdo con situaciones del contexto productivo y social",
                Descripcion = "01 Identificar los principios y leyes de la física en la solución de problemas de acuerdo al contexto productivo. " +
                    "02 Solucionar problemas asociados con el sector productivo con base en los principios y leyes de la física. " +
                    "03 Verificar las transformaciones físicas de la materia utilizando herramientas. " +
                    "04 Proponer acciones de mejora en los procesos productivos de acuerdo con los principios y leyes de la física."
            },
            new Competencia
            {
                Codigo = "37800",
                HorasAsignadas = 48,
                Tipo = "Transversal",
                Estado = "Activa",
                FaseFormativa = "Planeacion",
                Nombre = "Generar hábitos saludables de vida mediante la aplicación de programas de actividad física en los contextos productivos y sociales",
                Descripcion = "01 Desarrollar habilidades psicomotrices en el contexto productivo y social. " +
                    "02 Practicar hábitos saludables mediante la aplicación de fundamentos de nutrición. " +
                    "03 Ejecutar actividades de acondicionamiento físico orientadas hacia el mejoramiento de la condición física en los contextos productivo y social. " +
                    "04 Implementar un plan de ergonomía y pausas activas según las características de la función productiva."
            },
            new Competencia
            {
                Codigo = "37802",
                HorasAsignadas = 48,
                Tipo = "Transversal",
                Estado = "Activa",
                FaseFormativa = "Analisis",
                Nombre = "Desarrollar procesos de comunicación eficaces y efectivos, teniendo en cuenta situaciones de orden social, personal y productivo",
                Descripcion = "01 Analizar los componentes de la comunicación según sus características, intencionalidad y contexto. " +
                    "02 Argumentar en forma oral y escrita atendiendo las exigencias y particularidades de las diversas situaciones comunicativas mediante los distintos sistemas de representación. " +
                    "03 Relacionar los procesos comunicativos teniendo en cuenta criterios de lógica y racionalidad. " +
                    "04 Establecer procesos de enriquecimiento lexical y acciones de mejoramiento en el desarrollo de procesos comunicativos según requerimientos del contexto."
            },
            new Competencia
            {
                Codigo = "38558",
                HorasAsignadas = 48,
                Tipo = "Transversal",
                Estado = "Activa",
                FaseFormativa = "Evaluacion",
                Nombre = "Ejercer derechos fundamentales del trabajo en el marco de la constitución política y los convenios internacionales",
                Descripcion = "01 Reconocer el trabajo como factor de movilidad social y transformación vital con referencia a la fenomenología y a los derechos fundamentales en el trabajo. " +
                    "02 Valorar la importancia de la ciudadanía laboral con base en el estudio de los derechos humanos y fundamentales en el trabajo. " +
                    "03 Practicar los derechos fundamentales en el trabajo de acuerdo con la Constitución Política y los Convenios Internacionales. " +
                    "04 Participar en acciones solidarias teniendo en cuenta el ejercicio de los derechos humanos, de los pueblos y de la naturaleza."
            },
            new Competencia
            {
                Codigo = "38376",
                HorasAsignadas = 288,
                Tipo = "Tecnica",
                Estado = "Activa",
                FaseFormativa = "Analisis",
                Nombre = "Evaluar requisitos de la solución de software de acuerdo con metodologías de análisis y estándares",
                Descripcion = "01 Planear actividades de análisis de acuerdo con la metodología seleccionada. " +
                    "02 Modelar las funciones del software de acuerdo con el informe de requisitos. " +
                    "03 Desarrollar procesos lógicos a través de la implementación de algoritmos. " +
                    "04 Verificar los modelos realizados en la fase de análisis de acuerdo con lo establecido en el informe de requisitos."
            },
            new Competencia
            {
                Codigo = "37714",
                HorasAsignadas = 384,
                Tipo = "Transversal",
                Estado = "Activa",
                FaseFormativa = "Transversal",
                Nombre = "Interactuar en lengua inglesa de forma oral y escrita dentro de contextos sociales y laborales según los criterios establecidos por el Marco Común Europeo de Referencia para las Lenguas",
                Descripcion = "01 Comprender información sobre situaciones cotidianas y laborales actuales y futuras a través de interacciones sociales de forma oral y escrita. " +
                    "02 Intercambiar opiniones sobre situaciones cotidianas y laborales actuales, pasadas y futuras en contextos sociales orales y escritos. " +
                    "03 Discutir sobre posibles soluciones a problemas dentro de un rango variado de contextos sociales y laborales. " +
                    "04 Implementar acciones de mejora relacionadas con el uso de expresiones, estructuras y desempeño según los resultados de aprendizaje formulados para el programa. " +
                    "05 Presentar un proceso para la realización de una actividad en su quehacer laboral de acuerdo con los procedimientos establecidos desde su programa de formación. " +
                    "06 Explicar las funciones de su ocupación laboral usando expresiones de acuerdo al nivel requerido por el programa de formación."
            },
            new Competencia
            {
                Codigo = "38356",
                HorasAsignadas = 144,
                Tipo = "Tecnica",
                Estado = "Activa",
                FaseFormativa = "Evaluacion",
                Nombre = "Implementar la solución de software de acuerdo con los requisitos de operación y modelos de referencia",
                Descripcion = "01 Planear actividades de implantación del software de acuerdo con las condiciones del sistema. " +
                    "02 Desplegar el software de acuerdo con la arquitectura y las políticas establecidas. " +
                    "03 Documentar el proceso de implantación de software siguiendo estándares de calidad. " +
                    "04 Implantar el software de acuerdo con los niveles de servicio establecidos con el cliente."
            },
            new Competencia
            {
                Codigo = "38369",
                HorasAsignadas = 144,
                Tipo = "Tecnica",
                Estado = "Activa",
                FaseFormativa = "Evaluacion",
                Nombre = "Controlar la calidad del servicio de software de acuerdo con los estándares técnicos",
                Descripcion = "01 Incorporar actividades de aseguramiento de la calidad del software de acuerdo con estándares de la industria. " +
                    "02 Verificar la calidad del software de acuerdo con las prácticas asociadas en los procesos de desarrollo. " +
                    "03 Realizar actividades de mejora de la calidad del software a partir de los resultados de la verificación."
            },
            new Competencia
            {
                Codigo = "38367",
                HorasAsignadas = 144,
                Tipo = "Tecnica",
                Estado = "Activa",
                FaseFormativa = "Analisis",
                Nombre = "Estructurar propuesta técnica de servicio de tecnología de la información según requisitos técnicos y normativa",
                Descripcion = "01 Definir especificaciones técnicas del software de acuerdo con las características del software a construir. " +
                    "02 Elaborar propuesta técnica del software de acuerdo con las especificaciones técnicas definidas. " +
                    "03 Validar las condiciones de la propuesta técnica del software de acuerdo con los intereses de las partes."
            },
            new Competencia
            {
                Codigo = "590803",
                HorasAsignadas = 864,
                Tipo = "Practica",
                Estado = "Activa",
                FaseFormativa = "Evaluacion",
                Nombre = "Etapa Práctica",
                Descripcion = "Aplicar en la resolución de problemas reales del sector productivo, los conocimientos, habilidades y destrezas pertinentes a las competencias del programa de formación asumiendo estrategias y metodologías de autogestión."
            }
        );
        await context.SaveChangesAsync();
    }

    // ─────────────────────────────────────────────
    // 6. PLANES CONCERTADOS Y RESULTADOS DE APRENDIZAJE
    // ─────────────────────────────────────────────
    //
    // Códigos extraídos directamente del Reporte de Juicios Evaluativos de SOFIA Plus.
    // El Nombre se almacena como "{codigoResultado} - {descripcion}" (igual que
    // UpsertResultadoAprendizajeAsync), truncado a 200 caracteres.
    // Las descripciones conservan el prefijo numérico (01, 02…) tal como aparece en SOFIA.
    //
    private static async Task SeedResultadosAprendizajeAsync(AdsoDbContext context)
    {
        if (await context.PlanConcertado.AnyAsync()) return;

        // Índice de competencias por código → IdCompetencia
        var comps = await context.Competencia
            .AsNoTracking()
            .ToDictionaryAsync(c => c.Codigo, c => c.IdCompetencia);

        // Cada entrada: código de competencia → lista de (código resultado, descripción SOFIA)
        // El Nombre almacenado en BD es "{codigoResultado} - {descripción}" (max 200 chars).
        var catalogo = new List<(string CodComp, List<(string Cod, string Desc)> Resultados)>
        {
            // ── 36182 - Resultado de Aprendizaje de la Inducción ─────────────────
            ("36182", [
                ("593343", "01  IDENTIFICAR LA DINÁMICA ORGANIZACIONAL DEL SENA Y EL ROL DE LA FORMACIÓN PROFESIONAL INTEGRAL DE ACUERDO CON SU PROYECTO DE VIDA Y EL DESARROLLO PROFESIONAL."),
            ]),

            // ── 38561 - Gestionar procesos de cultura emprendedora y empresarial ─
            ("38561", [
                ("593342", "01  INTEGRAR ELEMENTOS DE LA CULTURA EMPRENDEDORA TENIENDO EN CUENTA EL PERFIL PERSONAL Y EL CONTEXTO DE DESARROLLO SOCIAL."),
                ("593259", "02  CARACTERIZAR LA IDEA DE NEGOCIO TENIENDO EN CUENTA LAS OPORTUNIDADES Y NECESIDADES DEL SECTOR PRODUCTIVO Y SOCIAL."),
                ("593340", "03  ESTRUCTURAR EL PLAN DE NEGOCIO DE ACUERDO CON LAS CARACTERÍSTICAS EMPRESARIALES Y TENDENCIAS DE MERCADO."),
                ("593341", "04  VALORAR LA PROPUESTA DE NEGOCIO CONFORME CON SU ESTRUCTURA Y NECESIDADES DEL SECTOR PRODUCTIVO Y SOCIAL."),
            ]),

            // ── 38392 - Establecer requisitos de la solución de software ─────────
            ("38392", [
                ("593346", "01  CARACTERIZAR LOS PROCESOS DE LA ORGANIZACIÓN DE ACUERDO CON EL SOFTWARE A CONSTRUIR."),
                ("593344", "02  RECOLECTAR INFORMACIÓN DEL SOFTWARE A CONSTRUIR DE ACUERDO CON LAS NECESIDADES DEL CLIENTE."),
                ("593347", "03  ESTABLECER LOS REQUISITOS DEL SOFTWARE DE ACUERDO CON LA INFORMACIÓN RECOLECTADA."),
                ("593345", "04  VALIDAR EL INFORME DE REQUISITOS DE ACUERDO CON LAS NECESIDADES DEL CLIENTE."),
            ]),

            // ── 38376 - Evaluar requisitos de la solución de software ────────────
            ("38376", [
                ("592375", "01 PLANEAR ACTIVIDADES DE ANÁLISIS DE ACUERDO CON LA METODOLOGÍA SELECCIONADA."),
                ("592373", "02  MODELAR LAS FUNCIONES DEL SOFTWARE DE ACUERDO CON EL INFORME DE REQUISITOS."),
                ("592376", "03  DESARROLLAR PROCESOS LÓGICOS A TRAVÉS DE LA IMPLEMENTACIÓN DE ALGORITMOS."),
                ("592374", "04  VERIFICAR LOS MODELOS REALIZADOS EN LA FASE DE ANÁLISIS DE ACUERDO CON LO ESTABLECIDO EN EL INFORME DE REQUISITOS."),
            ]),

            // ── 38362 - Diseñar la solución de software ──────────────────────────
            ("38362", [
                ("593103", "01  ELABORAR LOS ARTEFACTOS DE DISEÑO DEL SOFTWARE SIGUIENDO LAS PRÁCTICAS DE LA METODOLOGÍA SELECCIONADA."),
                ("593101", "02  ESTRUCTURAR EL MODELO DE DATOS DEL SOFTWARE DE ACUERDO CON LAS ESPECIFICACIONES DEL ANÁLISIS."),
                ("593100", "03  DETERMINAR LAS CARACTERÍSTICAS TÉCNICAS DE LA INTERFAZ GRÁFICA DEL SOFTWARE ADOPTANDO ESTÁNDARES."),
                ("593102", "04  VERIFICAR LOS ENTREGABLES DE LA FASE DE DISEÑO DEL SOFTWARE DE ACUERDO CON LO ESTABLECIDO EN EL INFORME DE ANÁLISIS."),
            ]),

            // ── 38356 - Implementar la solución de software ──────────────────────
            ("38356", [
                ("593111", "01  PLANEAR ACTIVIDADES DE IMPLANTACIÓN DEL SOFTWARE DE ACUERDO CON LAS CONDICIONES DEL SISTEMA."),
                ("593110", "02  DESPLEGAR EL SOFTWARE DE ACUERDO CON LA ARQUITECTURA Y LAS POLÍTICAS ESTABLECIDAS."),
                ("593109", "03  DOCUMENTAR EL PROCESO DE IMPLANTACIÓN DE SOFTWARE SIGUIENDO ESTÁNDARES DE CALIDAD."),
                ("593112", "04  IMPLANTAR EL SOFTWARE DE ACUERDO CON LOS NIVELES DE SERVICIO ESTABLECIDOS CON EL CLIENTE."),
            ]),

            // ── 38368 - Desarrollar la solución de software ──────────────────────
            ("38368", [
                ("593106", "01  PLANEAR ACTIVIDADES DE CONSTRUCCIÓN DEL SOFTWARE DE ACUERDO CON EL DISEÑO ESTABLECIDO."),
                ("593107", "02  CONSTRUIR LA BASE DE DATOS PARA EL SOFTWARE A PARTIR DEL MODELO DE DATOS."),
                ("593104", "03  CREAR COMPONENTES FRONT-END DEL SOFTWARE DE ACUERDO CON EL DISEÑO."),
                ("593108", "04  CODIFICAR EL SOFTWARE DE ACUERDO CON EL DISEÑO ESTABLECIDO."),
                ("593105", "05  REALIZAR PRUEBAS AL SOFTWARE PARA VERIFICAR SU FUNCIONALIDAD."),
            ]),

            // ── 38369 - Controlar la calidad del servicio de software ────────────
            ("38369", [
                ("593146", "01  INCORPORAR ACTIVIDADES DE ASEGURAMIENTO DE LA CALIDAD DEL SOFTWARE DE ACUERDO CON ESTÁNDARES DE LA INDUSTRIA."),
                ("593144", "02  VERIFICAR LA CALIDAD DEL SOFTWARE DE ACUERDO CON LAS PRÁCTICAS ASOCIADAS EN LOS PROCESOS DE DESARROLLO."),
                ("593145", "03  REALIZAR ACTIVIDADES DE MEJORA DE LA CALIDAD DEL SOFTWARE A PARTIR DE LOS RESULTADOS DE LA VERIFICACIÓN."),
            ]),

            // ── 38367 - Estructurar propuesta técnica de TI ──────────────────────
            ("38367", [
                ("593060", "01  DEFINIR ESPECIFICACIONES TÉCNICAS DEL SOFTWARE DE ACUERDO CON LAS CARACTERÍSTICAS DEL SOFTWARE A CONSTRUIR."),
                ("593062", "02  ELABORAR PROPUESTA TÉCNICA DEL SOFTWARE DE ACUERDO CON LAS ESPECIFICACIONES TÉCNICAS DEFINIDAS."),
                ("593061", "03  VALIDAR LAS CONDICIONES DE LA PROPUESTA TÉCNICA DEL SOFTWARE DE ACUERDO CON LOS INTERESES DE LAS PARTES."),
            ]),

            // ── 37371 - Herramientas informáticas (Clave) ────────────────────────
            ("37371", [
                ("593154", "01  ALISTAR HERRAMIENTAS DE TECNOLOGÍAS DE LA INFORMACIÓN Y LA COMUNICACIÓN (TIC), DE ACUERDO CON LAS NECESIDADES DE PROCESAMIENTO DE INFORMACIÓN Y COMUNICACIÓN."),
                ("593151", "02  APLICAR FUNCIONALIDADES DE HERRAMIENTAS Y SERVICIOS TIC, DE ACUERDO CON MANUALES DE USO, PROCEDIMIENTOS ESTABLECIDOS Y BUENAS PRÁCTICAS."),
                ("593153", "03  EVALUAR LOS RESULTADOS, DE ACUERDO CON LOS REQUERIMIENTOS."),
                ("593152", "04  OPTIMIZAR LOS RESULTADOS, DE ACUERDO CON LA VERIFICACIÓN."),
            ]),

            // ── 38560 - Razonamiento cuantitativo (Clave) ────────────────────────
            ("38560", [
                ("593256", "01  IDENTIFICAR MODELOS MATEMÁTICOS DE ACUERDO CON LOS REQUERIMIENTOS DEL PROBLEMA PLANTEADO  EN CONTEXTOS SOCIALES Y PRODUCTIVO."),
                ("593258", "02  PLANTEAR PROBLEMAS MATEMÁTICOS A PARTIR DE SITUACIONES GENERADAS EN EL CONTEXTO SOCIAL Y PRODUCTIVO."),
                ("593255", "03  RESOLVER PROBLEMAS MATEMÁTICOS A PARTIR DE SITUACIONES GENERADAS EN EL CONTEXTO SOCIAL Y PRODUCTIVO."),
                ("593257", "04  PROPONER ACCIONES DE MEJORA FRENTE A LOS RESULTADOS DE LOS PROCEDIMIENTOS MATEMÁTICOS DE ACUERDO CON EL PROBLEMA PLANTEADO."),
            ]),

            // ── 37801 - Ciencias naturales (Clave) ───────────────────────────────
            ("37801", [
                ("593162", "01  IDENTIFICAR LOS PRINCIPIOS Y LEYES DE LA FÍSICA EN LA SOLUCIÓN DE PROBLEMAS DE ACUERDO AL CONTEXTO PRODUCTIVO."),
                ("593159", "02  SOLUCIONAR PROBLEMAS ASOCIADOS CON EL SECTOR PRODUCTIVO CON BASE EN LOS PRINCIPIOS Y LEYES DE LA FÍSICA."),
                ("593161", "03  VERIFICAR LAS TRANSFORMACIONES FÍSICAS DE LA MATERIA UTILIZANDO HERRAMIENTAS TECNOLÓGICAS."),
                ("593160", "04  PROPONER ACCIONES DE MEJORA EN LOS PROCESOS PRODUCTIVOS DE ACUERDO CON LOS PRINCIPIOS Y LEYES DE LA FÍSICA."),
            ]),

            // ── 37799 - Seguridad y salud en el trabajo (Transversal) ────────────
            ("37799", [
                ("593156", "01  ANALIZAR LAS ESTRATEGIAS PARA LA PREVENCIÓN Y CONTROL DE LOS IMPACTOS AMBIENTALES Y DE LOS ACCIDENTES Y ENFERMEDADES LABORALES (ATEL) DE ACUERDO CON LAS POLÍTICAS ORGANIZACIONALES Y EL ENTORNO SOCIAL."),
                ("593158", "02  IMPLEMENTAR ESTRATEGIAS PARA EL CONTROL DE LOS IMPACTOS AMBIENTALES Y DE LOS ACCIDENTES Y ENFERMEDADES   DE ACUERDO  CON LOS PLANES Y PROGRAMAS  ESTABLECIDOS POR LA ORGANIZACIÓN."),
                ("593157", "03  REALIZAR SEGUIMIENTO Y ACOMPAÑAMIENTO AL DESARROLLO DE LOS PLANES Y PROGRAMAS AMBIENTALES Y SST, SEGÚN EL  ÁREA DE DESEMPEÑO."),
                ("593155", "04  PROPONER ACCIONES DE MEJORA PARA EL MANEJO AMBIENTAL Y EL CONTROL DE LA SST, DE ACUERDO CON ESTRATEGIAS DE TRABAJO, COLABORATIVO, COOPERATIVO Y COORDINADO EN EL CONTEXTO PRODUCTIVO Y SOCIAL."),
            ]),

            // ── 36180 - Ética para la cultura de paz (Transversal) ───────────────
            ("36180", [
                ("593149", "01  PROMOVER MI DIGNIDAD Y LA DEL OTRO A PARTIR DE LOS PRINCIPIOS Y VALORES ÉTICOS COMO APORTE EN LA INSTAURACIÓN DE UNA CULTURA DE PAZ."),
                ("593147", "02  ESTABLECER RELACIONES DE CRECIMIENTO PERSONAL Y COMUNITARIO A PARTIR DEL BIEN COMÚN COMO APORTE PARA EL DESARROLLO SOCIAL."),
                ("593148", "03  PROMOVER EL USO RACIONAL DE LOS RECURSOS NATURALES A PARTIR DE CRITERIOS DE SOSTENIBILIDAD Y SUSTENTABILIDAD ÉTICA Y NORMATIVA VIGENTE."),
                ("593150", "04  CONTRIBUIR CON EL FORTALECIMIENTO DE LA CULTURA DE PAZ A PARTIR DE LA DIGNIDAD HUMANA Y LAS ESTRATEGIAS PARA LA TRANSFORMACIÓN DE CONFLICTOS."),
            ]),

            // ── 38199 - Investigación formativa (Transversal) ────────────────────
            ("38199", [
                ("593236", "01  ANALIZAR EL CONTEXTO PRODUCTIVO SEGÚN SUS CARACTERÍSTICAS Y NECESIDADES."),
                ("593238", "02  ESTRUCTURAR EL PROYECTO DE ACUERDO A CRITERIOS DE LA INVESTIGACIÓN."),
                ("593237", "03  ARGUMENTAR ASPECTOS TEÓRICOS DEL PROYECTO SEGÚN REFERENTES NACIONALES E INTERNACIONALES."),
                ("593235", "04  PROPONER SOLUCIONES A LAS NECESIDADES DEL CONTEXTO SEGÚN RESULTADOS DE LA INVESTIGACIÓN."),
            ]),

            // ── 37800 - Hábitos saludables (Transversal) ─────────────────────────
            ("37800", [
                ("593120", "01  DESARROLLAR HABILIDADES PSICOMOTRICES EN EL CONTEXTO PRODUCTIVO Y SOCIAL."),
                ("593119", "02  PRACTICAR HÁBITOS SALUDABLES MEDIANTE LA APLICACIÓN DE  FUNDAMENTOS DE NUTRICIÓN E HIGIENE."),
                ("593121", "03  EJECUTAR ACTIVIDADES DE ACONDICIONAMIENTO FÍSICO ORIENTADAS HACIA EL MEJORAMIENTO DE LA CONDICIÓN FÍSICA EN LOS CONTEXTOS PRODUCTIVO Y SOCIAL."),
                ("593122", "04  IMPLEMENTAR UN PLAN DE ERGONOMÍA Y PAUSAS ACTIVAS SEGÚN LAS CARACTERÍSTICAS DE LA FUNCIÓN PRODUCTIVA."),
            ]),

            // ── 37802 - Comunicación eficaz (Transversal) ────────────────────────
            ("37802", [
                ("593225", "01  ANALIZAR LOS COMPONENTES DE LA COMUNICACIÓN SEGÚN SUS CARACTERÍSTICAS, INTENCIONALIDAD Y CONTEXTO."),
                ("593227", "02  ARGUMENTAR EN FORMA ORAL Y ESCRITA ATENDIENDO LAS EXIGENCIAS Y PARTICULARIDADES DE LAS DIVERSAS SITUACIONES COMUNICATIVAS MEDIANTE LOS DISTINTOS SISTEMAS DE REPRESENTACIÓN."),
                ("593224", "03  RELACIONAR LOS PROCESOS COMUNICATIVOS TENIENDO EN CUENTA CRITERIOS DE LÓGICA Y RACIONALIDAD."),
                ("593226", "04  ESTABLECER PROCESOS DE ENRIQUECIMIENTO LEXICAL Y ACCIONES DE MEJORAMIENTO EN EL DESARROLLO DE PROCESOS COMUNICATIVOS SEGÚN REQUERIMIENTOS DEL CONTEXTO."),
            ]),

            // ── 38558 - Derechos fundamentales del trabajo (Transversal) ─────────
            ("38558", [
                ("593243", "01- Reconocer el trabajo como factor de movilidad social y transformación vital con referencia a la fenomenología y a los derechos fundamentales en el trabajo."),
                ("593245", "02- Valorar la importancia de la ciudadanía laboral con base en el estudio de los derechos humanos y fundamentales en el trabajo."),
                ("593244", "03- Practicar los derechos fundamentales en el trabajo de acuerdo con la Constitución Política y los Convenios Internacionales."),
                ("593246", "04- Participar en acciones solidarias teniendo en cuenta el ejercicio de los derechos humanos, de los pueblos y de la naturaleza."),
            ]),

            // ── 37714 - Inglés (Transversal) ─────────────────────────────────────
            ("37714", [
                ("593117", "01  COMPRENDER INFORMACIÓN SOBRE SITUACIONES COTIDIANAS Y LABORALES ACTUALES Y FUTURAS A TRAVÉS DE INTERACCIONES SOCIALES DE FORMA ORAL Y ESCRITA."),
                ("593115", "02  INTERCAMBIAR OPINIONES SOBRE SITUACIONES COTIDIANAS Y LABORALES ACTUALES, PASADAS Y FUTURAS EN CONTEXTOS SOCIALES ORALES Y ESCRITOS."),
                ("593118", "03  DISCUTIR SOBRE POSIBLES SOLUCIONES A PROBLEMAS DENTRO DE UN RANGO VARIADO DE CONTEXTOS SOCIALES Y LABORALES."),
                ("593114", "04  IMPLEMENTAR ACCIONES DE MEJORA RELACIONADAS CON EL USO DE EXPRESIONES, ESTRUCTURAS Y DESEMPEÑO SEGÚN LOS RESULTADOS DE APRENDIZAJE FORMULADOS PARA EL PROGRAMA."),
                ("593116", "05  PRESENTAR UN PROCESO PARA LA REALIZACIÓN DE UNA ACTIVIDAD EN SU QUEHACER LABORAL DE ACUERDO CON LOS PROCEDIMIENTOS ESTABLECIDOS DESDE SU PROGRAMA DE FORMACIÓN."),
                ("593113", "06  EXPLICAR LAS FUNCIONES DE SU OCUPACIÓN LABORAL USANDO EXPRESIONES DE ACUERDO AL NIVEL REQUERIDO POR EL PROGRAMA DE FORMACIÓN."),
            ]),

            // ── 590803 - Etapa Práctica ───────────────────────────────────────────
            // En SOFIA Plus esta competencia se exporta con código "2" y nombre
            // "RESULTADOS DE APRENDIZAJE ETAPA PRACTICA"; el código del resultado
            // es "590803". El seeder la referencia por el código de Competencia en BD.
            ("590803", [
                ("590803", "APLICAR EN LA RESOLUCIÓN DE PROBLEMAS REALES DEL SECTOR PRODUCTIVO, LOS CONOCIMIENTOS, HABILIDADES Y DESTREZAS PERTINENTES A LAS COMPETENCIAS DEL PROGRAMA DE FORMACIÓN ASUMIENDO ESTRATEGIAS Y METODOLOGÍAS DE AUTOGESTIÓN"),
            ]),
        };

        foreach (var (codComp, resultados) in catalogo)
        {
            if (!comps.TryGetValue(codComp, out int idComp)) continue;

            // Crear el PlanConcertado base (sin documento — se asocia en la importación real)
            var plan = new PlanConcertado
            {
                IdCompetencia = idComp,
                IdInstructor  = null,
                IdDocumento   = null,
                Estado        = "Publicado",
                FechaCreacion = DateTime.Now,
                Activo        = true
            };
            context.PlanConcertado.Add(plan);
            await context.SaveChangesAsync();

            // Crear los ResultadosAprendizaje del plan
            foreach (var (cod, desc) in resultados)
            {
                string nombre = $"{cod} - {desc}";
                if (nombre.Length > 200) nombre = nombre[..200];

                context.ResultadoAprendizaje.Add(new ResultadoAprendizaje
                {
                    IdPlan = plan.IdPlan,
                    Codigo = cod,
                    Nombre = nombre
                });
            }
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedInstructoresAsync(AdsoDbContext context)
    {
        if (await context.InstructorPerfil.AnyAsync()) return;

        var tipoCC = await context.TipoDocumento.FirstAsync(t => t.Nombre == "CC");
        var rolInstructor = await context.Rol.FirstAsync(r => r.Nombre == "Instructor");

        var instructores = new[]
        {
        new { Doc = "12345678", Nombres = "Carlos", Apellidos = "Perez Gomez",    Correo = "cperez@sena.edu.co",    Contrato = "CTR-001-2024", Especialidad = "Desarrollo de Software",  Vinculacion = "Contratista" },
        new { Doc = "23456789", Nombres = "Maria",  Apellidos = "Lopez Torres",   Correo = "mlopez@sena.edu.co",    Contrato = "CTR-002-2024", Especialidad = "Bases de Datos",          Vinculacion = "Planta"      },
        new { Doc = "34567890", Nombres = "Jorge",  Apellidos = "Ramirez Castro",  Correo = "jramirez@sena.edu.co",  Contrato = "CTR-003-2024", Especialidad = "Redes y Seguridad",       Vinculacion = "Contratista" },
        new { Doc = "45678901", Nombres = "Ana",    Apellidos = "Martinez Diaz",   Correo = "amartinez@sena.edu.co", Contrato = "CTR-004-2024", Especialidad = "Ingeniería de Software",  Vinculacion = "Planta"      },
    };

        foreach (var i in instructores)
        {
            var persona = new Persona
            {
                IdTipoDocumento = tipoCC.IdTipoDocumento,
                NumeroDocumento = i.Doc,
                Nombres = i.Nombres,
                Apellidos = i.Apellidos,
                FechaCreacion = DateTime.Now
            };
            context.Persona.Add(persona);
            await context.SaveChangesAsync();

            var usuario = new Usuario
            {
                IdPersona = persona.IdPersona,
                IdRol = rolInstructor.IdRol,
                Correo = i.Correo,
                PasswordHash = [0x00],
                PasswordSalt = [0x00],
                Activo = true,
                PrimerLogin = true,
                IntentosFallidos = 0,
                FechaCreacion = DateTime.Now
            };
            context.Usuario.Add(usuario);
            await context.SaveChangesAsync();

            var perfil = new InstructorPerfil
            {
                IdUsuario = usuario.IdUsuario,
                NumeroContrato = i.Contrato,
                Especialidad = i.Especialidad,
                TipoVinculacion = i.Vinculacion,
                FechaVinculacion = DateOnly.FromDateTime(DateTime.Now),
                Activo = true
            };
            context.InstructorPerfil.Add(perfil);
            await context.SaveChangesAsync();
        }
    }

    // ─────────────────────────────────────────────
    // 8. PERFIL DE INSTRUCTOR PARA EL ADMINISTRADOR
    // ─────────────────────────────────────────────
    // Permite que el administrador sea asignado a competencias y reciba
    // el indicador visual "Asignada" en Fichas.razor cuando tiene FCRs activos.
    // Se registra DESPUÉS de SeedInstructoresAsync para no interferir con
    // su guard "if (AnyAsync()) return".
    private static async Task SeedAdminInstructorPerfilAsync(AdsoDbContext context)
    {
        var adminUsuario = await context.Usuario
            .FirstOrDefaultAsync(u => u.Correo == "ypushaina@sena.edu.co");

        if (adminUsuario is null) return;

        bool yaExiste = await context.InstructorPerfil
            .AnyAsync(ip => ip.IdUsuario == adminUsuario.IdUsuario);

        if (yaExiste) return;

        context.InstructorPerfil.Add(new InstructorPerfil
        {
            IdUsuario        = adminUsuario.IdUsuario,
            NumeroContrato   = "ADMIN-INST-001",
            Especialidad     = "Análisis y Desarrollo de Software",
            TipoVinculacion  = "Planta",
            FechaVinculacion = DateOnly.FromDateTime(DateTime.Now),
            Activo           = true
        });
        await context.SaveChangesAsync();
    }

    private static async Task SeedFichaCompetenciaAsync(AdsoDbContext context)
    {
        if (await context.FichaCompetencia.AnyAsync()) return;

        var fichas = await context.Ficha
            .Where(f => new[] { "2998177", "2930468", "3369736" }.Contains(f.NumeroFicha))
            .ToListAsync();

        var competencias = await context.Competencia.ToListAsync();

        foreach (var ficha in fichas)
        {
            foreach (var comp in competencias)
            {
                bool existe = await context.FichaCompetencia.AnyAsync(
                    x => x.IdFicha == ficha.IdFicha && x.IdCompetencia == comp.IdCompetencia);

                if (existe) continue;

                context.FichaCompetencia.Add(new FichaCompetencia
                {
                    IdFicha = ficha.IdFicha,
                    IdCompetencia = comp.IdCompetencia,
                    FechaInicio = ficha.FechaInicio,
                    FechaFin = ficha.FechaFin,
                    Estado = "Pendiente",
                    HorasEjecutadas = 0
                });
            }
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedAsistenciasAsync(AdsoDbContext context)
    {
        if (await context.Asistencia.AnyAsync()) return;

        var sesiones = await context.Sesion.ToListAsync();
        var aprendicesPorFicha = await context.AprendizPerfil
            .Include(a => a.Fichas)
            .ToListAsync();

        if (!sesiones.Any() || !aprendicesPorFicha.Any()) return;

        var asistencias = new List<Asistencia>();
        var rng = new Random(42);

        foreach (var sesion in sesiones)
        {
            var fc = await context.FichaCompetencia.FindAsync(sesion.IdFichaCompetencia);
            if (fc == null) continue;

            var aprendices = aprendicesPorFicha
                .Where(a => a.Fichas.Any(f => f.IdFicha == fc.IdFicha))
                .ToList();

            foreach (var aprendiz in aprendices)
            {
                bool ausente = rng.Next(0, 10) < 2;
                bool justificado = ausente && rng.Next(0, 2) == 1;
                asistencias.Add(new Asistencia
                {
                    IdSesion = sesion.IdSesion,
                    IdAprendiz = aprendiz.IdAprendiz,
                    Estado = ausente ? (justificado ? "Justificado" : "Ausente") : "Presente",
                    MotivoInasistencia = ausente ? "Calamidad doméstica" : null
                });
            }
        }

        context.Asistencia.AddRange(asistencias);
        await context.SaveChangesAsync();
    }

}