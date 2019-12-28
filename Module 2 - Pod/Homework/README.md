# homework

## Twoja aplikacja ma zostać przeniesiona do Pod. Odpowiedz na poniższe pytania w tym kontekscie.

> Uwaga: Jeżeli na przykład potrzebujesz Windows Containers to załóż, że są już dostępne. Jeżeli wiesz, że będziesz dzielił aplikację, to możesz założyć że to już się stało. Czyli podejdź do tego pozytywnie, a nie w stylu „nie-da-się”, bo technologia cały czas się rozwija, a problemy z architekturą zostają.

### Czy będzie wymagać użycia kontenerów typu Init? jeśli tak to dlaczego?

możliwe że dla serwisów serwujących html moglibyśmy wykorzystać InitContainer do wyciągania statycznych plików,
Zanim użyłbym InitContainers W pierwszej kolejności sprawdziłbym na ile obciążając jest dołączanie statycznych plików do kontenera.

### Czy będzie wymagać użycia wolumenów emptyDir? 
Tylko jeżeli mielibysmy uzyc initContainer :arrow_up:

w tym momencie żaden z serwisów nie uzywa lokalnych zasobów dyskowych, jeżeli potrzebujemy utrzymywać jakies pliki jest to zazwyczaj robione przez jakis 3rd party service np BLOB

### Czy posiada endpointy mogące służyć jako probe dla Health checks? Jeśli nie to czy można je zaimplementować bez problemu?

tak większość serwisów posiada endpoint `/ping` który można wykorzystać w livenessProbe. W przypadku redinessProbe, musielibyśmy przygotować `/health` endpoint który sprawdzałby kluczowe połączenia do innych serwisów

### Jaka klasa QoS będzie do niej pasować najlepiej? Dlaczego?

zz racji tego że jest to platforma e-commerce serwisy muszą być gotowe na przyjęcie zwiększonego ruchu ze strony klientów. W tym momencie wydaje mi sie że powinien to byc `QoS: Burstable` nie wiem natomiast czy nieprzypisywanie `limits` będzie miało negatywny wpływ na automatyczny scale-out serisów.

### Czy będą potrzebne postStart i preStop hook? Jeśli tak to dlaczego?

na ten moment wydaje mi się, że nie

### Czy aplikacja obsługuje SIG_TERM poprawnie? Jeśli nie to jak można to obejść?

tak

### Czy domyślny czas na zamknięcie aplikacji w Pod jest wystarczający?

30 sekund powinno być wystarczające, trzeba byloby to sprawdzić dla dużych "legacy" serwisów oraz serwisów które utzymują long-running background threads - ewentualnie dostosować