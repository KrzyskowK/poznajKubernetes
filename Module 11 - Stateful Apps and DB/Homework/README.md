# Praca domowa
 

### Bazując na wiedzy o StatefulSets i bazach danych w Kubernetes możesz teraz przeanalizować, w jaki sposób przechowywać stan aplikacji. Jeżeli nie masz swojego systemu, to wykonaj mentalne ćwiczenie „jakie wartości fajnie by było mieć”.

### - Czy wykorzystasz StatefulSets dla aplikacji? Dlaczego? Czy da się usunąć stan z aplikacji?
### - Stan to baza danych czy może też sesyjność/stanowość procesu w aplikacji?

Generalnie unikamy stanu w aplikacjach, jeżeli jakaś legacy aplikacja wykorzystuje session state wyciągamy go
do zewnętrznego session provider (np. Redis Session) lub przenieść do bazy danych.

W przypadku baz danych są to bazy relacyjne i już wykorzysujemy do ich hostowania Platform-as-a-Service.

### - Jeżeli baza danych w Kubernetes to dlaczego w nim, a nie maszyna wirtualna i/lub gotowa usługa?

Nie wiem czy znalazłbym zastosowanie konkretnie dla bazy w k8s, ewentualnie użyłbym bazy hostowanej w zwykłym docker container do pracy na lokalnej maszynie

### - Jaki typ bazy danych danych użyjesz w Kubernetes? Czy jest ona gotowa do współpracy z nim?

potencjalnie moglibyśmy przenieść do kubernetes stack ElasticSearcha https://www.elastic.co/downloads/elastic-cloud-kubernetes

