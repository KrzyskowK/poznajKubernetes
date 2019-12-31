# Serwis i Serwis Discovery

## Typy komunikacji wew. kubernetes

### container-to-container

- współdzielą tą samą sieć w `POD`
- mogą gadać do siebie po localhost

### pod-to-pod

- komunikacja pomiedzy podami w cluster
- komunikacja za pomoca `cluster network` (network plugin)

### pod-to-service: ClusterIP

- service typu `ClusterIP` (patrz niżej)
- serwis posiada jedno IP wyjściowe i może być podłączony za pomocą selectora do wielu `POD` 
- `kubeproxy` ustawia w `IP tables` dla każdego `node`
- ruch randomowo rozrzucany po wszystkich podach

```
kind: Service
apiVersion: v1
metadata:
  name: helloapp-port
spec:
  selector:
    app: helloapp
  type: ClusterIP
  ports:
  - name: http
    port: 80
    targetPort: 8080
    protocol: TCP
```

### external-to-service: NodePort

- service typu `NodePort` (patrz niżej)
- service dostaje zewnętrzne IP
- `kubeproxy` ustawia w `IP tables` dla każdego `node` regułę która przekierowuje na service
```
kind: Service
apiVersion: v1
metadata:
  name: helloapp
spec:
  selector:
    app: helloapp
  type: NodePort
  ports:
  - name: http
    port: 80
    targetPort: 8080
    protocol: TCP
```

### external-to-service: LoadBalancer

- service typu `LoadBalancer` rozszerza `NodePort`
- aby go wykorzystać, kubernetes musi być skonfigurowany z LoadBalancerem dostawcy

```
kind: Service
apiVersion: v1
metadata:
  name: helloapp
spec:
  selector:
    app: helloapp
  type: LoadBalancer
  ports:
  - name: http
    port: 80
    targetPort: 8080
    protocol: TCP
```

## Service

- obiekt grupujący pody
- możemy połączyć się do niego w `cluster network` za pomocą nazwy
- serwis domyślnie jest typu `ClusterIP` - ma przyspisany swój adres

pobierz wszystkie Services
```
kubectl get svc
kubectl get services
```
 
opis services
```
kubectl describe svc
```
 
## Endpoints

- zasób który może wskazać na zewnętrzne zasoby. 
- powinien występować w parze z `service`

```
kind: Service
apiVersion: v1
metadata:
  name: external-web
spec:
  ports:
  - name: http
    protocol: TCP
    port: 80
    targetPort: 80 
---
kind: Endpoints
apiVersion: v1
metadata:
  name: external-web
subsets: 
  - addresses:
    - ip: 1.1.1.1
    ports:
      - port: 80 
        name: http
```

pobierz wszystkie Endpointy
```
kubectl get endpoints
```
 
## Przydatne:

Pod do sprawdzania nslookup
```
kubectl run -it --rm tools --generator=run-pod/v1 --image=giantswarm/tiny-tools
```