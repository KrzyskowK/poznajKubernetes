# Praca domowa

### Bazując na swoim systemie, zastanów nad listą rzeczy poniżej i podziel się z nami przemyśleniami na Slacku. Jeżeli nie masz swojego systemu, to wykonaj mentalne ćwiczenie „jakie wartości fajnie by było mieć”.

- Na jakie i ile etykiet można by rozsądnie podzielić Twój system i dlaczego tak
- Jakie etykiety fajnie by było mieć by móc lepiej i sprawniej zarządzać grupą zasobów?
- Jakie adnotacje warto by wprowadzić i co one powinny zawierać?

---
Odpowiedź:

__labels__ -
myśle że wytyczne  podawane przez kubernetes byłyby jak najbardziej wystarczające

- `name` - nazwa aplikacji np: _appx / appy etc.._

- `instance` - każda aplikacja posiada więcej niż jedną instancję, dobrze byłoby móc ją znależć na podstawie logów np: _appx-[pod_uid] / appy-[pod_uid] etc.._
- `version` - każde deployment ustawia nową wersję, dobrze jest móc to zweryfikować np: _"5.7.21"_
- `component` - posiadamy wiele różnych typów aplikacji/narzędzi dobrze byłoby móc po nich szukać np: _api / ui / devtool / bus / database / job_
- `part-of` - system składa się wielu domen, w skład jednej domeny wchodzi często wiele mikroserwisów, dobrze jest to pogrupować np: _checkout / product / cart etc..._
- `managed-by` - różne części systemu są budowane i zarządzane przez różne zespoły, warto mieć tą wiedzę np: _team-abc / team-xyz / ops etc..._

__annotations__ -
tutaj dodałbym

- `description` - opis co robi aplikacja
- `repo` - url do repo
- `docs` - url do docs
- `deployment-pipeline` - url do deployment pipeline
- `build-piepline` url do build pipeline
- `contact` - kontakty do osoób odpowiedzialnych za aplikacje