# syntax=docker/dockerfile:1

# ---------- Build ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["IntelliDoc.sln", "."]
COPY ["Directory.Build.props", "."]
COPY ["src/IntelliDoc.Domain/IntelliDoc.Domain.csproj", "src/IntelliDoc.Domain/"]
COPY ["src/IntelliDoc.Application/IntelliDoc.Application.csproj", "src/IntelliDoc.Application/"]
COPY ["src/IntelliDoc.Infrastructure/IntelliDoc.Infrastructure.csproj", "src/IntelliDoc.Infrastructure/"]
COPY ["src/IntelliDoc.Api/IntelliDoc.Api.csproj", "src/IntelliDoc.Api/"]

RUN dotnet restore "src/IntelliDoc.Api/IntelliDoc.Api.csproj"

COPY . .
RUN dotnet publish "src/IntelliDoc.Api/IntelliDoc.Api.csproj" -c Release -o /app/publish --no-restore

# ---------- Runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Executa como usuário não-root (boa prática de segurança em contêineres)
RUN adduser --disabled-password --gecos "" --uid 5678 appuser && chown -R appuser /app
USER appuser

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "IntelliDoc.Api.dll"]