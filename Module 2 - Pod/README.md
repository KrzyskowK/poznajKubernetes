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

## Imperative work with k8s and pods

utworzenie definicji Pod i zapisanie go w pliku pod.yaml
```
kubectl run NAME --image=IMAGE --generator=run-pod/v1  --dry-run -o yaml > pod.yaml
```

`kubectl run pkdemoapp --image=pk.demoapp:0.0.14-alpine-selfcontained  --generator=run-pod/v1  --dry-run -o yaml > pod.yaml`


## Pod Phases

- Pending - zostanie przydzielony do noda, zaciągane obrazy, tworzona sieć
- Running - kontenery uruchomione i przynajmniej jeden już działa
- Succeeded - kiedy wszystkie kontener zakonczyl prace
- Failed - kiedy jeden z kontenerow zakonczyl sie z niepowodzeniem
- Unknown - nie można bylo uzyskać stanu PODa

## Pod Status

- PodScheduled - pod przypisany do nodea
- Ready - gotowy do przyjecia ruchu
- Initialized - wszystkie kontenery typu init wystartowaly
- Unschedulable - nie mozna przypisac z powodu braku zasobow
- ContainersReady - wszystkie contenery wystartowaly i sa gotowa

## Restart Policy

- Always - zawsze, niezaleznie od wyniku zakonczenia pracy kontenera
- OnFailure - w momencie gdy kontener sie wywaly
- Never - nigdy nie restartuj

- CrashLoopBackOff - mechanizm opozniajacy zapetlone restrtowanie PODa

## How to check pod statuses?

```
kubectl describe pod <NAME>
kubectl get pod <NAME> -o yaml
```

## InitContainer

- można użyć żeby dociągnąc np. statyczne dane przed uruchomieniem wlasciwego contenera
- init kontenery startują szeregowo
- po zakończeniu pracy ostatniego initContainera nastepuje uruchomienie containerow

## Volume:EmptyDir

- typ volumenu w kubernetess
- jest trzymany na nodzie
- przezywa restarty podow

## Lifecycle

- postStart (po uruchomieniu kontenera)
- preStop (przed ubiciem kontenera)