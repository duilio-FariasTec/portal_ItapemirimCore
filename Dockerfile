# ==============================================================================
# Dockerfile Multi-Stage — Portal Itapemirim (ANI Core API)
# Plataforma: .NET 10 (C# 13 / ASP.NET Core)
# Otimizado para AWS App Runner, Amazon ECS (Fargate) e Docker Local
# ==============================================================================

# ------------------------------------------------------------------------------
# 1. Base Runtime Image (leve, segura, não-root)
# ------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_EnableDiagnostics=0

# Garante permissões na pasta de dados para o usuário não-root (APP_UID = 1654 no .NET)
USER root
RUN mkdir -p /app/data && chown -R $APP_UID:$APP_UID /app

# ------------------------------------------------------------------------------
# 2. Build Stage (SDK completo para restauração e compilação com cache de camadas)
# ------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copia arquivos de solução e configuração global
COPY ["Directory.Build.props", "./"]
COPY ["Itapemirim.Ani.sln", "./"]

# Copia apenas os .csproj para aproveitar o cache de restauração do Docker
COPY ["src/Itapemirim.Ani.Domain/Itapemirim.Ani.Domain.csproj", "src/Itapemirim.Ani.Domain/"]
COPY ["src/Itapemirim.Ani.Application/Itapemirim.Ani.Application.csproj", "src/Itapemirim.Ani.Application/"]
COPY ["src/Itapemirim.Ani.Infrastructure/Itapemirim.Ani.Infrastructure.csproj", "src/Itapemirim.Ani.Infrastructure/"]
COPY ["src/Itapemirim.Ani.Api/Itapemirim.Ani.Api.csproj", "src/Itapemirim.Ani.Api/"]
COPY ["tests/Itapemirim.Ani.UnitTests/Itapemirim.Ani.UnitTests.csproj", "tests/Itapemirim.Ani.UnitTests/"]
COPY ["tests/Itapemirim.Ani.IntegrationTests/Itapemirim.Ani.IntegrationTests.csproj", "tests/Itapemirim.Ani.IntegrationTests/"]

# Restaura dependências do NuGet
RUN dotnet restore "Itapemirim.Ani.sln"

# Copia todo o código-fonte restante
COPY . .

# ------------------------------------------------------------------------------
# 3. Publish Stage (Compila e publica os binários finais otimizados de produção)
# ------------------------------------------------------------------------------
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR "/src/src/Itapemirim.Ani.Api"
RUN dotnet publish "Itapemirim.Ani.Api.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

# ------------------------------------------------------------------------------
# 4. Final Runtime Image (Contêiner final de execução)
# ------------------------------------------------------------------------------
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Garante que os arquivos publicados pertençam ao usuário não-root
USER root
RUN chown -R $APP_UID:$APP_UID /app

# Executa sob usuário com privilégios reduzidos ($APP_UID = 1654)
USER $APP_UID

ENTRYPOINT ["dotnet", "Itapemirim.Ani.Api.dll"]
