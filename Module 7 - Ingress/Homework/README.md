# Praca Domowa

### Bazując na wiedzy z poprzednich modułów i pracach domowych możesz teraz przeanalizować, w jaki sposób udostępniać aplikacje. Jeżeli nie masz swojego systemu, to wykonaj mentalne ćwiczenie „jakie wartości fajnie by było mieć”.

### Czy wykorzystasz Ingress zamiast wybranych metod publikacji z pracy domowej z modułu 5?
tak, nadal planuje użyć Ingress (zrobiłem mały reaserch podczas pracy nad modułem 5 wybiegający nieco poza zakres),
ten moduł utwierdził mnie w przeknoaniu, że to dobry wybór - zastosowałbym NGINX Ingress Controller ze względu na popularność i duże community w okół NGINX

### Czy będzie potrzebował różnych instalacji Ingress Controller?
tak, w tej chwili widzę zastosowanie dla conajmniej 3 różnych instancji
1) dla zastosowania zewnętrznego - aplikacje frontendowye dostępnych dla użytkowników
2) dla wykorzystania wewnętrzengo - backoffice - aplikacje frontendowe, gównie adminstration tools potrzebne dla ludzi z biznesu
3) dla wykorzystania wewnętrzengo - dostęp do backendowych serwisów dla developerów

### Jaki reguły Ingress wykorzystasz?
Głównie będzie to `virtual host routing` mamy kilka różnych hostów, dodatkowo będzie on zmiksowany z `fanout` - niektóre odzielne aplikacje są dostępne pod konkretną ścieżką na danym host. `capture all` do przekierowania wszystkiego co nie pasuje do reguł na 404 page

### Jakie typy certyfikatów wykorzystasz?
w tej chwili wydaje mi się, że zaimportujemy certyfikaty manualnie do `secret`, muszę poczytać trochę wiecej o możliwościach integracyjnych `cert-managera` dobrze byłoby nie martwić się o ręczne odświeżanie wygasających certyfikatów