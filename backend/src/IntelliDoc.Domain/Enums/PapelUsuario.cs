namespace IntelliDoc.Domain.Enums;

/// <summary>
/// Papéis de usuário (RBAC) definidos em RN05 (docs/02-regras-de-negocio.md).
/// Os valores numéricos correspondem exatamente aos usados no CHECK
/// constraint da coluna "Papel" em database/scripts/001_create_schema.sql -
/// não reordenar nem renumerar sem atualizar o script SQL e a migration.
/// </summary>
public enum PapelUsuario
{
    SuperAdmin = 0,
    AdminEmpresa = 1,
    Gestor = 2,
    Revisor = 3,
    Operador = 4
}