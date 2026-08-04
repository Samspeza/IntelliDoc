namespace IntelliDoc.Infrastructure.Identity;

/// <summary>Mapeada da seção "Jwt" de appsettings.json (Api/Worker).</summary>
public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string ChaveSecreta { get; init; } = string.Empty;
    public string Emissor { get; init; } = "IntelliDoc";
    public string Audiencia { get; init; } = "IntelliDoc.Clientes";
    public int AccessTokenMinutos { get; init; } = 15; // RNF/Etapa 4 §6
    public int RefreshTokenDias { get; init; } = 7;     // RN10
}