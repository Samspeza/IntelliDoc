using Serilog.Context;

namespace IntelliDoc.Api.Middlewares;

/// <summary>
/// Gera (ou reaproveita, se o cliente já enviou) um identificador de
/// correlação por requisição, propagando-o no header de resposta
/// X-Correlation-Id e no contexto de log do Serilog (RNF05,
/// docs/04-arquitetura.md §7).
///
/// Resultado prático: toda linha de log emitida durante uma requisição -
/// incluindo as emitidas pelos Pipeline Behaviors do MediatR (Etapa 9.3) -
/// carrega o mesmo CorrelationId, permitindo reconstruir o caminho completo
/// de uma requisição problemática nos logs agregados.
/// </summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    private const string HeaderName = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var valorExistente)
            && !string.IsNullOrWhiteSpace(valorExistente)
                ? valorExistente.ToString()
                : Guid.NewGuid().ToString();

        context.TraceIdentifier = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("EmpresaId", context.User.FindFirst("empresa_id")?.Value ?? "-"))
        {
            await next(context);
        }
    }
}