FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY Smartbot.sln ./Smartbot.sln
COPY src/Smartbot.Web/Smartbot.Web.csproj ./src/Smartbot.Web/Smartbot.Web.csproj
COPY Directory.Build.targets ./Directory.Build.targets

RUN dotnet restore Smartbot.sln

# Copy everything else and build
COPY . ./
ARG INFOVERSION="1.0.0+anonymous"
ARG VERSION="1.0.0"
RUN echo "Infoversion: $INFOVERSION"
RUN dotnet publish src/Smartbot.Web/Smartbot.Web.csproj -c Release -o /app/out/smartbot.web /p:Version=$VERSION /p:InformationalVersion=$INFOVERSION

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /smartbot.web
COPY --from=build-env /app/out/smartbot.web .
WORKDIR /
