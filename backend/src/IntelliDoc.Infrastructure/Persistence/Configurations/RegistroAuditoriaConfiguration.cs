using IntelliDoc.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntelliDoc.Infrastructure.Persistence.Configurations;

/// <summary>
/// Espelha a tabela "RegistrosAuditoria". RN33 (imutabilidade) é garantida
/// pela ausência de qualquer Command de Update/Delete na Application - esta
/// configuration não precisa (nem deve) expor nenhum mecanismo de alteração.
/// </summary>
public sealed class RegistroAuditoriaConfiguration : IEntityTypeConfiguration<RegistroAuditoria>
{
    public void Configure(EntityTypeBuilder<RegistroAuditoria> builder)
    {
        builder.ToTable("RegistrosAuditoria");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Acao).HasConversion<short>();
        builder.Property(a => a.EntidadeAfetada).HasMaxLength(100).IsRequired();
        builder.Property(a => a.DadosAntesJson).HasColumnName("DadosAntes").HasColumnType("jsonb");
        builder.Property(a => a.DadosDepoisJson).HasColumnName("DadosDepois").HasColumnType("jsonb");
        builder.Property(a => a.EnderecoIp).HasMaxLength(45).IsRequired();

        builder.HasIndex(a => new { a.EmpresaId, a.CriadoEm })
            .HasDatabaseName("IX_Auditoria_Empresa_CriadoEm")
            .IsDescending(false, true);
    }
}