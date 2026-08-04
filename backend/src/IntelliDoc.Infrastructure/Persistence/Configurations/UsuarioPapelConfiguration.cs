using IntelliDoc.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntelliDoc.Infrastructure.Persistence.Configurations;

/// <summary>Espelha a tabela "UsuarioPapeis" de 001_create_schema.sql.</summary>
public sealed class UsuarioPapelConfiguration : IEntityTypeConfiguration<UsuarioPapel>
{
    public void Configure(EntityTypeBuilder<UsuarioPapel> builder)
    {
        builder.ToTable("UsuarioPapeis");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Papel).HasConversion<short>();

        builder.HasIndex(p => new { p.UsuarioId, p.Papel }).IsUnique();
    }
}