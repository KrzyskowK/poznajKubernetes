### Bazując na wiedzy z poprzednich modułów możesz teraz przeanalizować w jaki sposób udostępniać aplikacje sieciowo w klastrze i po za klastrem. Jeżeli nie masz swojego systemu, to wykonaj mentalne ćwiczenie „jakie wartości fajnie by było mieć”.

### Czy będzie korzystać z Service Discovery? Jeśli tak to jakiego? Dlaczego?

`Service Discovery` - Service discovery via DNS wydaje się wystarczające, nazwy aplikacji oraz namespace nie będą się zmieniać

### Jakie typy serwisu wykorzysta do Services wykorzystasz? Dlaczego?
### Do publikacji aplikacji użyjesz NodePort czy LoadBalancer? Dlaczego?

`ClusterIp` - dla wszystkich api/bus/job które nie musza być dostępne z zewnątrz
`ExternalName` - dla wszystkich zewnętrznych usług które chciałybym mieć w dns np. zew baza sql, zew 3rd party services

Jeśli chodzi o wystawienie publicznych aplikacji na świat (frontend / public api), wydaje mi się że najlepiej sprawdziłby się `Ingress` - router na pojedynczym IP wychodzącym na świat, z regółami przekierowań ustawionymi dla `host header`.
Odnośnie `NodePort`/`LoadBalancer` według mnie mają tą wadę, że portem/ip trzeba zarządzać na zewnątrz klastra (przepuscic to przez jakiegos edge providera żeby odfiltrowac ddos etc.) Kiedy takich aplikacji wystawionych na świat jest dużo (jak w moim przypadku) wydaje mi się, że nie będzie to się dobrze skalować. 
W przypadku `LoadBalancera` dla AKS każde statyczne publiczne ip generuje koszty, w przypadku `NodePort` musiałbym również ustawić porty statycznie - tak żeby móc bez problemu oddtowrzyć klaster w takiej samej postaci (np w przypadku awarii datacenter)

