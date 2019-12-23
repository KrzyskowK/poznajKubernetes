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

żeby sprawdzić co składa sie na 208mb naszego obrazu, odpalamy
```
docker history pk.demoapp:0.0.6

IMAGE               CREATED             CREATED BY                                      SIZE                COMMENT
944f669ef583        13 days ago         /bin/sh -c #(nop)  ENTRYPOINT ["dotnet" "pk.…   0B
90596e655877        13 days ago         /bin/sh -c #(nop) COPY dir:7cc00117752339f87…   202kB
f97c584bf642        2 weeks ago         /bin/sh -c #(nop) WORKDIR /app                  0B
0056615deb30        2 weeks ago         /bin/sh -c #(nop)  EXPOSE 443                   0B
9e6f8f1a6243        2 weeks ago         /bin/sh -c #(nop)  EXPOSE 80                    0B
2f1c8c326bba        2 weeks ago         /bin/sh -c #(nop) WORKDIR /app                  0B
08096137b740        2 weeks ago         /bin/sh -c aspnetcore_version=3.1.0     && c…   17.8MB
<missing>           2 weeks ago         /bin/sh -c dotnet_version=3.1.0     && curl …   76.7MB
<missing>           2 weeks ago         /bin/sh -c apt-get update     && apt-get ins…   2.28MB
<missing>           2 weeks ago         /bin/sh -c #(nop)  ENV ASPNETCORE_URLS=http:…   0B
<missing>           2 weeks ago         /bin/sh -c apt-get update     && apt-get ins…   41.3MB
<missing>           4 weeks ago         /bin/sh -c #(nop)  CMD ["bash"]                 0B
<missing>           4 weeks ago         /bin/sh -c #(nop) ADD file:bc8179c87c8dbb3d9…   69.2MB
```

widzimy, że praktycznie cała objętość obrazu pochodzi z warstw obrazów bazowych, podmieniamy je zatem na lżejsze wersje 'alpine':
```
mcr.microsoft.com/dotnet/core/sdk      3.1                 9817c25953a8        2 weeks ago         689MB
mcr.microsoft.com/dotnet/core/sdk      3.1-alpine          de8009ab77a4        2 weeks ago         405MB
mcr.microsoft.com/dotnet/core/aspnet   3.1                 08096137b740        2 weeks ago         207MB
mcr.microsoft.com/dotnet/core/aspnet   3.1-alpine          21eae2e8220f        2 weeks ago         105MB
```

efekt jest natychmiastowy:
```
pk.demoapp 0.0.12-alpine - 105MB
```

i dla porządku warstwy najnowszego obrazu:
```
docker history pk.demoapp:0.0.11-alpine

IMAGE               CREATED             CREATED BY                                      SIZE                COMMENT
348d14236d75        5 minutes ago       /bin/sh -c #(nop)  ENTRYPOINT ["dotnet" "pk.…   0B
9c5fe5788926        5 minutes ago       /bin/sh -c #(nop) COPY dir:8578660aa126a1fe8…   201kB
b06992f1013c        13 days ago         /bin/sh -c #(nop) WORKDIR /app                  0B
24f210aa1414        13 days ago         /bin/sh -c #(nop)  EXPOSE 443                   0B
10c38469035f        13 days ago         /bin/sh -c #(nop)  EXPOSE 80                    0B
97e5101b42c9        13 days ago         /bin/sh -c #(nop) WORKDIR /app                  0B
21eae2e8220f        2 weeks ago         /bin/sh -c aspnetcore_version=3.1.0     && w…   17.8MB
<missing>           2 weeks ago         /bin/sh -c dotnet_version=3.1.0     && wget …   77.2MB
<missing>           2 weeks ago         /bin/sh -c #(nop)  ENV ASPNETCORE_URLS=http:…   0B
<missing>           2 weeks ago         /bin/sh -c apk add --no-cache     ca-certifi…   4.08MB
<missing>           2 months ago        /bin/sh -c #(nop)  CMD ["/bin/sh"]              0B
<missing>           2 months ago        /bin/sh -c #(nop) ADD file:fe1f09249227e2da2…   5.55MB
```

Możemy pójśc o krok dalej i użyć obrazu przeznaczonego dla aplikacji .netcore typu self-contained, obraz ten musi posiadać jedynie natywne zależności potrzebne do uruchomienia aplikacji .netcore ale bez potrzeby dołączania samego. runtime'u

W tym celu posłużymy się obrazem `mcr.microsoft.com/dotnet/core/runtime-deps` w wersji alpine, poniżej porównanie wszystkich 3 obrazów. Widzimy, że runtime-deps waży jedynie ~9MB

```
mcr.microsoft.com/dotnet/core/aspnet         3.1                 08096137b740        2 weeks ago         207MB
mcr.microsoft.com/dotnet/core/aspnet         3.1-alpine          21eae2e8220f        2 weeks ago         105MB
mcr.microsoft.com/dotnet/core/runtime-deps   3.1-alpine          fc2df120b436        2 weeks ago         9.63MB
```

oczywiście jeżeli pozbywamy sie runtimu z obrazu, musimy zawżeć wszystkie potrzebne zależności wewnątrz naszej aplikacji
w tym celu dodajemy do naszej komendy `dotnet publish` argumenty: `/p:PublishSingleFile=true /p:PublishTrimmed=true -r linux-musl-x64`.
Optymalizacja zależności wydłuża process publikowania aplikacji ale za to finalny rozmiar obrazu to:
```
pk.demoapp:0.0.14-alpine-selfcontained - 65.5MB
```
Jeżeli przyjrzymy się teraz jak wyglądają warstwy naszego obrazu, widzimy że nie mamy warstw dodających .netcore runtime i aspnet core które zajmowały ~95MB.
Zamiast tego niezbędne zależności dodane są do naszej aplikacji, widzimy że ta warstwa urosła do ~56MB.

```
docker history pk.demoapp:0.0.14-alpine-selfcontained   

IMAGE               CREATED             CREATED BY                                      SIZE                COMMENT
1e2b53807a81        35 minutes ago      /bin/sh -c #(nop)  ENTRYPOINT ["./pk.demoApp…   0B
c8a743e5d6a7        35 minutes ago      /bin/sh -c #(nop) COPY dir:8811fa6c2efe8f504…   55.9MB
42c95c63c2c0        39 minutes ago      /bin/sh -c #(nop) WORKDIR /app                  0B
82efa0c60d4a        39 minutes ago      /bin/sh -c #(nop)  EXPOSE 443                   0B
88f13701ba7a        39 minutes ago      /bin/sh -c #(nop)  EXPOSE 80                    0B
3f637dcacf39        39 minutes ago      /bin/sh -c #(nop) WORKDIR /app                  0B
fc2df120b436        2 weeks ago         /bin/sh -c #(nop)  ENV ASPNETCORE_URLS=http:…   0B
<missing>           2 weeks ago         /bin/sh -c apk add --no-cache     ca-certifi…   4.08MB
<missing>           2 months ago        /bin/sh -c #(nop)  CMD ["/bin/sh"]              0B
<missing>           2 months ago        /bin/sh -c #(nop) ADD file:fe1f09249227e2da2…   5.55MB
```

finalny dockerfile:
```
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
```

### Pomocne linki
https://www.hanselman.com/blog/OptimizingASPNETCoreDockerImageSizes.aspx
https://medium.com/@chrislewisdev/optimizing-your-net-core-docker-image-size-with-multi-stage-builds-778c577121d
https://www.talkingdotnet.com/create-trimmed-self-contained-executable-in-net-core-3-0/
https://www.hanselman.com/blog/MakingATinyNETCore30EntirelySelfcontainedSingleExecutable.aspx