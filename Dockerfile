FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY server/src/ProcureFlow.Api/ProcureFlow.Api.csproj server/src/ProcureFlow.Api/
COPY server/src/ProcureFlow.Application/ProcureFlow.Application.csproj server/src/ProcureFlow.Application/
COPY server/src/ProcureFlow.Domain/ProcureFlow.Domain.csproj server/src/ProcureFlow.Domain/
COPY server/src/ProcureFlow.Infrastructure/ProcureFlow.Infrastructure.csproj server/src/ProcureFlow.Infrastructure/

RUN dotnet restore server/src/ProcureFlow.Api/ProcureFlow.Api.csproj

COPY server server
RUN dotnet publish server/src/ProcureFlow.Api/ProcureFlow.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app
EXPOSE 7860

COPY --from=build /app/publish .

ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-7860} dotnet ProcureFlow.Api.dll"]
