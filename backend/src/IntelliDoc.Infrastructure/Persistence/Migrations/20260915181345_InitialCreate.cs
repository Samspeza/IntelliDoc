using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntelliDoc.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Empresas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CnpjOuIdentificador = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Ativa = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "text", nullable: true),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AtualizadoPor = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empresas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notificacoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioDestinoId = table.Column<Guid>(type: "uuid", nullable: true),
                    PapelDestino = table.Column<short>(type: "smallint", nullable: true),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Mensagem = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Lida = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DocumentoRelacionadoId = table.Column<Guid>(type: "uuid", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "text", nullable: true),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AtualizadoPor = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificacoes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RegistrosAuditoria",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: true),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: true),
                    Acao = table.Column<short>(type: "smallint", nullable: false),
                    EntidadeAfetada = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntidadeAfetadaId = table.Column<Guid>(type: "uuid", nullable: true),
                    DadosAntes = table.Column<string>(type: "jsonb", nullable: true),
                    DadosDepois = table.Column<string>(type: "jsonb", nullable: true),
                    EnderecoIp = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "text", nullable: true),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AtualizadoPor = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosAuditoria", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracoesEmpresa",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    LimiarConfiancaIa = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 70.00m),
                    TiposDocumentoAceitos = table.Column<string>(type: "jsonb", nullable: false),
                    SegregacaoRevisorAtiva = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "text", nullable: true),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AtualizadoPor = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracoesEmpresa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfiguracoesEmpresa_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: true),
                    Nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SenhaHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "text", nullable: true),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AtualizadoPor = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuarios_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Documentos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnviadoPorUsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    NomeArquivoOriginal = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    CaminhoArmazenamento = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TipoArquivo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    TamanhoBytes = table.Column<long>(type: "bigint", nullable: false),
                    TipoDocumento = table.Column<short>(type: "smallint", nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    PrioridadeRevisao = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ConfidenceScoreMedio = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    TentativasProcessamento = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    MotivoRejeicao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Arquivado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "text", nullable: true),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AtualizadoPor = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documentos_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documentos_Usuarios_EnviadoPorUsuarioId",
                        column: x => x.EnviadoPorUsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ExpiraEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevogadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "text", nullable: true),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AtualizadoPor = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioPapeis",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Papel = table.Column<short>(type: "smallint", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "text", nullable: true),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AtualizadoPor = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioPapeis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuarioPapeis_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CamposExtraidos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentoId = table.Column<Guid>(type: "uuid", nullable: false),
                    NomeCampo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ValorExtraidoIa = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ValorFinal = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ConfidenceScore = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    CorrigidoManualmente = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "text", nullable: true),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AtualizadoPor = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CamposExtraidos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CamposExtraidos_Documentos_DocumentoId",
                        column: x => x.DocumentoId,
                        principalTable: "Documentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HistoricoStatusDocumento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentoId = table.Column<Guid>(type: "uuid", nullable: false),
                    StatusAnterior = table.Column<short>(type: "smallint", nullable: false),
                    StatusNovo = table.Column<short>(type: "smallint", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: true),
                    Motivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "text", nullable: true),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AtualizadoPor = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoStatusDocumento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoricoStatusDocumento_Documentos_DocumentoId",
                        column: x => x.DocumentoId,
                        principalTable: "Documentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CamposExtraidos_DocumentoId",
                table: "CamposExtraidos",
                column: "DocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracoesEmpresa_EmpresaId",
                table: "ConfiguracoesEmpresa",
                column: "EmpresaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_Empresa_Prioridade_Status",
                table: "Documentos",
                columns: new[] { "EmpresaId", "PrioridadeRevisao", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_Empresa_Status",
                table: "Documentos",
                columns: new[] { "EmpresaId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_EnviadoPorUsuarioId",
                table: "Documentos",
                column: "EnviadoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Empresas_CnpjOuIdentificador",
                table: "Empresas",
                column: "CnpjOuIdentificador",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HistoricoStatus_DocumentoId",
                table: "HistoricoStatusDocumento",
                column: "DocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Notificacoes_Usuario_Lida",
                table: "Notificacoes",
                columns: new[] { "UsuarioDestinoId", "Lida" });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UsuarioId_RevogadoEm",
                table: "RefreshTokens",
                columns: new[] { "UsuarioId", "RevogadoEm" });

            migrationBuilder.CreateIndex(
                name: "IX_Auditoria_Empresa_CriadoEm",
                table: "RegistrosAuditoria",
                columns: new[] { "EmpresaId", "CriadoEm" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioPapeis_UsuarioId_Papel",
                table: "UsuarioPapeis",
                columns: new[] { "UsuarioId", "Papel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_EmpresaId",
                table: "Usuarios",
                column: "EmpresaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CamposExtraidos");

            migrationBuilder.DropTable(
                name: "ConfiguracoesEmpresa");

            migrationBuilder.DropTable(
                name: "HistoricoStatusDocumento");

            migrationBuilder.DropTable(
                name: "Notificacoes");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "RegistrosAuditoria");

            migrationBuilder.DropTable(
                name: "UsuarioPapeis");

            migrationBuilder.DropTable(
                name: "Documentos");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Empresas");
        }
    }
}
