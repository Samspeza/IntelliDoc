using IntelliDoc.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntelliDoc.Infrastructure.Persistence.Configurations;

/// <summary>
/// Espelha a tabela "Documentos" de 001_create_schema.sql, incluindo os
/// índices compostos (EmpresaId, Status) e (EmpresaId, PrioridadeRevisao,
/// Status) que suportam as listagens mais frequentes do sistema (UC16, UC19).
/// </summary>
public sealed class DocumentoConfiguration : IEntityTypeConfiguration<Documento>
{
    public void Configure(EntityTypeBuilder<Documento> builder)
    {
        builder.ToTable("Documentos");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.NomeArquivoOriginal).HasMaxLength(300).IsRequired();
        builder.Property(d => d.CaminhoArmazenamento).HasMaxLength(500).IsRequired();
        builder.Property(d => d.TipoArquivo).HasMaxLength(10).IsRequired();
        builder.Property(d => d.TipoDocumento).HasConversion<short>();
        builder.Property(d => d.Status).HasConversion<short>();
        builder.Property(d => d.MotivoRejeicao).HasMaxLength(500);
        builder.Property(d => d.PrioridadeRevisao).HasDefaultValue(false);
        builder.Property(d => d.TentativasProcessamento).HasDefaultValue(0);
        builder.Property(d => d.Arquivado).HasDefaultValue(false);

        // Value Object ConfidenceScore mapeado como coluna decimal nullable.
        builder.OwnsOne(d => d.ScoreMedio, score =>
        {
            score.Property(s => s.Valor)
                .HasColumnName("ConfidenceScoreMedio")
                .HasColumnType("decimal(5,2)");
        });

        builder.HasOne<Empresa>()
            .WithMany()
            .HasForeignKey(d => d.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(d => d.EnviadoPorUsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(d => d.Campos)
            .WithOne()
            .HasForeignKey(c => c.DocumentoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.Historico)
            .WithOne()
            .HasForeignKey(h => h.DocumentoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(d => d.Campos).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(d => d.Historico).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(d => new { d.EmpresaId, d.Status })
            .HasDatabaseName("IX_Documentos_Empresa_Status");

        builder.HasIndex(d => new { d.EmpresaId, d.PrioridadeRevisao, d.Status })
            .HasDatabaseName("IX_Documentos_Empresa_Prioridade_Status");
    }
}