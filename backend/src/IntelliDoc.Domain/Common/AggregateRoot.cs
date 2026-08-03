namespace IntelliDoc.Domain.Common;

/// <summary>
/// Marca uma entidade como raiz de agregado (Aggregate Root), conforme os
/// agregados identificados em docs/07-diagrama-entidades-dominio.md (§1).
/// Somente Aggregate Roots possuem repositório próprio na Application;
/// entidades filhas (ex.: CampoExtraido, HistoricoStatusDocumento) só são
/// alteradas através do agregado ao qual pertencem.
/// </summary>
public abstract class AggregateRoot : BaseEntity
{
}