using AutoMapper;
using IntelliDoc.Domain.Entities;
using IntelliDoc.Application.Documentos.Dtos;

namespace IntelliDoc.Application.Common.Mappings;

/// <summary>
/// Perfil único de mapeamento Entity → DTO. Registrado automaticamente por
/// AddAutoMapper(assembly) em DependencyInjection (Etapa 9.3).
///
/// Nota sobre Value Objects: ScoreMedio e Confidence são
/// ConfidenceScore (VO), por isso são mapeados explicitamente para o
/// decimal interno - o AutoMapper não infere isso sozinho.
/// Enums viram string (nome) para desacoplar o frontend dos valores
/// numéricos persistidos no banco.
/// </summary>
public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CampoExtraido, CampoExtraidoDto>()
            .ForCtorParam(nameof(CampoExtraidoDto.ConfidenceScore),
                opt => opt.MapFrom(src => src.Confidence.Valor));

        CreateMap<HistoricoStatusDocumento, HistoricoStatusDto>()
            .ForCtorParam(nameof(HistoricoStatusDto.StatusAnterior),
                opt => opt.MapFrom(src => src.StatusAnterior.ToString()))
            .ForCtorParam(nameof(HistoricoStatusDto.StatusNovo),
                opt => opt.MapFrom(src => src.StatusNovo.ToString()))
            .ForCtorParam(nameof(HistoricoStatusDto.NomeUsuario),
                opt => opt.MapFrom(src => (string?)null))
            .ForCtorParam(nameof(HistoricoStatusDto.OcorridoEm),
                opt => opt.MapFrom(src => src.CriadoEm));

        CreateMap<Documento, DocumentoResumoDto>()
            .ForCtorParam(nameof(DocumentoResumoDto.TipoDocumento),
                opt => opt.MapFrom(src => src.TipoDocumento.ToString()))
            .ForCtorParam(nameof(DocumentoResumoDto.Status),
                opt => opt.MapFrom(src => src.Status.ToString()))
            .ForCtorParam(nameof(DocumentoResumoDto.ConfidenceScoreMedio),
                opt => opt.MapFrom(src => src.ScoreMedio != null ? src.ScoreMedio.Valor : (decimal?)null));

        CreateMap<Documento, DocumentoDetalheDto>()
            .ForCtorParam(nameof(DocumentoDetalheDto.TipoDocumento),
                opt => opt.MapFrom(src => src.TipoDocumento.ToString()))
            .ForCtorParam(nameof(DocumentoDetalheDto.Status),
                opt => opt.MapFrom(src => src.Status.ToString()))
            .ForCtorParam(nameof(DocumentoDetalheDto.ConfidenceScoreMedio),
                opt => opt.MapFrom(src => src.ScoreMedio != null ? src.ScoreMedio.Valor : (decimal?)null))
            .ForCtorParam(nameof(DocumentoDetalheDto.Campos),
                opt => opt.MapFrom(src => src.Campos))
            .ForCtorParam(nameof(DocumentoDetalheDto.Historico),
                opt => opt.MapFrom(src => src.Historico));
    }
}