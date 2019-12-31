# Obiekt serwisu – Ćwiczenia

## Ćwiczenie 1

### container-to-container w Pod. Wykorzystaj do tego nginx.

`container-to-container.yaml:`
```
apiVersion: v1
kind: Pod
metadata:
  name: nginx
spec:
  containers:
  - image: nginx
    name: nginx
    ports:
      - containerPort: 80
        name: http
        protocol: TCP
  - image: busybox
    name: busybox
    command: ['sleep', '100']
```

```
> kubectl apply -f container-to-container.yaml
> kubectl exec nginx -c busybox -- wget -qO- localhost:80
<!DOCTYPE html>
<html>
<head>
<title>Welcome to nginx!</title>
<style>
...
```

### Komunikacja pomiędzy Podami – Pod-to-Pod. Wykorzystaj do tego nginx.

`pod-to-pod.yaml:`
```
apiVersion: v1
kind: Pod
metadata:
  name: nginx
spec:
  containers:
  - image: nginx
    name: nginx
    ports:
      - containerPort: 80
        name: http
        protocol: TCP
---
apiVersion: v1
kind: Pod
metadata:
  name: busybox
spec:
  containers:
  - image: busybox
    name: busybox
    command: ['sleep', '100']
```
```
> kubectl apply -f pod-to-pod.yaml
> kubectl get pods -o wide
NAME      READY   STATUS    RESTARTS   AGE   IP           NODE             NOMINATED NODE   READINESS GATES
busybox   1/1     Running   0          16s   10.1.0.135   docker-desktop   <none>           <none>
nginx     1/1     Running   0          16s   10.1.0.134   docker-desktop   <none>           <none>

> kubectl exec busybox -- wget -qO- 10.1.0.134
<!DOCTYPE html>
<html>
<head>
<title>Welcome to nginx!</title>
<style>
...
```

### Wykorzystaj nginx i wystaw go za pomocą serwisu ClusterIP w środku klastra.

`clusterip-service.yaml:`
```
apiVersion: v1
kind: Service
metadata:
  name: clusterip-service
spec:
  type: ClusterIP
  selector:
    app: nginx
  ports:
  - name: http
    port: 8080
    protocol: TCP
    targetPort: http
---
apiVersion: v1
kind: Pod
metadata:
  name: nginx1
  labels:
    app: nginx
spec:
  containers:
  - image: nginx
    name: nginx
    ports:
      - containerPort: 80
        name: http
        protocol: TCP
---
apiVersion: v1
kind: Pod
metadata:
  name: nginx2
  labels:
    app: nginx
spec:
  containers:
  - image: nginx
    name: nginx
    ports:
      - containerPort: 80
        name: http
        protocol: TCP
```

```
> kubectl apply -f .\clusterip-service.yaml
service/clusterip-service created
pod/nginx1 created
pod/nginx2 created
```

Widzimy, że IP podów `nginx1` i `nginx2` są przypisane w endpointach `clusterip-service` service
```
> kubectl get pods -o wide
NAME     READY   STATUS    RESTARTS   AGE   IP           NODE             NOMINATED NODE   READINESS GATES
nginx1   1/1     Running   0          10m   10.1.0.136   docker-desktop   <none>           <none>
nginx2   1/1     Running   0          10m   10.1.0.137   docker-desktop   <none>           <none>

> kubectl get endpoints
NAME                ENDPOINTS                     AGE
clusterip-service   10.1.0.136:80,10.1.0.137:80   13m
kubernetes          192.168.65.3:6443             23d

> kubectl describe service clusterip-service
Name:              clusterip-service
Namespace:         default
Labels:            <none>
Annotations:       kubectl.kubernetes.io/last-applied-configuration:
                     {"apiVersion":"v1","kind":"Service","metadata":{"annotations":{},"name":"clusterip-service","namespace":"default"},"spec":{"ports":[{"name...
Selector:          app=nginx
Type:              ClusterIP
IP:                10.101.162.234
Port:              http  8080/TCP
TargetPort:        http/TCP
Endpoints:         10.1.0.136:80,10.1.0.137:80
Session Affinity:  None
Events:            <none>
```


sprawdzamy czy nginx jest dostępny pod adresem IP servisu
```
> kubectl get svc 
NAME                TYPE        CLUSTER-IP       EXTERNAL-IP   PORT(S)    AGE
clusterip-service   ClusterIP   10.101.162.234   <none>        8080/TCP   63s
kubernetes          ClusterIP   10.96.0.1        <none>        443/TCP    23d

> kubectl run -it --rm tools --generator=run-pod/v1 --image=giantswarm/tiny-tools
# curl 10.101.162.234:8080
<!DOCTYPE html>
<html>
<head>
<title>Welcome to nginx!</title>
...
```

sprawdzamy również czy możemy dostać się do nginx przez nazwę naszego serwisu
```
> kubectl run -it --rm tools --generator=run-pod/v1 --image=giantswarm/tiny-tools
# curl clusterip-service:8080
<!DOCTYPE html>
<html>
<head>
<title>Welcome to nginx!</title>
...
```

### Wykorzystaj nginx i wystaw go na świat za pomocą serwisu NodePort w dwóch opcjach: bez wskazywania portu dla NodePort i ze wskazaniem.

`nodePort-service.yaml:`
```
apiVersion: v1
kind: Service
metadata:
  name: nodeport-service
spec:
  type: NodePort
  selector:
    app: nginx
  ports:
  - name: http
    port: 8080
    protocol: TCP
    targetPort: http
---
apiVersion: v1
kind: Pod
metadata:
  name: nginx1
  labels:
    app: nginx
spec:
  containers:
  - image: nginx
    name: nginx
    ports:
      - containerPort: 80
        name: http
        protocol: TCP
---
apiVersion: v1
kind: Pod
metadata:
  name: nginx2
  labels:
    app: nginx
spec:
  containers:
  - image: nginx
    name: nginx
    ports:
      - containerPort: 80
        name: http
        protocol: TCP
```

```
> kubectl apply -f .\nodePort-service.yaml
service/nodeport-service created
pod/nginx1 unchanged
pod/nginx2 unchanged
```

sprawdzamy port przydzielony dla naszego serwisu
```
> kubectl get service 
NAME                TYPE        CLUSTER-IP       EXTERNAL-IP   PORT(S)          AGE
clusterip-service   ClusterIP   10.101.162.234   <none>        8080/TCP         22m
kubernetes          ClusterIP   10.96.0.1        <none>        443/TCP          23d
nodeport-service    NodePort    10.103.140.66    <none>        8080:31076/TCP   25s
```

sprawdzamy czy jesteśmy w stanie dostać się do nginx spoza klastra
```
> curl http://localhost:31076
<!DOCTYPE html>
<html>
<head>
<title>Welcome to nginx!</title>
...
```

następnie wystawiamy wersję `nodePort` z przypisanym portem `31666`
`nodePort-service-port:`
```
apiVersion: v1
kind: Service
metadata:
  name: nodeport-service-port
spec:
  type: NodePort
  selector:
    app: nginx
  ports:
  - name: http
    port: 8080
    protocol: TCP
    targetPort: http
    nodePort: 31666
...
```

```
> kubectl apply -f .\nodePort-service-port.yaml
service/nodeport-service-port created
pod/nginx1 unchanged
pod/nginx2 unchanged
```

Widzimy że port został poprawnie podpięty
```
> kubectl get service nodeport-service-port
NAME                    TYPE       CLUSTER-IP      EXTERNAL-IP   PORT(S)          AGE
nodeport-service-port   NodePort   10.106.184.29   <none>        8080:31666/TCP   30s
```
a nginx jest teraz również dostępny z zewnątrz pod adresem http://localhost:31666
```
> curl http://localhost:31666
<!DOCTYPE html>
<html>
<head>
<title>Welcome to nginx!</title>
...
```

### Wykorzystaj nginx i wystaw go na świat za pomocą serwisu typu LoadBalancer,

`loadBalancer-service.yaml:`
```
apiVersion: v1
kind: Service
metadata:
  name: loadbalancer-service
spec:
  type: LoadBalancer
  selector:
    app: nginx
  ports:
  - name: http
    port: 8080
    protocol: TCP
    targetPort: http
...
```

```
> kubectl apply -f .\loadBalancer-service.yaml
service/loadbalancer-service created
pod/nginx1 unchanged
pod/nginx2 unchanged
```

widzimy że `loadbalancer-service` został udostępniony pod zewnetrzym IP
```
> kubectl get service
NAME                   TYPE           CLUSTER-IP      EXTERNAL-IP   PORT(S)          AGE
kubernetes             ClusterIP      10.96.0.1       <none>        443/TCP          23d
loadbalancer-service   LoadBalancer   10.111.82.111   localhost     8080:30224/TCP   3m17s
```

nasz nginx jest teraz dostępny pod adresem http://localhost:8080
```
> curl http://localhost:8080
<!DOCTYPE html>
<html>
<head>
<title>Welcome to nginx!</title>
```


## Ćwiczenie 2

### Utwórz dwa Pody z aplikacją helloapp, które mają po jednym wspólnym Label, oraz posiadają oprócz tego inne Label (poniżej przykład).
```
# Pod 1
labels:
  app: helloapp
  ver: v1

# Pod 2
labels:
  app: helloapp
  ver: v1
```
### Do tak utworzonych Podów podepnij serwis i sprawdź jak się zachowuje.

