using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdsoLabs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarFaseFormativaYDescripcionCompetencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "fase_formativa",
                schema: "adso",
                table: "Competencia",
                type: "nvarchar(12)",
                maxLength: 12,
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Competencia_fase_formativa",
                schema: "adso",
                table: "Competencia",
                sql: "fase_formativa IS NULL OR fase_formativa IN ('Induccion','Analisis','Planeacion','Ejecucion','Evaluacion','Transversal')");

            // Backfill de fase_formativa y descripcion para las 20 competencias ya sembradas
            // (bases de datos existentes) — valores idénticos a los ahora asignados en
            // AdsoDbContextSeeder.SeedCompetenciasAsync (usado solo en bases nuevas).
            migrationBuilder.Sql(@"
UPDATE adso.Competencia SET fase_formativa = 'Induccion', descripcion = '01 Identificar la dinámica organizacional del SENA y el rol de la formación profesional integral de acuerdo con su proyecto de vida y el desarrollo profesional.' WHERE codigo = '36182';
UPDATE adso.Competencia SET fase_formativa = 'Evaluacion', descripcion = '01 Integrar elementos de la cultura emprendedora teniendo en cuenta el perfil personal y el contexto de desarrollo social. 02 Caracterizar la idea de negocio teniendo en cuenta las oportunidades y necesidades del sector productivo y social. 03 Estructurar el plan de negocio de acuerdo con las características empresariales y tendencias de mercado. 04 Valorar la propuesta de negocio conforme con su estructura y necesidades del sector productivo y social.' WHERE codigo = '38561';
UPDATE adso.Competencia SET fase_formativa = 'Planeacion', descripcion = '01 Elaborar los artefactos de diseño del software siguiendo las prácticas de la metodología seleccionada. 02 Estructurar el modelo de datos del software de acuerdo con las especificaciones del análisis. 03 Determinar las características técnicas de la interfaz gráfica del software adoptando estándares. 04 Verificar los entregables de la fase de diseño del software de acuerdo con lo establecido en el informe de análisis.' WHERE codigo = '38362';
UPDATE adso.Competencia SET fase_formativa = 'Ejecucion', descripcion = '01 Planear actividades de construcción del software de acuerdo con el diseño establecido. 02 Construir la base de datos para el software a partir del modelo de datos. 03 Crear componentes front-end del software de acuerdo con el diseño. 04 Codificar el software de acuerdo con el diseño establecido. 05 Realizar pruebas al software para verificar su funcionalidad.' WHERE codigo = '38368';
UPDATE adso.Competencia SET fase_formativa = 'Planeacion', descripcion = '01 Analizar las estrategias para la prevención y control de los impactos ambientales y de los accidentes y enfermedades laborales (ATEL) de acuerdo con las políticas organizacionales. 02 Implementar estrategias para el control de los impactos ambientales y de los accidentes y enfermedades de acuerdo con los planes y programas establecidos por la organización. 03 Realizar seguimiento y acompañamiento al desarrollo de los planes y programas ambientales y SST, según el área de desempeño. 04 Proponer acciones de mejora para el manejo ambiental y el control de la SST, de acuerdo con estrategias de trabajo colaborativo, cooperativo y coordinado en el contexto productivo.' WHERE codigo = '37799';
UPDATE adso.Competencia SET fase_formativa = 'Analisis', descripcion = '01 Alistar herramientas de tecnologías de la información y la comunicación (TIC), de acuerdo con las necesidades de procesamiento de información y comunicación. 02 Aplicar funcionalidades de herramientas y servicios TIC, de acuerdo con manuales de uso, procedimientos establecidos y buenas prácticas. 03 Evaluar los resultados, de acuerdo con los requerimientos. 04 Optimizar los resultados, de acuerdo con la verificación.' WHERE codigo = '37371';
UPDATE adso.Competencia SET fase_formativa = 'Analisis', descripcion = '01 Caracterizar los procesos de la organización de acuerdo con el software a construir. 02 Recolectar información del software a construir de acuerdo con las necesidades del cliente. 03 Establecer los requisitos del software de acuerdo con la información recolectada. 04 Validar el informe de requisitos de acuerdo con las necesidades del cliente.' WHERE codigo = '38392';
UPDATE adso.Competencia SET fase_formativa = 'Planeacion', descripcion = '01 Promover mi dignidad y la del otro a partir de los principios y valores éticos como aporte en la instauración de una cultura de paz. 02 Establecer relaciones de crecimiento personal y comunitario a partir del bien común como aporte para el desarrollo social. 03 Promover el uso racional de los recursos naturales a partir de criterios de sostenibilidad y sustentabilidad ética y normativa vigente. 04 Contribuir con el fortalecimiento de la cultura de paz a partir de la dignidad humana y las estrategias para la transformación de conflictos.' WHERE codigo = '36180';
UPDATE adso.Competencia SET fase_formativa = 'Planeacion', descripcion = '01 Analizar el contexto productivo según sus características y necesidades. 02 Estructurar el proyecto de acuerdo a criterios de la investigación. 03 Argumentar aspectos teóricos del proyecto según referentes nacionales e internacionales. 04 Proponer soluciones a las necesidades del contexto según resultados de la investigación.' WHERE codigo = '38199';
UPDATE adso.Competencia SET fase_formativa = 'Planeacion', descripcion = '01 Identificar modelos matemáticos de acuerdo con los requerimientos del problema planteado en contextos sociales y productivos. 02 Plantear problemas matemáticos a partir de situaciones generadas en el contexto social y productivo. 03 Resolver problemas matemáticos a partir de situaciones generadas en el contexto social y productivo. 04 Proponer acciones de mejora frente a los resultados de los procedimientos matemáticos de acuerdo con el problema planteado.' WHERE codigo = '38560';
UPDATE adso.Competencia SET fase_formativa = 'Planeacion', descripcion = '01 Identificar los principios y leyes de la física en la solución de problemas de acuerdo al contexto productivo. 02 Solucionar problemas asociados con el sector productivo con base en los principios y leyes de la física. 03 Verificar las transformaciones físicas de la materia utilizando herramientas. 04 Proponer acciones de mejora en los procesos productivos de acuerdo con los principios y leyes de la física.' WHERE codigo = '37801';
UPDATE adso.Competencia SET fase_formativa = 'Planeacion', descripcion = '01 Desarrollar habilidades psicomotrices en el contexto productivo y social. 02 Practicar hábitos saludables mediante la aplicación de fundamentos de nutrición. 03 Ejecutar actividades de acondicionamiento físico orientadas hacia el mejoramiento de la condición física en los contextos productivo y social. 04 Implementar un plan de ergonomía y pausas activas según las características de la función productiva.' WHERE codigo = '37800';
UPDATE adso.Competencia SET fase_formativa = 'Analisis', descripcion = '01 Analizar los componentes de la comunicación según sus características, intencionalidad y contexto. 02 Argumentar en forma oral y escrita atendiendo las exigencias y particularidades de las diversas situaciones comunicativas mediante los distintos sistemas de representación. 03 Relacionar los procesos comunicativos teniendo en cuenta criterios de lógica y racionalidad. 04 Establecer procesos de enriquecimiento lexical y acciones de mejoramiento en el desarrollo de procesos comunicativos según requerimientos del contexto.' WHERE codigo = '37802';
UPDATE adso.Competencia SET fase_formativa = 'Evaluacion', descripcion = '01 Reconocer el trabajo como factor de movilidad social y transformación vital con referencia a la fenomenología y a los derechos fundamentales en el trabajo. 02 Valorar la importancia de la ciudadanía laboral con base en el estudio de los derechos humanos y fundamentales en el trabajo. 03 Practicar los derechos fundamentales en el trabajo de acuerdo con la Constitución Política y los Convenios Internacionales. 04 Participar en acciones solidarias teniendo en cuenta el ejercicio de los derechos humanos, de los pueblos y de la naturaleza.' WHERE codigo = '38558';
UPDATE adso.Competencia SET fase_formativa = 'Analisis', descripcion = '01 Planear actividades de análisis de acuerdo con la metodología seleccionada. 02 Modelar las funciones del software de acuerdo con el informe de requisitos. 03 Desarrollar procesos lógicos a través de la implementación de algoritmos. 04 Verificar los modelos realizados en la fase de análisis de acuerdo con lo establecido en el informe de requisitos.' WHERE codigo = '38376';
UPDATE adso.Competencia SET fase_formativa = 'Transversal', descripcion = '01 Comprender información sobre situaciones cotidianas y laborales actuales y futuras a través de interacciones sociales de forma oral y escrita. 02 Intercambiar opiniones sobre situaciones cotidianas y laborales actuales, pasadas y futuras en contextos sociales orales y escritos. 03 Discutir sobre posibles soluciones a problemas dentro de un rango variado de contextos sociales y laborales. 04 Implementar acciones de mejora relacionadas con el uso de expresiones, estructuras y desempeño según los resultados de aprendizaje formulados para el programa. 05 Presentar un proceso para la realización de una actividad en su quehacer laboral de acuerdo con los procedimientos establecidos desde su programa de formación. 06 Explicar las funciones de su ocupación laboral usando expresiones de acuerdo al nivel requerido por el programa de formación.' WHERE codigo = '37714';
UPDATE adso.Competencia SET fase_formativa = 'Evaluacion', descripcion = '01 Planear actividades de implantación del software de acuerdo con las condiciones del sistema. 02 Desplegar el software de acuerdo con la arquitectura y las políticas establecidas. 03 Documentar el proceso de implantación de software siguiendo estándares de calidad. 04 Implantar el software de acuerdo con los niveles de servicio establecidos con el cliente.' WHERE codigo = '38356';
UPDATE adso.Competencia SET fase_formativa = 'Evaluacion', descripcion = '01 Incorporar actividades de aseguramiento de la calidad del software de acuerdo con estándares de la industria. 02 Verificar la calidad del software de acuerdo con las prácticas asociadas en los procesos de desarrollo. 03 Realizar actividades de mejora de la calidad del software a partir de los resultados de la verificación.' WHERE codigo = '38369';
UPDATE adso.Competencia SET fase_formativa = 'Analisis', descripcion = '01 Definir especificaciones técnicas del software de acuerdo con las características del software a construir. 02 Elaborar propuesta técnica del software de acuerdo con las especificaciones técnicas definidas. 03 Validar las condiciones de la propuesta técnica del software de acuerdo con los intereses de las partes.' WHERE codigo = '38367';
UPDATE adso.Competencia SET fase_formativa = 'Evaluacion', descripcion = 'Aplicar en la resolución de problemas reales del sector productivo, los conocimientos, habilidades y destrezas pertinentes a las competencias del programa de formación asumiendo estrategias y metodologías de autogestión.' WHERE codigo = '590803';
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Competencia_fase_formativa",
                schema: "adso",
                table: "Competencia");

            migrationBuilder.DropColumn(
                name: "fase_formativa",
                schema: "adso",
                table: "Competencia");
        }
    }
}
