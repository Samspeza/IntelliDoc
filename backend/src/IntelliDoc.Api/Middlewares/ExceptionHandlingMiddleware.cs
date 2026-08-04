using System.Net;
using System.Text.Json;
using IntelliDoc.Application.Common.Exceptions;
using IntelliDoc.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using ValidationException = IntelliDoc.Application.Common.Exceptions.ValidationException;

namespace IntelliDoc.Api.Middlewares;

/// <summary>
/// Único ponto de tratamento de exceções da Api. Substitui try/catch
/// espalhados pelos Controllers - nenhum Controller desta solução captura
/// exceção manualmente (Etapa 5, decisão de organização). Mapeamento:
///
///   DomainException / RegraDeNegocioException  -> 400 Bad Request
///   TransicaoStatusInvalidaException            -> 400 Bad Request (subtipo de DomainException)
///   Application.ValidationException             -> 400 Bad Request (com erros por campo)
///   ForbiddenAccessException                     -> 403 Forbidden
///   NotFoundException                            -> 404 Not Found
///   qualquer outra exceção                       -> 500 Internal Server Error (sem detalhes internos na resposta)
///
/// Formato de resposta: ProblemDetails (RFC 7807), padrão do ASP.NET Core,
/// já reconhecido nativamente pelo Swagger/OpenAPI (RNF02).
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
                HttpStatusCode.BadRequest,
                new ValidationProblemDetails(validationEx.Errors)
                {
                    Title = "Um ou mais erros de validação ocorreram.",
                    Status = (int)HttpStatusCode.BadRequest
                }),

            TransicaoStatusInvalidaException transicaoEx => (
                HttpStatusCode.BadRequest,
                (ProblemDetails)new ProblemDetails
                {
                    Title = "Transição de status inválida.",
                    Detail = transicaoEx.Message,
                    Status = (int)HttpStatusCode.BadRequest
                }),

            DomainException domainEx => (
                HttpStatusCode.BadRequest,
                new ProblemDetails
                {
                    Title = "Regra de negócio violada.",
                    Detail = domainEx.Message,
                    Status = (int)HttpStatusCode.BadRequest
                }),

            ForbiddenAccessException forbiddenEx => (
                HttpStatusCode.Forbidden,
                new ProblemDetails
                {
                    Title = "Acesso negado.",
                    Detail = forbiddenEx.Message,
                    Status = (int)HttpStatusCode.Forbidden
                }),

            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                new ProblemDetails
                {
                    Title = "Recurso não encontrado.",
                    Detail = notFoundEx.Message,
                    Status = (int)HttpStatusCode.NotFound
                }),

            _ => (
                HttpStatusCode.InternalServerError,
                new ProblemDetails
                {
                    Title = "Ocorreu um erro inesperado.",
                    // Detail proposital genérico - detalhes reais só vão para o log (não para a resposta).
                    Detail = "Tente novamente mais tarde. Se o problema persistir, contate o suporte.",
                    Status = (int)HttpStatusCode.InternalServerError
                })
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            logger.LogError(exception, "Erro não tratado: {Message}", exception.Message);
        }
        else
        {
            logger.LogWarning("Requisição rejeitada ({StatusCode}): {Message}", (int)statusCode, exception.Message);
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
    }
}