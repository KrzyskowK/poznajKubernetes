# Stateful Apps

https://kubernetes.io/docs/tutorials/stateful-application/basic-stateful-set/#cascading-delete

## StatefulSet

template:
```
apiversion: apps/v1
kind: StatefulSet
metadata:
  name: web
spec:
  podManagementPolicy: "OrderedReady"
  replicas: 3
  selector:
    matchlabels:
      app: nginx
  template:
    metadata:
      labels:
        app:nginx
    spec:
      containers:
      - name: nginx
        image: nginx
        ports:
        - containerPort: 80
          name: web
```

```
kubectl get statefulset
```

`podManagementPolicy` - mówi w jaki sposób powinne być tworzone pod. Do wyboru mamy `OrderedReady` (default) i `Parallel`

## hedless services

- idealny do wystawienia aplikacji stanowej 
- pozwala na `Stable Network Identities` - każdy pod ma stałą nazwę sieciową `<nazwa statefulset>-<index>.<headless svc>`
- serwis bez clusterIp
- nie uzywa kube-proxy
- zwraca adresy ip podw

```
apiVersion: v1
kind: Service
metadata:
  name: nginx
  labels:
    app: nginx
spec:
  ports:
  - port: 80
    name: web
  clusterIP: None
  selector:
    app: nginx
```

## skalowanie

scaling up - zwiekszanie liczby replik pod po podzie (tak samo jak tworzenie)

scaling down - zmniejszanie liczby replik pod po podzie (zdejmujemy od najwyższego indexku do najniższego)

```
kubectl scale statefulset web --replicas=5
```

## StatefulSet Update Strategy

- rollingUpdate
- onDelete

```
apiversion: apps/v1
kind: StatefulSet
metadata:
  name: web
spec:
  updateStrategy:
    type: RollingUpdate
...
```

⚠️ nie możemy zaktualizować update strategy więc musimy najpier usunąć nasz statefulset, jeżeli chcemy uniknąć kasowania pod musimy uruchomic polecenie usunięcia wraz z "cascade==false"

```
kubectl delete statefulset web --cascade=false
statefulset.apps "web" deleted
```

## wolumeny

- statefulset potrafi samodzielnie tworzyc PersistenVolumeClaim

```
apiversion: apps/v1
kind: StatefulSet
metadata:
  name: web
spec:
  volumeClaimTemplates:
  -metadata:
    name: data
  spec:
    storageClassName: someClasee
    accessModes: [ "ReadWriteOnce"]
    resources:
      requests:
        storage: 10Mi
...
```

## Bazy danych w kubernetes

https://cloud.google.com/blog/products/databases/to-run-or-not-to-run-a-database-on-kubernetes-what-to-consider

do deploymentu baz danych używamy zawsze `statefulset` nigdy `deployment`. Wyjątkiem są bazy służące do cachowani jak: Redis/Memcache

baza danych musi zawsze byc w wiecej niz jedenj instancji zeby zapewnic wysoka dostepnosc

do rzpechowywania plików stosujemy wolumeny, zeby dalo sie odyzyskac dane w przypadku faila

baza kuberenes friendly:
- posiada autofailover
- przezywa restarty
- autoreplikacja
- sharding
Przykładowe bazy:
- elasticsearch
- cassandra
- mongoDB
  

### Przykład stawiania mongodb:

najłatwiej poprzez `helm`
```
helm install pk-demo stable/mongodb -f https://raw.githubusercontent.com/helm/charts/master/stable/
 mongodb/values-production.yaml
```