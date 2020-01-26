
# Persistent Volumes – Ćwiczenia

## Przepisz ćwiczenie z Wolumeny w Pod – Ćwiczenia tak by zostały wykorzystane wolumeny za pomocą:

---

### Użyj PersistentVolume i PersistentVolumeClaim aby zcacheować repozytorium git za pomocą InitContainer

tworzymy definicje PersistenceVolume
```
apiVersion: v1
kind: PersistentVolume
metadata:
  name: demo-pv
  labels:
    purpose: demo
spec:
  storageClassName: ""
  capacity:
    storage: 10Mi
  persistentVolumeReclaimPolicy: Retain
  accessModes:
  - ReadWriteOnce
  hostPath:
    path: /d/k8s_vol/persistent_volume/retain
```
dodajemy definicję PersistentVolumeClaim który pozwoli nam zamontować dysk jako volumen. 
Za pomocą `matchLabels` wskazujemy `label` zgody z naszym PersistenVolumne, upewniamy się również że `accessModes` w PersistentVolumeClaim i PersistenVolume są zgodne
```
apiVersion: v1
kind: PersistentVolumeClaim
metadata:
  name: demo-pvc
spec:
  storageClassName: ""
  selector:
    matchLabels:
      purpose: demo
  accessModes:
  - ReadWriteOnce
  resources:
    requests:
      storage: 0.5Mi
```
dodajemy definicję pod która w InitContainer wyciągnie repozytorium git i zapisze pliki na PersistentVolume, a następnie podmontuje i udostępni index.html przez instancję nginx
```
apiVersion: v1
kind: Pod
metadata:
  name: web-pv-static
spec:
  initContainers:
    - image: ubuntu:latest
      name: setup
      volumeMounts:
        - name: workdir
          mountPath: /repo
      command: ["/bin/sh", "-c"]
      args:
        - apt-get update;
          apt-get install -y git;
          git clone "https://github.com/KrzyskowK/poznajKubernetes.git" /repo;
  containers:
    - image: nginx:1.17.6
      name: web
      ports:
        - containerPort: 80
      volumeMounts:
        - name: workdir
          mountPath: /usr/share/nginx/html
  volumes:
    - name: workdir
      persistentVolumeClaim:
        claimName: demo-pvc
```
wgrywamy konfiguracją na klaster:
```
> kubectl apply -f .\pv-pvc-static.yaml
persistentvolume/demo-pv created
persistentvolumeclaim/demo-pvc created
pod/web-pv-static created
```
widzimy, że jako pierwsze zostaje stworzony PVC, który w momenci pojawienia się PV zmienai status na `Bound`
```
> kubectl get pvc
NAME       STATUS   VOLUME    CAPACITY   ACCESS MODES   STORAGECLASS   AGE
demo-pvc   Bound    demo-pv   10Mi       RWO                           13s

> kubectl get pv
NAME      CAPACITY   ACCESS MODES   RECLAIM POLICY   STATUS   CLAIM              STORAGECLASS   REASON   AGE
demo-pv   10Mi       RWO            Retain           Bound    default/demo-pvc                           11s
```
widzimy, że pod został poprawnie uruchomiony
```
> kubectl get pod web-pv-static -o wide
NAME            READY   STATUS    RESTARTS   AGE     IP           NODE             NOMINATED NODE   READINESS GATES
web-pv-static   1/1     Running   0          2m34s   10.1.1.209   docker-desktop   <none>           <none>
```
sprawdzamy, czy repo zostało poprawnie wyciągniete i nasz nginx serwuje teraz https://github.com/KrzyskowK/poznajKubernetes/blob/master/index.html
```
> kubectl port-forward web-pv-static 8080:80

> curl localhost:8080
<h1>Ahoj! 🚢📦🏴‍☠️</h1>
```
sukces. Sprawdzimy teraz co się stanie jeżeli uruchomimy następny pod z volume przypiętym do tego samego pvc
```
apiVersion: v1
kind: Pod
metadata:
  name: web-pv-static-2
spec:
  containers:
    - image: nginx:1.17.6
      name: web-pv-static-2
      ports:
        - containerPort: 80
      volumeMounts:
        - name: workdir
          mountPath: /usr/share/nginx/html
  volumes:
    - name: workdir
      persistentVolumeClaim:
        claimName: demo-pvc
```
widzimy że nowo dodany pod `web-pv-static-2` jest w stanie poprawnie odczytać index.html z istniejącego PV
```
> kubectl apply -f .\pv-pvc-static-pod-attched-after.yaml
pod/web-pv-static-2 created
> kubectl port-forward web-pv-static-2 8080:80
Forwarding from 127.0.0.1:8080 -> 80
Forwarding from [::1]:8080 -> 80

> curl localhost:8080
<h1>Ahoj! 🚢📦🏴‍☠️</h1>
```
sprawdźmy co się stanie jeżeli dodamy teraz pod który będzie próbował nadpisać `index.html` w initContainer
```
apiVersion: v1
kind: Pod
metadata:
  name: web-pv-static-3
spec:
  initContainers:
    - image: ubuntu:latest
      name: setup
      volumeMounts:
        - name: workdir
          mountPath: /repo
      command: ["/bin/sh", "-c"]
      args:
        - echo "hello !!!" > /repo/index.html;
  containers:
    - image: nginx:1.17.6
      name: web
      ports:
        - containerPort: 80
      volumeMounts:
        - name: workdir
          mountPath: /usr/share/nginx/html
  volumes:
    - name: workdir
      persistentVolumeClaim:
        claimName: demo-pvc
```
Widzimy, że index został `index.html` został poprawnie nadpisany w PV, widzimy również, że ma to efekt dla wszyztkich pod które hostowały ten sam `index.html`
```
> kubectl apply -f pv-pvc-static-overwrite.yaml
pod/web-pv-static-3 created

> kubectl port-forward web-pv-static-3 8080:80
> curl localhost:8080
hello !!!

> kubectl port-forward web-pv-static-2 8080:80
> curl localhost:8080
hello !!!

> kubectl port-forward web-pv-static 8080:80
> curl localhost:8080
hello !!!
```
---

### Użyj PersistentVolume i PersistentVolumeClaim aby dostarczyć plik konfiguracyjny za pomocą subpath

Na początek spróbujmy ponownie użyć istniejącego PV `demo-pv`, stwórzmy jedynie nowy PersistentVolumeClaim `demo-pv-subpath`.
Widzimy jednak, że nowy PVC nie zamontuje się automatycznie do PV
```
> kubectl get pvc
NAME               STATUS    VOLUME    CAPACITY   ACCESS MODES   STORAGECLASS   AGE
demo-pvc           Bound     demo-pv   10Mi       RWO                           104m
demo-pvc-subpath   Pending                                                      53s
```
Jeżeli sprawdzimy eventy `demo-pvc-subpath` dowiemy się, że `no persistent volumes available for this claim`.
Dzieje się tak ponieważ jeden statyczny PV może być w danej chwili wykorzystywany przez jedno PVC. Dodatkow `demo-pvc` zostało stworzone z `reclaimPolicy: retain`
co oznacz że nawet po usunięciu obecnie z-boundowaneg claima `demo-pvc` nie będziemy mogli się do niego podłączyć dopóki nie zostanie on manualnie wyczyszczony
```
> kubectl describe pvc demo-pvc-subpath
Name:          demo-pvc-subpath
Namespace:     default
Status:        Pending
Volume:
Labels:        <none>
Annotations:   kubectl.kubernetes.io/last-applied-configuration:
                 {"apiVersion":"v1","kind":"PersistentVolumeClaim","metadata":{"annotations":{},"name":"demo-pvc-subpath","namespace":"default"},"spec":{"a...
Finalizers:    [kubernetes.io/pvc-protection]
Capacity:
Access Modes:
VolumeMode:    Filesystem
Events:
  ----       ------         ----              ----                         -------
  Normal     FailedBinding  3s (x8 over 83s)  persistentvolume-controller  no persistent volumes available for this claim and no storage class is set
Mounted By:  web-pv-static-subpath
```

tworzymy zatem nowy PersistentVolume `demo-pv-subpath`, aktualizujemy PersistentVolumeClaim `demo-pv-subpath` tak aby korzystał z `demo-pv-subpath`.
Następnie w InitContainer naszego pod stworzymy nowy plik `index.html` który następnie zamontujemy jako `subpath` do wlaściwego kontenera
```
apiVersion: v1
kind: Pod
metadata:
  name: web-pv-static-subpath
spec:
  initContainers:
    - image: ubuntu:latest
      name: setup
      volumeMounts:
        - name: workdir
          mountPath: /tempdir
      command: ["/bin/sh", "-c"]
      args:
        - echo "hello from subpath!!!" > /tempdir/index.html;
  containers:
    - image: nginx:1.17.6
      name: web
      ports:
        - containerPort: 80
      volumeMounts:
        - name: workdir
          mountPath: /usr/share/nginx/html/index.html
          subPath: index.html
  volumes:
    - name: workdir
      persistentVolumeClaim:
        claimName: demo-pvc-subpath
```
Dodajemy konfigurację do klastra, widzimy że nasz PVC został podłączony do PV.
```
> kubectl apply -f .\pv-pvc-static-subpath.yaml
persistentvolume/demo-pv-subpath created
persistentvolumeclaim/demo-pvc-subpath configured
pod/web-pv-static-subpath created

> kubectl get pvc
NAME               STATUS   VOLUME            CAPACITY   ACCESS MODES   STORAGECLASS   AGE
demo-pvc-subpath   Bound    demo-pv-subpath   10Mi       RWO                           5m34s
```
Sprawdzamy czy nasz `index.html` został odpowiedni podmontowany jako subpath
```
> kubectl port-forward web-pv-static-subpath 8080:80
> curl localhost:8080
hello from subpath!!!
```
---

### PersistenVolumeProvisioner aby zcacheować repozytorium git za pomocą InitContainer

Zaczynamy od stworzenia `storageClass` która zapewni nam dostęp do defaultowego provisionera dostępnego w k8s docker for windows
```
> kubectl get sc 
NAME                 PROVISIONER          AGE
hostpath (default)   docker.io/hostpath   6d17h
```
Template naszego storage class:
```
apiVersion: storage.k8s.io/v1
kind: StorageClass
metadata:
  name: demo-sc
provisioner: docker.io/hostpath
reclaimPolicy: Retain
allowVolumeExpansion: true
```
następnie tworzymy nowy PVC który będzie z niej korzystał
```
apiVersion: v1
kind: PersistentVolumeClaim
metadata:
  name: demo-pvc-sc
spec:
  storageClassName: "demo-sc"
  accessModes:
  - ReadWriteOnce
  resources:
    requests:
      storage: 0.5Mi
```
Na koniec modyfikujemy definicje naszego pod z pierwszego zadania, tak aby korzystał z volume `demo-pvc-sc`.
Następnie Dodajemy konfigurację do klastra
```
> kubectl apply -f .\sc-pvc-dynamic.yaml
storageclass.storage.k8s.io/demo-sc created
persistentvolumeclaim/demo-pvc-sc created
pod/web-pv-dynamic created
```
Widzimy, że nasz PVC został z-boundowany do PV, w odróznieniu od statycznych PVC widzimy że przypisane mu został jedynie zarequestowane 0.5Mi
```
> kubectl get pvc
NAME               STATUS   VOLUME                                     CAPACITY   ACCESS MODES   STORAGECLASS   AGE
demo-pvc           Bound    demo-pv                                    10Mi       RWO                           165m
demo-pvc-sc        Bound    pvc-050e4fb5-4043-11ea-b471-00155d006a01   512Ki      RWO            demo-sc        20s
demo-pvc-subpath   Bound    demo-pv-subpath                            10Mi       RWO                           62m
```
Potwierdzamy, że POD zadziałał zgodnie z oczekiwaniami
```
> kubectl port-forward web-pv-dynamic 8080:80

> curl localhost:8080
<h1>Ahoj! 🚢📦🏴‍☠️</h1
```
---

### PersistenVolumeProvisioner aby dostarczyć plik konfiguracyjny za pomocą subpath

Tworzymy PVC który użyje ponownie storageClass `demo-sc` stworzonej w poprzednim zadaniu
```
apiVersion: v1
kind: PersistentVolumeClaim
metadata:
  name: demo-pvc-sc-subpath
spec:
  storageClassName: "demo-sc"
  accessModes:
  - ReadWriteOnce
  resources:
    requests:
      storage: 0.5Mi
```
delikatnie modyfikujemy POD użyty powyżej dla przykładu ze statycznym PV-PVC żeby korzystał z volume `demo-pvc-sc-subpath`
i wgrywamy konfiguracją na klaster
```
> kubectl apply -f .\sc-pvc-dynamic-subpath.yaml
persistentvolumeclaim/demo-pvc-sc-subpath created
pod/web-pv-dynamic-subpath created
```
widzimy, że nasz PVC został bez problemu przypięty do już wykorzystywanego storageClass
```
> kubectl get pvc
NAME                  STATUS   VOLUME                                     CAPACITY   ACCESS MODES   STORAGECLASS   AGE
demo-pvc              Bound    demo-pv                                    10Mi       RWO                           3h14m
demo-pvc-sc           Bound    pvc-050e4fb5-4043-11ea-b471-00155d006a01   512Ki      RWO            demo-sc        29m
demo-pvc-sc-subpath   Bound    pvc-1544348f-4047-11ea-b471-00155d006a01   512Ki      RWO            demo-sc        19s
demo-pvc-subpath      Bound    demo-pv-subpath                            10Mi       RWO                           91m
```
Potwierdzamy, że nasz index.html został dodany za pomocą subpath jest hostowany przez nginx
```
> kubectl port-forward web-pv-dynamic-subpath 8080:80

> curl localhost:8080
hello from subpath!!!
```

---

### Odpowiedz sobie na pytanie kiedy może Ci się przydać Twoja własna klasa StorageClass?