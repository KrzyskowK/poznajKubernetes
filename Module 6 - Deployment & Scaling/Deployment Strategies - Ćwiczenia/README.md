# Korzystając z wiedzy na temat rolling update przetestuj:

## Działanie trybu Recreate

Przygotowujemy prosty plik deploymentu ze strategią typu recreate. `recreate.yaml`:
```
apiVersion: apps/v1
kind: Deployment
metadata:
  name: recreate
spec:
  replicas: 6
  selector:
    matchLabels:
      app: helloapp
  strategy:
    type: Recreate
  template:
    metadata:
      labels:
        app: helloapp
    spec:
      containers:
      - image: poznajkubernetes/helloapp:svc
        name: helloapp
        env:
         - name: version
           value: v1.0.0
```

Rozpoczynamy wdrożenie
```
> kubectl apply -f .\recreate.yaml
deployment.apps/recreate created

> kubectl get deployment -w
NAME       READY   UP-TO-DATE   AVAILABLE   AGE
recreate   0/6     0            0           0s 
recreate   0/6     0            0           0s
recreate   0/6     0            0           0s
recreate   0/6     6            0           0s
recreate   1/6     6            1           3s
recreate   2/6     6            2           4s
recreate   3/6     6            3           4s
recreate   4/6     6            4           4s
recreate   5/6     6            5           4s
recreate   6/6     6            6           4s

> kubectl get replicaset -w
NAME                 DESIRED   CURRENT   READY   AGE
recreate-bc5f65fd5   6         0         0       0s 
recreate-bc5f65fd5   6         0         0       0s
recreate-bc5f65fd5   6         6         0       0s
recreate-bc5f65fd5   6         6         1       3s
recreate-bc5f65fd5   6         6         2       3s
recreate-bc5f65fd5   6         6         3       4s
recreate-bc5f65fd5   6         6         4       4s
recreate-bc5f65fd5   6         6         5       4s
recreate-bc5f65fd5   6         6         6       4s

> kubectl get pod -w
NAME                       READY   STATUS    RESTARTS   AGE
recreate-bc5f65fd5-c5wkf   0/1     Pending   0          0s 
recreate-bc5f65fd5-c5wkf   0/1     Pending   0          0s 
recreate-bc5f65fd5-jktkc   0/1     Pending   0          0s
recreate-bc5f65fd5-vnj44   0/1     Pending   0          0s
recreate-bc5f65fd5-c5wkf   0/1     ContainerCreating   0          0s
recreate-bc5f65fd5-tn4sg   0/1     Pending             0          0s
recreate-bc5f65fd5-jktkc   0/1     Pending             0          0s
recreate-bc5f65fd5-9qb54   0/1     Pending             0          0s
recreate-bc5f65fd5-vnj44   0/1     Pending             0          0s
recreate-bc5f65fd5-rncz5   0/1     Pending             0          0s
recreate-bc5f65fd5-tn4sg   0/1     Pending             0          0s
recreate-bc5f65fd5-9qb54   0/1     Pending             0          0s
recreate-bc5f65fd5-rncz5   0/1     Pending             0          0s
recreate-bc5f65fd5-jktkc   0/1     ContainerCreating   0          0s
recreate-bc5f65fd5-vnj44   0/1     ContainerCreating   0          0s
recreate-bc5f65fd5-tn4sg   0/1     ContainerCreating   0          0s
recreate-bc5f65fd5-9qb54   0/1     ContainerCreating   0          0s
recreate-bc5f65fd5-rncz5   0/1     ContainerCreating   0          0s
recreate-bc5f65fd5-tn4sg   1/1     Running             0          3s
recreate-bc5f65fd5-9qb54   1/1     Running             0          3s
recreate-bc5f65fd5-vnj44   1/1     Running             0          4s
recreate-bc5f65fd5-jktkc   1/1     Running             0          4s
recreate-bc5f65fd5-c5wkf   1/1     Running             0          4s
recreate-bc5f65fd5-rncz5   1/1     Running             0          4s
```

teraz, zmieniamy w naszym `pod` zmienną środowiskową z `version=v1.0.0` na `version=v2.0.0` i aktualizujemy wdrożenie
```
> kubectl apply -f .\recreate.yaml
deployment.apps/recreate configured

> kubectl get deployment -w
NAME       READY   UP-TO-DATE   AVAILABLE   AGE
recreate   6/6     6            6           2m59s
recreate   6/6     0            6           2m59s
recreate   0/6     0            0           2m59s
recreate   0/6     0            0           3m6s
recreate   0/6     0            0           3m6s
recreate   0/6     6            0           3m6s
recreate   1/6     6            1           3m9s
recreate   2/6     6            2           3m9s
recreate   3/6     6            3           3m10s
recreate   4/6     6            4           3m10s
recreate   5/6     6            5           3m11s
recreate   6/6     6            6           3m11s

> kubectl get replicaset -w
NAME                 DESIRED   CURRENT   READY   AGE
recreate-bc5f65fd5   0         6         6       2m59s
recreate-bc5f65fd5   0         6         6       2m59s
recreate-bc5f65fd5   0         0         0       2m59s
recreate-7db67d9f46   6         0         0       0s
recreate-7db67d9f46   6         0         0       0s
recreate-7db67d9f46   6         6         0       0s
recreate-7db67d9f46   6         6         1       3s
recreate-7db67d9f46   6         6         2       3s
recreate-7db67d9f46   6         6         3       4s
recreate-7db67d9f46   6         6         4       4s
recreate-7db67d9f46   6         6         5       5s
recreate-7db67d9f46   6         6         6       5s

> kubectl get pod -w
NAME                       READY   STATUS              RESTARTS   AGE
recreate-bc5f65fd5-tn4sg   1/1     Running             0          3s
recreate-bc5f65fd5-9qb54   1/1     Running             0          3s
recreate-bc5f65fd5-vnj44   1/1     Running             0          4s
recreate-bc5f65fd5-jktkc   1/1     Running             0          4s
recreate-bc5f65fd5-c5wkf   1/1     Running             0          4s
recreate-bc5f65fd5-rncz5   1/1     Running             0          4s
recreate-bc5f65fd5-jktkc   1/1     Terminating         0          2m59s
recreate-bc5f65fd5-c5wkf   1/1     Terminating         0          2m59s      
recreate-bc5f65fd5-9qb54   1/1     Terminating         0          2m59s      
recreate-bc5f65fd5-rncz5   1/1     Terminating         0          2m59s
recreate-bc5f65fd5-vnj44   1/1     Terminating         0          2m59s
recreate-bc5f65fd5-tn4sg   1/1     Terminating         0          2m59s      
recreate-bc5f65fd5-rncz5   0/1     Terminating         0          3m1s
recreate-bc5f65fd5-tn4sg   0/1     Terminating         0          3m1s
recreate-bc5f65fd5-9qb54   0/1     Terminating         0          3m1s
recreate-bc5f65fd5-vnj44   0/1     Terminating         0          3m1s
recreate-bc5f65fd5-jktkc   0/1     Terminating         0          3m2s
recreate-bc5f65fd5-c5wkf   0/1     Terminating         0          3m2s
recreate-bc5f65fd5-tn4sg   0/1     Terminating         0          3m3s
recreate-bc5f65fd5-tn4sg   0/1     Terminating         0          3m3s       
recreate-bc5f65fd5-vnj44   0/1     Terminating         0          3m3s
recreate-bc5f65fd5-vnj44   0/1     Terminating         0          3m3s
recreate-bc5f65fd5-9qb54   0/1     Terminating         0          3m4s
recreate-bc5f65fd5-9qb54   0/1     Terminating         0          3m4s
recreate-bc5f65fd5-rncz5   0/1     Terminating         0          3m4s
recreate-bc5f65fd5-rncz5   0/1     Terminating         0          3m4s       
recreate-bc5f65fd5-jktkc   0/1     Terminating         0          3m5s
recreate-bc5f65fd5-jktkc   0/1     Terminating         0          3m5s
recreate-bc5f65fd5-c5wkf   0/1     Terminating         0          3m6s
recreate-bc5f65fd5-c5wkf   0/1     Terminating         0          3m6s       
recreate-7db67d9f46-mkb85   0/1     Pending             0          0s
recreate-7db67d9f46-mkb85   0/1     Pending             0          0s
recreate-7db67d9f46-c26qj   0/1     Pending             0          0s
recreate-7db67d9f46-pz67z   0/1     Pending             0          0s        
recreate-7db67d9f46-pz67z   0/1     Pending             0          0s        
recreate-7db67d9f46-frmb6   0/1     Pending             0          0s
recreate-7db67d9f46-c26qj   0/1     Pending             0          0s        
recreate-7db67d9f46-6gnpx   0/1     Pending             0          0s
recreate-7db67d9f46-swsbw   0/1     Pending             0          0s        
recreate-7db67d9f46-frmb6   0/1     Pending             0          0s
recreate-7db67d9f46-swsbw   0/1     Pending             0          0s
recreate-7db67d9f46-6gnpx   0/1     Pending             0          0s        
recreate-7db67d9f46-mkb85   0/1     ContainerCreating   0          1s
recreate-7db67d9f46-pz67z   0/1     ContainerCreating   0          2s
recreate-7db67d9f46-c26qj   0/1     ContainerCreating   0          2s
recreate-7db67d9f46-frmb6   0/1     ContainerCreating   0          3s
recreate-7db67d9f46-pz67z   1/1     Running             0          3s
recreate-7db67d9f46-frmb6   1/1     Running             0          3s
recreate-7db67d9f46-mkb85   1/1     Running             0          4s
recreate-7db67d9f46-c26qj   1/1     Running             0          4s
recreate-7db67d9f46-swsbw   1/1     Running             0          5s
recreate-7db67d9f46-6gnpx   1/1     Running             0          5s
```

widzimy, że w pierwszej kolejności wszystkie `pod` powiązane z `replicaset recreate-bc5f65fd5` zostają wyłączone.
Następnie `deployment` tworzy nowy `replicaset recreate-7db67d9f46` który zaczyna podnosić jednocześnie 6 replik `pod` w nowej wersją.

Widzimy, że w przypadku tej aplikacji downtime - od momentu usunięcia ostatnieg pod w starej wersji do momentu zaraportowania pierwszego pod jako ready - wynosił ok. 10 sekund,
uzyskanie pełnej dostępności zajeło ok 13 sekund.
```
recreate   0/6     0            0           2m59s
recreate   1/6     6            1           3m9s
recreate   6/6     6            6           3m11s
```

## Szybki i wolny rolling update

Przygotowujemy prosty plik deploymentu ze strategią typu rollingupdate. 
Definiujemy maksymalną liczbę nowych instancji na `1` i maksymalną liczbę niedostępnych startych instancji na `0` (tak żeby stara instacja byla usuwana dopiero po tym jak stworzymy jedną nową)

`slow-rolling-update.yaml`:
```
apiVersion: apps/v1
kind: Deployment
metadata:
  name: rollingupdate
spec:
  replicas: 6
  selector:
    matchLabels:
      app: helloapp
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxSurge: 1
      maxUnavailable: 0
  template:
    metadata:
      labels:
        app: helloapp
    spec:
      containers:
      - image: poznajkubernetes/helloapp:svc
        name: helloapp
        env:
         - name: version
           value: v3.0.0
```

dodajemy `deployment` do konfiguracji klastra

```
> kubectl apply -f slow-rolling-update.yaml
deployment.apps/rollingupdate created

> kubectl get deployment
NAME            READY   UP-TO-DATE   AVAILABLE   AGE         
rollingupdate   6/6     6            6           4s 

> kubectl get replicaset -w
NAME                      DESIRED   CURRENT   READY   AGE 
rollingupdate-dbccc5d6c   6         6         6       4s

> kubectl get pod
NAME                            READY   STATUS    RESTARTS   AGE
rollingupdate-dbccc5d6c-jfmpm   1/1     Running             0          3s    
rollingupdate-dbccc5d6c-644lt   1/1     Running             0          3s    
rollingupdate-dbccc5d6c-pzzq2   1/1     Running             0          3s    
rollingupdate-dbccc5d6c-q76tm   1/1     Running             0          3s    
rollingupdate-dbccc5d6c-kxp5d   1/1     Running             0          4s    
rollingupdate-dbccc5d6c-gztnh   1/1     Running             0          4s 
```

teraz, zmieniamy w naszym `pod` zmienną środowiskową z `version=v3.0.0` na `version=v4.0.0` i aktualizujemy wdrożenie

```
> kubectl apply -f slow-rollingdate configured     yment & Scalin-update.yaml             tes\Module 6 - Deplo
deployment.apps/rollingupt Strategies - Ćwiczdate configured

> kubectl get deployment -w
NAME            READY   UP-TO-DATE   AVAILABLE   AGE   
rollingupdate   6/6     2            6           2m33s
rollingupdate   6/6     3            6           2m33s
rollingupdate   7/6     3            7           2m36s
rollingupdate   7/6     4            7           2m36s
rollingupdate   6/6     4            6           2m36s
rollingupdate   7/6     4            7           2m38s
rollingupdate   7/6     5            7           2m38s
rollingupdate   6/6     5            6           2m38s
rollingupdate   7/6     5            7           2m40s
rollingupdate   6/6     5            6           2m40s
rollingupdate   6/6     6            6           2m40s
rollingupdate   7/6     6            7           2m42s
rollingupdate   6/6     6            6           2m42s

> kubectl get replicaset -w
NAME                      DESIRED   CURRENT   READY   AGE
rollingupdate-dbccc5d6c   6         6         6       4s 
rollingupdate-7c79cc54d   1         0         0       0s
rollingupdate-7c79cc54d   1         0         0       0s
rollingupdate-7c79cc54d   1         1         0       0s
rollingupdate-7c79cc54d   1         1         1       2s 
rollingupdate-dbccc5d6c   5         6         6       2m30s
rollingupdate-7c79cc54d   2         1         1       2s
rollingupdate-dbccc5d6c   5         6         6       2m30s
rollingupdate-7c79cc54d   2         1         1       2s 
rollingupdate-dbccc5d6c   5         5         5       2m30s
rollingupdate-7c79cc54d   2         2         1       2s
rollingupdate-7c79cc54d   2         2         2       5s 
rollingupdate-dbccc5d6c   4         5         5       2m33s
rollingupdate-7c79cc54d   3         2         2       5s
rollingupdate-dbccc5d6c   4         5         5       2m33s
rollingupdate-dbccc5d6c   4         4         4       2m33s
rollingupdate-7c79cc54d   3         2         2       5s
rollingupdate-7c79cc54d   3         3         2       5s
rollingupdate-7c79cc54d   3         3         3       8s
rollingupdate-dbccc5d6c   3         4         4       2m36s
rollingupdate-7c79cc54d   4         3         3       8s
rollingupdate-7c79cc54d   4         3         3       8s
rollingupdate-dbccc5d6c   3         4         4       2m36s
rollingupdate-7c79cc54d   4         4         3       8s 
rollingupdate-dbccc5d6c   3         3         3       2m36s
rollingupdate-7c79cc54d   4         4         4       10s
rollingupdate-7c79cc54d   5         4         4       10s
rollingupdate-7c79cc54d   5         4         4       10s
rollingupdate-dbccc5d6c   2         3         3       2m38s
rollingupdate-7c79cc54d   5         5         4       10s
rollingupdate-dbccc5d6c   2         2         2       2m38s
rollingupdate-7c79cc54d   5         5         5       12s
rollingupdate-dbccc5d6c   1         2         2       2m40s
rollingupdate-7c79cc54d   6         5         5       12s
rollingupdate-dbccc5d6c   1         2         2       2m40s
rollingupdate-7c79cc54d   6         5         5       12s
rollingupdate-dbccc5d6c   1         1         1       2m40s
rollingupdate-7c79cc54d   6         6         5       12s
rollingupdate-7c79cc54d   6         6         6       14s
rollingupdate-dbccc5d6c   0         1         1       2m42s
rollingupdate-dbccc5d6c   0         1         1       2m42s
rollingupdate-dbccc5d6c   0         0         0       2m42s

```

widzimy że deployment stworzył nowy `replicaset rollingupdate-7c79cc54d`.
`rollingupdate-7c79cc54d` tworzy jednego nowego `pod` zgodnie z `maxSurge=1`. W momencie gdy nowy `pod` jest `READY` starty  `replicaset rollingupdate-dbccc5d6c` wyłącza pojedynczego pod.
Zgodnie z ustawionym `maxUnavailable=0` widzimy, że nasz deployment posiada podczas rolloutu 7 dostępnych podów na 6 żądanych.

```
> kubectl get deployment -w
NAME            READY   UP-TO-DATE   AVAILABLE   AGE  
rollingupdate   7/6     4            7           2m36s
```

Spróbujmy teraz dokonać aktualizacji deploymentu dla `maxSurge=3` a `maxUnavailable=1` (fast-rolling-update.yaml)

```
> kubectl apply -f .\fast-rolling-update.yaml
deployment.apps/rollingupdate configured

kubectl get deployment -w
NAME            READY   UP-TO-DATE   AVAILABLE   AGE
rollingupdate   6/6     6            6           5s
rollingupdate   6/6     6            6           30s
rollingupdate   6/6     6            6           30s
rollingupdate   6/6     0            6           30s
rollingupdate   6/6     0            6           30s
rollingupdate   5/6     0            5           30s
rollingupdate   5/6     3            5           30s
rollingupdate   5/6     4            5           30s
rollingupdate   6/6     4            6           32s
rollingupdate   6/6     4            6           32s
rollingupdate   6/6     4            6           32s
rollingupdate   5/6     4            5           32s
rollingupdate   5/6     5            5           32s
rollingupdate   6/6     5            6           33s
rollingupdate   6/6     5            6           33s
rollingupdate   6/6     5            6           33s
rollingupdate   5/6     5            5           33s
rollingupdate   5/6     6            5           33s
rollingupdate   6/6     6            6           33s
rollingupdate   6/6     6            6           33s
rollingupdate   5/6     6            5           33s
rollingupdate   6/6     6            6           34s
rollingupdate   6/6     6            6           34s
rollingupdate   5/6     6            5           34s
rollingupdate   6/6     6            6           36s
rollingupdate   6/6     6            6           36s
rollingupdate   5/6     6            5           36s
rollingupdate   6/6     6            6           37s

> kubectl get replicaset -w
NAME                      DESIRED   CURRENT   READY   AGE
rollingupdate-b6f997645   6         6         6       5s
rollingupdate-647885c5db   3         0         0       0s
rollingupdate-b6f997645    5         6         6       30s
rollingupdate-647885c5db   3         0         0       0s
rollingupdate-647885c5db   4         0         0       0s
rollingupdate-b6f997645    5         6         6       30s
rollingupdate-b6f997645    5         5         5       30s
rollingupdate-647885c5db   4         3         0       0s
rollingupdate-647885c5db   4         3         0       0s
rollingupdate-647885c5db   4         4         0       0s
rollingupdate-647885c5db   4         4         1       2s 
rollingupdate-b6f997645    4         5         5       32s
rollingupdate-647885c5db   5         4         1       2s
rollingupdate-b6f997645    4         5         5       32s
rollingupdate-b6f997645    4         4         4       32s
rollingupdate-647885c5db   5         4         1       2s
rollingupdate-647885c5db   5         5         1       2s
rollingupdate-647885c5db   5         5         2       3s
rollingupdate-b6f997645    3         4         4       33s
rollingupdate-647885c5db   6         5         2       3s
rollingupdate-b6f997645    3         4         4       33s
rollingupdate-647885c5db   6         5         2       3s
rollingupdate-b6f997645    3         3         3       33s
rollingupdate-647885c5db   6         6         2       3s
rollingupdate-647885c5db   6         6         3       3s
rollingupdate-b6f997645    2         3         3       33s
rollingupdate-b6f997645    2         3         3       33s
rollingupdate-b6f997645    2         2         2       33s
rollingupdate-647885c5db   6         6         4       4s
rollingupdate-b6f997645    1         2         2       34s
rollingupdate-b6f997645    1         2         2       34s
rollingupdate-b6f997645    1         1         1       34s
rollingupdate-647885c5db   6         6         5       6s
rollingupdate-b6f997645    0         1         1       36s
rollingupdate-b6f997645    0         1         1       36s
rollingupdate-b6f997645    0         0         0       36s
rollingupdate-647885c5db   6         6         6       7s
```

widzimy, że zgodnie z ustawieniami liczba stworzonych podów podczas `rollout` nie przekracza 3 instancji powyżej docelowego stanu
```
NAME                       DESIRED   CURRENT   READY   AGE
rollingupdate-b6f997645    5         5         5       30s
rollingupdate-647885c5db   4         3         0       0s
```
widzimy również, że zgodnie z `maxUnavailable` mamy jednocześnie niedostępny co najwyżej 1 pod
```
NAME            READY   UP-TO-DATE   AVAILABLE   AGE
rollingupdate   5/6     5            5           32s
```

## Zmiany na deployment bez liveness i readiness

przeprowadzone w zadaniu powyżej ⬆️

bez `redinessProbe` i `livenessProbe` pod od momentu utworzenia jest traktowany jako `READY`.
Może to negatywnie wpłynąć na `rollingupdate` ponieważ możemy uzyskać w rzeczywistości więcej niedostępnych podów niż ustawiliśmy

## Zmiany na deployment z działającym readiness

Do template pod dodajemy `readinessProbe` która powinna otrzymać odpowiedź o tym że `container` jest gotowy po 20sec. `rolling-update-rediness-on.yaml`:
```
apiVersion: apps/v1
kind: Deployment
metadata:
  name: rollingupdate
spec:
  replicas: 2
  selector:
    matchLabels:
      app: helloapp
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxSurge: 2
      maxUnavailable: 1
  template:
    metadata:
      labels:
        app: helloapp
    spec:
      containers:
      - image: poznajkubernetes/pkad:blue
        name: helloapp
        ports:
        - containerPort: 8080
          name: http
          protocol: TCP
        readinessProbe:
          httpGet:
            path: /ready
            port: 8080
          initialDelaySeconds: 20
          periodSeconds: 5
          timeoutSeconds: 1
        env:
        - name: version
          value: v9.0.0
```

```
> kubectl apply -f .\rolling-update-rediness-on.yaml    
deployment.apps/rollingupdate created

> kubectl get deployment -w
NAME            READY   UP-TO-DATE   AVAILABLE   AGE 
rollingupdate   2/2     2            2           39s

> kubectl get replicaset -w
NAME                       DESIRED   CURRENT   READY   AGE  
rollingupdate-855b85fb97   2         2         2       43s

> kubectl get pod -w
NAME                             READY   STATUS    RESTARTS   AGE  
rollingupdate-855b85fb97-78bnv   1/1     Running   0          47s
rollingupdate-855b85fb97-lztq4   1/1     Running   0          47s
```

teraz zmieniamy wersję env variable na `version=v.10.0.0`, i aktualizujemy deployment
```
> kubectl apply -f .\rolling-update-rediness-on.yaml    
deployment.apps/rollingupdate configured

> kubectl get deployment -w
NAME            READY   UP-TO-DATE   AVAILABLE   AGE
rollingupdate   2/2     2            2           39s
rollingupdate   2/2     2            2           55s
rollingupdate   2/2     2            2           55s
rollingupdate   2/2     0            2           55s
rollingupdate   1/2     2            1           55s
rollingupdate   2/2     2            2           77s
rollingupdate   2/2     2            2           77s
rollingupdate   1/2     2            1           77s
rollingupdate   2/2     2            2           78s

> kubectl get replicaset -w
NAME                       DESIRED   CURRENT   READY   AGE
rollingupdate-855b85fb97   2         2         2       43s
rollingupdate-79d6bb969c   2         0         0       0s
rollingupdate-855b85fb97   1         2         2       55s
rollingupdate-79d6bb969c   2         0         0       0s 
rollingupdate-855b85fb97   1         2         2       55s
rollingupdate-855b85fb97   1         1         1       55s
rollingupdate-79d6bb969c   2         2         0       0s 
rollingupdate-79d6bb969c   2         2         1       22s
rollingupdate-855b85fb97   0         1         1       77s
rollingupdate-855b85fb97   0         1         1       77s
rollingupdate-855b85fb97   0         0         0       77s
rollingupdate-79d6bb969c   2         2         2       23s

> kubectl get pod -w
NAME                             READY   STATUS    RESTARTS   AGE
rollingupdate-855b85fb97-78bnv   1/1     Running   0          47s
rollingupdate-855b85fb97-lztq4   1/1     Running   0          47s
rollingupdate-79d6bb969c-5rm64   0/1     Pending   0          0s
rollingupdate-79d6bb969c-5rm64   0/1     Pending   0          0s
rollingupdate-79d6bb969c-w56jk   0/1     Pending   0          0s
rollingupdate-79d6bb969c-w56jk   0/1     Pending   0          0s
rollingupdate-855b85fb97-lztq4   1/1     Terminating   0          55s
rollingupdate-79d6bb969c-5rm64   0/1     ContainerCreating   0          0s
rollingupdate-79d6bb969c-w56jk   0/1     ContainerCreating   0          0s
rollingupdate-79d6bb969c-5rm64   0/1     Running             0          1s
rollingupdate-855b85fb97-lztq4   0/1     Terminating         0          56s
rollingupdate-855b85fb97-lztq4   0/1     Terminating         0          57s
rollingupdate-855b85fb97-lztq4   0/1     Terminating         0          57s
rollingupdate-79d6bb969c-w56jk   0/1     Running             0          2s
rollingupdate-79d6bb969c-w56jk   1/1     Running             0          22s
rollingupdate-855b85fb97-78bnv   1/1     Terminating         0          77s
rollingupdate-79d6bb969c-5rm64   1/1     Running             0          23s
rollingupdate-855b85fb97-78bnv   0/1     Terminating         0          79s
rollingupdate-855b85fb97-78bnv   0/1     Terminating         0          80s
rollingupdate-855b85fb97-78bnv   0/1     Terminating         0          80s
```

widzimy, że czas przełączenia pomiędzy nową a starą instancją pod wynosił ~20sec.
`Deployment` musiał zaczekać az nowy `pod (rollingupdate-79d6bb969c)` zaraportuje, że jest `READY` przed wyłączeniem kolejnego startego `pod (rollingupdate-855b85fb97)`
```
rollingupdate   1/2     2            1           55s
rollingupdate   2/2     2            2           77s
rollingupdate   1/2     2            1           77s
rollingupdate   2/2     2            2           78s
```

```
> kubectl describe deployment rollingupdate

...
Type    Reason             Age    From                   Message
----    ------             ----   ----                   -------
Normal  ScalingReplicaSet  7m51s  deployment-controller  Scaled up replica set rollingupdate-855b85fb97 to 2
Normal  ScalingReplicaSet  6m56s  deployment-controller  Scaled up replica set rollingupdate-79d6bb969c to 2
Normal  ScalingReplicaSet  6m56s  deployment-controller  Scaled down replica set rollingupdate-855b85fb97 to 1
Normal  ScalingReplicaSet  6m34s  deployment-controller  Scaled down replica set rollingupdate-855b85fb97 to 0
```

Podsumowjąc: dopiero dodanie `redinessProbe` daje nam pewność że warunek `maxUnavailable` podczas deploymentu bedzie spelniony

## Zmiany na deployment z niedziałającym readiness (podaj np. błędny endpoint do sprawdzania)

Teraz dokonamy zmiany dla template z poprzedniego zadania i ustawimy `readinessProbe` na endpoint który nie istnieje `path: /ready-nope`(`rolling-update-rediness-broken.yaml`)

```
> kubectl apply -f .\rolling-update-readiness-broken.yaml
deployment.apps/rollingupdate configured

> kubectl get deployment -w
NAME            READY   UP-TO-DATE   AVAILABLE   AGE
rollingupdate   2/2     2            2           52s
rollingupdate   2/2     2            2           85s
rollingupdate   2/2     2            2           85s
rollingupdate   2/2     0            2           85s
rollingupdate   2/2     2            2           85s
rollingupdate   1/2     2            1           85s
rollingupdate   1/2     2            1           11m

> kubectl rollout status deployment rollingupdate -w
Waiting for deployment "rollingupdate" rollout to finish: 1 old replicas are 
pending termination...
error: deployment "rollingupdate" exceeded its progress deadline
```

po 600 sekundach (defaultowe ustawienie dla `progressDeadlineSeconds`) deployment zakończył się timeoutem ponieważ nowe pod nie były w stanie przejść do stanu `READY`.
W wyniku timeoutu zostaliśmy tylko z jednym działającym `pod` (jeden został usunięty ze względu na `maxUnavailable=1`)

```
> kubectl get pod
NAME                             READY   STATUS    RESTARTS   AGE
rollingupdate-689fb4ddcd-rv8f7   0/1     Running   0          15m
rollingupdate-689fb4ddcd-s5jk6   0/1     Running   0          15m
rollingupdate-7b4c84ccbb-5dp2v   1/1     Running   0          16m
```

teraz testowo możemy wykonać rollback naszego deploymentu

```
> kubectl rollout undo deployment rollingupdate
deployment.extensions/rollingupdate rolled back
```

```
> kubectl get pod -w
NAME                             READY   STATUS    RESTARTS   AGE
rollingupdate-7b4c84ccbb-5dp2v   1/1     Running   0          21m
rollingupdate-7b4c84ccbb-rmwbj   0/1     Running   0          16s
rollingupdate-7b4c84ccbb-rmwbj   1/1     Running   0          25s

> kubectl get deployment -w
NAME            READY   UP-TO-DATE   AVAILABLE   AGE
rollingupdate   1/2     2            1           21m
rollingupdate   2/2     2            2           21m

> kubectl get replicaset -w
NAME                       DESIRED   CURRENT   READY   AGE
rollingupdate-689fb4ddcd   2         2         0       0s
rollingupdate-7b4c84ccbb   1         1         1       85s
rollingupdate-7b4c84ccbb   1         1         1       21m
rollingupdate-7b4c84ccbb   2         1         1       21m
rollingupdate-689fb4ddcd   0         2         0       19m
rollingupdate-7b4c84ccbb   2         1         1       21m
rollingupdate-7b4c84ccbb   2         2         1       21m
rollingupdate-689fb4ddcd   0         2         0       19m
rollingupdate-689fb4ddcd   0         0         0       19m
rollingupdate-7b4c84ccbb   2         2         2       21m
```

w wyniku rollbacku przywróciliśmy poprzednią wersję `replicaset rollingupdate-7b4c84ccbb ` i ponownie mamy dostępne dwie instancję `pod`