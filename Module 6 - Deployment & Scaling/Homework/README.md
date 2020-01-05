## Skoro wiesz jak działa ReplicaSet i Deployment:

### 1. Stwórz Deployment dla Podów które w poprzednim module Twoje serwisy udostępniały
### 2. Spróbuj to zrobić deklaratywnie i imperatywnie – Deployment jak i Serwis

Imperatywnie:
```
> kubectl create deployment pkad --image=poznajkubernetes/pkad:blue 
```

Deklaratywnie:
```
apiVersion: apps/v1
kind: Deployment
metadata:
  creationTimestamp: null
  labels:
    app: pkad
  name: pkad
spec:
  replicas: 1
  selector:
    matchLabels:
      app: pkad
  strategy: {}
  template:
    metadata:
      creationTimestamp: null
      labels:
        app: pkad
    spec:
      containers:
      - image: poznajkubernetes/pkad:blue
        name: pkad
        resources: {}
status: {}
```

```
> kubectl get deployment
NAME   READY   UP-TO-DATE   AVAILABLE   AGE 
pkad   1/1     1            1  
```

### 3. Zeskaluj aplikację do 3 replik
```
> kubectl scale deployment pkad --replicas=3 --record
deployment.extensions/pkad scaled

> kubectl get deployment 
NAME   READY   UP-TO-DATE   AVAILABLE   AGE  
pkad   3/3     3            3           3m59s
```

### 4. Zeskaluj aplikację do 1 repliki jeżeli aktualnie ma ona 3 działające repliki

```
> kubectl scale deployment pkad --replicas=1 --current-replicas=3 --record
deployment.extensions/pkad scaled

> kubectl get deployment 
NAME   READY   UP-TO-DATE   AVAILABLE   AGE  
pkad   1/1     1            1           6m19s
```

### 5. Wyciągnij historię pierwszego deploymentu

```
> kubectl rollout history deploy pkad --revision=1    
deployment.extensions/pkad with revision #1
Pod Template:
  Labels:       
    app=pkad
    pod-template-hash=747d9bf94c
  Annotations:  kubernetes.io/change-cause: kubectl.exe scale deployment pkad --replicas=1 --current-replicas=3 --record=true
  Containers:
   pkad:
    Image:       poznajkubernetes/pkad:blue
    Port:        <none>
    Host Port:   <none>
    Environment: <none>
    Mounts:      <none>
  Volumes:       <none>
```
note: mamy tylko jeden rollout ponieważ skalowanie ustawialismy imperatywnie

### 6. Zweryfikuj, że deployment się udał

```
> kubectl rollout status deploy pkad
deployment "pkad" successfully rolled out
```

### 7. Zastanów się ile dodatkowych sekund potrzebuje Twoja aplikacja by poprawnie wystartować

Jeżeli mówimy o "rzeczywistym" projekcie, to trzeba byloby się przyjrzeć każdej z aplikacji z osobna.
Myśle że `initialDelaySeconds: 10` na `redinessProbe` byłby wystarczający dla większości.