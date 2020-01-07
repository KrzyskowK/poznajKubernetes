# Ingress

## Ingress Contoller

- obiekt umoliwiający dostania się do serwisów z zewnatrz klastra
- pewna forma reverse-proxy
- dostęp do serwisów definiowany za pomocą zbioru reguł

przykład
```
https://moj.klaster.pl/api/v1 -> service-wersja1
https://moj.klaster.pl/api/v2 -> service-wersja2
```

### rodzaje ingress controller

- nginx ingress controller
- traefik
- F5 BIG-IP Controller (fizyczny F5 wpięty w klaster)
- Ambasador
- Service Mesh, np. Istio
- Skipper (od zalando) do testów A/B https://github.com/zalando/skipper/blob/master/docs/kubernetes/ingress-controller.md

⚠️ __ingress nie korzysta z serwisu przy przekierowaniu ruchu, jedynie zczytuje adresy do pod (utworzone przez serwis)__

### Ingress Controller Dobre Praktyki 

- używamy wiele installacji ingress controllera (osobny dla publicznych, osobny dla niepublicznych)
- używamy wiele instancji ingress controllera
- używaj NGINX bo toubleshooting jest prosty (duze community)
- używaj deamonsetów

### instalacja ingress controller

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

## Ingress Object

```
apiVersion: networking.k8s.io/v1beta1 #extensions/v1beta1
kind: Ingress
metadata:
  name: name-ing
  annotations: #here we put configuration (depends of Ingress Controller implementation)
    nginx.ingress.kubernetes.io/rewrite-target: / 
spec:
  rules:
  - http:
    paths:
    - path: /t
      backend: 
        serviceName: test
        servicePort: 80
    - backend: 
      serviceName: test
      servicePort: 80
```


### Ingress Object - Reguły routingu w Nginx

⚠️ __inne implementacje ingress mogą udostępniać różne rodzaje routingu__

1. __capture all__ - routing na pojedynczy serwis

przykładowe mapowanie: `http://moj.klaster.pl/<path> -> svc/test/<path>`

```
apiVersion: networking.k8s.io/v1beta1 #extensions/v1beta1
kind: Ingress
metadata:
  name: name-ing
spec:
  backend: 
    serviceName: test
    servicePort: 80
```

⚠️ __w wersji NGINX nie działa nazwanie portu :(__

2. __fanout__ - routing na bazie scieżki

przykłądowe mapowanie: `http://moj.klaster.pl/v1/<path> -> svc/test-v1/<path>` i `http://moj.klaster.pl/v2/<path> -> svc/test-v2/<path>`

```
apiVersion: networking.k8s.io/v1beta1 #extensions/v1beta1
kind: Ingress
metadata:
  name: name-ing
  annotations:
    nginx.ingress.kubernetes.io/rewrite-target: / 
spec:
  rules:
  - http:
    paths:
    - path: /v1
      backend: 
        serviceName: test-v1
        servicePort: 80
    - path: /v2
      backend: 
        serviceName: test-v2
        servicePort: 80
```

⚠️ __od wersji `0.22.2` NGINX Ingress Controller w path uzywamy regex__

```
apiVersion: networking.k8s.io/v1beta1 #extensions/v1beta1
kind: Ingress
metadata:
  name: name-ing
  annotations:
    nginx.ingress.kubernetes.io/rewrite-target: /$1
spec:
  rules:
  - http:
    paths:
    - path: /v1/?(.*)
      backend: 
        serviceName: test-v1
        servicePort: 80
    - path: /v2/?(.*)
      backend: 
        serviceName: test-v2
        servicePort: 80
```

3. __name based virtual hosting__ - routinh na bazie host header (mozemy użyc np. subdomain)

przykłądowe mapowanie: `http://test-v1.klaster.pl/<path> -> svc/test-v1/<path>` i `http://test-v2.klaster.pl/v2/<path> -> svc/test-v2/<path>`

```
apiVersion: networking.k8s.io/v1beta1 #extensions/v1beta1
kind: Ingress
metadata:
  name: name-ing
spec:
  rules:
  - host: test-v1.klaster.pl
    http:
      paths:
      - backend:
        serviceName: test-v1
        servicePort: 80
  - host: test-v2.klaster.pl
    http:
      paths:
      - backend:
        serviceName: test-v2
        servicePort: 80
```