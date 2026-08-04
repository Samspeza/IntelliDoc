using IntelliDoc.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntelliDoc.Infrastructure.Persistence.Configurations;

/// <summary>Espelha a tabela "Empresas" de database/scripts/001_create_schema.sql.</summary>
public sealed class EmpresaConfiguration : IEntityTypeConfiguration<Empresa>
{
    public void Configure(EntityTypeBuilder<Empresa> builder)
    {
        builder.ToTable("Empresas");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nome).HasMaxLength(200).IsRequired();
        builder.Property(e => e.CnpjOuIdentificador).HasMaxLength(20);
        builder.Property(e => e.Ativa).HasDefaultValue(true);

        builder.HasIndex(e => e.CnpjOuIdentificador).IsUnique();

        // Relação 1:1 com ConfiguracaoEmpresa - entidade filha do agregado,
        // sempre carregada junto (não é lazy: Include explícito nas Queries
        // que precisam, ou eager load configurado nos Handlers específicos).
        builder.HasOne(e => e.Configuracao)
            .WithOne()
            .HasForeignKey<ConfiguracaoEmpresa>(c => c.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}