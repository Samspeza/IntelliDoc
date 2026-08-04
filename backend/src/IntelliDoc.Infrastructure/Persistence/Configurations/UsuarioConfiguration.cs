using IntelliDoc.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntelliDoc.Infrastructure.Persistence.Configurations;

/// <summary>Espelha a tabela "Usuarios" de 001_create_schema.sql.</summary>
public sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Nome).HasMaxLength(150).IsRequired();
        builder.Property(u => u.SenhaHash).HasMaxLength(500).IsRequired();
        builder.Property(u => u.Ativo).HasDefaultValue(true);

        // Value Object Email mapeado como coluna simples "Email" (varchar) -
        // OwnsOne com WithOwner + Property, evitando expor Email.Valor como
        // objeto complexo separado na tabela.
        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Valor)
                .HasColumnName("Email")
                .HasMaxLength(256)
                .IsRequired();

            email.HasIndex(e => e.Valor).IsUnique();
        });

        builder.HasOne<Empresa>()
            .WithMany()
            .HasForeignKey(u => u.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.Papeis)
            .WithOne()
            .HasForeignKey(p => p.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.RefreshTokens)
            .WithOne()
            .HasForeignKey(t => t.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        // Coleções privadas (_papeis, _refreshTokens) expostas como
        // IReadOnlyCollection: instrui o EF a usar o campo de apoio
        // diretamente (não a propriedade pública somente-leitura).
        builder.Navigation(u => u.Papeis).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(u => u.RefreshTokens).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}