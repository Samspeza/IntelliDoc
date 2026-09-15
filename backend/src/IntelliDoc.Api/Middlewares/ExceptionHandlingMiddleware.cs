using System.Text.Json;
using IntelliDoc.Application.Common.Exceptions;
using IntelliDoc.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using ValidationException = IntelliDoc.Application.Common.Exceptions.ValidationException;

namespace IntelliDoc.Api.Middlewares;

/// <summary>
/// Ponto ÚNICO de tradução de exceções para respostas HTTP, no formato
/// ProblemDetails (RFC 7807). Elimina try/catch repetido em cada Controller
/// e garante que o cliente sempre receba um corpo de erro consistente.
///
/// Mapeamento:
///   ValidationException        -> 400 (com dicionário de erros por campo)
///   DomainException            -> 400 (violação de regra de negócio, RN*)
///   ForbiddenAccessException   -> 403
///   NotFoundException          -> 404
///   demais                     -> 500 (detalhes NUNCA expostos ao cliente,
///                                       apenas logados - evita vazar stack
///                                       trace/estrutura interna)
/// </summary>
public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await TratarExcecaoAsync(context, ex);
        }
    }

    private async Task TratarExcecaoAsync(HttpContext context, Exception exception)
    {
        var (statusCode, problemDetails) = exception switch
        {
            ValidationException validationEx => (
                StatusCodes.Status400BadRequest,
                (ProblemDetails)new ValidationProblemDetails(validationEx.Errors)
                {
                    Title = "Erro de validação",
                    Status = StatusCodes.Status400BadRequest,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                }),

            DomainException domainEx => (
                StatusCodes.Status400BadRequest,
                new ProblemDetails
                {
                    Title = "Regra de negócio violada",
                    Detail = domainEx.Message,
                    Status = StatusCodes.Status400BadRequest,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                }),

            ForbiddenAccessException forbiddenEx => (
                StatusCodes.Status403Forbidden,
                new ProblemDetails
                {
                    Title = "Acesso negado",
                    Detail = forbiddenEx.Message,
                    Status = StatusCodes.Status403Forbidden,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3"
                }),

            NotFoundException notFoundEx => (
                StatusCodes.Status404NotFound,
                new ProblemDetails
                {
                    Title = "Recurso não encontrado",
                    Detail = notFoundEx.Message,
                    Status = StatusCodes.Status404NotFound,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
                }),

            _ => (
                StatusCodes.Status500InternalServerError,
                new ProblemDetails
                {
                    Title = "Erro interno do servidor",
                    // Mensagem genérica de propósito - o detalhe real vai
                    // apenas para o log, nunca para o cliente.
                    Detail = "Ocorreu um erro inesperado ao processar a requisição.",
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                })
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Erro não tratado na requisição {Method} {Path}",
                context.Request.Method, context.Request.Path);
        }
        else
        {
            logger.LogWarning("Requisição {Method} {Path} rejeitada ({StatusCode}): {Message}",
                context.Request.Method, context.Request.Path, statusCode, exception.Message);
        }

        problemDetails.Instance = context.Request.Path;
        problemDetails.Extensions["correlationId"] = context.TraceIdentifier;

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}