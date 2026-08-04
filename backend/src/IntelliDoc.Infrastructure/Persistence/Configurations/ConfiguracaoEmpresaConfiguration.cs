using IntelliDoc.Domain.Entities;
using IntelliDoc.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntelliDoc.Infrastructure.Persistence.Configurations;

/// <summary>Espelha a tabela "ConfiguracoesEmpresa" de 001_create_schema.sql.</summary>
public sealed class ConfiguracaoEmpresaConfiguration : IEntityTypeConfiguration<ConfiguracaoEmpresa>
{
    public void Configure(EntityTypeBuilder<ConfiguracaoEmpresa> builder)
    {
        builder.ToTable("ConfiguracoesEmpresa");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.LimiarConfiancaIa).HasColumnType("decimal(5,2)").HasDefaultValue(70.00m);
        builder.Property(c => c.SegregacaoRevisorAtiva).HasDefaultValue(false);

        // TiposDocumentoAceitos: List<TipoDocumento> mapeado para jsonb,
        // conforme decisão de modelagem (docs/06-modelagem-banco.md).
        builder.Property(c => c.TiposDocumentoAceitos)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<TipoDocumento>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<TipoDocumento>())
            .HasColumnType("jsonb");

        builder.HasIndex(c => c.EmpresaId).IsUnique();
    }
}