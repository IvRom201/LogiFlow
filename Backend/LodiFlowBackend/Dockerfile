FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["LogiFlow.Domain/LogiFlow.Domain.csproj", "LogiFlow.Domain/"]
COPY ["LogiFlow.Application/LogiFlow.Application.csproj", "LogiFlow.Application/"]
COPY ["LogiFlow.Infrastructure/LogiFlow.Infrastructure.csproj", "LogiFlow.Infrastructure/"]
COPY ["LogiFlow.WebApi/LogiFlow.WebApi.csproj", "LogiFlow.WebApi/"]

RUN dotnet restore "LogiFlow.WebApi/LogiFlow.WebApi.csproj"

COPY . .

RUN dotnet publish "LogiFlow.WebApi/LogiFlow.WebApi.csproj" -c Release -o /app/publish --no-restore

FROM runtime AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "LogiFlow.WebApi.dll"]