# Praca domowa
 
### 1. Spróbuj podpiąć się wykorzystująć PersistentVolumeClaim pod inny dysk niż domyślny (zamiast innego dysku można użyć PV:local).

Trochę teorii:

https://kubernetes.io/docs/concepts/storage/volumes/#local

Q: Jaką przewagę ma `PersistentVolume - local` nad `PersistenVolume - hostPath`?

A: W przypadku `hostPath` nasz PersistenVolume jest przypięty do konretnego `node`, w związku z tym pod które korzystając z tego PV muszą zostać zaschedulowane na tym konkretnym nodzie. `local` dostarcza mechanizm który pozwala automatycznie zaschedulować odpowiednie pod na odpowiednim `node` za pomocą ustawień w `nodeAffinity`

---

Stwórzmy zatem klaster z więcej niż jednym nodem.
Musimy pamiętać o tym, że nasze nody są zdeployowane przez kind jako docker images, musimy zatem umożliwić im dostęp do plików w naszym OS
https://kind.sigs.k8s.io/docs/user/configuration/
```
kind: Cluster
apiVersion: kind.x-k8s.io/v1alpha4
nodes:
- role: control-plane
- role: worker
  extraMounts:
  - hostPath: D:\k8s_vol\persistent_volume\local\node1
    containerPath: /files
- role: worker
  extraMounts:
  - hostPath: D:\k8s_vol\persistent_volume\local\node2
    containerPath: /files
```
Dzięki takiej konfiguracji każdy nodę będzie teraz posiadał folder `/files` odpowiednio zmapowany do ścieżki na naszym lokalnym dysku.

Tworzymy klaster

```
> kind create cluster --config kind-three-nodes.yaml

> kubectl get nodes
NAME                 STATUS   ROLES    AGE     VERSION
kind-control-plane   Ready    master   2m28s   v1.17.0
kind-worker          Ready    <none>   107s    v1.17.0
kind-worker2         Ready    <none>   107s    v1.17.0
```

Tworzymy definicję persistent volume dla typu local, który będzię udostępniał swoją lokację `/files` (stworzoną przed chwilą podczas konfigurowania nodów). W nodeAffinity przypisujemy PV local jedynie do `kind-worker2` za pomocą labelki `kubernetes.io/hostname`, dzięki temu wszystkie pod które będą używać później `pv-local` powinny zostać automatycznie zamontowane tylko na node `kind-worker2`.
```
apiVersion: v1
kind: PersistentVolume
metadata:
  name: pv-local
  labels:
    purpose: demo
spec:
  capacity:
    storage: 10Mi
  volumeMode: Filesystem
  accessModes:
  - ReadWriteOnce
  persistentVolumeReclaimPolicy: Delete
  local:
    path: /files
  nodeAffinity:
    required:
      nodeSelectorTerms:
      - matchExpressions:
        - key: kubernetes.io/hostname
          operator: In
          values:
          - kind-worker2
```
dodajemy do tego PVC
```
apiVersion: v1
kind: PersistentVolumeClaim
metadata:
  name: pvc-local
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
Stwórzmy zatem PV i PVC an klastrze
```
> kubectl apply -f .\pv-and-pvc-local.yaml
persistentvolume/pv-local created
persistentvolumeclaim/pvc-local created

> kubectl get pv
NAME       CAPACITY   ACCESS MODES   RECLAIM POLICY   STATUS   CLAIM              STORAGECLASS   REASON   AGE
pv-local   10Mi       RWO            Delete           Bound    default/pvc-local                           6m12s

> kubectl get pvc
NAME        STATUS   VOLUME     CAPACITY   ACCESS MODES   STORAGECLASS   AGE
pvc-local   Bound    pv-local   10Mi       RWO                           6m19s
```

Teraz dodajmy deployment który stworzy dla nas 5 instancji nginx które wykorzystają `pvc-local` do hostowanie pliku `index.html`
```
apiVersion: apps/v1
kind: Deployment
metadata:
  name: web-pvc-local
spec:
  replicas: 5
  selector:
    matchLabels:
      app: webapp
  strategy: {}
  template:
    metadata:
      name: web-pvc-local
      labels:
        app: webapp
    spec:
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
            claimName: pvc-local
```
Tworzymy nasz deployment i sprawdzamy gdzie zostały zdeployowane nasze pody
```
> kubectl apply -f .\deployment-pvc-local.yaml       
deployment.apps/web-pvc-local created

> kubectl get pod -o wide
NAME                             READY   STATUS    RESTARTS   AGE   IP           NODE           NOMINATED NODE   READINESS GATES
web-pvc-local-6744568454-b4zsc   1/1     Running   0          10s   10.244.2.5   kind-worker2   <none>           <none>
web-pvc-local-6744568454-bt2pn   1/1     Running   0          10s   10.244.2.4   kind-worker2   <none>           <none>
web-pvc-local-6744568454-gxcnd   1/1     Running   0          10s   10.244.2.7   kind-worker2   <none>           <none>
web-pvc-local-6744568454-jphw4   1/1     Running   0          10s   10.244.2.3   kind-worker2   <none>           <none>
web-pvc-local-6744568454-v9vqz   1/1     Running   0          10s   10.244.2.6   kind-worker2   <none>           <none>
```
Widzimy, że zgodnie z przewidywaniami żaden z nich nie został stworzony na `kind-worker`.

Jeżeli teraz dodamy do naszej ścieżki index.html, powinniśmy widzeć, że został on zaserwowany przez którąkolwiek z instancji pod
```
> echo "hello from node-worker2 local PV" > /mnt/d/k8s_vol/persistent_volume/local/node2/index.html

> kubectl port-forward web-pvc-local-6744568454-b4zsc 8080:80

> curl localhost:8080
hello from node-worker2 local PV
```

---

### 2. Wykorzystaj typ wolumenu: projected by połączyć przynajmniej dwa typy wolumenów

Trochę teorii:

https://kubernetes.io/docs/tasks/configure-pod-container/configure-projected-volume-storage/
https://kubernetes.io/docs/concepts/storage/volumes/#projected

Q: Czym jest `projected` Volume?

A: sposób na podmontowanie do pod kilku volumenów do tej samej ścieżki

---

Spróbujmy teraz skorzystać w pod z volume typu `projected`, w tym celu stworzymy dwa configMaps. jeden z nich będzie zawierać `udhcpd.conf` a drugi `appsettings.json`. Następnie zamontujemy obie konfigurację jako projected pod ścieżką `/allconfigs` w naszym kontenerze.
```
apiVersion: v1
kind: ConfigMap
metadata:
  name: config1
data:
  udhcpd.conf: "hello from udhcpd.conf :wave:" 
---
apiVersion: v1
kind: ConfigMap
metadata:
  name: config2
data:
  appsettings.json: "hello from appsettings.json :wave:"
---
apiVersion: v1
kind: Pod
metadata:
  name: busybox
spec:
  containers:
  - name: busybox
    command: ["sleep", "3000"]
    volumeMounts:
    - name: all-in-one
      mountPath: /allconfigs
    image: busybox
  volumes:
    - name: all-in-one
      projected:
        sources:
        - configMap:
            name: config1
        - configMap:
            name: config2
```

```
> kubectl apply -f .\pod-projection-volume.yaml
configmap/config1 unchanged
configmap/config2 unchanged
pod/busybox configured
```
Sprawdzamy czy `appsettings.json` i `udhcpd.conf` leżą w `/allconfigs` tak jak się spodziewaliśmy
```
> kubectl exec -it busybox ls /allconfigs
appsettings.json  udhcpd.conf
```