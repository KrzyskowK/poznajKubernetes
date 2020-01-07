# Ingress

- obiekt umoliwiający dostania się do serwisów z zewnatrz klastra
- pewna forma reverse-proxy
- dostęp do serwisów definiowany za pomocą zbioru reguł

przykład
```
https://moj.klaster.pl/api/v1 -> service-wersja1
https://moj.klaster.pl/api/v2 -> service-wersja2
```

## rodzaje ingress

- nginx ingress controller
- traefik
- F5 BIG-IP Controller (fizyczny F5 wpięty w klaster)
- Ambasador
- Service Mesh, np. Istio
- Skipper (od zalando) do testów A/B https://github.com/zalando/skipper/blob/master/docs/kubernetes/ingress-controller.md

⚠️ __ingress nie korzysta z serwisu przy przekierowaniu ruchu, jedynie zczytuje adresy do pod (utworzone przez serwis)__

## instalacja ingress

https://github.com/kubernetes/ingress-nginx/blob/master/docs/deploy/index.md

A. Można wykorzystać manifesty 
- instalujemy ogólny manifest
- instalujemy części specificzne dla platformy (np. na Azure - domyślny load balancer)

B. Można wykorzystać Chart z repozytorum `helm`

```
# dodanie repo do helm
helm repo add stable https://kubernetes-charts.storage.googleapis.com/ 

# utworzenie namespace
kubectl create ns ingress-external 

# instalacja ingress korzystając z helm
helm install ingress-external stable/nginx-ingress --set controller.ingressClass=ingress-external --set controller.replicaCount=2 --set controller.service.externalTrafficPolicy=Local --set controller.image.tag=0.26.1 --namespace=ingress-external

# sprawdzenie statusu instalacji ingress
kubectl get all -n ingress-external
```

⚠️ __na klastrze możemy użyć więcej niż jedenj instancji ingress controller (wybór za pomocą ingress.class)__

## Helm

- package manager dla kubernetes
- narzędzie do templatowania

## Dobre Praktyki

- używamy wiele installacji ingress controllera (osobny dla publicznych, osobny dla niepublicznych)
- używamy wiele instancji ingress controllera
- używaj NGINX bo toubleshooting jest prosty (duze community)
- używaj deamonsetów