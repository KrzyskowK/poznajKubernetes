FROM mcr.microsoft.com/dotnet/core/runtime-deps:3.1-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build
WORKDIR /src
COPY ["pk.demoApp.csproj", "./"]
RUN dotnet restore "./pk.demoApp.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "pk.demoApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "pk.demoApp.csproj" -c Release /p:PublishSingleFile=true /p:PublishTrimmed=true -r linux-musl-x64 -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["./pk.demoApp"]