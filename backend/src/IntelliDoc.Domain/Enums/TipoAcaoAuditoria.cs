namespace IntelliDoc.Domain.Enums;

/// <summary>
/// Ações sensíveis registradas na trilha de auditoria, conforme RN32
/// (docs/02-regras-de-negocio.md). Lista extensível: novas ações sensíveis
/// devem ser adicionadas aqui antes de serem auditadas em qualquer Command
/// Handler.
/// </summary>
public enum TipoAcaoAuditoria
{
    Login = 0,
    CriarUsuario = 1,
    EditarUsuario = 2,
    MudarPapel = 3,
    AprovarDocumento = 4,
    RejeitarDocumento = 5,
    AlterarConfiguracaoEmpresa = 6,
    CriarEmpresa = 7,
    DesativarEmpresa = 8,
    AtivarEmpresa = 9,
    ArquivarDocumento = 10,
    ReenviarDocumento = 11
}