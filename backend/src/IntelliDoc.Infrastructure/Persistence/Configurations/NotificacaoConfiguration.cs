using IntelliDoc.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntelliDoc.Infrastructure.Persistence.Configurations;

/// <summary>Espelha a tabela "Notificacoes" de 001_create_schema.sql.</summary>
public sealed class NotificacaoConfiguration : IEntityTypeConfiguration<Notificacao>
{
    public void Configure(EntityTypeBuilder<Notificacao> builder)
    {
        builder.ToTable("Notificacoes");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Titulo).HasMaxLength(200).IsRequired();
        builder.Property(n => n.Mensagem).HasMaxLength(1000).IsRequired();
        builder.Property(n => n.PapelDestino).HasConversion<short?>();
        builder.Property(n => n.Lida).HasDefaultValue(false);

        builder.HasIndex(n => new { n.UsuarioDestinoId, n.Lida })
            .HasDatabaseName("IX_Notificacoes_Usuario_Lida");
    }
}