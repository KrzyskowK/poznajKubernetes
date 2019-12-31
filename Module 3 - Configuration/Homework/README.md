### Skoro już wiesz jak działa przekazywanie konfiguracji do aplikacji w K8s, zastanów się i opisz (najlepiej na Slack), co w swojej aplikacji byś gdzie umieścił. Do dyspozycji masz:
- Zmienne środowiskowe
- ConfigMap
- Secrets
- Zewnętrzne narzędzia integrujące się z K8s

Odpowiedź:

__Stan rzeczy na dzień dzisiejszy:__

W tej chwili większość serwisów pobiera konfigurację z plików konfiguracyjnych  - zazwyczaj pojedynczy appsettings.json. 

appsettings.json jest zarządzany przez deployment tool, który "merguje" do niego zmienne jak i dane wrażliwe, nastepnie robi deploy applikacji i appsettings.json na środowisko.

każda aplikacja posiada osobny pipeline do deploymentu

system składa się z > 100 mikoreserwisów

__Najprostsze rozwiązanie (wymagające minimalenej zmiany kodu applikacji):__

Deployment tool nadal "merguje" appsettings.json, następnie tworzy `configMap` (unikatowy dla aplikacji) do którego za pomocą `--from-file` wgrywamy konfigurację.
Następnie `pod` z aplikacją montuje `configMap` jako wolumen.
Niestety appsettings.json jest obecnie trzymany w tym samym folderze co aplikacja w związky z czym w aplikacjach trzeba będzie zmienić ścieżki do appsettings.json (minimialna zmiana kodu o której wspomniałem)

rozwiązanie to wymaga ograniczenia dostępu do `configMap` i `pod` na produkcji jako że sekrety będą tam zapisane plain textem.

z tego miejsca możemy iść dalej i wprowadzić `secret` (wraz z szyfrowaniem) i osobne appsettings.secret.json (to będzie wymagać kolejnych zmian w aplikacji)

Jeszcze jedna uwaga:
szkoda że nie da się podmonotwać wolumenu w miejsce konkretnego pliku np /app/appsettings.json, tak żeby zaoszczędzić sobie zmian w kodzie - Próbowałem triku z `subPath`, ale nie działa tak jak się tego spodziewałem.


`Zmienne środowiskowe` - prawdopodobnie użylibyśmy ich do dostarczenia danych związanych z infrastruktura potrzebnych do logowania/distribution tracing (nazwa noda/poda/ip etc)

`Zewnętrzne narzędzia integrujące się z K8s` - w tej chwili wydaje mi się że nasze istenijące rozwiązanie jest bardzo dobre. Deployment tool bierze wersję aplikacji (builda) dokłada do niego konfigurację i wypuszcza nową wersję deploymentu na środowisko. Gdy zmieniamy konfigurację w deployment tool - musimy wydać nową wersję deploymentu (wersja builda aplikacji oczywiście pozostaje ta sama)

Wyciągnięcie konfiguracji do zewnętrznego narzędzia niepotrzebnie skomplikowałoby ten proces, i utrudniłoby namierzanie przyczyny fackupu - musiałbym porównywać wersję z dwóch różnych tooli żeby ustalić kiedy co było deployowane

Dodatkowo wiązałoby się to z potrzebą zmian w aplikacjach sposobu wyciągania konfiguracji

Niewątpliwą zaletą jaką tutaj widzę jest zwiększenie security - sensitie data jest dostępne już tylko wyłącznie wewnątrz nasze aplikacji






