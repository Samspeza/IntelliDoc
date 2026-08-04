using Serilog.Context;

namespace IntelliDoc.Api.Middlewares;

/// <summary>
/// Gera (ou reaproveita, se o cliente já enviou) um X-Correlation-Id por
/// requisição e o injeta no contexto de log do Serilog via LogContext -
/// todo log emitido durante o processamento desta requisição (Api, EF Core,
/// Behaviors do MediatR) passa a incluir esse Id automaticamente,
/// viabilizando rastrear uma requisição de ponta a ponta nos logs
/// (RNF05, docs/04-arquitetura.md §7).
/// Registrado bem no início da pipeline, em Program.cs.
/// </summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    private const string HeaderName = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var valorExistente)
            ? valorExistente.ToString()
            : Guid.NewGuid().ToString();

        context.Response.Headers[HeaderName] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }
}