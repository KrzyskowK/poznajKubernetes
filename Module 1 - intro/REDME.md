# Docker – Ćwiczenia

## 1. Czy udało się uruchomić projekt w Docker, tak? Dlaczego? Nie? Dlaczego?

komendy dockerowe zarowno pod windows jak i `wsl:v1` działają bez zarzutu, napotkalem natomiat na problem podczas pracy z `wsl:v2`

```
Cannot connect to the Docker daemon at tcp://localhost:2375. Is the docker daemon running?
```

po ponownym sprawdzeniu wszystkich konfiguracji, restarcie hyper-v i 30 minutach googlania postanowiłem dać sobie spokój i wrocić do distro z `wsl:v1`

issue podobny do tego z którym sie spotkałem jest już otwarty repo WSL:
https://github.com/microsoft/WSL/issues/4321


jeżeli ktoś miał podobny problem i udało mu się go rozwiązać to dajcie znac :)

## 2. Czy i jak udało się zoptymalizować czas budowania obrazu? Jaki rozmiar obrazu udało się osiągnąć? Z jakiego bazowego?

dockerfile wygenerowany za pomocą dockerowej wtyczki do vs code
początkowy rozmiar obrazu:
```
pk.demoapp 0.0.6 - 208MB
```

oraz dockerfile:
```
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["pk.demoApp.csproj", "./"]
RUN dotnet restore "./pk.demoApp.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "pk.demoApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "pk.demoApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "pk.demoApp.dll"]

```

początkowy dockerfile wyglada calkiem niezle. Mamy multistage build, aplikacje budujemy i publikujemy przy użyciu "cięższego" obrazu sdk, a rezultaty kopiujemy do lżejszego obrazu zawierającego jedynie runtime

w pierwszej kolejności podmieniłem obrazy bazowe na wersje alpine, które są znacznie lżejsze
```
mcr.microsoft.com/dotnet/core/sdk      3.1                 9817c25953a8        2 weeks ago         689MB
mcr.microsoft.com/dotnet/core/sdk      3.1-alpine          de8009ab77a4        2 weeks ago         405MB
mcr.microsoft.com/dotnet/core/aspnet   3.1                 08096137b740        2 weeks ago         207MB
mcr.microsoft.com/dotnet/core/aspnet   3.1-alpine          21eae2e8220f        2 weeks ago         105MB
```
nie przyniosło to jednak szokujących rezultatów:

```
pk.demoapp 0.0.6-alpine - 202MB
```