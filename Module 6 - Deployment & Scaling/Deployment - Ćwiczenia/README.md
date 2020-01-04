# Tworzenie, zarządzanie i aktualizacje Deployment – Ćwiczenia

### Co się stanie kiedy dodasz Pod spełniający selektor ReplicaSet?

przygotowujemy podstawowy plik deploymentu `basic-deployment.yaml`
```
apiVersion: apps/v1
kind: Deployment
metadata:
  name: deploy-demo
spec:
  replicas: 2
  selector:
    matchLabels:
      app: helloapp
  strategy: {}
  template:
    metadata:
      labels:
        app: helloapp
    spec:
      containers:
      - image: poznajkubernetes/helloapp:svc
        name: helloapp
```

T
```
> kubectl apply -f basic-deployment.yaml

> kubectl get deployment -w
NAME          READY   UP-TO-DATE   AVAILABLE   AGE     
deploy-demo   0/2     0            0           0s      
deploy-demo   0/2     0            0           0s      
deploy-demo   0/2     0            0           0s      
deploy-demo   0/2     2            0           0s      
deploy-demo   1/2     2            1           2s      
deploy-demo   2/2     2            2           2s  

> kubectl get replicaset -w
NAME                   DESIRED   CURRENT   READY   AGE    
deploy-demo-b766d4dd   2         0         0       0s     
deploy-demo-b766d4dd   2         0         0       0s     
deploy-demo-b766d4dd   2         2         0       0s     
deploy-demo-b766d4dd   2         2         1       2s     
deploy-demo-b766d4dd   2         2         2       2s

> kubectl get pod -w
NAME                         READY   STATUS              RESTARTS   AGE
deploy-demo-b766d4dd-p778g   0/1     Pending             0          0s
deploy-demo-b766d4dd-46rgx   0/1     Pending             0          0s
deploy-demo-b766d4dd-p778g   0/1     Pending             0          0s
deploy-demo-b766d4dd-46rgx   0/1     Pending             0          0s
deploy-demo-b766d4dd-p778g   0/1     ContainerCreating   0          0s        
deploy-demo-b766d4dd-46rgx   0/1     ContainerCreating   0          0s        
deploy-demo-b766d4dd-p778g   1/1     Running             0          2s        
deploy-demo-b766d4dd-46rgx   1/1     Running             0          2s  
```

teraz przygotowujemy definicje pod z labelką `app: helloapp`. `basic-helloapp.yaml`:
```
apiVersion: v1
kind: Pod
metadata:
  name: helloapp
  labels:
    app: helloapp
spec:
  containers:
  - image: poznajkubernetes/helloapp:svc
    name: helloapp
    ports:
      - containerPort: 8080
        name: http
        protocol: TCP
```

następnie wrzucamy naszego poda do konfiguracji klastra:
```
> kubectl apply -f basic-helloapp.yaml
> kubectl get pod --show-labels

NAME                         READY   STATUS    RESTARTS   AGE    LABELS
deploy-demo-b766d4dd-46rgx   1/1     Running   0          9m1s   app=helloapp,pod-template-hash=b766d4dd
deploy-demo-b766d4dd-p778g   1/1     Running   0          9m1s   app=helloapp,pod-template-hash=b766d4dd
helloapp                     1/1     Running   0          39s    app=helloapp

> kubectl get replicaset
NAME                   DESIRED   CURRENT   READY   AGE
deploy-demo-b766d4dd   2         2         2       10m
```

widzimy że `pod` spełniający selektor `app: helloapp` z `deployment` został dodany jednak nie zmieniło to niczego w `replicaset`

jeżeli spróbujemy teraz zmienić definicję naszego `deployment` na `replicas: 1` i wgrać ponownie na klaster
```
> kubectl apply -f .\basic-deployment.yaml
deployment.apps/deploy-demo configured
```

widzimy jednak że również takaz zmiana nie wpłynęła na usunięcie "dodatkowego" pod
```
> kubectl get deployment -w
NAME          READY   UP-TO-DATE   AVAILABLE   AGE     
deploy-demo   2/2     2            2           2s      
deploy-demo   2/1     2            2           13m
deploy-demo   2/1     2            2           13m
deploy-demo   1/1     1            1           13m

> kubectl get replicaset -w
NAME                   DESIRED   CURRENT   READY   AGE   
deploy-demo-b766d4dd   2         2         2       2s    
deploy-demo-b766d4dd   1         2         2       13m
deploy-demo-b766d4dd   1         2         2       13m
deploy-demo-b766d4dd   1         1         1       13m

> kubectl get pod -w
NAME                         READY   STATUS    RESTARTS   AGE     
deploy-demo-b766d4dd-p778g   1/1     Running             0          2s       
deploy-demo-b766d4dd-46rgx   1/1     Running             0          2s       
helloapp                     0/1     Pending             0          0s       
helloapp                     0/1     Pending             0          0s       
helloapp                     0/1     ContainerCreating   0          0s       
helloapp                     1/1     Running             0          2s       
deploy-demo-b766d4dd-p778g   1/1     Terminating         0          13m
deploy-demo-b766d4dd-p778g   0/1     Terminating         0          13m

> kubectl get pod
NAME                         READY   STATUS    RESTARTS   AGE
deploy-demo-b766d4dd-46rgx   1/1     Running   0          14m 
helloapp                     1/1     Running   0          6m9s
```

jeżeli spróbujemy teraz zmienić definicję naszego `deployment` na `replicas: 3` i wgrać ponownie na klaster, równie nie przyniesie to żadnych rezultatów.
Spróbujmy zatem jeszcze raz przyjrzeć się `labels` dołączonym do `pod`.

```
> kubectl get pod --show-labels
NAME                         READY   STATUS    RESTARTS   AGE   LABELS
deploy-demo-b766d4dd-46rgx   1/1     Running   0          20m   app=helloapp,pod-template-hash=b766d4dd
deploy-demo-b766d4dd-sqstv   1/1     Running   0          22s   app=helloapp,pod-template-hash=b766d4dd
deploy-demo-b766d4dd-vkwnt   1/1     Running   0          22s   app=helloapp,pod-template-hash=b766d4dd
helloapp                     1/1     Running   0          12m   app=helloapp
```

Widzmimy, że dodatkowo `pody` stworzone przez `deployment` mają przypisaną labelke `pod-template-hash=b766d4dd`, spróbujmy dodać ją do naszego `pod`
```
> kubectl label pod helloapp pod-template-hash=b766d4dd
pod/helloapp labeled
```

Widzimy, że spowodowało to podpięcie `pod: helloapp` do `replicaset`, co automatycznie spowodowało usunięcie jednego z `pod` tak żeby liczba replik zgadzała się z definicją `replicaset`
```
> kubectl get deployment -w
NAME          READY   UP-TO-DATE   AVAILABLE   AGE 
deploy-demo   3/3     3            3           20m
deploy-demo   4/3     4            4           31m
deploy-demo   3/3     3            3           31m

> kubectl get replicaset -w
NAME                   DESIRED   CURRENT   READY   AGE   
deploy-demo-b766d4dd   3         3         3       20m
deploy-demo-b766d4dd   3         4         4       31m
deploy-demo-b766d4dd   3         3         3       31m

kubectl get pod -w
NAME                         READY   STATUS    RESTARTS   AGE
deploy-demo-b766d4dd-sqstv   1/1     Running             0          2s
deploy-demo-b766d4dd-vkwnt   1/1     Running             0          2s
helloapp                     1/1     Running             0          22m
helloapp                     1/1     Running             0          22m
deploy-demo-b766d4dd-vkwnt   1/1     Terminating         0          10m      
```

co się stanie jeżeli usuniemy `label` `app: helloapp` z naszego pod?

```
> kubectl label pod helloapp app-  
pod/helloapp labeled
```

widzimy, że `pod: helloapp` został odpięty od `replicaset` a w brakujące miejsce został stworzony nowy pod

```
> kubectl get deployment -w
NAME          READY   UP-TO-DATE   AVAILABLE   AGE
deploy-demo   3/3     3            3           31m
deploy-demo   2/3     2            2           36m
deploy-demo   2/3     3            2           36m
deploy-demo   2/3     3            3           36m
deploy-demo   3/3     3            3           37m

> kubectl get replicaset -w
NAME                   DESIRED   CURRENT   READY   AGE   
deploy-demo-b766d4dd   3         3         3       31m   
deploy-demo-b766d4dd   3         2         2       36m
deploy-demo-b766d4dd   3         3         2       36m
deploy-demo-b766d4dd   3         3         3       37m

> kubectl get pod -w
NAME                         READY   STATUS    RESTARTS   AGE
deploy-demo-b766d4dd-thldw   0/1     Pending             0          0s
deploy-demo-b766d4dd-thldw   0/1     Pending             0          0s       
deploy-demo-b766d4dd-thldw   0/1     ContainerCreating   0          0s
deploy-demo-b766d4dd-thldw   1/1     Running             0          2s

> kubectl get pod --show-labels    
NAME                         READY   STATUS    RESTARTS   AGE     LABELS
deploy-demo-b766d4dd-46rgx   1/1     Running   0          41m     app=helloapp,pod-template-hash=b766d4dd
deploy-demo-b766d4dd-sqstv   1/1     Running   0          21m     app=helloapp,pod-template-hash=b766d4dd
deploy-demo-b766d4dd-thldw   1/1     Running   0          4m54s   app=helloapp,pod-template-hash=b766d4dd
helloapp                     1/1     Running   0          33m     pod-template-hash=b766d4dd
```

### Jak zadziała minReadySeconds bez readiness i liveliness probes?

### Do czego może Ci się przydać matchExpressions?

### Jak najlepiej (według Ciebie) zarządzać historią zmian w deploymentach?

### Co się stanie jak usuniesz ReplicaSet stworzony przez Deployment?

### Czy Pod może definiować więcej etykiet niż ReplicaSet ma zdefiniowane w selectorze?

### Czy ReplicaSet może definiować więcej etykiet w selektorze niz Pod ma zdefiniowane?