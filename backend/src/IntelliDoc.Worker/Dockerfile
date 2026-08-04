# syntax=docker/dockerfile:1

# ---------- Build ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["IntelliDoc.sln", "."]
COPY ["src/IntelliDoc.Domain/IntelliDoc.Domain.csproj", "src/IntelliDoc.Domain/"]
COPY ["src/IntelliDoc.Application/IntelliDoc.Application.csproj", "src/IntelliDoc.Application/"]
COPY ["src/IntelliDoc.Infrastructure/IntelliDoc.Infrastructure.csproj", "src/IntelliDoc.Infrastructure/"]
COPY ["src/IntelliDoc.Worker/IntelliDoc.Worker.csproj", "src/IntelliDoc.Worker/"]

RUN dotnet restore "src/IntelliDoc.Worker/IntelliDoc.Worker.csproj"

COPY . .
RUN dotnet publish "src/IntelliDoc.Worker/IntelliDoc.Worker.csproj" -c Release -o /app/publish --no-restore

# ---------- Runtime ----------
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS runtime
WORKDIR /app

# Bibliotecas nativas exigidas pelo pacote Tesseract .NET (OCR, Etapa 4)
RUN apt-get update \
    && apt-get install -y --no-install-recommends libleptonica-dev libtesseract-dev tesseract-ocr-por \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Diretórios montados como volumes no docker-compose.yml (Etapa 14)
VOLUME ["/data/storage", "/data/tessdata"]

ENTRYPOINT ["dotnet", "IntelliDoc.Worker.dll"]