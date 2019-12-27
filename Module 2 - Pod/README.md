# Intro

## Pod

- basic work item in kubernetes
- usually contains single docker image besisdes adapter/sidecar pattern
- efemeryczne (spelnia to zasade 12factor app - Disposability)

## Container in Pod

- share same IP address
- share same port range
- share same volume

- from the outside we have single ip address and separte ports that leads to an application

## Commands

utworzenie pod z pliku z zapamiętaną konfiguracją
```
kubectl create -f pod.yaml --save-config=true
```

utworzenie lub update pod na podstawie pliku
```
kubectl apply -f pod.yaml
```

podmiana wersji
```
kubectl replace -f pod.yaml
```

pobranie definicji pod
```
kubectl get pod NAME -o yaml|json
```

pobranie pod z watched
```
kubectl get pods -w
```

usunięcie pod na podstawie pliku z definicją
```
kubectl delete -f pod.yaml
kubectl delete pod NAME
```