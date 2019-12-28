# Ćwiczenie

## 1. Stwórz Pod na bazie obrazu z modułu 1

generujemy `kuard.yml` za pomocą:

```
> kubectl run kuard --image=poznajkubernetes/kuard --restart=Never --dry-run -o yaml > kuard.yml
```

```
apiVersion: v1
kind: Pod
metadata:
  creationTimestamp: null
  labels:
    run: kuard
  name: kuard
spec:
  containers:
  - image: poznajkubernetes/kuard
    name: kuard
    resources: {}
  dnsPolicy: ClusterFirst
  restartPolicy: Never
status: {}
```

### Zobacz w jakim stanie on się znajduje:

```
> kubectl get pod kuard

NAME    READY   STATUS    RESTARTS   AGE
kuard   1/1     Running   0          158m
```

### Podejrzyj jego logi:

```
> kubectl logs kuard

2019/12/28 08:58:01 Starting pkad version: blue
2019/12/28 08:58:01 **********************************************************************
2019/12/28 08:58:01 * WARNING: This server may expose sensitive
2019/12/28 08:58:01 * and secret information. Be careful.
2019/12/28 08:58:01 **********************************************************************
2019/12/28 08:58:01 Config:
{
  "address": ":8080",
  "debug": false,
  "debug-sitedata-dir": "./sitedata",  
  "keygen": {
    "enable": false,
    "exit-code": 0,
    "exit-on-complete": false,
    "memq-queue": "",
    "memq-server": "",
    "num-to-gen": 0,
    "time-to-run": 0
  },
  "liveness": {
    "fail-next": 0
  },
  "readiness": {
    "fail-next": 0
  },
  "tls-address": ":8443",
  "tls-dir": "/tls"
}
2019/12/28 08:58:01 Could not find certificates to serve TLS
2019/12/28 08:58:01 Serving on HTTP on 
:8080
2019/12/28 10:23:58 127.0.0.1:38572 GET /
2019/12/28 10:23:58 Loading template for index.html
```

### Wykonaj w koneterze wylistowanie katalogów:

```
> kubectl exec kuard ls

bin
dev
etc
home
lib
media
mnt
opt
pkad
proc
root
run
sbin
srv
sys
tmp
usr
var
```

### Odpytaj się http://localhost:PORT w koneterze:

```
> kubectl exec kuard -- wget -qO- http://localhost:8080

<!doctype html>

<html lang="en">
<head>
  <meta charset="utf-8">

  <title>Poznaj Kubernetes Demo</title>

  <link rel="stylesheet" href="/static/css/bootstrap.min.css">
  <link rel="stylesheet" href="/static/css/styles.css">

  <script>
...
```

### Zrób to samo z powołanego osobno poda (kubectl run):

tworzymy nowego poda o nazwie `kuard1`
```
> kubectl run kuard1 --image=poznajkubernetes/kuard --restart=Never

pod/kuard1 created
```

za pomoca `kubectl describe pod kuard` sprawdzamy adress ip POD wewnatrz kalstra
```
> kubectl describe pod kuard

Name:               kuard
Namespace:          default
Priority:           0
PriorityClassName:  <none>
Node:               docker-desktop/192.168.65.3
Start Time:         Sat, 28 Dec 2019 09:57:55 +0100
Labels:             run=kuard
Annotations:        kubectl.kubernetes.io/last-applied-configuration:
                      {"apiVersion":"v1","kind":"Pod","metadata":{"annotations":{},"creationTimestamp":null,"labels":{"run":"kuard"},"name":"kuard","namespace":...
Status:             Running
IP:                 10.1.0.78
Containers:
  kuard:
    Container ID:   docker://116459e25ca507fb762f2158769dd5a48f1a9ee28ee8647b4401e27cef462036
    Image:          poznajkubernetes/kuard
    Image ID:       docker-pullable://poznajkubernetes/kuard@sha256:230ff75987cf38d9d90ac1684d445f2d02f3edfa45865a0de35bc94f4a38c83b
    Port:           <none>
    Host Port:      <none>
    State:          Running
      Started:      Sat, 28 Dec 2019 09:58:01 +0100
    Ready:          True
    Restart Count:  0
    Environment:    <none>
    Mounts:
      /var/run/secrets/kubernetes.io/serviceaccount from default-token-g76vf (ro)
Conditions:
  Type              Status
  Initialized       True
  Ready             True
  ContainersReady   True
  PodScheduled      True
Volumes:
  default-token-g76vf:
    Type:        Secret (a volume populated by a Secret)
    SecretName:  default-token-g76vf
    Optional:    false
QoS Class:       BestEffort
Node-Selectors:  <none>
Tolerations:     node.kubernetes.io/not-ready:NoExecute for 300s
                 node.kubernetes.io/unreachable:NoExecute for 300s
Events:          <none>
```

nastepnie na `kuard1` odpytujemy o http://10.1.0.78:8080

```
> kubectl exec kuard1 -- wget -qO- http://10.1.0.78:8080

<!doctype html>

<html lang="en">
<head>
  <meta charset="utf-8">

  <title>Poznaj Kubernetes Demo</title>

  <link rel="stylesheet" href="/static/css/bootstrap.min.css">
  <link rel="stylesheet" href="/static/css/styles.css">

  <script>
...
```

### Dostań się do poda za pomocą przekierowania portów:

```
> kubectl port-forward kuard 80:8080   

Forwarding from 127.0.0.1:80 -> 8080
Forwarding from [::1]:80 -> 8080
```

nastepnie wchodzimy w przeglądarce na http://localhost:80

### Dostań się do poda za pomocą API Server:

odpalamy
```
> kubectl proxy

Starting to serve on 127.0.0.1:8001
```
następnie wchodzimy na http://127.0.0.1:8001/api/v1/pods
i znajdujemy selflink do `kuard` http://127.0.0.1:8001/api/v1/namespaces/default/pods/kuard

## 2. Stwórz Pod zawierający dwa kontenery – busybox i poznajkubernetes/helloapp:multi

tworzymy `multi.yml`

```
apiVersion: v1
kind: Pod
metadata:
  creationTimestamp: null
  labels:
    run: multi
  name: multi
spec:
  containers:
  - image: busybox
    name: busybox
    resources: {}
  - image: poznajkubernetes/helloapp:multi
    name: helloapp
    resources: {}
  dnsPolicy: ClusterFirst
  restartPolicy: Never
status: {}
```

dodajemy POD do klastra

```
> kubectl apply -f multi.yml

pod/multi created
```

### Zweryfikuj, że Pod działa poprawnie:

widzmy że tylko jeden kontener wystartował

```
> kubectl get pod
NAME     READY   STATUS    RESTARTS   AGE
multi    1/2     Running   0          18s
```

### Jak nie działa, dowiedz się dlaczego:

sprawdzamy stan poda za pomocą

```
> kubectl describe pod multi
```

w sekcji containers widzimy że `busybox` zakończył pracę

```
  State:          Terminated
      Reason:       Completed
      Exit Code:    0
      Started:      Sat, 28 Dec 2019 13:08:32 +0100
      Finished:     Sat, 28 Dec 2019 13:08:32 +0100
```

### Zastanów się nad rozwiązaniem problemu jeżeli istnieje – co można by było zrobić i jak

jeżeli chcielibyśmy żeby busybox pozostawał uruchomiony wystarczy dodac `commad: ['sleep', <ANY_TIME>]` do `multi.yml`
