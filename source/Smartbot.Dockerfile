FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY Smartbot.sln ./Smartbot.sln

COPY src/Smartbot.Web/Smartbot.Web.csproj ./src/Smartbot.Web/Smartbot.Web.csproj
COPY src/Smartbot.Data/Smartbot.Data.csproj ./src/Smartbot.Data/Smartbot.Data.csproj
COPY src/Slackbot.Net.Extensions.Smartbot.Worker/Slackbot.Net.Extensions.Smartbot.Worker.csproj ./src/Slackbot.Net.Extensions.Smartbot.Worker/Slackbot.Net.Extensions.Smartbot.Worker.csproj


COPY test/Smartbot.Tests/Smartbot.Tests.csproj ./test/Smartbot.Tests/Smartbot.Tests.csproj
COPY Directory.Build.targets ./Directory.Build.targets

RUN dotnet restore Smartbot.sln

# Copy everything else and build
COPY . ./
RUN dotnet publish src/Smartbot.Web/Smartbot.Web.csproj -c Release -o /app/out/smartbot.web

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /smartbot.web
COPY --from=build-env /app/out/smartbot.web .
WORKDIR /
