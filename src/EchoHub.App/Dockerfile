FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY EchoHub.Core/EchoHub.Core.csproj EchoHub.Core/
COPY EchoHub.Infrastructure/EchoHub.Infrastructure.csproj EchoHub.Infrastructure/
COPY EchoHub.App/EchoHub.App.csproj EchoHub.App/
RUN dotnet restore EchoHub.App/EchoHub.App.csproj

COPY . .
RUN dotnet publish EchoHub.App/EchoHub.App.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "EchoHub.App.dll"]
