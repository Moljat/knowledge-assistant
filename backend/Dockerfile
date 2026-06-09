FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY KnowledgeAssistant.sln global.json ./
COPY backend/src/KnowledgeAssistant.Domain/KnowledgeAssistant.Domain.csproj backend/src/KnowledgeAssistant.Domain/
COPY backend/src/KnowledgeAssistant.Application/KnowledgeAssistant.Application.csproj backend/src/KnowledgeAssistant.Application/
COPY backend/src/KnowledgeAssistant.Infrastructure/KnowledgeAssistant.Infrastructure.csproj backend/src/KnowledgeAssistant.Infrastructure/
COPY backend/src/KnowledgeAssistant.Api/KnowledgeAssistant.Api.csproj backend/src/KnowledgeAssistant.Api/

RUN dotnet restore backend/src/KnowledgeAssistant.Api/KnowledgeAssistant.Api.csproj

COPY backend/ backend/
RUN dotnet publish backend/src/KnowledgeAssistant.Api/KnowledgeAssistant.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "KnowledgeAssistant.Api.dll"]
