using System.Text.RegularExpressions;
using IntelliDoc.Application.Common.Interfaces;

namespace IntelliDoc.Infrastructure.Extraction;

/// <summary>
/// Aplica heurísticas (regex) sobre o texto OCR bruto para extrair campos
/// estruturados (CNPJ, datas, valores monetários) e classificar o tipo de
/// documento. Esta é a implementação PADRÃO/DEMONSTRATIVA descrita em
/// docs/04-arquitetura.md ("Escopo da IA/OCR") - suficiente para o projeto
/// funcionar de ponta a ponta localmente. Uma implementação de produção
/// substituiria isto por uma chamada a um modelo de IA multimodal (mantendo
/// a mesma interface IDocumentExtractionService), sem exigir nenhuma
/// mudança em Application/Domain.
/// </summary>
public sealed partial class AiFieldParsingService
{
    public (string TipoDocumento, List<CampoExtraidoBruto> Campos) ExtrairCampos(string textoOcr)
    {
        var campos = new List<CampoExtraidoBruto>();

        var cnpj = CnpjRegex().Match(textoOcr);
        if (cnpj.Success)
        {
            campos.Add(new CampoExtraidoBruto("CnpjFornecedor", cnpj.Value, ConfiancaPorTamanhoDeMatch(cnpj.Value, minimo: 18)));
        }

        var data = DataRegex().Match(textoOcr);
        if (data.Success)
        {
            campos.Add(new CampoExtraidoBruto("DataEmissao", data.Value, 85m));
        }

        var valor = ValorMonetarioRegex().Match(textoOcr);
        if (valor.Success)
        {
            campos.Add(new CampoExtraidoBruto("ValorTotal", valor.Value, 80m));
        }

        if (campos.Count == 0)
        {
            // Nenhum campo reconhecido: ainda assim retorna um campo
            // "TextoCompleto" com confiança baixa, para o documento cair em
            // AguardandoRevisao com PrioridadeRevisao=true (RN16) em vez de
            // falhar o processamento por completo.
            campos.Add(new CampoExtraidoBruto("TextoCompleto", Truncar(textoOcr, 500), 30m));
        }

        var tipo = ClassificarTipoDocumento(textoOcr);
        return (tipo, campos);
    }

    private static string ClassificarTipoDocumento(string texto)
    {
        var textoLower = texto.ToLowerInvariant();

        if (textoLower.Contains("nota fiscal") || textoLower.Contains("nf-e"))
        {
            return "NotaFiscal";
        }

        if (textoLower.Contains("recibo"))
        {
            return "Recibo";
        }

        if (textoLower.Contains("contrato"))
        {
            return "Contrato";
        }

        return "NaoClassificado";
    }

    private static decimal ConfiancaPorTamanhoDeMatch(string valor, int minimo) =>
        valor.Length >= minimo ? 95m : 70m;

    private static string Truncar(string texto, int tamanho) =>
        texto.Length <= tamanho ? texto : texto[..tamanho];

    [GeneratedRegex(@"\d{2}\.\d{3}\.\d{3}/\d{4}-\d{2}")]
    private static partial Regex CnpjRegex();

    [GeneratedRegex(@"\d{2}/\d{2}/\d{4}|\d{4}-\d{2}-\d{2}")]
    private static partial Regex DataRegex();

    [GeneratedRegex(@"R\$\s?\d{1,3}(\.\d{3})*(,\d{2})?")]
    private static partial Regex ValorMonetarioRegex();
}