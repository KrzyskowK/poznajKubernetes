# DaemonSet

służy do umieszczenia 1 instacji pod na każdym węźle
zastosowania:
- pody zbierajace i przesyłanie logów
- pody odpowiedzialne za sieć (Network Plugin)
- reverse proxy dla service mesh (Linkerd2)
- pody z ingress controller

template:

```
apiVersion: apps/v1
kind: DaemonSet
metadata:
  name: daemon-set
spec:
  selector:
    matchLabels:
      name: <pod-label>
  template: # template of replicaset
    metadata:
      labels: <pod-label>
    spec:
      containers:
      - name: <pod-name>
        image: <image>
```

### update strategy

`rollingUpdate` - stary pod jest kasowany a w jego miejsce wstawiany nowy
```
apiVersion: apps/v1
kind: DaemonSet
metadata:
  name: daemon-set
spec:
  updateStrategy:
    type: RollingUpdate
    rollingUpdate:
      maxUnavailable: 2
      minReadySeconds: 30
```

`onDelete` - nowa konfiguracja zaaplikowana dopiero gdy ręcznie usuniemy pod
```
apiVersion: apps/v1
kind: DaemonSet
metadata:
  name: daemon-set
spec:
  updateStrategy:
    type: OnDelete
```


### rollout

```
kubectl rollout history daeomnset <NAME>
kubectl rollout history daeomnset <NAME> --revision=<REVISION>
kubectl rollout undo daeomnset <NAME> --to-revision=<REVISION>
```

