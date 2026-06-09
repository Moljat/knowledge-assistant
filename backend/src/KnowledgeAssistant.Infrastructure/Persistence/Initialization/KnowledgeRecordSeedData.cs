using KnowledgeAssistant.Domain.Entities;

namespace KnowledgeAssistant.Infrastructure.Persistence.Initialization;

public static class KnowledgeRecordSeedData
{
    public const string Source = "Knowledge Assistant Demo Seed v1";

    public static IReadOnlyList<KnowledgeRecordSeedDefinition> Records { get; } =
    [
        new(
            "Política de atención a clientes",
            """
            Las solicitudes de clientes deben registrarse con fecha, responsable y prioridad.
            Los casos críticos se escalan al líder de servicio y reciben seguimiento hasta su
            resolución documentada.
            """,
            KnowledgeRecordType.Document,
            Activate: true),
        new(
            "Notas de reunión comercial",
            """
            El equipo comercial identificó oportunidades para mejorar el seguimiento de
            cotizaciones, estandarizar motivos de pérdida y compartir aprendizajes entre
            sucursales.
            """,
            KnowledgeRecordType.Note,
            Activate: false),
        new(
            "Registro de mejora operativa",
            """
            Se propone revisar semanalmente los tiempos de preparación de pedidos, documentar
            incidencias recurrentes y priorizar acciones que reduzcan retrabajos.
            """,
            KnowledgeRecordType.BusinessRecord,
            Activate: true)
    ];
}
