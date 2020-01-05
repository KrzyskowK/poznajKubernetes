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


## Zmiany na deployment bez liveness i readiness


## Zmiany na deployment z działającym readiness


## Zmiany na deployment z niedziałającym readiness (podaj np. błędny endpoint do sprawdzania)