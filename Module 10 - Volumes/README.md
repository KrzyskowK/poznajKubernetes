# Volume

## do czego używamy?

### komunikacja i synchronizacja
- komunikacja między dwoma kontenerami (jeden produkuje logi, drugi konsumuje i je wysyła)
- przykład `emptyDir` - żyje tak długo jak POD

### cache
- typowy przykład: generowanie thumbnails
- przykład `emptyDir` - żyje tak długo jak POD

### host filesystem
- `hostPath` - żyje tak długo jak NODE
- gdy potrzebujemy diagnostyki noda, 
- gdy potrzebujemy rozwiązania do replikacji danych
- podłaczenie wydajnych dyskow (NoSQL)

### persistant data
- wszystko co musi przetrwać śmierć POD i NODE
- np. bazy danych
- 3rd party vendo

## Rodzaje Volumenów

specificzne dla kubernetes (nie zależne od vendorów)
- `emptyDir` - trzymane na POD
- `hostPath` - trzymane na NODE
- `configMap`
- `secret`
- `persistentVolumeClaim` - trzymane na fizycznym dysku poza klastrem


## PersistentVolumes

- zasób umożliwiający dostęp do fizycznych dysków
- ⚠️ nie znajdują się w żadnym namespace!

template:
```
apiVersion: v1
kind: PersistentVolume
metadata:
  name: pkad-pv
  labels:
    some: label
spec:
  storageClassName: ""
  capacity:
    storage: 10Mi
  persistentVolumeReclaimPolicy: Retain
  accessModes:
  - ReadWriteOnce
  hostPath:
    path: /tmp/pkadhtml
```

```
kubectl get pv
```
### PersistentVolumes - storageClassName

### PersistentVolumes - capacity
-

### PersistentVolumes - persistentVolumeReclaimPolicy
- co ma się stać z dyskiem gdy nie jest już potrzebny (gdy usuwamy Claim)
- mamy dostępne `Retain`, `Recycle`, `Delete`

`Recycle` - może zostać użyty przez inny PersistentVolumeClaim

`Retain` - zatrzymujemy dysk, nikt inny nie może go użyć

`Delete` - usuwamy dysk zaraz po odmonotwaniu

### PersistentVolumes - accessModes
- określa jak dysko może być używany przez nody
- mamy dostępne: `ReadWriteOnce`, `ReadWriteMany`, `ReadOnlyMany`

`ReadWriteOnce` - the volume can be mounted as read-write by a single node

`ReadWriteMany` - the volume can be mounted as read-write by many nodes

`ReadOnlyMany` - the volume can be mounted read-only by many nodes

## PersistentVolumeClaim

template:
```
apiVersion: v1
kind: PersistentVolumeClaim
metadata:
  namespace: test
  name: pkad-pvc
  labels:
    app: test
spec:
  storageClassName: ""
  selector:
    matchLabels:
      some: label
  accessModes:
  - ReadWriteOnce
  resources:
    requests:
      storage: 1Mi
```

```
kubectl get pvc
```

### dodawanie claim do POD

```
...
volumes:
- name: pkad-data
  persistentVolumeClaim:
    claimName: pkad-pvc
```

## StorageClass in PersistenVolumeProvisioner

- `storageClassName==""` - nie wykorzystujemy provisionera
- `storageClassName=="pk-sc"` - spróbujemy wykorzystać provisioner opisany jako "pk-sc"
- brak `storageClassName` - wybieramy domyślny storageClassName

```
kubectl get storageclass
```

template dla storageClass
```
apiVersion: storage.k8s.io/v1
kind: StorageClass
metadata:
  name: pk-sc
provisioner: docker.io/hostpath
reclaimPolicy: Retain
allowVolumeExpansion: true
```