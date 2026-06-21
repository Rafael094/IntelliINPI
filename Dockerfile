FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore IntelliINPI.sln

RUN dotnet publish src/IntelliINPI.Api/IntelliINPI.Api.csproj `
    -c Release `
    -o /app/publish `
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "IntelliINPI.Api.dll"]
