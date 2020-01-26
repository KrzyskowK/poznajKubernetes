
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

---

### PersistenVolumeProvisioner aby zcacheować repozytorium git za pomocą InitContainer

---

### PersistenVolumeProvisioner aby dostarczyć plik konfiguracyjny za pomocą subpath

---

### Odpowiedz sobie na pytanie kiedy może Ci się przydać Twoja własna klasa StorageClass?