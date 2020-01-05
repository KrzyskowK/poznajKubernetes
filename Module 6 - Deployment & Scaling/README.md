# Deployment & Scaling

## ReplicaSet
- podstawowy obiekt zarzdzajcy tworzeniem/usuwaniem/skalowanie podów
- template w replicaset jest niemutowalny (do czasu ubicia poda) - w związku z czym nie da się łątwo np. podbic wersji obrazu

definicja:
```
apiVersion: apps/v1
kind: ReplicaSet
metadata:
  name: replicaset-demo
spec:
  replicas: 3
  selector:
    matchLabels:
      app: nginx
      env: prod
  template:
    metadata:
      labels:
        app: nginx
        env: prod
    spec:
      containers:
      - name: nginx
        image: nginx:blue
        ports:
        - containerPort: 80
```

pobranie replica set
```
kubectl get replicaset
```

## Deployment

- zarządza podami za pomocą replicaset (daje możliwość kontrolowanego updatu aplikacji)
- rowzwiązuje problem niemutowalności templatów replicaset
- umożliwia rollback

### jak działa deployment?

- deployment tworzy nowy replicaSet-v2
- deployment wykonuje skalowanie (in) na startej replicaSet-v1 i (out) na nowej replicaSet-v2
- (mechanizm może się różnić w zależności od strategii updatu)

### typy deployment strategy: recreate

- usuwa instancje poprzedniej wersji (skalujac instance do zera) i tworzy nowe replica set
- dobre dla aplikacji które nie mogą działać współbierznie w nowej i starej wersji

```
spec:
  strategy
    type: Recreate
```

### typy deployment strategy: rollingupdate

- deployment w stylu zero downtime
- `maxSurge` - ile pod (nowych) maksymalnie może być dodawanych na raz
- `maxUnavailable` - ile pod (starych) może być niedostępnych przy wdrozeniu na raz
```
spec:
  strategy
    type: RollingUpdate
    rollingUpdate:
      maxSurge: 1 albo ilość %
      maxUnavailable: 1 albo ilość %
```

### zarządzanie deploymentem

Szybkie tworzenie deploymentu:
```
> kubectl run deployx --restart=Always --replicas=3 --image=nginx --dry-run -o yaml
> kubectl create deployment deployx --image=nginx --save-config --dry-run -o yaml
```
pobranie deploymentow:
```
kubectl get deployment
```

definicja:
```
apiVersion: apps/v1
kind: Deployment
metadata
  name: deployment-demo
spec:
  minReadySeconds: 3 # minimalna liczba sekund ile kontener musi dzialac bez crasha zeby byc uznanym za żywy
  progressDeadlineSeconds: 10 # jeżeli przez 10 sekund nie ma zmiany stanu to musimy zakończyć deployment
  replicas: 3
  revisionHistoryLimit: 3
  selector:
    matchLabels:
      app: nginx
      env: prod
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxSurge: 1
      maxUnavailable: 0
  template:
    metadata:
      labels:
        app: nginx
        env: prod
    spec:
      containers:
      - name: nginx
        image: nginx:blue
        ports:
        - containerPort: 80
```

### rollout

- `kubectl rollout status` - do sprawdzenia statusu wdrożenia
- rollout zawsze przypisuje nowy numer revision do wydawanego deploymentu
- mozemy dodac do adnotacji `kubernetes.io/change-cause: <text>` dzieki czemu zostanie to dodane do rollout history jako `change-cause`

status
```
kubectl rollout status deployment <NAME>
```
zatrzymanie/wznowienie
```
kubectl rollout pause deployment <NAME>
kubectl rollout resume deployment <NAME>
```
historia wdrożenia
```
kubectle rollout history deploy <deploymentName>
```
wycofanie wdrożenia
```
kubectl rollout undo deploy <deploymentName> 
kubectl rollout undo deploy <deploymentName> --to-revision=<id z historii>
```

