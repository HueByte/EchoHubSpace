FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY EchoHub.Core/EchoHub.Core.csproj EchoHub.Core/
COPY EchoHub.App/EchoHub.App.csproj EchoHub.App/
COPY EchoHub.Infrastructure/EchoHub.Infrastructure.csproj EchoHub.Infrastructure/
RUN dotnet restore EchoHub.Infrastructure/EchoHub.Infrastructure.csproj

COPY . .
RUN dotnet publish EchoHub.Infrastructure/EchoHub.Infrastructure.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "EchoHub.Infrastructure.dll"]
