FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY src/ProcureFlow.Api/ProcureFlow.Api.csproj src/ProcureFlow.Api/
COPY src/ProcureFlow.Application/ProcureFlow.Application.csproj src/ProcureFlow.Application/
COPY src/ProcureFlow.Domain/ProcureFlow.Domain.csproj src/ProcureFlow.Domain/
COPY src/ProcureFlow.Infrastructure/ProcureFlow.Infrastructure.csproj src/ProcureFlow.Infrastructure/

RUN dotnet restore src/ProcureFlow.Api/ProcureFlow.Api.csproj

COPY . .
RUN dotnet publish src/ProcureFlow.Api/ProcureFlow.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ProcureFlow.Api.dll"]
