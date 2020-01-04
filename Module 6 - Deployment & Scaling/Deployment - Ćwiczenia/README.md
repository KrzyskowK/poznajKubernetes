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

Widzmimy, że dodatkowo `pody` stworzone przez `deployment` mają przypisaną labelke `pod-template-hash=b766d4dd`

https://kubernetes.io/docs/concepts/workloads/controllers/deployment/#pod-template-hash-label
> The pod-template-hash label is added by the Deployment controller to every ReplicaSet that a Deployment creates or adopts.
This label ensures that child ReplicaSets of a Deployment do not overlap. It is generated by hashing the PodTemplate of the ReplicaSet and using the resulting hash as the label value that is added to the ReplicaSet selector, Pod template labels, and in any existing Pods that the ReplicaSet might have.

spróbujmy zatem dodać ją do naszego `pod`
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

na podstawie poprzedniej definicji deploymentu tworzymy nową z `minReadySeconds: 10` (`basic-deployment-minReadySeconds.yaml`)
```
> kubectl delete deployment deploy-demo
deployment "deploy-demo" deleted
> kubectl apply -f .\basic-deployment-minReadySeconds.yaml
deployment.apps/deploy-demo created
```

widzimy że brak readiness i liveness nie wpłynął na `replicaset`. Deployment po upływie 10 sekund ustawił wszystkie pody w tryb `available`

```
> kubectl get deployment -w
NAME          READY   UP-TO-DATE   AVAILABLE   AGEE                                                 
deploy-demo   0/3     0            0           0s 
deploy-demo   0/3     0            0           0s
deploy-demo   0/3     0            0           0s
deploy-demo   0/3     3            0           0s
deploy-demo   1/3     3            0           2s
deploy-demo   2/3     3            0           2s
deploy-demo   3/3     3            0           2s
deploy-demo   3/3     3            3           12s
```

### Do czego może Ci się przydać matchExpressions?

np. żeby w jawny sposob zapisać że coś nie powinno być zdeployowane na jakieś środowisko

### Jak najlepiej (według Ciebie) zarządzać historią zmian w deploymentach?

Deployment tool powinien dodawać adnotację `kobernetes.io/change-cause` tak żebyśmy mogli to zobaczyć w `rollout history`

### Co się stanie jak usuniesz ReplicaSet stworzony przez Deployment?

```
> kubectl delete replicaset deploy-demo-b766d4dd
replicaset.extensions "deploy-demo-b766d4dd" deleted
```

Usunięcie `replicaset` spowoduję usunięcie wszystkich podległych mu `pod`. 
W tym samym momencie `deployment` odtworzy `replicaset` który kolejno odtworzy wszystkie `pod`.

```
> kubectl get deployment -w
NAME          READY   UP-TO-DATE   AVAILABLE   AGE
deploy-demo   3/3     3            3           9m54s
deploy-demo   0/3     0            0           9m54s
deploy-demo   0/3     3            0           9m54s
deploy-demo   1/3     3            0           9m56s
deploy-demo   2/3     3            0           9m56s
deploy-demo   3/3     3            0           9m57s
deploy-demo   3/3     3            3           10m

> kubectl get replicaset -w
NAME                   DESIRED   CURRENT   READY   AGE   
deploy-demo-b766d4dd   3         3         3       9m54s
deploy-demo-b766d4dd   3         0         0       0s
deploy-demo-b766d4dd   3         0         0       0s
deploy-demo-b766d4dd   3         3         0       0s
deploy-demo-b766d4dd   3         3         1       2s
deploy-demo-b766d4dd   3         3         2       2s
deploy-demo-b766d4dd   3         3         3       3s
deploy-demo-b766d4dd   3         3         3       13s

> kubectl get pod -w
NAME                         READY   STATUS    RESTARTS   AGE
deploy-demo-b766d4dd-bphwp   0/1     Pending             0          0s
deploy-demo-b766d4dd-ts2sp   0/1     Pending             0          0s
deploy-demo-b766d4dd-5rjdg   0/1     Pending             0          0s
deploy-demo-b766d4dd-bphwp   0/1     Pending             0          0s
deploy-demo-b766d4dd-ts2sp   0/1     Pending             0          0s
deploy-demo-b766d4dd-5rjdg   0/1     Pending             0          0s
deploy-demo-b766d4dd-bphwp   0/1     ContainerCreating   0          0s
deploy-demo-b766d4dd-ts2sp   0/1     ContainerCreating   0          0s
deploy-demo-b766d4dd-5rjdg   0/1     ContainerCreating   0          0s
deploy-demo-b766d4dd-b6x8c   1/1     Terminating         0          9m54s
deploy-demo-b766d4dd-wzfll   1/1     Terminating         0          9m54s
deploy-demo-b766d4dd-vdvqh   1/1     Terminating         0          9m54s
deploy-demo-b766d4dd-vdvqh   0/1     Terminating         0          9m55s
deploy-demo-b766d4dd-ts2sp   1/1     Running             0          2s
deploy-demo-b766d4dd-vdvqh   0/1     Terminating         0          9m56s
deploy-demo-b766d4dd-vdvqh   0/1     Terminating         0          9m56s
deploy-demo-b766d4dd-vdvqh   0/1     Terminating         0          9m56s
deploy-demo-b766d4dd-wzfll   0/1     Terminating         0          9m56s
deploy-demo-b766d4dd-5rjdg   1/1     Running             0          2s
deploy-demo-b766d4dd-bphwp   1/1     Running             0          3s
deploy-demo-b766d4dd-b6x8c   0/1     Terminating         0          9m57s
deploy-demo-b766d4dd-wzfll   0/1     Terminating         0          9m58s
deploy-demo-b766d4dd-wzfll   0/1     Terminating         0          9m58s    
deploy-demo-b766d4dd-b6x8c   0/1     Terminating         0          10m
deploy-demo-b766d4dd-b6x8c   0/1     Terminating         0          10m    
```

### Czy Pod może definiować więcej etykiet niż ReplicaSet ma zdefiniowane w selectorze?

tak

### Czy ReplicaSet może definiować więcej etykiet w selektorze niz Pod ma zdefiniowane?

nie, selector zawsze działa jako `AND` w zwiazku z czym wszystkie `labels` muszą być zdefiniowane w `pod`