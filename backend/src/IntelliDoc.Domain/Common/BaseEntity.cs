using IntelliDoc.Domain.Events;

namespace IntelliDoc.Domain.Common;

/// <summary>
/// Classe base para todas as entidades do domínio.
/// Fornece identidade (Guid) e as colunas de auditoria padrão descritas
/// em docs/06-modelagem-banco.md (§1 - Convenções gerais).
/// Os campos de auditoria são preenchidos automaticamente pelo
/// AuditableEntitySaveChangesInterceptor (Infrastructure), nunca
/// diretamente pelo código de negócio.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;

    public string? CriadoPor { get; private set; }

    public DateTime? AtualizadoEm { get; private set; }

    public string? AtualizadoPor { get; private set; }

    /// <summary>
    /// Chamado exclusivamente pelo interceptor de persistência (Infrastructure)
    /// ao inserir a entidade. Não deve ser chamado por código de domínio ou de
    /// aplicação diretamente.
    /// </summary>
    public void DefinirCriacao(string? usuario)
    {
        CriadoEm = DateTime.UtcNow;
        CriadoPor = usuario;
    }

    /// <summary>
    /// Chamado exclusivamente pelo interceptor de persistência (Infrastructure)
    /// ao atualizar a entidade.
    /// </summary>
    public void DefinirAtualizacao(string? usuario)
    {
        AtualizadoEm = DateTime.UtcNow;
        AtualizadoPor = usuario;
    }

    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Eventos de domínio pendentes de publicação (ex.: DocumentoAprovadoEvent).
    /// Despachados pelo AuditableEntitySaveChangesInterceptor após o SaveChanges
    /// ter sucesso, garantindo que o evento só é publicado se a transação
    /// realmente foi persistida.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AdicionarEvento(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void LimparEventos() => _domainEvents.Clear();

    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity other)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return GetType() == other.GetType() && Id == other.Id;
    }

    public override int GetHashCode() => (GetType(), Id).GetHashCode();
}