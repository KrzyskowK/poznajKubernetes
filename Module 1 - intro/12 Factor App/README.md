# 12 Factor App – Ćwiczenia

https://12factor.net/

## Jakie praktyki z 12Factor mój projekt aktualnie spełnia? Jakich praktyk z 12Factor nie spełnia? Czy mogę tak zmodyfikować projekt, by spełniał wszystkie praktyki 12Factor?


### 1. Codebase
> One codebase tracked in revision control, many deploys

#### Spełnione ✔️
- wszystkie aplikacje są trzymane w .git repo,
- wersjonowanie builda jest oddzielone od processu deploymentu
- ten sam build jest deployowany na różne srodowiska (dev/test/prod) 
- podczas deploymentu build jest parowany z konfiguracją odpowiednią dla danego środowiska
- różne środowiska mogą działać z róznymi wersjami buildów

#### Do czego moża się przyczepić ⛔️
niektóre aplikacje składające się z frontendu (web SPA) oraz .api są trzymane w jednym repozytorium, są buildowane zawsze razem (jeden task na jenkins), ale posiadają oddzielne pipeliny do deploymentu dla frontend i backendu

teoretycznie, narusza to
> If there are multiple codebases, it’s not an app – it’s a distributed system. Each component in a distributed system is an app, and each can individually comply with twelve-factor.

Wydaje mi się jednak, że rozdzielanie repozytorium w przypadku gdy codebase jest ze sobą ściśle powiązany, nie mamy dużych korzyści z rozdzielania go. W zamian otrzymujemy łatwiejsze zarządzanie kompatybilnością pomiedzy frontendem i backendem (zawsze maja przypisana ta sama wersje builda).
Idąc tym tokiem myślenia powinnismy również zmergować pipline do deploymentu w jeden wspólny dla frontu i backendu.

#### Spostrzeżenia 🤔
Popularność zyskują ostatnio tzw `monorepo` które wydają się nie spełniać tej zasady


### 2. Dependencies
> Explicitly declare and isolate dependencies

#### Spełnione ✔️
- zależności wyodrębnione i dociągane za pomocą package managerów takich jak nugeta, npm etc


### 3. Config
> Store config in the environment

#### Spełnione ✔️

- codebase trzyma template pliku konfiguracyjnego z kluczami do konfiguracji, wartości są przechowywane w systemie do deploymentu per środowisko
- pliki configuracyjne są dodane do .gitignore co ogranicza szanse przypadkowego wcommitowania

#### Do czego moża się przyczepić ⛔️

zmienne nie są wstrzykiwane jako _environment variables_. Da się to poprawić, wymagałoby to zmian w codebase

### 4. Backing services
> Treat backing services as attached resources

#### Spełnione ✔️

- wszystkie 3rd party services jak db/smtp/messaging/loggery/monitory metryk są połączone z aplikacją poprzez connection string w konfiguracji
- w każdym momencie można zmienić connection string dla danego 3rd party service i zredeployowac aplikacje

### 5. Build, release, run
> Strictly separate build and run stages

#### Spełnione ✔️

- _build stage_: konwertuje kod z repo w wykonywalnego builda (ew. w bundla), przydziela mu odpowiednią wersje 
- _release stage_: tool do deploymentu pobiera gotowego builda, dołącza do niego odpowiednią konfiguracje i wypycha na serwer. Build tool zapewnia wersjonowanie deploymentow oraz możliwość wglądu i zarządzania deploymentami.
- _run stage_: automatycznie handlowane przez _execution environment_ po zdeployowaniu nowej wersji builda

#### Do czego moża się przyczepić ⛔️

build tool pozwala na podmianę konfiguracji i wykonanie re-deploymentu bez podbicia wersji deploymentu. Narusza to zasadę
> Releases are an append-only ledger and a release cannot be mutated once it is created. Any change must create a new release.

Nie jest to praktykowane w naszym procesie CI/CD jednak możliwe, wprowadza sporo zamieszania podczas audytowanie deploymentów.

### 6. Processes
> Execute the app as one or more stateless processes

#### Spełnione ✔️
- aby zachować odpowiednią redundancję serwisy są uruchamiane zawszę w większej liczbie instancji niż 1 - statelessowe podejście mamy więc by default

### 7. Port binding
> Export services via port binding

#### Spełnione ✔️
- handlowane przez IIS, Azure ASP (webapp), ngnix (w zaleznosci od tego gdzie lezy aplikacja)
- applikacje nie wiedzą na jakim porcie chodzą.

### 8. Concurrency
> Scale out via the process model

#### Spełnione ✔️
- wszystkie aplikacje są w pełni gotowe na scale-out 

### 9. Disposability
> Maximize robustness with fast startup and graceful shutdown

#### Do czego moża się przyczepić ⛔️

- nie wszędzie gdzie jest to możliwe (i potrzebne) korzystamy z asynchronicznego przetwarzania
- nie wszystkie aplikacje są w stanie szybko wstać

### 10. Dev/prod parity
> Keep development, staging, and production as similar as possible

#### Spełnione ✔️
- _Time between deploys_ - skrócony do minimum, każdy task jest deployowalny na produkcję
- _Code authors vs code deployers_ - wszystko dzieje się w obrębie jedengo zespołu
- _Dev vs production environments_ - dążymy do tego żeby były jak najbardziej podobne, ze względów oczywistych działają w mniejszej skali, używamy również zastępczych "backing service" dla nieprodukcyjnych środowisk np. aggregator loggow

### 11. Logs
> Treat logs as event streams

#### Spełnione ✔️
- każda aplikacja wypycha logi w batchach do sinka który pcha je dalej do aggregatora logow w stylu ELK/splunk
- jest możliwośc wyrzucania logów do wielu lokalizacji jednoczesnie

### 12. Admin processes
> Run admin/management tasks as one-off processes

#### Spełnione ✔️
- tak, staramy się aby wszystkie nowe zmiany administracyjne wypuszczane były jako część CI/CD np. migracje na relacyjnych bazach odpalane za pomocą dbup, instalacja dodatkowych agentow do zbierania metryk etc.

#### Do czego moża się przyczepić ⛔️

- część jednorazowych operacji na bazie, w konfiguracji sieci czy w Azure ASP nadal jest obslugiwana recznie przez zespół OPS.  

## Czy architektura rozwiązania umożliwi konteneryzację?

tak, większość aplikacji jest już skonteneryzowana.
Część legacy aplikacji nadal wymaga jednak pełnego .net framework w związku z tym muszą byc uruchamiane w kontenerach windowsowych.
