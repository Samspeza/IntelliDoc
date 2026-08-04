using IntelliDoc.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntelliDoc.Infrastructure.Persistence.Configurations;

/// <summary>Espelha a tabela "CamposExtraidos" de 001_create_schema.sql.</summary>
public sealed class CampoExtraidoConfiguration : IEntityTypeConfiguration<CampoExtraido>
{
    public void Configure(EntityTypeBuilder<CampoExtraido> builder)
    {
        builder.ToTable("CamposExtraidos");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.NomeCampo).HasMaxLength(100).IsRequired();
        builder.Property(c => c.ValorExtraidoIa).HasMaxLength(500);
        builder.Property(c => c.ValorFinal).HasMaxLength(500);
        builder.Property(c => c.CorrigidoManualmente).HasDefaultValue(false);

        builder.OwnsOne(c => c.Confidence, confidence =>
        {
            confidence.Property(v => v.Valor)
                .HasColumnName("ConfidenceScore")
                .HasColumnType("decimal(5,2)")
                .IsRequired();
        });

        builder.HasIndex(c => c.DocumentoId).HasDatabaseName("IX_CamposExtraidos_DocumentoId");
    }
}