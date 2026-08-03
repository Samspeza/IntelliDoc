namespace IntelliDoc.Application.Common.Interfaces;

/// <summary>
/// Abstrai o relógio do sistema. Usar esta interface em vez de
/// DateTime.UtcNow diretamente em Handlers permite testes unitários
/// determinísticos (ex.: simular "amanhã" para testar expiração de token).
/// </summary>
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}