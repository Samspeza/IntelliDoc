using IntelliDoc.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntelliDoc.Infrastructure.Persistence.Configurations;

/// <summary>
/// Espelha a tabela "HistoricoStatusDocumento". Nenhum método de UPDATE é
/// exposto pela entidade de domínio (RN23/RN33 - imutabilidade), então
/// esta configuration não precisa de nenhuma restrição especial adicional
/// além do mapeamento padrão.
/// </summary>
public sealed class HistoricoStatusDocumentoConfiguration : IEntityTypeConfiguration<HistoricoStatusDocumento>
{
    public void Configure(EntityTypeBuilder<HistoricoStatusDocumento> builder)
    {
        builder.ToTable("HistoricoStatusDocumento");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.StatusAnterior).HasConversion<short>();
        builder.Property(h => h.StatusNovo).HasConversion<short>();
        builder.Property(h => h.Motivo).HasMaxLength(500);

        builder.HasIndex(h => h.DocumentoId).HasDatabaseName("IX_HistoricoStatus_DocumentoId");
    }
}