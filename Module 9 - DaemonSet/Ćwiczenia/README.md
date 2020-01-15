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

widzimy, że tym razem mamy dostępne 3 nody
```
> kubectl get nodes
NAME                 STATUS   ROLES    AGE     VERSION
kind-control-plane   Ready    master   2m30s   v1.17.0
kind-worker          Ready    <none>   113s    v1.17.0
kind-worker2         Ready    <none>   114s    v1.17.0
```

### Przetestuj tworzenie DaemonSet na swojej aplikacji lub korzystając z obrazu PKAD

### Sprawdź zachowanie aktualizacji dla RollingUpdate 

### RollingUpdate - w przypadku błędnie działającego health check

### RollingUpdate - przetestuj działanie rollout undo w celu przywrócenia poprzedniej wersji.

### Sprawdź zachowanie aktualizacji dla OnDelete 

### OnDelete - w przypadku błędnie działającego health check

### OnDelete - przetestuj działanie rollout undo w celu przywrócenia poprzedniej wersji.