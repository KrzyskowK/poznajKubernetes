# Ćwiczenia

### 1. setup wielu nodów za pomocą `kind` na windows https://kind.sigs.k8s.io/docs/user/quick-start

instalacja `kind`
```
> choco install kind
```

```
> kind --version
kind version 0.7.0

```

tworzymy nowy klaster
```
> kind create cluster

Creating cluster "kind" ...
 • Ensuring node image (kindest/node:v1.17.0) �  ...
 ✓ Ensuring node image (kindest/node:v1.17.0) �
 • Preparing nodes �   ...
 ✓ Preparing nodes �
 • Writing configuration �  ...
 ✓ Writing configuration �
 • Starting control-plane �️  ...
 ✓ Starting control-plane �️
 • Installing CNI �  ...
 ✓ Installing CNI �
 • Installing StorageClass �  ...
 ✓ Installing StorageClass �
Set kubectl context to "kind-kind"
You can now use your cluster with:

kubectl cluster-info --context kind-kind

Not sure what to do next? � Check out https://kind.sigs.k8s.io/docs/user/quick-start/
```
nowy klaster powinien zostać automatycznie dodany do `config` w kubectl.
```
> kubectl config view

apiVersion: v1
clusters:
- cluster:
    certificate-authority-data: DATA+OMITTED
    server: https://kubernetes.docker.internal:6443
  name: docker-desktop
- cluster:
    certificate-authority-data: DATA+OMITTED
    server: https://127.0.0.1:32768
  name: kind-kind
contexts:
- context:
    cluster: docker-desktop
    user: docker-desktop
  name: docker-desktop
- context:
    cluster: docker-desktop
    user: docker-desktop
  name: docker-for-desktop
- context:
    cluster: kind-kind
    user: kind-kind
  name: kind-kind
current-context: kind-kind
kind: Config
preferences: {}
users:
- name: docker-desktop
  user:
    client-certificate-data: REDACTED
    client-key-data: REDACTED
- name: kind-kind
  user:
    client-certificate-data: REDACTED
    client-key-data: REDACTED
```

Widzimy również że automatycznie został on ustawiony jako `current-context`
możemy zatem sprawdzić czy faktycznie nasz klient `kubectl` jest podłączony do klastra `kind-kind` odpytując o dostępne nodes

```
> kubectl config current-context
kind-kind

> kubectl get nodes
NAME                 STATUS   ROLES    AGE    VERSION
kind-control-plane   Ready    master   4m9s   v1.17.0
```
jeżeli chcemy przełączyc nasz `current-context` na klaster zarządzany przez docker for windows możemy to zrobić tak:

```
> kubectl config use-context docker-desktop
Switched to context "docker-desktop".

> kubectl get nodes
NAME             STATUS   ROLES    AGE   VERSION
docker-desktop   Ready    master   38d   v1.14.8
```

https://kubernetes.io/docs/reference/kubectl/cheatsheet/ - przydatny link z cheatsheet dotyczące `config` i `context`

Skoro już wiemy że `kind` działa, możemy teraz usunąć defaultowy klaster, żeby następnie stworzyć klaster z kilkoma worker-nodami. Widzimy, że po usunięciu klastra, `kind` automatycznie usuwa go również z `config` kubectl
```
> kind delete cluster --name=kind
Deleting cluster "kind" ...

> kubectl config view
apiVersion: v1
clusters:
- cluster:
    certificate-authority-data: DATA+OMITTED
    server: https://kubernetes.docker.internal:6443
  name: docker-desktop
contexts:
- context:
    cluster: docker-desktop
    user: docker-desktop
  name: docker-desktop
- context:
    cluster: docker-desktop
    user: docker-desktop
  name: docker-for-desktop
current-context: ""
kind: Config
preferences: {}
users:
- name: docker-desktop
  user:
    client-certificate-data: REDACTED
    client-key-data: REDACTED
```

do stworzenia bardziej zaawansowanego klastra użyjemy konfiguracji `kind-three-nodes.yaml`:
```
# three node (two workers) cluster config
kind: Cluster
apiVersion: kind.x-k8s.io/v1alpha4
nodes:
- role: control-plane
- role: worker
- role: worker
```

```
> kind create cluster --config kind-three-nodes.yaml

Creating cluster "kind" ...
 • Ensuring node image (kindest/node:v1.17.0) �  ...
 ✓ Ensuring node image (kindest/node:v1.17.0) �
 • Preparing nodes � � �   ...
 ✓ Preparing nodes � � �
 • Writing configuration �  ...
 ✓ Writing configuration �
 • Starting control-plane �️  ...
 ✓ Starting control-plane �️
 • Installing CNI �  ...
 ✓ Installing CNI �
 • Installing StorageClass �  ...
 ✓ Installing StorageClass �
 • Joining worker nodes �  ...
 ✓ Joining worker nodes �
Set kubectl context to "kind-kind"
You can now use your cluster with:

kubectl cluster-info --context kind-kind

Thanks for using kind! �
```

widzimy, że tym razem mamy dostępne 3 nody. ZŁOTO 🤩🏆🥇
```
> kubectl get nodes
NAME                 STATUS   ROLES    AGE     VERSION
kind-control-plane   Ready    master   2m30s   v1.17.0
kind-worker          Ready    <none>   113s    v1.17.0
kind-worker2         Ready    <none>   114s    v1.17.0
```
---

### Przetestuj tworzenie DaemonSet na swojej aplikacji lub korzystając z obrazu PKAD

### 1.1 RollingUpdate - sprawdź zachowanie aktualizacji
tworzymy template dla `daemontset` i zapisujemy jako `daemon-rollingUpdate.yaml`:
```
apiVersion: apps/v1
kind: DaemonSet
metadata:
  name: daemon-rolling-update
spec:
  updateStrategy:
    type: RollingUpdate
    rollingUpdate:
      maxUnavailable: 1
  selector:
    matchLabels:
      name: pkad
  template:
    metadata:
      labels: 
        name: pkad
    spec:
      containers:
      - name: pkad
        image: poznajkubernetes/pkad:blue
        readinessProbe:
          httpGet:
            path: /ready
            port: 8080
```
następnie dodajemy konfigurację do naszego klastra
```
> kubectl apply -f daemon-rollingUpdate.yaml
daemonset.apps/daemon-rolling-update created
```
widzimy, że deamon set został poprawnie utworzony
```
> kubectl get ds -o wide
NAME                    DESIRED   CURRENT   READY   UP-TO-DATE   AVAILABLE   NODE SELECTOR   AGE     CONTAINERS   IMAGES                       SELECTOR
daemon-rolling-update   2         2         2       2            2           <none>          3m45s   pkad         poznajkubernetes/pkad:blue   name=pkad
```
widzimy również, że jedna instancja pod została umieszczona na każdym z worker nodów

```
> kubectl get pod -o wide

NAME                          READY   STATUS    RESTARTS   AGE     IP           NODE           NOMINATED NODE   READINESS GATES
daemon-rolling-update-ljjmk   1/1     Running   0          4m20s   10.244.1.2   kind-worker2   <none>           <none>
daemon-rolling-update-mm9nq   1/1     Running   0          4m20s   10.244.2.2   kind-worker    <none>           <none>
```

w historii wdrożeń widzimy, że została utworzona rewizja nr 1
```
> kubectl rollout history ds daemon-rolling-update
daemonset.apps/daemon-rolling-update
REVISION  CHANGE-CAUSE
1         <none>
```

teraz podmieniamy wersję obrazu w `daemon-rollingUpdate.yaml` na
```
image: poznajkubernetes/pkad:red
```
i aktualizujemy konfigurację

```

```
obraz w deamonset zostaje zaktualizowany na `pkad:red`
```
> kubectl get ds -o wide
NAME                    DESIRED   CURRENT   READY   UP-TO-DATE   AVAILABLE   NODE SELECTOR   AGE     CONTAINERS   IMAGES                      SELECTOR
daemon-rolling-update   2         2         1       0            1           <none>          9m24s   pkad         poznajkubernetes/pkad:red   name=pkad
```
następuje również podmiana podów. Na `node/kind-worker` pod `mm9nq` zostaje zastąpione przez `zh9qb`. Gdy `zh9qb` przechodzi w stan `READY` następuje wyłączenie `ljjmk` na `node/kind-worker2` a w jego miejsce zostaje uruchomiony `rcb4v`
```
> kubectl get pod -w
NAME                          READY   STATUS                RESTARTS   AGE
daemon-rolling-update-ljjmk   1/1     Running               0          7m37s
daemon-rolling-update-mm9nq   1/1     Running               0          7m37s
daemon-rolling-update-mm9nq   1/1     Terminating           0          9m16s
daemon-rolling-update-mm9nq   0/1     Terminating           0          9m17s
daemon-rolling-update-mm9nq   0/1     Terminating           0          9m25s
daemon-rolling-update-mm9nq   0/1     Terminating           0          9m25s
daemon-rolling-update-zh9qb   0/1     Pending               0          0s
daemon-rolling-update-zh9qb   0/1     Pending               0          0s
daemon-rolling-update-zh9qb   0/1     ContainerCreating     0          0s
daemon-rolling-update-zh9qb   0/1     Running               0          8s
daemon-rolling-update-zh9qb   1/1     Running               0          13s
daemon-rolling-update-ljjmk   1/1     Terminating           0          9m38s
daemon-rolling-update-ljjmk   0/1     Terminating           0          9m39s
daemon-rolling-update-ljjmk   0/1     Terminating           0          9m44s
daemon-rolling-update-ljjmk   0/1     Terminating           0          9m45s
daemon-rolling-update-rcb4v   0/1     Pending               0          0s
daemon-rolling-update-rcb4v   0/1     Pending               0          0s
daemon-rolling-update-rcb4v   0/1     ContainerCreating     0          0s
daemon-rolling-update-rcb4v   0/1     Running               0          6s
daemon-rolling-update-rcb4v   1/1     Running               0          11s

> kubectl get pod -o wide
NAME                          READY   STATUS    RESTARTS   AGE     IP           NODE           NOMINATED NODE   READINESS GATES
daemon-rolling-update-rcb4v   1/1     Running   0          2m26s   10.244.1.3   kind-worker2   <none>           <none>
daemon-rolling-update-zh9qb   1/1     Running   0          2m46s   10.244.2.3   kind-worker    <none>           <none>
```

w historii wdrożeń widzmy nowy wpis
```
> kubectl rollout history ds daemon-rolling-update
daemonset.apps/daemon-rolling-update
REVISION  CHANGE-CAUSE
1         <none>
2         <none>
```

### 1.2 RollingUpdate - w przypadku błędnie działającego health check

aby zasymulować problem w uruchomieniu pod, podmieniamy `ReadinessProbe` path na nieistniejący url
```
...
readinessProbe:
          httpGet:
            path: /not-ready-at-all
            port: 8080
```

```
> kubectl apply -f deamon-rollingUpdate-broken.yaml
daemonset.apps/daemon-rolling-update configured
```
W wyniku nowego rollout pod `rcb4v` zostaje wyłączony a w jego miejsce uruchomiony `5rk8w`. Pod `5rk8w` jednak nigdy nie zaraportuje stanu `READY`.

Nasz rollout "zawiśnie" oczeując na update stanu poda
```
> kubectl rollout status ds daemon-rolling-update
Waiting for daemon set "daemon-rolling-update" rollout to finish: 1 out of 2 new pods have been updated...
```

W skutek tego nasz deamonset zostaje pozostawiony tylko z jednym działającym pod
```
> kubectl get pod -w
NAME                          READY   STATUS    RESTARTS   AGE
daemon-rolling-update-5rk8w   0/1     Running   0          16s
daemon-rolling-update-zh9qb   1/1     Running   0          15m

>  kubectl get ds -o wide
NAME                    DESIRED   CURRENT   READY   UP-TO-DATE   AVAILABLE   NODE SELECTOR   AGE   CONTAINERS   IMAGES                      SELECTOR
daemon-rolling-update   2         2         1       1            1           <none>          37m   pkad         poznajkubernetes/pkad:red   name=pkad
```

### 1.3 RollingUpdate - przetestuj działanie rollout undo w celu przywrócenia poprzedniej wersji.

```
> kubectl rollout history ds daemon-rolling-update
daemonset.apps/daemon-rolling-update
REVISION  CHANGE-CAUSE
1         <none>
3         <none>
4         <none>

> kubectl rollout undo ds daemon-rolling-update --to-revision=2
daemonset.apps/daemon-rolling-update rolled back
```

w miejsce niedziałającego pod `5rk8w` zostaje stworzony nowy `jtjhb` który poprawnie ustawia się w stan `READY`

```
> kubectl get pod -o wide -w
NAME                          READY   STATUS    RESTARTS   AGE   IP           NODE           NOMINATED NODE   READINESS GATES
daemon-rolling-update-jtjhb   1/1     Running   0          7s    10.244.1.5   kind-worker2   <none>           <none>
daemon-rolling-update-zh9qb   1/1     Running   0          28m   10.244.2.3   kind-worker    <none>           <none>
```
rollout zostaje zakończony sukcesem
```
> kubectl rollout status ds daemon-rolling-update
daemon set "daemon-rolling-update" successfully rolled out

> kubectl get ds -o wide -w
NAME                    DESIRED   CURRENT   READY   UP-TO-DATE   AVAILABLE   NODE SELECTOR   AGE   CONTAINERS   IMAGES                      SELECTOR
daemon-rolling-update   2         2         2       2            2           <none>          46m   pkad         poznajkubernetes/pkad:red   name=pkad
```

---

### 2.1 OnDelete - sprawdź zachowanie aktualizacji

tworzymy template dla `daemontset` i zapisujemy jako `daemon-on-delete.yaml`:
```
apiVersion: apps/v1
kind: DaemonSet
metadata:
  name: ds-on-delete
spec:
  updateStrategy:
    type: OnDelete
  selector:
    matchLabels:
      name: pkad
  template:
    metadata:
      labels: 
        name: pkad
    spec:
      containers:
      - name: pkad
        image: poznajkubernetes/pkad:red
        readinessProbe:
          httpGet:
            path: /ready
            port: 8080
```

wdrażamy konfigurację i widzimy, że każdy node otrzymuje jedną instancję naszego pod
```
> kubectl apply -f daemon-on-delete.yaml
daemonset.apps/ds-on-delete created

>  kubectl get ds -o wide
NAME           DESIRED   CURRENT   READY   UP-TO-DATE   AVAILABLE   NODE SELECTOR   AGE   CONTAINERS   IMAGES                      SELECTOR
ds-on-delete   2         2         2       2            2           <none>          41s   pkad         poznajkubernetes/pkad:red   name=pkad

> kubectl get pod -o wide
NAME                 READY   STATUS    RESTARTS   AGE   IP           NODE           NOMINATED NODE   READINESS GATES
ds-on-delete-qhj9b   1/1     Running   0          71s   10.244.1.7   kind-worker2   <none>           <none>
ds-on-delete-zm7mw   1/1     Running   0          71s   10.244.2.4   kind-worker    <none>           <none>
```

teraz podmieniamy wersję obrazu w `daemon-on-delete.yaml` na
```
image: poznajkubernetes/pkad:blue
```
i aktualizujemy konfigurację. Widzimy, że IMAGE w naszym `daemonset` zostaje zaktualizowany, jednak liczba podów będących UP-TO-DATE wynosi 0
```
> kubectl apply -f daemon-on-delete.yaml
daemonset.apps/ds-on-delete configured

> kubectl get ds -o wide -w
NAME           DESIRED   CURRENT   READY   UP-TO-DATE   AVAILABLE   NODE SELECTOR   AGE     CONTAINERS   IMAGES                      SELECTOR
ds-on-delete   2         2         2       2            2           <none>          2m47s   pkad         poznajkubernetes/pkad:red   name=pkad
ds-on-delete   2         2         2       0            2           <none>          3m4s    pkad         poznajkubernetes/pkad:blue   name=pkad

> kubectl get pod -o wide -w
NAME                 READY   STATUS    RESTARTS   AGE     IP           NODE           NOMINATED NODE   READINESS GATES
ds-on-delete-qhj9b   1/1     Running   0          2m38s   10.244.1.7   kind-worker2   <none>           <none>
ds-on-delete-zm7mw   1/1     Running   0          2m38s   10.244.2.4   kind-worker    <none>           <none>
```
widzimy, że dodana została nowa wersja wdrożenia
```
> kubectl rollout history ds ds-on-delete
daemonset.apps/ds-on-delete
REVISION  CHANGE-CAUSE
1         <none>
2         <none>
```
z ciekawostek widzimy, że rollout status jest niedostępny gdy `OnDelete` jest ustawione jako rolloutStrategy
```
> kubectl rollout status ds ds-on-delete
error: rollout status is only available for RollingUpdate strategy type
```

jeżeli teraz usuniemy jeden z pod, zostanie on automatycznie zastąpiony pod w nowej wersji.
```
> kubectl delete pod ds-on-delete-qhj9b
pod "ds-on-delete-qhj9b" deleted

> kubectl get pod -o wide -w
NAME                 READY   STATUS    RESTARTS   AGE    IP           NODE           NOMINATED NODE   READINESS GATES
ds-on-delete-qhj9b   1/1     Running   0          9m3s   10.244.1.7   kind-worker2   <none>           <none>
ds-on-delete-zm7mw   1/1     Running   0          9m3s   10.244.2.4   kind-worker    <none>           <none>
ds-on-delete-qhj9b   1/1     Terminating   0          9m17s   10.244.1.7   kind-worker2   <none>           <none>
ds-on-delete-qhj9b   0/1     Terminating   0          9m18s   10.244.1.7   kind-worker2   <none>           <none>
ds-on-delete-qhj9b   0/1     Terminating   0          9m19s   10.244.1.7   kind-worker2   <none>           <none>
ds-on-delete-qhj9b   0/1     Terminating   0          9m19s   10.244.1.7   kind-worker2   <none>           <none>
ds-on-delete-zlthj   0/1     Pending       0          0s      <none>       <none>         <none>           <none>
ds-on-delete-zlthj   0/1     Pending       0          0s      <none>       kind-worker2   <none>           <none>
ds-on-delete-zlthj   0/1     ContainerCreating   0          0s      <none>       kind-worker2   <none>           <none>
ds-on-delete-zlthj   0/1     Running             0          2s      10.244.1.8   kind-worker2   <none>           <none>
ds-on-delete-zlthj   1/1     Running             0          4s      10.244.1.8   kind-worker2   <none>           <none>

> kubectl get ds -o wide -w
NAME           DESIRED   CURRENT   READY   UP-TO-DATE   AVAILABLE   NODE SELECTOR   AGE     CONTAINERS   IMAGES                      SELECTOR
ds-on-delete   2         2         2       2            2           <none>          2m47s   pkad         poznajkubernetes/pkad:red   name=pkad
ds-on-delete   2         2         2       0            2           <none>          3m4s    pkad         poznajkubernetes/pkad:blue   name=pkad
ds-on-delete   2         2         1       0            1           <none>          9m18s   pkad         poznajkubernetes/pkad:blue   name=pkad
ds-on-delete   2         2         1       1            1           <none>          9m19s   pkad         poznajkubernetes/pkad:blue   name=pkad
ds-on-delete   2         2         2       1            2           <none>          9m23s   pkad         poznajkubernetes/pkad:blue   name=pkad
```

Widzmy, że liczba UP-TO-DATE wzrosła do `1` a usnięty pod `qhj9b` z `node/kind-worker2` został zastąpiony przez `zlthj`. Jeżeli analogicznie usuniemy teraz pod z `node/kind-worker` powinniśmy otrzymać drugi pod będący UP-TO-DATE z ostatnią wersją daemonset

```

> kubectl delete pod ds-on-delete-zm7mw
pod "ds-on-delete-zm7mw" deleted

> kubectl get ds -o wide -w
NAME           DESIRED   CURRENT   READY   UP-TO-DATE   AVAILABLE   NODE SELECTOR   AGE     CONTAINERS   IMAGES                      SELECTOR
ds-on-delete   2         2         2       2            2           <none>          13m     pkad         poznajkubernetes/pkad:blue   name=pkad
```


### 2.2 OnDelete - w przypadku błędnie działającego health check


aby zasymulować problem w uruchomieniu pod, podmieniamy `ReadinessProbe` path na nieistniejący url
```
...
readinessProbe:
          httpGet:
            path: /not-ready-at-all
            port: 8080
```

```
> kubectl apply -f deamon-on-delete-broken.yaml
daemonset.apps/daemon-on-delete-update configured
```
W wyniku nowego rollout ponownie otrzymujemy `0` podów będących UP-TO-DATE
```
> kubectl get ds -o wide -w
NAME           DESIRED   CURRENT   READY   UP-TO-DATE   AVAILABLE   NODE SELECTOR   AGE     CONTAINERS   IMAGES                      SELECTOR
ds-on-delete   2         2         2       0            2           <none>          18m     pkad         poznajkubernetes/pkad:blue   name=pkad
```

Jeżeli teraz usuniemy pod z `node/kind-worker` zostanie on odtworzny w nowej konfiguracji która nigdy nie zaraportuje stanu READY
```
> kubectl delete pod ds-on-delete-zlthj
pod "ds-on-delete-zlthj" deleted

> kubectl get pod -o wide -w
NAME                 READY   STATUS    RESTARTS   AGE     IP           NODE           NOMINATED NODE   READINESS GATES
ds-on-delete-dkcdf   1/1     Running   0          6m32s   10.244.2.5   kind-worker    <none>           <none>
ds-on-delete-zlthj   1/1     Running   0          11m     10.244.1.8   kind-worker2   <none>           <none>
ds-on-delete-zlthj   1/1     Terminating   0          11m     10.244.1.8   kind-worker2   <none>           <none>
ds-on-delete-zlthj   0/1     Terminating   0          11m     10.244.1.8   kind-worker2   <none>           <none>
ds-on-delete-zlthj   0/1     Terminating   0          11m     <none>       kind-worker2   <none>           <none>
ds-on-delete-zlthj   0/1     Terminating   0          11m     <none>       kind-worker2   <none>           <none>
ds-on-delete-zlthj   0/1     Terminating   0          11m     <none>       kind-worker2   <none>           <none>
ds-on-delete-qcz8m   0/1     Pending       0          1s      <none>       <none>         <none>           <none>
ds-on-delete-qcz8m   0/1     Pending       0          1s      <none>       kind-worker2   <none>           <none>
ds-on-delete-qcz8m   0/1     ContainerCreating   0          1s      <none>       kind-worker2   <none>           <none>
ds-on-delete-qcz8m   0/1     Running             0          2s      10.244.1.9   kind-worker2   <none>           <none>
```

W skutek tego nasz deamonset zostaje pozostawiony tylko z jednym działającym pod
```
>  kubectl get ds -o wide
NAME                    DESIRED   CURRENT   READY   UP-TO-DATE   AVAILABLE   NODE SELECTOR   AGE   CONTAINERS   IMAGES                      SELECTOR
ds-on-delete   2         2         1       1            1           <none>          20m     pkad         poznajkubernetes/pkad:blue   name=pkad
```

### 2.3 OnDelete - przetestuj działanie rollout undo w celu przywrócenia poprzedniej wersji.

teraz możemy wycofać zmiany z ostatniej rewizji
```
> kubectl rollout history ds ds-on-delete
daemonset.apps/ds-on-delete
REVISION  CHANGE-CAUSE
1         <none>
2         <none>
3         <none>

> kubectl rollout undo ds ds-on-delete --to-revision=2
daemonset.apps/daemon-rolling-update rolled back
```

następnie usunąć pod stworzonego w wyniku wdrozenia nr 3
```
> kubectl delete pod ds-on-delete-qcz8m
pod "ds-on-delete-qcz8m" deleted

> kubectl get pod -o wide -w
NAME                 READY   STATUS    RESTARTS   AGE     IP           NODE           NOMINATED NODE   READINESS GATES
ds-on-delete-qcz8m   0/1     Terminating         0          4m50s   10.244.1.9   kind-worker2   <none>           <none>
ds-on-delete-mdvmv   0/1     Pending             0          0s      <none>       <none>         <none>           <none>
ds-on-delete-mdvmv   0/1     Pending             0          1s      <none>       kind-worker2   <none>           <none>
ds-on-delete-mdvmv   0/1     ContainerCreating   0          1s      <none>       kind-worker2   <none>           <none>
ds-on-delete-mdvmv   0/1     Running             0          2s      10.244.1.10   kind-worker2   <none>           <none>
ds-on-delete-mdvmv   1/1     Running             0          4s      10.244.1.10   kind-worker2   <none>           <none>
```

w wyniku czego pod w wersji 2 zostaje odtworzony a nasz daemonset ponownie raportuje wszystkie pod jako UP-TO-DATE
```
> kubectl get ds -o wide -w
NAME           DESIRED   CURRENT   READY   UP-TO-DATE   AVAILABLE   NODE SELECTOR   AGE   CONTAINERS   IMAGES                       SELECTOR
ds-on-delete   2         2         2       2            2           <none>          27m   pkad         poznajkubernetes/pkad:blue   name=pkad
```